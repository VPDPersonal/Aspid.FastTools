using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Repair tool for missing <c>[SerializeReference]</c> types. It runs in two modes that share one results list:
    /// <list type="bullet">
    /// <item><b>Single asset</b> — assigning the asset field scans that one file and lists every orphaned managed
    /// reference (at any nesting depth, on any child object) as a full-width Fix row, fixed by rewriting the stored
    /// type directly in the YAML so it never needs Prefab Mode.</item>
    /// <item><b>Project</b> — the <c>Scan Project</c> button sweeps every text asset under <c>Assets/</c>, groups the
    /// broken references by their stored (now unloadable) type, and offers a single bulk <c>Fix all</c> per group:
    /// one type pick + one confirmation rewrites every entry across every affected file.</item>
    /// </list>
    /// </summary>
    internal sealed class SerializeReferenceProjectView : VisualElement
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
        private const string PickerClass = RootClass + "__picker";
        private const string PickerAttachedClass = PickerClass + "--attached";

        private const string GroupClass = RootClass + "__group";
        private const string GroupMigrateClass = GroupClass + "--migrate";
        private const string GroupPickingClass = GroupClass + "--picking";
        private const string GroupHeaderHoverClass = GroupClass + "--header-hover";
        private const string GroupDividerClass = RootClass + "__group-divider";
        private const string GroupSweepClass = RootClass + "__group-sweep";
        private const string GroupSweepMigrateClass = GroupSweepClass + "--migrate";
        private const string GroupHeaderRowClass = RootClass + "__group-header-row";
        private const string GroupHeaderRowStaticClass = GroupHeaderRowClass + "--static";
        private const string GroupHeaderClass = RootClass + "__group-header";
        private const string GroupCountClass = RootClass + "__group-count";
        private const string GroupFixAllClass = RootClass + "__group-fix-all";
        private const string GroupFixAllMigrateClass = GroupFixAllClass + "--migrate";
        private const string GroupActionClass = RootClass + "__group-action";
        private const string GroupActionInfoClass = GroupActionClass + "--info";
        private const string GroupEntryClass = RootClass + "__group-entry";
        private const string GroupEntryPathClass = RootClass + "__group-entry-path";
        private const string GroupEntryRidClass = RootClass + "__group-entry-rid";
        private const string GroupEntryFieldClass = RootClass + "__group-entry-field";
        private const string NavTargetClass = RootClass + "__nav-target";
        private const string NavTargetFocusedClass = NavTargetClass + "--focused";

        // Chevron on the "Fix all (N)" dropdown button; only the glyph differs between the two states.
        private const string FixArrowCollapsed = "▼";
        private const string FixArrowExpanded = "▲";

        // Scan button label: cold call-to-action before the first scan, quiet refresh once the index is warm.
        private const string ScanLabel = "Scan Project";
        private const string RescanLabel = "Rescan";

        private readonly VisualElement _empty;
        private readonly VisualElement _results;
        private readonly AspidLabel _resultsHeader;
        private readonly VisualElement _summaries;
        private readonly Label _resultsHint;
        private readonly VisualElement _legend;
        private readonly VisualElement _list;
        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;
        private VisualElement _openPickerCard;
        private readonly AspidGradientButton _scanButton;
        private readonly ScrollView _scroll;

        // Keyboard navigation: one flat focus ring over every actionable element in visual order — Rescan first,
        // then each card's Fix all / action row / entry rows. -1 means nothing is highlighted.
        private readonly List<(VisualElement Element, Action Activate)> _navTargets = new();
        private int _navIndex = -1;

        // Required-violations audit has no incrementally-maintained index like SerializeReferenceTypeUsageIndex, so it
        // is only (re)scanned on an explicit Scan/Rescan click, not on every Initialize() (tab switch would otherwise
        // pay for a full project sweep). Static so the result survives the view being rebuilt on a tab switch.
        private static bool _requiredIsWarm;
        private static IReadOnlyList<GateViolation> _requiredViolationsCache = Array.Empty<GateViolation>();

        private static IReadOnlyList<GateViolation> RequiredViolationsForRender =>
            _requiredIsWarm ? _requiredViolationsCache : Array.Empty<GateViolation>();

        /// <summary>
        /// Jump from a project-audit result row to that asset's Inspect graph. Wired by the host window.
        /// </summary>
        public Action<Object> OnInspectAsset;

        /// <summary>
        /// Reports this view's state-tone to the host window, which owns the shared dotted canvas. Wired by the window.
        /// </summary>
        public Action<Color> OnCanvasTone;

        public SerializeReferenceProjectView()
        {
            var root = this;
            style.flexGrow = 1;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            var panelTitle = new AspidLabel("Find missing references", AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(PanelTitleClass);

            var panelDescription = new Label(
                    "Sweep every asset under Assets/ for broken [SerializeReference] types and bulk-fix them by type.")
                .AddClass(PanelDescriptionClass);

            // Label flips between ScanLabel and RescanLabel as the index warms.
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

            // Color key for the two card accents; only shown when both are actually on screen (see RenderGroups).
            _legend = new VisualElement()
                .AddClass(LegendClass)
                .AddClass(LegendHiddenClass)
                .AddChild(BuildLegendItem("Broken — pick a replacement", info: false))
                .AddChild(BuildLegendItem("Renamed — one-click migrate", info: true));

            // Receipt stack: one help-box per bulk Fix all, kept across chained fixes and cleared only on a fresh scan.
            _summaries = new VisualElement().AddClass(SummaryListClass);

            _list = new VisualElement();

            _results = new VisualElement()
                .AddClass(ResultsClass)
                .AddChild(_resultsHeader)
                .AddChild(_resultsHint)
                .AddChild(_legend)
                .AddChild(_summaries)
                .AddChild(_list);

            // One scroll spans the whole view, so the panel scrolls away with the group list instead of staying pinned.
            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(panel)
                .AddChild(_empty)
                .AddChild(_results);

            _scroll = new ScrollView().AddClass(ScrollClass);
            _scroll.AddChild(content);

            root.AddChild(_scroll);

            // Arrow-key navigation: the root holds keyboard focus (grabbed on attach, re-grabbed when the picker
            // closes) so key events reach OnNavKeyDown even before anything is highlighted; events from focused
            // descendants bubble here too.
            focusable = true;
            RegisterCallback<KeyDownEvent>(OnNavKeyDown);
            RegisterCallback<AttachToPanelEvent>(_ => schedule.Execute(() => Focus()));

            ResetNavTargets();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Keyboard navigation
        // ---------------------------------------------------------------------------------------------------------

        private void OnNavKeyDown(KeyDownEvent evt)
        {
            // The open type picker owns the keyboard (its own search + list navigation) — stay out of its way.
            if (_openPicker is not null) return;

            // FunctionKey rides along with arrows on some platforms; any real modifier means the key is not ours.
            if ((evt.modifiers & ~EventModifiers.FunctionKey) != 0) return;

            switch (evt.keyCode)
            {
                case KeyCode.DownArrow:
                    MoveNavFocus(+1);
                    evt.StopPropagation();
                    break;

                case KeyCode.UpArrow:
                    MoveNavFocus(-1);
                    evt.StopPropagation();
                    break;

                case KeyCode.Return or KeyCode.KeypadEnter when _navIndex >= 0:
                    _navTargets[_navIndex].Activate();
                    evt.StopPropagation();
                    break;

                case KeyCode.Escape when _navIndex >= 0:
                    ClearNavFocus();
                    evt.StopPropagation();
                    break;
            }
        }

        // First press (nothing highlighted) lands on Rescan whichever arrow is hit; after that the ring clamps at
        // both ends instead of wrapping.
        private void MoveNavFocus(int delta)
        {
            if (_navTargets.Count == 0) return;
            SetNavFocus(_navIndex < 0 ? 0 : Mathf.Clamp(_navIndex + delta, 0, _navTargets.Count - 1));
        }

        private void SetNavFocus(int index, bool scrollTo = true)
        {
            if (_navIndex == index) return;

            ClearNavFocus();
            _navIndex = index;

            var element = _navTargets[index].Element;
            // Gradient buttons paint their hover in code (accent overlay + tinted labels); the focused class's flat
            // fill would just show through their fading gradient as a gray pill, so they take ONLY the programmatic
            // hover and the plain rows take ONLY the class.
            if (element is AspidGradientButton button) button.Highlighted = true;
            else element.AddToClassList(NavTargetFocusedClass);
            SetHeaderSweep(element, on: true);
            if (scrollTo) _scroll.ScrollTo(element);
        }

        private void ClearNavFocus()
        {
            if (_navIndex >= 0 && _navIndex < _navTargets.Count)
            {
                var element = _navTargets[_navIndex].Element;
                if (element is AspidGradientButton button) button.Highlighted = false;
                else element.RemoveFromClassList(NavTargetFocusedClass);
                SetHeaderSweep(element, on: false);
            }

            _navIndex = -1;
        }

        // A focused Fix all header also lights its card's divider sweep — the same card-level hover mirror the
        // mouse path drives in CreateHeaderSweep — so keyboard focus and mouse hover render identically.
        private static void SetHeaderSweep(VisualElement element, bool on)
        {
            if (element.ClassListContains(GroupFixAllClass))
                element.parent?.EnableInClassList(GroupHeaderHoverClass, on);
        }

        // Every render pass rebuilds the ring from scratch (the old elements are gone with _list.Clear()). Rescan is
        // always slot 0; a highlight sitting on it survives the rebuild, so Enter-on-Rescan keeps its highlight.
        private void ResetNavTargets()
        {
            var keepScanFocus = _navIndex == 0;
            ClearNavFocus();
            _navTargets.Clear();

            RegisterNavTarget(_scanButton, ScanProject);
            if (keepScanFocus) SetNavFocus(0, scrollTo: false);
        }

        private void RegisterNavTarget(VisualElement element, Action activate)
        {
            // EnableInClassList (not AddToClassList): Rescan is re-registered on every reset and must not stack copies.
            element.EnableInClassList(NavTargetClass, true);
            _navTargets.Add((element, activate));
        }

        // Cold index: open idle and wait for a deliberate Scan click — the cold sweep parses every asset's YAML behind
        // a blocking bar, so it must never run unasked. Warm index: re-deriving groups is a cheap in-memory filter, so
        // results survive a tab switch. The breakage-notification deep-link bypasses this and calls ScanProject directly.
        public void Initialize()
        {
            if (SerializeReferenceTypeUsageIndex.IsWarm || _requiredIsWarm) RenderWarmGroups();
            else ShowIdle();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Project mode
        // ---------------------------------------------------------------------------------------------------------

        // Sweeps the project for missing references and groups them by stored broken type (slow when the index is cold).
        public void ScanProject()
        {
            if (_list is null) return;

            ClosePicker();
            ClearSummaries();

            // Unlike the missing-type index, the required-field scan has nothing incremental behind it — this is the
            // one deliberate moment it pays for a full project sweep (see RequiredViolationsForRender).
            _requiredViolationsCache = CollectRequiredViolations();
            _requiredIsWarm = true;

            RenderWarmGroups();
        }

        // Collects the unresolved set from the warm index and paints it; shared by Scan/Rescan and Initialize's warm restore.
        private void RenderWarmGroups()
        {
            if (_list is null) return;
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            var groups = CollectProjectGroups(out var canceled);
            RenderGroups(groups, RequiredViolationsForRender, canceled);
        }

        // Full project sweep for unset [TypeSelector(Required = true)] fields, reusing the same headless scanner the
        // build/CI gate uses. Skipped entirely when the gate is switched Off — a required audit nobody wants to fail
        // or warn on shouldn't cost a full-project YAML sweep on every Scan click either.
        private static IReadOnlyList<GateViolation> CollectRequiredViolations() =>
            SerializeReferenceSettings.BuildSeverity == GateSeverity.Off
                ? Array.Empty<GateViolation>()
                : SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly);

        // Paints a collected group set: count header + hint + one card per broken-type group plus one Required
        // violations card, or the terminal hero when both are empty. ApplyGroupFix/ClearGroupToNull special-case the
        // came-back-clean case so their summary HelpBox survives (see there).
        private void RenderGroups(List<ProjectGroup> groups, IReadOnlyList<GateViolation> requiredViolations, bool canceled)
        {
            _list.Clear();
            ResetNavTargets();

            var missingCount = groups.Sum(group => group.Entries.Count);
            var requiredCount = requiredViolations.Count;

            if (missingCount == 0 && requiredCount == 0)
            {
                ShowEmptyState(
                    success: !canceled,
                    title: canceled ? "Scan canceled" : "Project clean",
                    message: canceled
                        ? "The project scan was canceled before finding any missing references."
                        : "No missing managed references or unset required fields found anywhere under Assets/.");
                return;
            }

            // Pending migrations sink to the very bottom, below the Required violations card too: the whole amber
            // band (broken groups, then required fields) stacks first and the calm blue one-click cards close the
            // list. Each band keeps the scanner's order.
            var migrations = new List<ProjectGroup>();
            foreach (var group in groups)
            {
                if (IsMigrationGroup(group)) migrations.Add(group);
                else _list.AddChild(BuildGroupCard(group));
            }

            // The header splits the migration entries out of the missing count — a [MovedFrom] rename with a
            // one-click fix shouldn't inflate the alarm number.
            var migrationCount = migrations.Sum(group => group.Entries.Count);
            ShowResults(
                BuildResultsHeaderText(missingCount - migrationCount, migrationCount, requiredCount),
                SerializeReferenceCanvasStyle.Warning);
            _resultsHint.text = BuildResultsHintText(canceled, requiredCount > 0);

            // The amber/blue key only earns its row when both accents are on screen at once.
            var hasAmber = groups.Count > migrations.Count || requiredCount > 0;
            _legend.EnableInClassList(LegendHiddenClass, migrations.Count == 0 || !hasAmber);

            if (requiredCount > 0)
                _list.AddChild(BuildRequiredGroupCard(requiredViolations));

            foreach (var group in migrations)
                _list.AddChild(BuildGroupCard(group));
        }

        // An authoritative [MovedFrom] rename whose target also fits the group's field constraint — the same gate
        // BuildGroupCard applies before offering Migrate all (see there for why the constraint matters).
        private static bool IsMigrationGroup(ProjectGroup group)
        {
            var constraint = group.ResolveConstraint();
            return SerializeReferenceMovedFromResolver.TryResolve(group.StoredType, out var target) &&
                (constraint == typeof(object) || constraint.IsAssignableFrom(target));
        }

        private static string BuildCountText(int count, string noun) =>
            count == 1 ? $"1 {noun}" : $"{count} {(noun.EndsWith("y") ? noun[..^1] + "ies" : noun + "s")}";

        // Only non-zero parts make the header; brokenCount is the missing total MINUS the pending-migration entries,
        // which get their own calmer "pending migration" wording.
        private static string BuildResultsHeaderText(int brokenCount, int migrationCount, int requiredCount)
        {
            var parts = new List<string>(3);
            if (brokenCount > 0) parts.Add(BuildCountText(brokenCount, "missing reference"));
            if (migrationCount > 0) parts.Add(BuildCountText(migrationCount, "pending migration"));
            if (requiredCount > 0) parts.Add(BuildCountText(requiredCount, "required violation"));

            return string.Join(", ", parts);
        }

        private static string BuildResultsHintText(bool canceled, bool hasRequiredViolations)
        {
            var hint = canceled
                ? "Scan canceled — showing partial results. Fix all re-points a group's every entry to one replacement, or to <None>."
                : "Each group is a broken stored type — Fix all re-points its every entry to one replacement, or to <None>.";

            if (hasRequiredViolations)
                hint += " Click a required-violation row to jump to its asset.";

            return hint;
        }

        // One dot + caption pair of the accent legend: amber (default) for the broken/required band, info blue for
        // the pending-migration cards.
        private static VisualElement BuildLegendItem(string text, bool info)
        {
            var dot = new VisualElement().AddClass(LegendDotClass);
            if (info) dot.AddClass(LegendDotInfoClass);

            return new VisualElement()
                .AddClass(LegendItemClass)
                .AddChild(dot)
                .AddChild(new Label(text).AddClass(LegendTextClass));
        }

        // Shared "no missing references left" branch for ApplyGroupFix/ClearGroupToNull: stays in the results region
        // (not the clean-state hero) so the fix's summary receipt survives, while still surfacing whatever Required
        // violations card RequiredViolationsForRender currently reports (empty right after ClearGroupToNull, which
        // invalidates the cache instead of risking a stale under-report — see its _requiredIsWarm = false).
        private void ShowMissingReferencesClean()
        {
            _list.Clear();
            ResetNavTargets();
            var requiredViolations = RequiredViolationsForRender;

            ShowResults(
                requiredViolations.Count == 0 ? "No missing references" : $"No missing references, {BuildCountText(requiredViolations.Count, "required violation")}",
                SerializeReferenceCanvasStyle.Success);
            _resultsHint.text = "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";
            _legend.AddClass(LegendHiddenClass);

            if (requiredViolations.Count > 0)
                _list.AddChild(BuildRequiredGroupCard(requiredViolations));
        }

        // Groups every unresolved managed reference by stored type, backed by the shared usage index. The out
        // parameter is kept for the call sites but is always false: the index warm-up runs to completion.
        private static List<ProjectGroup> CollectProjectGroups(out bool canceled)
        {
            canceled = false;
            var byType = new Dictionary<string, ProjectGroup>(StringComparer.Ordinal);

            foreach (var usage in SerializeReferenceTypeUsageIndex.EnumerateUnresolved())
            {
                var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                if (string.IsNullOrEmpty(path)) continue;

                var key = SerializeReferenceHelpers.StoredTypeKey(usage.StoredType);
                if (!byType.TryGetValue(key, out var group))
                {
                    group = new ProjectGroup(usage.StoredType);
                    byType.Add(key, group);
                }

                group.Add(path, new MissingReferenceEntry(usage.FileId, usage.Rid, usage.StoredType));
            }

            var groups = byType.Values.ToList();
            groups.Sort((a, b) => b.Entries.Count.CompareTo(a.Entries.Count));
            return groups;
        }

        // A broken-type group card: the whole header is one clickable row that toggles the type picker, with the bulk
        // "Fix all (N) ▼" action on the right. Entries are deliberately not individually fixable in project mode —
        // the per-row Fix affordance is reserved for single-asset mode.
        private VisualElement BuildGroupCard(ProjectGroup group)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var constraint = group.ResolveConstraint();
            var displayName = group.DisplayName;

            // An authoritative [MovedFrom] rename: Unity already migrates these in memory at load — only the files
            // still store the old name. The target must also fit the group's field constraint: Migrate all bypasses
            // the picker's assignability guarantee, and an incompatible target would be nulled by Unity at load.
            var isMigration = SerializeReferenceMovedFromResolver.TryResolve(group.StoredType, out var migrationTarget) &&
                (constraint == typeof(object) || constraint.IsAssignableFrom(migrationTarget));

            // Card-level modifier so card-wide states (the --picking accent frame) can follow the card's own
            // accent — a migration card is info-toned end to end, never the broken-card amber.
            if (isMigration) card.AddClass(GroupMigrateClass);

            // Built first so the type name + count can be docked into its body; the captured local is assigned before use.
            AspidGradientButton fixAll = null;
            fixAll = new AspidGradientButton(BuildFixAllLabel(group, expanded: false, isMigration),
                    _ => ToggleGroupPicker(group, constraint, fixAll))
                .AddClass(GroupFixAllClass);
            // A migration card keeps its calm info tone end to end — the amber Fix all accent is the "broken" alarm.
            if (isMigration) fixAll.AddClass(GroupFixAllMigrateClass);
            RegisterNavTarget(fixAll, () => ToggleGroupPicker(group, constraint, fixAll));
            fixAll.tooltip = constraint == typeof(object)
                ? $"{displayName}\nMixed or unresolvable field types — the picker is unconstrained (any managed-reference type)."
                : $"{displayName}\nConstrained to {constraint.FullName}.";

            // The type name + count line, ignored for picking so clicks fall through to the button's own handler.
            var header = new AspidLabel(group.StoredType.Class, AspidLabelPreset.Default
                    .SetLabelStatus(isMigration ? StatusStyle.Type.Info : StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(GroupHeaderClass)
                .SetPickingMode(PickingMode.Ignore);

            var count = new Label(BuildGroupCountText(group))
                .AddClass(GroupCountClass)
                .SetPickingMode(PickingMode.Ignore);

            var info = new VisualElement()
                .AddClass(GroupHeaderRowClass)
                .AddChild(header)
                .AddChild(count);
            info.pickingMode = PickingMode.Ignore;

            fixAll.AddLeadingContent(info);
            card.AddChild(fixAll);

            // Header divider + its underline sweep (the Welcome cards' idiom): the sweep rides the divider line,
            // amber for a broken group, the calm info tone on a migration card. Both hide while the picker is
            // docked — the dropdown is inserted right after the header and they would land under it.
            card.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                    .SetTheme(ThemeStyle.Type.Light)
                    .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                .AddClass(GroupDividerClass));

            var sweep = CreateHeaderSweep(fixAll, GroupSweepClass, GroupHeaderHoverClass);
            if (isMigration) sweep.AddClass(GroupSweepMigrateClass);
            card.AddChild(sweep);

            if (isMigration)
            {
                // Not a guess, so it replaces the Smart Fix row: same confirm + diff preview + Undo flow as a picked fix.
                card.AddChild(BuildGroupActionRow(
                    $"Migrate all ({group.Entries.Count}) → {migrationTarget.Name}",
                    $"Every entry resolves to {migrationTarget.FullName} via its declared [MovedFrom] — Unity already " +
                    "migrates them in memory when the asset loads. Migrating rewrites the stored type name in the " +
                    "files so they match the code; the attribute can be removed once no file stores the old name.",
                    info: true,
                    () => ApplyGroupFix(group, migrationTarget)));
            }
            else if (TryGetGroupSuggestion(group, constraint, out var suggestion))
            {
                // Reuse the shared label/detail builders so the Smart Fix copy never drifts from the inspector notice.
                card.AddChild(BuildGroupActionRow(
                    $"Smart Fix {SerializeReferenceHelpers.GetSuggestionLabel(suggestion)}",
                    SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                    info: false,
                    () => ApplyGroupFix(group, suggestion.Type)));
            }

            foreach (var entry in group.Entries)
                card.AddChild(BuildGroupEntryRow(entry));

            return card;
        }

        // The accent hairline that scales in under a flat header button while it is hovered — shared idiom with the
        // Welcome sample cards. The sweep is the button's sibling, so USS :hover can't reach it; the button mirrors
        // its hover onto a container modifier (resolved lazily via parent, at event time) the sweep rule listens to.
        private VisualElement CreateHeaderSweep(AspidGradientButton header, string sweepClass, string hoverClass)
        {
            var sweep = new VisualElement()
                .AddClass(sweepClass)
                .SetPickingMode(PickingMode.Ignore);

            header.RegisterCallback<MouseEnterEvent>(_ => header.parent?.AddToClassList(hoverClass));
            // Composes with the keyboard ring: while the header is the nav-focused element the sweep stays lit
            // after the mouse leaves (mirrors AspidGradientButton.Highlighted keeping the glow on).
            header.RegisterCallback<MouseLeaveEvent>(_ =>
            {
                if (IsNavFocused(header)) return;
                header.parent?.RemoveFromClassList(hoverClass);
            });

            return sweep;
        }

        private bool IsNavFocused(VisualElement element) =>
            _navIndex >= 0 && _navIndex < _navTargets.Count && _navTargets[_navIndex].Element == element;

        // A one-click bulk action (Smart Fix / Migrate all) as a member of the entry-row family: a left-aligned
        // accent verb over the same flat hover fill as the ping rows below it, instead of a filled gradient pill
        // floating over the glass card. Each card keeps one accent: warning amber for a Smart Fix guess on a
        // broken card, info for a pending migration.
        private VisualElement BuildGroupActionRow(string text, string tooltipText, bool info, Action onClick)
        {
            var row = new Label(text).AddClass(GroupActionClass);
            if (info) row.AddClass(GroupActionInfoClass);
            row.tooltip = tooltipText;
            row.RegisterCallback<ClickEvent>(_ => onClick());
            RegisterNavTarget(row, onClick);
            return row;
        }

        private static string BuildGroupCountText(ProjectGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText}";
        }

        // The header verb plus a trailing chevron; only the glyph changes when the picker opens (ClosePicker relies
        // on that). A broken group's picker fixes ("Fix all"); on a migration card nothing is broken and the picker
        // is the manual escape hatch beside the one-click Migrate all row, so its verb is "Reassign all".
        private static string BuildFixAllLabel(ProjectGroup group, bool expanded, bool isMigration) =>
            $"{(isMigration ? "Reassign all" : "Fix all")} ({group.Entries.Count})  {(expanded ? FixArrowExpanded : FixArrowCollapsed)}";

        // Read-only entry row: clicking jumps to the asset — the bulk Fix above is the only mutation in project mode.
        private VisualElement BuildGroupEntryRow(ProjectEntry entry)
        {
            var row = new VisualElement().AddClass(GroupEntryClass);

            var path = MakeSelectable(new Label(entry.AssetPath)
                .AddClass(GroupEntryPathClass));
            path.tooltip = entry.AssetPath;

            var rid = MakeSelectable(new Label($"rid {entry.Entry.Rid}")
                .AddClass(GroupEntryRidClass));

            row.AddChild(path).AddChild(rid);
            RegisterEntryRowClick(row, entry.AssetPath);

            return row;
        }

        // The path is the payload users most often need outside Unity, so entry-row text is selectable for copying.
        // A drag-select ends in the same ClickEvent as a plain click, so the row's jump only fires when the click
        // lands on text without leaving a selection behind.
        private static Label MakeSelectable(Label label)
        {
            label.selection.isSelectable = true;
            label.selection.doubleClickSelectsWord = true;
            label.selection.tripleClickSelectsLine = true;
            return label;
        }

        private void RegisterEntryRowClick(VisualElement row, string assetPath)
        {
            row.RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.target is TextElement text && text.selection.HasSelection()) return;
                JumpToAsset(assetPath);
            });

            RegisterNavTarget(row, () => JumpToAsset(assetPath));
            row.AddManipulator(new ContextualMenuManipulator(evt => PopulateEntryContextMenu(evt, assetPath)));
        }

        // Right-click alternatives to the row's default left-click jump. Runs after the selectable labels populate
        // their own items (bubble-up), so the menu is wiped first to drop their Copy entry — Cmd+C on a selection
        // still copies, and the menu stays the same three items wherever the click lands.
        private void PopulateEntryContextMenu(ContextualMenuPopulateEvent evt, string assetPath)
        {
            for (var i = evt.menu.MenuItems().Count - 1; i >= 0; i--)
                evt.menu.RemoveItemAt(i);

            evt.menu.AppendAction("Open in Asset References", _ => JumpToAsset(assetPath));

            evt.menu.AppendAction(
                "Open in Prefab Mode",
                _ => UnityEditor.SceneManagement.PrefabStageUtility.OpenPrefab(assetPath),
                assetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                    ? DropdownMenuAction.Status.Normal
                    : DropdownMenuAction.Status.Disabled);

            evt.menu.AppendAction("Select in Project", _ =>
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (asset is null) return;

                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            });
        }

        // The group's bulk picker, inline below the Fix all button, constrained to the group's intersected field type.
        private void ToggleGroupPicker(ProjectGroup group, Type constraint, AspidGradientButton button)
        {
            var wasOpen = _openPickerRow == button;
            ClosePicker();
            if (wasOpen) return;

            var view = BuildPickerView(constraint, assemblyQualifiedName =>
            {
                // <None> emits an empty name: clear the group to null instead of treating it as a no-op.
                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    ClearGroupToNull(group);
                    return;
                }

                var type = ResolveType(assemblyQualifiedName);
                if (type is not null) ApplyGroupFix(group, type);
            });

            OpenPickerBelow(button, view);
            button.Text = BuildFixAllLabel(group, expanded: true, IsMigrationGroup(group));
        }

        // Rewrites every entry in the group to newType after a mandatory confirmation. Rewrites are batched per file
        // so each affected asset is reimported exactly once.
        private void ApplyGroupFix(ProjectGroup group, Type newType)
        {
            if (newType is null) return;
            ClosePicker();

            var entries = FilterWritableEntries(group.Entries, out var skipped);

            if (entries.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Repair Missing References",
                    "All references in this group live in open scene(s) or Prefab Mode. Close them and rescan, " +
                    "or repair the fields directly in the Inspector.",
                    "OK");
                return;
            }

            var files = entries.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();
            var skippedNote = skipped > 0
                ? $"\n\n{skipped} reference(s) in open scene(s) or Prefab Mode will be skipped."
                : string.Empty;

            // When the group's picker fell back to an unconstrained list because its entries' declared field types
            // disagree, the single chosen type cannot fit every entry — warn that the mismatched ones null on reimport.
            group.ResolveConstraint(out var mixedFieldTypes);
            var mixedNote = mixedFieldTypes
                ? "\n\nField types in this group differ — the chosen type may not fit every entry; incompatible ones " +
                  "will become null on reimport."
                : string.Empty;

            var managedType = ManagedTypeName.FromType(newType);

            // The preview is computed by the same scan the rewrite applies, so it shows exactly what gets written.
            var diff = BuildDiffPreview(entries, managedType);

            if (!EditorUtility.DisplayDialog(
                    "Repair Missing References",
                    $"Rewrite {entries.Count} reference(s) in {files} file(s) to '{newType.FullName}'?\n\n" +
                    diff +
                    "This edits the asset files directly; an Undo button on the summary can revert it." + skippedNote + mixedNote,
                    "Rewrite",
                    "Cancel"))
                return;

            var rewritten = BatchRewriteEntries(entries, managedType, "Repairing References");

            SerializeReferenceRepairSuggestions.ClearCache();

            var summaryTitle = rewritten == 1 ? "Rewrote 1 reference" : $"Rewrote {rewritten} references";
            var summaryBody = $"Replaced missing '{group.DisplayName}' with '{newType.FullName}'.";
            if (skipped > 0)
                summaryBody += $" Skipped {skipped} in open scene(s) or Prefab Mode.";

            // Undo re-points the entries back to the original (now-missing) stored type. Only the type line moved —
            // the data blocks were never touched on disk — so flipping it back is a faithful revert.
            var originalType = group.StoredType;
            var missingName = group.DisplayName;
            var appliedName = newType.FullName;
            void Undo(VisualElement receipt) => UndoGroupFix(entries, originalType, managedType, missingName, appliedName, receipt);

            if (_scanButton is not null) _scanButton.Text = RescanLabel;
            var groups = CollectProjectGroups(out var canceled);

            if (groups.Count == 0)
            {
                // The fix cleared the last broken type. Stay in the results region instead of the "Project clean"
                // hero, which would hide the summary HelpBox receipt; the hero is reserved for an explicit Rescan.
                ShowMissingReferencesClean();
            }
            else
            {
                RenderGroups(groups, RequiredViolationsForRender, canceled);
            }

            ShowSummary(summaryTitle, summaryBody, Undo);
        }

        // Clears every entry in the group to null. Closed assets are nulled in the YAML directly; assets open in
        // Prefab Mode / a loaded scene cannot be rewritten on disk (the open copy would clobber it on save), so those
        // are nulled on the live object and stay in the audit until saved. NOT undoable: the broken payload is discarded.
        private void ClearGroupToNull(ProjectGroup group)
        {
            ClosePicker();

            SplitWritableEntries(group.Entries, out var onDisk, out var inMemory);
            if (onDisk.Count == 0 && inMemory.Count == 0) return;

            var fileCount = onDisk.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();
            var total = onDisk.Count + inMemory.Count;

            var openNote = inMemory.Count > 0
                ? $"\n\n{inMemory.Count} reference(s) are open in Prefab Mode or a scene — those are nulled on the live " +
                  "object and saved with the asset (the audit keeps listing them until you save)."
                : string.Empty;
            var diskNote = onDisk.Count > 0
                ? $" {onDisk.Count} on disk in {fileCount} file(s) are edited directly."
                : string.Empty;

            if (!EditorUtility.DisplayDialog(
                    "Clear Missing References",
                    $"Clear {total} reference(s) to null?\n\n" +
                    BuildClearPreview(group.Entries) +
                    $"This nulls every field holding the broken '{group.DisplayName}' and discards its payload." +
                    diskNote + " It cannot be undone." + openNote,
                    "Clear",
                    "Cancel"))
                return;

            var clearedOnDisk = BatchNullEntries(onDisk, "Clearing References");
            var clearedInMemory = ClearOpenEntriesInMemory(inMemory, group.StoredType);
            var cleared = clearedOnDisk + clearedInMemory;

            SerializeReferenceRepairSuggestions.ClearCache();

            // Nothing actually changed (every edit failed) — skip the receipt rather than claim a cleared count of 0.
            if (cleared == 0)
            {
                if (_scanButton is not null) _scanButton.Text = RescanLabel;
                RenderGroups(CollectProjectGroups(out var rescanCanceled), RequiredViolationsForRender, rescanCanceled);
                return;
            }

            var summaryTitle = cleared == 1 ? "Cleared 1 reference" : $"Cleared {cleared} references";
            var summaryBody = $"Set missing '{group.DisplayName}' to null.";
            if (clearedInMemory > 0)
                summaryBody += clearedInMemory == 1
                    ? " 1 was nulled in memory — save the asset to persist it (still listed until saved)."
                    : $" {clearedInMemory} were nulled in memory — save the assets to persist them (still listed until saved).";

            // Unlike Fix all (which only swaps a stored type name, never nulls anything), Clear to null CAN turn a
            // required field that held a broken-but-non-null reference into a genuine unset-required violation — drop
            // the stale cache so the Required violations card doesn't under-report until the user rescans.
            _requiredIsWarm = false;

            if (_scanButton is not null) _scanButton.Text = RescanLabel;
            var groups = CollectProjectGroups(out var canceled);

            if (groups.Count == 0)
            {
                // Same came-back-clean handling as ApplyGroupFix: keep the receipt visible instead of the hero.
                ShowMissingReferencesClean();
            }
            else
            {
                RenderGroups(groups, RequiredViolationsForRender, canceled);
            }

            // No Undo: clearing discards the broken payload (see ClearGroupToNull). The receipt is a plain record.
            ShowSummary(summaryTitle, summaryBody, onUndo: null);
        }

        // Splits entries into those safe to rewrite on disk and those open in Prefab Mode / a loaded scene, which
        // must be repaired in memory instead.
        private static void SplitWritableEntries(IReadOnlyList<ProjectEntry> source, out List<ProjectEntry> onDisk, out List<ProjectEntry> inMemory)
        {
            var prefabStagePath = CurrentPrefabStagePath();
            onDisk = new List<ProjectEntry>(source.Count);
            inMemory = new List<ProjectEntry>();

            foreach (var entry in source)
            {
                if (IsEntryWritable(entry, prefabStagePath)) onDisk.Add(entry);
                else inMemory.Add(entry);
            }
        }

        // Nulls each open entry on its live object (the file rewrite is skipped for open assets). The file is unchanged,
        // so these stay in the audit until the asset is saved. Returns how many were cleared.
        private static int ClearOpenEntriesInMemory(IReadOnlyList<ProjectEntry> entries, ManagedTypeName storedType)
        {
            var cleared = 0;
            foreach (var entry in entries)
            {
                if (SerializeReferenceHelpers.TryClearMissingReferenceInMemory(entry.AssetPath, entry.Entry.Rid, storedType))
                    cleared++;
            }

            return cleared;
        }

        // Nulls every entry to the null managed-reference id and drops its payload, batched per file behind a cancel-free
        // progress bar (StartAssetEditing defers each reimport to one pass at the end). Returns how many were cleared.
        private static int BatchNullEntries(IReadOnlyList<ProjectEntry> entries, string progressTitle)
        {
            var byFile = entries
                .GroupBy(entry => entry.AssetPath, StringComparer.Ordinal)
                .ToArray();

            var cleared = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < byFile.Length; i++)
                {
                    var file = byFile[i];
                    EditorUtility.DisplayProgressBar(
                        progressTitle,
                        $"{file.Key}  ({i + 1}/{byFile.Length})",
                        (float)i / byFile.Length);

                    var changed = false;
                    foreach (var entry in file)
                    {
                        if (!SerializeReferenceYamlEditor.TryNullReference(file.Key, entry.Entry.FileId, entry.Entry.Rid))
                            continue;

                        cleared++;
                        changed = true;
                    }

                    if (changed) AssetDatabase.ImportAsset(file.Key, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            return cleared;
        }

        // Capped file + rid list for the confirmation. No before/after lines — the whole entry is being dropped.
        private static string BuildClearPreview(List<ProjectEntry> entries)
        {
            const int maxShown = 8;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Clears:");

            var shown = 0;
            foreach (var entry in entries)
            {
                if (shown >= maxShown)
                {
                    builder.AppendLine($"  …and {entries.Count - shown} more");
                    break;
                }

                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid})");
                shown++;
            }

            builder.AppendLine();
            return builder.ToString();
        }

        // A scene or prefab loaded in the editor would race a file rewrite — the in-memory copy wins on the next save
        // and silently clobbers the on-disk edit. Returns the entries safe to write; reports how many were held back.
        private static List<ProjectEntry> FilterWritableEntries(IReadOnlyList<ProjectEntry> source, out int skipped)
        {
            var prefabStagePath = CurrentPrefabStagePath();
            var writable = new List<ProjectEntry>(source.Count);
            skipped = 0;

            foreach (var entry in source)
            {
                if (IsEntryWritable(entry, prefabStagePath)) writable.Add(entry);
                else skipped++;
            }

            return writable;
        }

        private static string CurrentPrefabStagePath() =>
            UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;

        // See FilterWritableEntries: an open asset's in-memory copy would clobber the file edit on the next save.
        private static bool IsEntryWritable(ProjectEntry entry, string prefabStagePath)
        {
            var openInScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(entry.AssetPath).isLoaded;
            var openInPrefabMode = !string.IsNullOrEmpty(prefabStagePath) &&
                                   string.Equals(prefabStagePath, entry.AssetPath, StringComparison.Ordinal);

            return !openInScene && !openInPrefabMode;
        }

        // Rewrites every entry's stored type to targetType, batched per file. StartAssetEditing defers each
        // ImportAsset to one pass at the end. Shared by the forward fix and Undo.
        private static int BatchRewriteEntries(IReadOnlyList<ProjectEntry> entries, ManagedTypeName targetType, string progressTitle)
        {
            var byFile = entries
                .GroupBy(entry => entry.AssetPath, StringComparer.Ordinal)
                .ToArray();

            var rewritten = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < byFile.Length; i++)
                {
                    var file = byFile[i];
                    EditorUtility.DisplayProgressBar(
                        progressTitle,
                        $"{file.Key}  ({i + 1}/{byFile.Length})",
                        (float)i / byFile.Length);

                    var changed = false;
                    foreach (var entry in file)
                    {
                        if (!SerializeReferenceYamlEditor.TryRewriteType(file.Key, entry.Entry.FileId, entry.Entry.Rid, targetType))
                            continue;

                        rewritten++;
                        changed = true;
                    }

                    if (changed) AssetDatabase.ImportAsset(file.Key, ImportAssetOptions.ForceUpdate);
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
            }

            return rewritten;
        }

        // Reverts one bulk fix by re-pointing its entries back to the original (now-missing) stored type. Only this
        // fix's own receipt is dropped — receipts for other still-applied fixes survive, unlike a full Rescan.
        private void UndoGroupFix(IReadOnlyList<ProjectEntry> entries, ManagedTypeName originalType, ManagedTypeName appliedType, string missingName, string appliedName, VisualElement receipt)
        {
            // The asset may have opened in a scene / Prefab Mode since the fix; apply the same guard as the forward fix.
            var writable = FilterWritableEntries(entries, out var skipped);

            // Only entries that STILL hold the type this receipt applied may be re-pointed — the group can have been
            // re-broken and fixed to a DIFFERENT type since, and blindly rewriting would destroy that newer fix.
            // "Still holds it" == a rewrite towards the applied type whose old line already equals its new line.
            var revertible = new List<ProjectEntry>(writable.Count);
            var diverged = 0;
            foreach (var entry in writable)
            {
                if (SerializeReferenceYamlEditor.TryComputeRewrite(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid, appliedType, out var edit) &&
                    edit.IsValid && string.Equals(edit.OldLine, edit.NewLine, StringComparison.Ordinal))
                    revertible.Add(entry);
                else
                    diverged++;
            }

            if (revertible.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Undo Repair",
                    diverged > 0
                        ? "These references no longer hold the type this fix applied (they were re-pointed or removed " +
                          "since), so there is nothing this undo can safely revert."
                        : "These references now live in open scene(s) or Prefab Mode. Close them and try the undo again.",
                    "OK");
                return;
            }

            var files = revertible.Select(entry => entry.AssetPath).Distinct(StringComparer.Ordinal).Count();
            var skippedNote = skipped > 0
                ? $"\n\n{skipped} reference(s) in open scene(s) or Prefab Mode will be skipped."
                : string.Empty;
            var divergedNote = diverged > 0
                ? $"\n\n{diverged} reference(s) no longer hold '{appliedName}' (changed since this fix) and will be left alone."
                : string.Empty;

            if (!EditorUtility.DisplayDialog(
                    "Undo Repair",
                    $"Re-point {revertible.Count} reference(s) in {files} file(s) back to the missing '{missingName}'?\n\n" +
                    $"This restores the broken state you had before replacing it with '{appliedName}', and edits the " +
                    "asset files directly." + skippedNote + divergedNote,
                    "Undo",
                    "Cancel"))
                return;

            var reverted = BatchRewriteEntries(revertible, originalType, "Undoing Repair");

            SerializeReferenceRepairSuggestions.ClearCache();

            // Drop only this receipt — the others describe fixes still applied. RenderGroups rebuilds only _list,
            // never _summaries, so the surviving receipts stay put.
            receipt?.RemoveFromHierarchy();
            var groups = CollectProjectGroups(out var canceled);
            RenderGroups(groups, RequiredViolationsForRender, canceled);

            // BatchRewriteEntries can come up short if a file changed between the check and the write — report the real count.
            var undoTitle = reverted == 1 ? "Reverted 1 reference" : $"Reverted {reverted} references";
            var undoBody = $"Re-pointed back to the missing '{missingName}'.";
            if (diverged > 0) undoBody += $" Left {diverged} alone (no longer '{appliedName}').";
            if (reverted < revertible.Count) undoBody += $" {revertible.Count - reverted} could not be rewritten.";
            ShowSummary(undoTitle, undoBody, null);
        }

        // Old -> new preview of the YAML the bulk fix will rewrite, using the same TryComputeRewrite the rewrite
        // applies, so the preview is exactly what gets written. Capped so the confirmation stays readable.
        private static string BuildDiffPreview(List<ProjectEntry> entries, ManagedTypeName newType)
        {
            const int maxShown = 8;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Changes:");

            // Compute first, render second: an uncomputable entry must neither vanish silently nor inflate the
            // "…and N more" remainder.
            var edits = new List<(ProjectEntry entry, RewriteEdit edit)>(entries.Count);
            foreach (var entry in entries)
            {
                if (SerializeReferenceYamlEditor.TryComputeRewrite(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid, newType, out var edit))
                    edits.Add((entry, edit));
            }

            for (var i = 0; i < edits.Count && i < maxShown; i++)
            {
                var (entry, edit) = edits[i];
                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid}):");
                builder.AppendLine($"    - {edit.OldLine.Trim()}");
                builder.AppendLine($"    + {edit.NewLine.Trim()}");
            }

            if (edits.Count > maxShown)
                builder.AppendLine($"  …and {edits.Count - maxShown} more");

            var uncomputable = entries.Count - edits.Count;
            if (uncomputable > 0)
                builder.AppendLine($"  ({uncomputable} entr{(uncomputable == 1 ? "y" : "ies")} could not be previewed)");

            builder.AppendLine();
            return builder.ToString();
        }

        // Smart Fix: rank the stored type against the constraint-filtered pool, surfaced only above the confidence
        // threshold. Quick-apply bypasses the picker — safe only because Rank enforces the constraint internally,
        // so the suggestion is always assignable.
        private static bool TryGetGroupSuggestion(ProjectGroup group, Type constraint, out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            var first = group.Entries[0];
            var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(first.AssetPath, first.Entry.FileId, first.Entry.Rid);

            var ranked = SerializeReferenceRepairSuggestions.Rank(group.StoredType, fieldNames, constraint);
            if (ranked.Count == 0) return false;

            suggestion = ranked[0];
            return true;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Required violations
        // ---------------------------------------------------------------------------------------------------------

        // Flat read-only list of every unset [TypeSelector(Required = true)] field, fed by the same headless scanner
        // as the build/CI gate. No bulk fix here — unlike a broken type, an empty required field has nothing sensible
        // to auto-assign, so the row's only affordance is jumping to the offending asset (where the graph's inline
        // Assign Required picker lives).
        private VisualElement BuildRequiredGroupCard(IReadOnlyList<GateViolation> violations)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var header = new AspidLabel("Required violations", AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(GroupHeaderClass)
                .SetPickingMode(PickingMode.Ignore);

            var files = violations.Select(violation => violation.AssetPath).Distinct(StringComparer.Ordinal).Count();
            var count = new Label($"{BuildCountText(violations.Count, "entry")} · {(files == 1 ? "1 file" : $"{files} files")}")
                .AddClass(GroupCountClass)
                .SetPickingMode(PickingMode.Ignore);

            var info = new VisualElement()
                .AddClass(GroupHeaderRowClass)
                .AddClass(GroupHeaderRowStaticClass)
                .AddChild(header)
                .AddChild(count);
            info.pickingMode = PickingMode.Ignore;

            card.AddChild(info);

            // Same header divider as the Fix-all cards, keeping every card's header/body split on one line — but no
            // sweep: this header row is static, there is nothing to hover.
            card.AddChild(new AspidDividingLine(AspidDividingLinePreset.Default
                    .SetTheme(ThemeStyle.Type.Light)
                    .SetSize(AspidDividingLineSizeStyle.Type.Thin))
                .AddClass(GroupDividerClass));

            // Per-card memo, keyed by asset path: several violations commonly share one asset (e.g. a prefab with
            // multiple unset required fields), so this keeps LoadAllAssetsAtPath to once per distinct file instead of
            // once per row.
            var componentCache = new Dictionary<string, Object[]>(StringComparer.Ordinal);
            foreach (var violation in violations)
                card.AddChild(BuildRequiredViolationRow(violation, componentCache));

            return card;
        }

        // Read-only entry row: asset path on the left, "Component.field" on the right; the whole row jumps to the
        // asset — same cross-link as a broken-reference row (BuildGroupEntryRow).
        private VisualElement BuildRequiredViolationRow(GateViolation violation, Dictionary<string, Object[]> componentCache)
        {
            var row = new VisualElement().AddClass(GroupEntryClass);

            var path = MakeSelectable(new Label(violation.AssetPath)
                .AddClass(GroupEntryPathClass));
            path.tooltip = violation.AssetPath;

            var field = MakeSelectable(new Label(BuildRequiredViolationFieldText(violation, componentCache))
                .AddClass(GroupEntryFieldClass));

            row.AddChild(path).AddChild(field);
            RegisterEntryRowClick(row, violation.AssetPath);

            return row;
        }

        // Cross-link shared by every read-only audit row: jump to the asset's full Inspect graph; ping as a
        // fallback when hosted standalone.
        private void JumpToAsset(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (asset is null) return;

            if (OnInspectAsset is not null) OnInspectAsset(asset);
            else EditorGUIUtility.PingObject(asset);
        }

        // "Component.field" for the entry row's right column; GateViolation itself carries no owning-object type,
        // only the asset path and file id, so the component name is resolved on demand for display.
        private static string BuildRequiredViolationFieldText(GateViolation violation, Dictionary<string, Object[]> componentCache)
        {
            var component = ResolveComponentName(violation, componentCache);
            return string.IsNullOrEmpty(component) ? violation.FieldPath : $"{component}.{violation.FieldPath}";
        }

        // Best-effort owning-object type name. Saved assets are object-loaded (once per distinct path, memoised in
        // componentCache) and matched by file id — the same lookup
        // SerializeReferenceGateScanner.CollectRequiredViolations uses internally to build each violation, just for
        // display here. Scenes cannot be object-loaded (see SerializeReferenceHelpers.IsScene), so a scene row shows
        // the field path alone rather than guessing a component name.
        private static string ResolveComponentName(GateViolation violation, Dictionary<string, Object[]> componentCache)
        {
            if (SerializeReferenceHelpers.IsScene(violation.AssetPath)) return string.Empty;

            if (!componentCache.TryGetValue(violation.AssetPath, out var assets))
            {
                assets = AssetDatabase.LoadAllAssetsAtPath(violation.AssetPath);
                componentCache[violation.AssetPath] = assets;
            }

            foreach (var asset in assets)
            {
                if (asset == null) continue;
                if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out long fileId) && fileId == violation.FileId)
                    return asset.GetType().Name;
            }

            return string.Empty;
        }

        // ---------------------------------------------------------------------------------------------------------
        // Shared picker / results plumbing
        // ---------------------------------------------------------------------------------------------------------

        private TypeSelectorView BuildPickerView(Type constraint, Action<string> onSelected)
        {
            var baseType = constraint ?? typeof(object);

            return new TypeSelectorView(
                filter: new TypeSelectorFilter
                {
                    Types = new[] { baseType },
                    Predicate = SerializeReferenceHelpers.IsAssignableManagedReference,
                    AdditionalTypes = baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType),
                    ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                },
                currentAqn: null, // the bulk group picker has no current value — nothing (not even <None>) wears the check
                onSelected: onSelected,
                onDismiss: ClosePicker);
        }

        private void OpenPickerBelow(AspidGradientButton anchor, TypeSelectorView view)
        {
            _openPicker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(PickerClass)
                .AddChild(view);

            _openPickerRow = anchor;

            // The header button is a direct child of the group card, so the picker drops right below it inside the
            // card; the ?? fallback keeps a sane target if the button is ever hosted outside a card.
            var card = anchor.parent;
            var container = card ?? _list;
            container.InsertChild(container.IndexOf(anchor) + 1, _openPicker);

            // __group--picking + __picker--attached weld the header, selector and entry rows into one active card.
            if (card is not null)
            {
                _openPickerCard = card;
                _openPickerCard.AddClass(GroupPickingClass);
                _openPicker.AddClass(PickerAttachedClass);
            }

            view.FocusPicker();
        }

        private void ClosePicker()
        {
            _openPicker?.RemoveFromHierarchy();
            // No group reference here, but only the chevron glyph differs between labels — swap it in place.
            if (_openPickerRow is not null)
                _openPickerRow.Text = _openPickerRow.Text.Replace(FixArrowExpanded, FixArrowCollapsed);
            _openPickerCard?.RemoveClass(GroupPickingClass);

            _openPicker = null;
            _openPickerRow = null;
            _openPickerCard = null;

            // The dismissed picker leaves keyboard focus dangling on its (removed) search field; reclaim it so the
            // arrow-key ring keeps working. Guarded — ClosePicker also runs from render paths before attach.
            if (panel is not null) Focus();
        }

        private static Type ResolveType(string assemblyQualifiedName) =>
            string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

        private void ShowEmptyState(bool success, string title, string message)
        {
            ResetNavTargets();
            _results.AddClass(ResultsHiddenClass);
            _empty.RemoveClass(EmptyHiddenClass);
            _empty.Clear();
            OnCanvasTone?.Invoke(success ? SerializeReferenceCanvasStyle.Success : SerializeReferenceCanvasStyle.Info);

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

        // Cold-index idle state until the first scan. No results list yet — the project is unscanned, so "clean"
        // cannot be claimed.
        private void ShowIdle() => ShowEmptyState(
            success: false,
            title: "Project not scanned",
            message: "Run Scan Project to map every broken [SerializeReference] type across your assets — then repair each missing type in bulk.");

        // The tone is explicit per call site: the missing-references sweep tones Warning, while the came-back-clean
        // receipt tones Success rather than leaving a clean state on an amber backdrop.
        private void ShowResults(string headerText, Color tone)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);
            _resultsHeader.Text = headerText;
            OnCanvasTone?.Invoke(tone);
        }

        // Appends one receipt to the running stack (newest at the bottom) rather than overwriting the previous; only
        // ClearSummaries resets it on the next fresh scan. The Undo button reverts exactly this fix.
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

        // ---------------------------------------------------------------------------------------------------------
        // Project scan data
        // ---------------------------------------------------------------------------------------------------------

        private readonly struct ProjectEntry
        {
            public readonly string AssetPath;
            public readonly MissingReferenceEntry Entry;

            public ProjectEntry(string assetPath, MissingReferenceEntry entry)
            {
                AssetPath = assetPath;
                Entry = entry;
            }
        }

        // All broken references sharing one stored type across the project. Resolves a single picker constraint by
        // intersecting the entries' declared field types, falling back to typeof(object) when they disagree.
        private sealed class ProjectGroup
        {
            public readonly ManagedTypeName StoredType;
            public readonly List<ProjectEntry> Entries = new();

            private readonly HashSet<string> _files = new(StringComparer.Ordinal);
            private readonly Dictionary<string, Dictionary<(long fileId, long rid), Type>> _constraintCache = new(StringComparer.Ordinal);

            public ProjectGroup(ManagedTypeName storedType) => StoredType = storedType;

            public int FileCount => _files.Count;

            public string DisplayName => StoredType.DisplayName;

            public void Add(string assetPath, MissingReferenceEntry entry)
            {
                Entries.Add(new ProjectEntry(assetPath, entry));
                _files.Add(assetPath);
            }

            // Per-file constraint maps are built once and cached, so the intersection costs one scan per distinct asset.
            public Type ResolveConstraint() => ResolveConstraint(out _);

            // Reports whether the typeof(object) fallback came from the field types disagreeing (vs. one being
            // unrecoverable) — the bulk-fix confirmation warns on that case.
            public Type ResolveConstraint(out bool mixedFieldTypes)
            {
                mixedFieldTypes = false;
                Type common = null;

                foreach (var entry in Entries)
                {
                    if (!_constraintCache.TryGetValue(entry.AssetPath, out var map))
                    {
                        map = SerializeReferenceHelpers.BuildConstraintMap(entry.AssetPath);
                        _constraintCache[entry.AssetPath] = map;
                    }

                    // A field type we cannot recover (a reference nested in a missing parent, or an orphaned rid no
                    // field points at) leaves the group unconstrained — a tighter guess could hide a valid pick.
                    if (!map.TryGetValue((entry.Entry.FileId, entry.Entry.Rid), out var fieldType) || fieldType is null)
                        return typeof(object);

                    if (common is null)
                    {
                        common = fieldType;
                    }
                    else if (common != fieldType)
                    {
                        mixedFieldTypes = true;
                        return typeof(object);
                    }
                }

                return common ?? typeof(object);
            }
        }
    }
}
