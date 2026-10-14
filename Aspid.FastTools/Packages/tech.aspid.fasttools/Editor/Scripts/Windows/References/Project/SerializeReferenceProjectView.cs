using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;
using static Aspid.FastTools.SerializeReferences.Editors.SerializeReferenceAuditUI;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal sealed partial class SerializeReferenceProjectView : VisualElement
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";

        private const string RootClass = "aspid-fasttools-repair-references";
        private const string ContentClass = RootClass + "__content";
        private const string PanelClass = RootClass + "__panel";
        private const string PanelTitleClass = RootClass + "__panel-title";
        private const string PanelDescriptionClass = RootClass + "__panel-description";
        private const string ScanProjectClass = RootClass + "__scan-project";
        private const string ScanProjectIconClass = RootClass + "__scan-project-icon";
        private const string EmptyClass = RootClass + "__empty";
        private const string EmptyHiddenClass = EmptyClass + "--hidden";
        private const string EmptyIconClass = RootClass + "__empty-icon";
        private const string EmptyIconInfoClass = EmptyIconClass + "--info";
        private const string EmptyIconSuccessClass = EmptyIconClass + "--success";
        private const string EmptyTitleClass = RootClass + "__empty-title";
        private const string EmptyMessageClass = RootClass + "__empty-message";
        private const string ResultsClass = RootClass + "__results";
        private const string ResultsHiddenClass = ResultsClass + "--hidden";
        private const string ResultsHeaderClass = RootClass + "__results-header";
        private const string ResultsHintClass = RootClass + "__results-hint";
        private const string ResultsHintHiddenClass = ResultsHintClass + "--hidden";
        private const string SummaryListClass = RootClass + "__summary-list";
        private const string SummaryClass = RootClass + "__summary";
        private const string SummaryUndoClass = RootClass + "__summary-undo";
        private const string ScrollClass = RootClass + "__scroll";
        private const string NavTargetClass = RootClass + "__nav-target";
        private const string NavTargetFocusedClass = NavTargetClass + "--focused";

        private const string ScanLabel = "Scan Project";
        private const string RescanLabel = "Rescan";

        private readonly VisualElement _empty;
        private readonly VisualElement _results;
        private readonly AspidLabel _resultsHeader;
        private readonly VisualElement _summaries;
        private readonly Label _resultsHint;
        private readonly VisualElement _list;
        private readonly AspidGradientButton _scanButton;
        private readonly ScrollView _scroll;

        private readonly NavRing _ring;

        // Rows shown so far per card, by card key; a card that is not here shows its first page.
        private readonly Dictionary<string, int> _shownRows = new(StringComparer.Ordinal);

        // The row to focus while the list is rebuilt after Show more: its card key and its index in that card.
        private (string Key, int Index) _focusRow;

        // Which list layout is on screen, so that Show more rebuilds the same one and keeps its header.
        private bool _isMissingClean;

        private readonly AuditPickerHost _picker;


        // Required-field scanning has no incremental index; cache results across tab switches until an explicit rescan.
        private static bool _requiredIsWarm;
        private static bool _requiredCheckDisabled;
        private static IReadOnlyList<GateViolation> _requiredViolationsCache = Array.Empty<GateViolation>();

        private static IReadOnlyList<GateViolation> RequiredViolationsForRender =>
            _requiredIsWarm ? _requiredViolationsCache : Array.Empty<GateViolation>();

        // Stored type names have no incremental index either: Scan Project reads them, a type name fix re-reads its files.
        private static bool _typeNamesIsWarm;
        private static List<MissingTypeNameLocation> _typeNamesCache = new();

        private static IReadOnlyList<MissingTypeNameLocation> TypeNamesForRender =>
            _typeNamesIsWarm ? _typeNamesCache : Array.Empty<MissingTypeNameLocation>();

        // Files Scan Project could not read (binary, Git LFS pointer, locked or otherwise unopenable): their absence
        // from the lists proves nothing.
        private static int _unreadFileCount;

        private static RequiredAuditState RequiredAudit =>
            SerializeReferenceProjectSummary.GetRequiredAuditState(
                _requiredIsWarm, _requiredCheckDisabled, SerializeReferenceSettings.BuildSeverity == GateSeverity.Off);

        public Action<Object> OnInspectAsset;

        public Action<StatusStyle.Type> OnCanvasStatus;

        // The window keeps this view between visits to its tab, so the summaries and their Undo survive them; Rescan
        // empties the list.
        public readonly List<RepairSummary> Summaries = new();

        public SerializeReferenceProjectView()
        {
            var root = this;
            style.flexGrow = 1;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(RootClass);

            var panelTitle = new AspidLabel("Find missing references", AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(PanelTitleClass);

            var panelDescription = new Label(
                    "Sweep every asset under Assets/ for broken [SerializeReference] types and SerializableType names, " +
                    "and bulk-fix them by type.")
                .AddClass(PanelDescriptionClass);

            // The icon and the accent label mark the band as a button; a bare label read as an empty field.
            _scanButton = new AspidGradientButton(ScanLabel, _ => ScanProject())
                .AddClass(ScanProjectClass);
            _scanButton.AddLeadingContent(BuildRescanIcon(ScanProjectIconClass));

            var panel = new VisualElement()
                .AddClass(PanelClass)
                .AddChild(panelTitle)
                .AddChild(panelDescription)
                .AddChild(_scanButton);

            _empty = new VisualElement().AddClass(EmptyClass);

            _resultsHeader = new AspidLabel(string.Empty, AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H4)
                    .SetLineTheme(ThemeStyle.Type.Dark)
                    .SetLineStatus(StatusStyle.Type.Warning))
                .AddClass(ResultsHeaderClass);

            _resultsHint = new Label(string.Empty).AddClass(ResultsHintClass).AddClass(ResultsHintHiddenClass);

            _summaries = new VisualElement().AddClass(SummaryListClass);

            _list = new VisualElement();

            _results = new VisualElement()
                .AddClass(ResultsClass)
                .AddChild(_resultsHeader)
                .AddChild(_resultsHint)
                .AddChild(_summaries)
                .AddChild(_list);

            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(panel)
                .AddChild(_empty)
                .AddChild(_results);

            _scroll = new ScrollView().AddClass(ScrollClass);
            _scroll.AddChild(content);
            ReferenceEmptyStateLayout.PinIcon(_empty, _scroll);

            root.AddChild(_scroll);

            _picker = new AuditPickerHost(this, _list, _pickerClassSet);

            _ring = new NavRing(
                host: this,
                navTargetClass: NavTargetClass,
                focusedClass: NavTargetFocusedClass,
                scrollTo: element => _scroll.ScrollTo(element),
                isSuspended: () => _picker.IsOpen);

            ResetNavTargets();
        }

        private void ResetNavTargets()
        {
            _ring.Clear(keepFocusedElement: true);
            RegisterNavTarget(_scanButton, ScanProject);
        }

        private void RegisterNavTarget(VisualElement element, Action activate) => _ring.Register(element, activate);

        private static bool IsWarm => SerializeReferenceTypeUsageIndex.IsWarm || _requiredIsWarm || _typeNamesIsWarm;

        public void Initialize()
        {
            foreach (var summary in Summaries)
                ShowSummary(summary);

            Refresh();
        }

        // The window keeps this view between visits to its tab; the index may have changed since, so the groups are
        // listed again.
        internal void Refresh()
        {
            _picker.Close();

            if (!IsWarm)
            {
                ShowIdle();
                return;
            }

            // A repair can leave nothing to list, which RenderGroups shows as the empty state with the summaries hidden.
            if (Summaries.Count > 0) RerenderAfterBulkEdit();
            else RenderWarmGroups();
        }

        public void ScanProject()
        {
            if (_list is null) return;

            _picker.Close();

            // A pull may have changed the committed severity and excluded folders since the Editor loaded them.
            SerializeReferenceSettings.ReloadShared();

            // Each sweep has a cancelable progress bar. Cancelling keeps the earlier results, the summaries and their
            // Undo, since a half-finished scan would pass for a clean project.
            var requiredCheckDisabled = SerializeReferenceSettings.BuildSeverity == GateSeverity.Off;
            var requiredViolations = CollectRequiredViolations(requiredCheckDisabled);
            if (requiredViolations is null) return;

            var unscanned = new List<(string AssetPath, AssetFileFormat Format)>();
            var typeNames = MissingTypeNames.ScanProject(unscanned: unscanned);
            if (typeNames is null) return;

            ClearSummaries();
            _shownRows.Clear();
            MissingReferenceGroup.ClearConstraintCache();

            _requiredCheckDisabled = requiredCheckDisabled;
            _requiredViolationsCache = requiredViolations;
            _requiredIsWarm = true;

            _typeNamesCache = typeNames;
            _typeNamesIsWarm = true;

            _unreadFileCount = SerializeReferenceGateScanner.CountReportableUnscanned(unscanned, EditorSettings.serializationMode);

            var notice = SerializeReferenceGateScanner.DescribeUnscanned(unscanned, EditorSettings.serializationMode);
            if (notice is not null) Debug.LogWarning(notice);

            RenderWarmGroups();
        }

        private void RenderWarmGroups()
        {
            if (_list is null) return;
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            RenderGroups(MissingReferenceGroup.CollectFromIndex(), RequiredViolationsForRender);
        }

        // Null when the progress bar is cancelled.
        private static IReadOnlyList<GateViolation> CollectRequiredViolations(bool checkDisabled)
        {
            if (checkDisabled) return Array.Empty<GateViolation>();

            try
            {
                return SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly, (progress, path) =>
                    EditorUtility.DisplayCancelableProgressBar("Scanning Required Fields", path, progress));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // Keeps a warm audit current after a bulk edit by re-checking only the files it rewrote on disk.
        private static void RefreshRequiredViolations(IEnumerable<string> editedPaths)
        {
            if (RequiredAudit != RequiredAuditState.Checked) return;

            _requiredViolationsCache = SerializeReferenceGateScanner.RescanRequiredFields(_requiredViolationsCache, editedPaths);
        }

        private void RenderGroups(List<MissingReferenceGroup> groups, IReadOnlyList<GateViolation> requiredViolations)
        {
            _isMissingClean = false;
            _list.Clear();
            ResetNavTargets();

            var overrides = MissingReferenceGroup.CollectOverridesFromIndex();
            var pendingOverrides = overrides.Count(entry => MissingReferenceGroup.OverrideMigrationTarget(entry) is not null);
            var typeNameGroups = MissingTypeNameGroup.Build(TypeNamesForRender);
            var missingCount = groups.Sum(group => group.Entries.Count) + overrides.Count + TypeNamesForRender.Count;
            var requiredCount = requiredViolations.Count;

            if (missingCount == 0 && requiredCount == 0)
            {
                // An audit that never ran, went stale or was switched off has found nothing, which is not the same
                // as a clean project.
                var (success, title, message) = SerializeReferenceProjectSummary.BuildNothingFoundState(
                    RequiredAudit, typeNamesScanned: _typeNamesIsWarm, unreadFileCount: _unreadFileCount);
                ShowEmptyState(success, title, message);
                return;
            }

            var broken = new List<(MissingReferenceGroup Group, MissingReferenceMigration Migration)>();
            var migrations = new List<(MissingReferenceGroup Group, MissingReferenceMigration Migration)>();
            foreach (var group in groups)
            {
                var migration = new MissingReferenceMigration(group);
                (migration.IsMigration ? migrations : broken).Add((group, migration));
            }

            var migrationCount = migrations.Sum(entry => entry.Group.Entries.Count) + pendingOverrides;
            ShowResults(
                SerializeReferenceProjectSummary.BuildResultsHeaderText(
                    missingCount - migrationCount - TypeNamesForRender.Count, migrationCount, requiredCount, TypeNamesForRender.Count),
                StatusStyle.Type.Warning);

            // Only these cards have a Fix all; a renamed type has Reassign all and Migrate all instead.
            var hasBrokenGroups = broken.Count > 0 || typeNameGroups.Count > 0;
            SetResultsHint(SerializeReferenceProjectSummary.BuildResultsHintText(
                RequiredAudit,
                typeNamesScanned: _typeNamesIsWarm,
                unreadFileCount: _unreadFileCount));
            SetResultsGuide(SerializeReferenceProjectSummary.BuildResultsGuideText(
                requiredCount > 0,
                hasOverrides: overrides.Count > 0,
                hasBrokenGroups: hasBrokenGroups));

            // The cards fall into headed sections, so the colour of a title needs no legend. Required fields and
            // prefab instance overrides always get their heading, since their cards carry no title of their own; a list
            // of only broken or only renamed types needs none, as its heading would repeat the results header.
            var allOverridesPending = overrides.Count > 0 && overrides.Count == pendingOverrides;
            var sectioned = requiredCount > 0 || overrides.Count > 0 || (hasBrokenGroups && migrations.Count > 0);
            var isFirst = true;

            void AddHeading(string title, StatusStyle.Type status, string tooltip)
            {
                if (!sectioned) return;

                _list.AddChild(BuildSectionHeading(title, status, tooltip, isFirst));
                isFirst = false;
            }

            if (hasBrokenGroups)
            {
                AddHeading(BrokenSectionTitle, StatusStyle.Type.Warning, BrokenSectionTooltip);

                foreach (var (group, migration) in broken)
                    _list.AddChild(BuildGroupCard(group, migration));

                foreach (var group in typeNameGroups)
                    _list.AddChild(BuildTypeNameGroupCard(group));
            }

            if (requiredCount > 0)
            {
                AddHeading(RequiredSectionTitle, StatusStyle.Type.Warning, RequiredSectionTooltip);
                AddRequiredFileCards(requiredViolations);
            }

            if (overrides.Count > 0)
            {
                AddHeading(OverridesSectionTitle, allOverridesPending ? StatusStyle.Type.Info : StatusStyle.Type.Warning,
                    OverridesSectionTooltip);
                AddOverrideFileCards(overrides);
            }

            if (migrations.Count > 0)
            {
                AddHeading(RenamedSectionTitle, StatusStyle.Type.Info, RenamedSectionTooltip);

                foreach (var (group, migration) in migrations)
                    _list.AddChild(BuildGroupCard(group, migration));
            }
        }

        // Builds the list again for the data on screen, with the layout and the header it already has.
        private void RerenderList()
        {
            if (_isMissingClean) ShowMissingReferencesClean();
            else RenderWarmGroups();
        }

        private void RerenderAfterBulkEdit()
        {
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            var groups = MissingReferenceGroup.CollectFromIndex();
            if (groups.Count == 0 && MissingReferenceGroup.CollectOverridesFromIndex().Count == 0 && TypeNamesForRender.Count == 0)
                ShowMissingReferencesClean();
            else RenderGroups(groups, RequiredViolationsForRender);
        }

        private void ShowMissingReferencesClean()
        {
            _isMissingClean = true;
            _list.Clear();
            ResetNavTargets();
            var requiredViolations = RequiredViolationsForRender;

            ShowResults(
                requiredViolations.Count == 0
                    ? "No missing references"
                    : $"No missing references, {BuildCountText(requiredViolations.Count, "required violation")}",
                SerializeReferenceProjectSummary.GetMissingReferencesCleanStatus(
                    RequiredAudit, requiredViolations.Count > 0, typeNamesScanned: _typeNamesIsWarm, unreadFileCount: _unreadFileCount));
            SetResultsGuide(requiredViolations.Count > 0
                ? SerializeReferenceProjectSummary.BuildResultsGuideText(hasRequiredViolations: true, hasBrokenGroups: false)
                : string.Empty);
            SetResultsHint(SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(
                RequiredAudit, requiredViolations.Count > 0, typeNamesScanned: _typeNamesIsWarm, unreadFileCount: _unreadFileCount));

            if (requiredViolations.Count == 0) return;

            // The file cards carry no section name of their own, so the heading names them here too.
            _list.AddChild(BuildSectionHeading(RequiredSectionTitle, StatusStyle.Type.Warning, RequiredSectionTooltip, isFirst: true));
            AddRequiredFileCards(requiredViolations);
        }

        // An empty hint takes no line, so the legend moves up under the header.
        private void SetResultsHint(string text)
        {
            _resultsHint.text = text;
            _resultsHint.EnableInClassList(ResultsHintHiddenClass, string.IsNullOrEmpty(text));
        }

        private void SetResultsGuide(string text)
        {
            _resultsHeader.tooltip = text;
        }

        private void ShowEmptyState(bool success, string title, string message)
        {
            ResetNavTargets();
            _results.AddClass(ResultsHiddenClass);
            _empty.RemoveClass(EmptyHiddenClass);
            _empty.Clear();
            OnCanvasStatus?.Invoke(success ? StatusStyle.Type.Success : StatusStyle.Type.Info);

            var icon = new VisualElement()
                .AddClass(EmptyIconClass)
                .AddClass(success ? EmptyIconSuccessClass : EmptyIconInfoClass);

            var titlePreset = AspidLabelPreset.Default
                .SetLabelTheme(success ? ThemeStyle.Type.Light : ThemeStyle.Type.Lightness)
                .SetLabelSize(AspidLabelSizeStyle.Type.H3)
                .SetLineSize(AspidDividingLineSizeStyle.Type.None);

            if (success) titlePreset = titlePreset.SetLabelStatus(StatusStyle.Type.Success);

            _empty.AddChild(icon)
                .AddChild(new AspidLabel(title, titlePreset).AddClass(EmptyTitleClass))
                .AddChild(new Label(message).AddClass(EmptyMessageClass));
        }

        private void ShowIdle() => ShowEmptyState(
            success: false,
            title: "Project not scanned",
            message: "Run Scan Project to map every broken [SerializeReference] type and SerializableType name across your " +
                     "assets — then repair each missing type in bulk.");

        private void ShowResults(string headerText, StatusStyle.Type status)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);
            _resultsHeader.Text = headerText;
            OnCanvasStatus?.Invoke(status);
        }

        private void AddSummary(RepairSummary summary)
        {
            Summaries.Add(summary);
            ShowSummary(summary);
        }

        private void ShowSummary(RepairSummary summary)
        {
            var box = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Warning))
                .AddClass(SummaryClass);
            box.Title = summary.Title;
            box.Message = summary.Message;

            if (summary.Receipt is not null)
                box.AddChild(BuildUndoButton(() => UndoGroupFix(summary, box)));
            else if (summary.TypeNameReceipt is not null)
                box.AddChild(BuildUndoButton(() => UndoTypeNameFix(summary, box)));

            _summaries.AddChild(box);
        }

        private static Button BuildUndoButton(Action undo) =>
            new Button(undo) { text = "Undo" }
                .AddClass(AspidStyles.ActionButtonClass)
                .AddClass(SummaryUndoClass);

        private void ClearSummaries()
        {
            Summaries.Clear();
            _summaries?.Clear();
        }
    }
}
