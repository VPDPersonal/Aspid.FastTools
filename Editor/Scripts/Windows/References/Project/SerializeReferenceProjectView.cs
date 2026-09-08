using System;
using System.Linq;
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
        private const string LegendClass = RootClass + "__legend";
        private const string LegendHiddenClass = LegendClass + "--hidden";
        private const string LegendItemClass = RootClass + "__legend-item";
        private const string LegendDotClass = RootClass + "__legend-dot";
        private const string LegendDotInfoClass = LegendDotClass + "--info";
        private const string LegendTextClass = RootClass + "__legend-text";
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
        private readonly VisualElement _legend;
        private readonly VisualElement _list;
        private readonly AspidGradientButton _scanButton;
        private readonly ScrollView _scroll;

        private readonly NavRing _ring;

        private readonly AuditPickerHost _picker;

        private static readonly LegendClasses _legendClassSet = new(LegendItemClass, LegendDotClass, LegendDotInfoClass, LegendTextClass);

        // Required-field scanning has no incremental index; cache results across tab switches until an explicit rescan.
        private static bool _requiredIsWarm;
        private static IReadOnlyList<GateViolation> _requiredViolationsCache = Array.Empty<GateViolation>();

        private static IReadOnlyList<GateViolation> RequiredViolationsForRender =>
            _requiredIsWarm ? _requiredViolationsCache : Array.Empty<GateViolation>();

        public Action<Object> OnInspectAsset;

        public Action<StatusStyle.Type> OnCanvasStatus;

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
                    "Sweep every asset under Assets/ for broken [SerializeReference] types and bulk-fix them by type.")
                .AddClass(PanelDescriptionClass);

            _scanButton = new AspidGradientButton(ScanLabel, _ => ScanProject())
                .AddClass(ScanProjectClass);

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

            _resultsHint = new Label(string.Empty).AddClass(ResultsHintClass);

            _legend = new VisualElement()
                .AddClass(LegendClass)
                .AddClass(LegendHiddenClass)
                .AddChild(BuildLegendItem("Broken — pick a replacement", info: false, _legendClassSet))
                .AddChild(BuildLegendItem("Renamed — one-click migrate", info: true, _legendClassSet));

            _summaries = new VisualElement().AddClass(SummaryListClass);

            _list = new VisualElement();

            _results = new VisualElement()
                .AddClass(ResultsClass)
                .AddChild(_resultsHeader)
                .AddChild(_resultsHint)
                .AddChild(_legend)
                .AddChild(_summaries)
                .AddChild(_list);

            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(panel)
                .AddChild(_empty)
                .AddChild(_results);

            _scroll = new ScrollView().AddClass(ScrollClass);
            _scroll.AddChild(content);

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

        public void Initialize()
        {
            if (SerializeReferenceTypeUsageIndex.IsWarm || _requiredIsWarm) RenderWarmGroups();
            else ShowIdle();
        }

        public void ScanProject()
        {
            if (_list is null) return;

            _picker.Close();
            ClearSummaries();

            _requiredViolationsCache = CollectRequiredViolations();
            _requiredIsWarm = true;

            RenderWarmGroups();
        }

        private void RenderWarmGroups()
        {
            if (_list is null) return;
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            RenderGroups(MissingReferenceGroup.CollectFromIndex(), RequiredViolationsForRender);
        }

        private static IReadOnlyList<GateViolation> CollectRequiredViolations() =>
            SerializeReferenceSettings.BuildSeverity == GateSeverity.Off
                ? Array.Empty<GateViolation>()
                : SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly);

        private void RenderGroups(List<MissingReferenceGroup> groups, IReadOnlyList<GateViolation> requiredViolations)
        {
            _list.Clear();
            ResetNavTargets();

            var missingCount = groups.Sum(group => group.Entries.Count);
            var requiredCount = requiredViolations.Count;

            if (missingCount == 0 && requiredCount == 0)
            {
                ShowEmptyState(
                    success: true,
                    title: "Project clean",
                    message: "No missing managed references or unset required fields found anywhere under Assets/.");
                return;
            }

            var migrations = new List<(MissingReferenceGroup Group, MissingReferenceMigration Migration)>();
            foreach (var group in groups)
            {
                var migration = new MissingReferenceMigration(group);
                if (migration.IsMigration) migrations.Add((group, migration));
                else _list.AddChild(BuildGroupCard(group, migration));
            }

            var migrationCount = migrations.Sum(entry => entry.Group.Entries.Count);
            ShowResults(
                SerializeReferenceProjectSummary.BuildResultsHeaderText(missingCount - migrationCount, migrationCount, requiredCount),
                StatusStyle.Type.Warning);
            _resultsHint.text = SerializeReferenceProjectSummary.BuildResultsHintText(requiredCount > 0);

            var hasAmber = groups.Count > migrations.Count || requiredCount > 0;
            _legend.EnableInClassList(LegendHiddenClass, migrations.Count == 0 || !hasAmber);

            if (requiredCount > 0)
                _list.AddChild(BuildRequiredGroupCard(requiredViolations));

            foreach (var (group, migration) in migrations)
                _list.AddChild(BuildGroupCard(group, migration));
        }

        private void RerenderAfterBulkEdit()
        {
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            var groups = MissingReferenceGroup.CollectFromIndex();
            if (groups.Count == 0) ShowMissingReferencesClean();
            else RenderGroups(groups, RequiredViolationsForRender);
        }

        private void ShowMissingReferencesClean()
        {
            _list.Clear();
            ResetNavTargets();
            var requiredViolations = RequiredViolationsForRender;

            ShowResults(
                requiredViolations.Count == 0
                    ? "No missing references"
                    : $"No missing references, {BuildCountText(requiredViolations.Count, "required violation")}",
                StatusStyle.Type.Success);
            _resultsHint.text = "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";
            _legend.AddClass(LegendHiddenClass);

            if (requiredViolations.Count > 0)
                _list.AddChild(BuildRequiredGroupCard(requiredViolations));
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
            message: "Run Scan Project to map every broken [SerializeReference] type across your assets — then repair each missing type in bulk.");

        private void ShowResults(string headerText, StatusStyle.Type status)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);
            _resultsHeader.Text = headerText;
            OnCanvasStatus?.Invoke(status);
        }

        private void ShowSummary(string title, string message, Action<VisualElement> onUndo)
        {
            var summary = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Warning))
                .AddClass(SummaryClass);
            summary.Title = title;
            summary.Message = message;

            if (onUndo is not null)
                summary.AddChild(new AspidGradientButton("Undo", _ => onUndo(summary)).AddClass(SummaryUndoClass));

            _summaries.AddChild(summary);
        }

        private void ClearSummaries() => _summaries?.Clear();
    }
}
