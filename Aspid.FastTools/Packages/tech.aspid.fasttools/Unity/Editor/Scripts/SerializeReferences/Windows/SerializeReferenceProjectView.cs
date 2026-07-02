using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Types;
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
        private const string SummaryListClass = RootClass + "__summary-list";
        private const string SummaryClass = RootClass + "__summary";
        private const string SummaryUndoClass = RootClass + "__summary-undo";
        private const string ScrollClass = RootClass + "__scroll";
        private const string EntryClass = RootClass + "__entry";
        private const string EntryRidClass = RootClass + "__entry-rid";
        private const string PickerClass = RootClass + "__picker";
        private const string PickerAttachedClass = PickerClass + "--attached";

        private const string GroupClass = RootClass + "__group";
        private const string GroupPickingClass = GroupClass + "--picking";
        private const string GroupHeaderRowClass = RootClass + "__group-header-row";
        private const string GroupHeaderClass = RootClass + "__group-header";
        private const string GroupCountClass = RootClass + "__group-count";
        private const string GroupFixAllClass = RootClass + "__group-fix-all";
        private const string GroupFixAllMigrateClass = GroupFixAllClass + "--migrate";
        private const string GroupSuggestClass = RootClass + "__group-suggest";
        private const string GroupMigrateClass = RootClass + "__group-migrate";
        private const string GroupEntryClass = RootClass + "__group-entry";
        private const string GroupEntryPathClass = RootClass + "__group-entry-path";
        private const string GroupEntryRidClass = RootClass + "__group-entry-rid";

        // The bulk-fix button is a single dropdown button: one "Fix all (N)" label with a trailing chevron that flips
        // ▼→▲ while the type picker is open (see BuildFixAllLabel). Only the glyph differs between the two states.
        private const string FixArrowCollapsed = "▼";
        private const string FixArrowExpanded = "▲";

        // The scan button's label adapts to the index state: a deliberate "Scan Project" call-to-action while the
        // index is cold (the first build is expensive), a quiet "Rescan" refresh once a scan has warmed it.
        private const string ScanLabel = "Scan Project";
        private const string RescanLabel = "Rescan";

        private VisualElement _empty;
        private VisualElement _results;
        private AspidLabel _resultsHeader;
        private VisualElement _summaries;
        private Label _resultsHint;
        private VisualElement _list;
        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;
        private VisualElement _openPickerCard;
        private AspidGradientButton _scanButton;

        /// <summary>Jump from a project-audit result row to that asset's Inspect graph. Wired by the host window.</summary>
        public Action<Object> OnInspectAsset;

        /// <summary>Reports this view's state-tone to the host window, which owns the shared dotted canvas. Wired by the window.</summary>
        public Action<Color> OnCanvasTone;

        public SerializeReferenceProjectView()
        {
            var root = this;
            style.flexGrow = 1;
            root.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            // A full-width translucent header panel gives the Scan Project action a purposeful home, stacked: a title
            // and a one-line description of what the audit does, then a full-width Scan Project button below. The
            // dotted canvas (owned by the host window) reads through the panel's semi-transparent fill. The title is
            // phrased around the action rather than repeating the tab's "Project References" name.
            var panelTitle = new AspidLabel("Find missing references", AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(PanelTitleClass);

            var panelDescription = new Label(
                    "Sweep every asset under Assets/ for broken [SerializeReference] types and bulk-fix them by type.")
                .AddClass(PanelDescriptionClass);

            // Stored as a field so its label can flip between the cold CTA and the warm refresh (see ScanLabel /
            // RescanLabel). It opens with the cold label; Initialize / ScanProject reconcile it with the index state.
            _scanButton = new AspidGradientButton(ScanLabel, _ => ScanProject())
                .AddClass(ScanProjectClass);

            var panel = new VisualElement()
                .AddClass(PanelClass)
                .AddChild(panelTitle)
                .AddChild(panelDescription)
                .AddChild(_scanButton);

            // The two terminal states (no asset / nothing to repair) share one hero centred in the space below the
            // card; scan results swap it for a warning-accented header, a short hint and the row list.
            _empty = new VisualElement().AddClass(EmptyClass);

            _resultsHeader = new AspidLabel(string.Empty, AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H4)
                    .SetLineTheme(ThemeStyle.Type.Dark)
                    .SetLineStatus(StatusStyle.Type.Warning))
                .AddClass(ResultsHeaderClass);

            _resultsHint = new Label(string.Empty).AddClass(ResultsHintClass);

            // A running stack of help-boxes — one receipt per bulk Fix all, in the order they ran — so chaining fixes
            // across groups keeps every prior summary on screen instead of overwriting it; the stack is cleared only on
            // a fresh scan (Rescan / Initialize). Empty until the first Fix all, so the container has no footprint then.
            // Each box is amber-toned (Warning) so it sits in the Project References view's warning family — the amber
            // results header, group cards and canvas tone — rather than reading as a foreign green block.
            _summaries = new VisualElement().AddClass(SummaryListClass);

            _list = new VisualElement();

            _results = new VisualElement()
                .AddClass(ResultsClass)
                .AddChild(_resultsHeader)
                .AddChild(_resultsHint)
                .AddChild(_summaries)
                .AddChild(_list);

            // One scroll spans the whole view between the window's tabs and footer: the Find-missing panel, the empty
            // hero, the results header/hint/summary and the group list all live inside it, so the panel scrolls away
            // with the group list rather than staying pinned above a separately-scrolling list.
            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(panel)
                .AddChild(_empty)
                .AddChild(_results);

            var scroll = new ScrollView().AddClass(ScrollClass);
            scroll.AddChild(content);

            root.AddChild(scroll);
        }

        // The tab-switch entry point. On a cold index the tab opens idle and waits for a deliberate Scan Project
        // click: the cold sweep parses every candidate asset's YAML behind a blocking bar (slow on large projects),
        // so it must never run unasked. Once the index is warm, though — the user already scanned this session, so
        // the data is already in memory — re-deriving the groups is a cheap in-memory filter with no YAML sweep, so
        // we re-render them here. That is what makes the results survive a plain tab switch instead of resetting to
        // idle every time the user leaves and comes back; the warm index is the source of truth and Rescan refreshes
        // it. The breakage-notification deep-link still bypasses this and forces a scan (the host window calls
        // ScanProject directly), because the user opened it specifically to see the broken references.
        public void Initialize()
        {
            if (SerializeReferenceTypeUsageIndex.IsWarm) RenderWarmGroups();
            else ShowIdle();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Project mode
        // ---------------------------------------------------------------------------------------------------------

        // Sweeps every candidate text asset under Assets/, finds the missing references in each, and groups them by
        // their stored broken type. The sweep is the slow part (it parses every asset's YAML), so it runs behind a
        // cancelable progress bar; cancelling shows whatever was collected so far.
        public void ScanProject()
        {
            if (_list is null) return;

            // Rescan starts from a clean slate: drop any open picker and the running summary stack the previous scan
            // left behind, then re-collect. RenderWarmGroups does the collect/render and the refresh-label flip — the
            // same path Initialize takes when it restores results on a warm index.
            ClosePicker();
            ClearSummaries();
            RenderWarmGroups();
        }

        // Collects the current unresolved set from the (now warm) index and paints it. Shared by the Scan/Rescan
        // button and Initialize's warm-restore path. A scan always warms the index, so the button is a refresh
        // affordance from here on — whether this is the first cold scan the user triggered or a warm rescan.
        private void RenderWarmGroups()
        {
            if (_list is null) return;
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            var groups = CollectProjectGroups(out var canceled);
            RenderGroups(groups, canceled);
        }

        // Paints a freshly-collected group set into the results region: the count header + hint and one card per
        // group, or the terminal "Project clean" / "Scan canceled" hero when there is nothing to list. Shared by the
        // Rescan entry point and the post-fix re-sweep — except ApplyGroupFix special-cases the came-back-clean case
        // so the rewrite's summary HelpBox survives instead of being replaced by the hero (see there).
        private void RenderGroups(List<ProjectGroup> groups, bool canceled)
        {
            _list.Clear();

            if (groups.Count == 0)
            {
                ShowEmptyState(
                    success: !canceled,
                    title: canceled ? "Scan canceled" : "Project clean",
                    message: canceled
                        ? "The project scan was canceled before finding any missing references."
                        : "No missing managed references found anywhere under Assets/.");
                return;
            }

            var entryCount = groups.Sum(group => group.Entries.Count);
            ShowResults(entryCount == 1 ? "1 missing reference" : $"{entryCount} missing references",
                SerializeReferenceCanvasStyle.Warning);
            _resultsHint.text = canceled
                ? "Scan canceled — showing partial results. Each group is a broken type; Fix all re-points every entry (or pick <None> to clear them to null) across every file at once."
                : "Each group is a broken stored type. Fix all picks one replacement and re-points every entry across every affected file at once — or pick <None> to clear them to null.";

            foreach (var group in groups)
                _list.AddChild(BuildGroupCard(group));
        }

        // Groups every unresolved managed reference in the project by stored type. Backed by the shared usage index, so
        // a warm index makes repeat scans an in-memory filter (the one-time cold warm shows its own progress bar). The
        // index is built from the same SerializeReferenceGraphScanner / StoredTypeResolves path that
        // FindMissingReferences uses, so the unresolved set matches the old per-asset sweep. The out parameter is kept
        // for the call site but is always false: the warm-up runs to completion (see the index) rather than being
        // cancelable mid-scan.
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

        // A broken-type group card: the whole header is one flat clickable row — the broken type name + entry/file
        // counts on the left, the bulk "Fix all (N) ▼" action on the right — so clicking anywhere on the header
        // toggles the type picker. An optional Smart Fix quick-apply sits below it, then one ping-only row per
        // entry. Entries are intentionally not individually fixable here — the per-row Fix affordance is reserved for
        // single-asset mode, where the whole row is the button; adding a second per-entry action under a bulk-fix
        // group would fight that layout, so project mode is group-level only (a precise per-asset fix is one
        // ObjectField pick away).
        private VisualElement BuildGroupCard(ProjectGroup group)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var constraint = group.ResolveConstraint();
            var displayName = group.DisplayName;

            // An authoritative [MovedFrom] rename: these entries are not really broken — Unity migrates them in
            // memory at load; only the files still store the old name. The card keeps the same layout but reads as a
            // calm migration (info header, "Migrate all" instead of the heuristic Smart Fix suggestion below).
            // The target must also fit the group's field constraint: Migrate all hands the type straight to
            // ApplyGroupFix, bypassing the picker's assignability guarantee — a rename that also changed the type's
            // bases would rewrite files into references Unity nulls at load, hiding a listed breakage. Such a group
            // falls through to the ordinary warning card, where Rank's constraint-filtered pool refuses the target.
            var isMigration = SerializeReferenceMovedFromResolver.TryResolve(group.StoredType, out var migrationTarget) &&
                (constraint == typeof(object) || constraint.IsAssignableFrom(migrationTarget));

            // The header button. Built first so the type name + count can be docked into its body to the left of the
            // action label; the click handler toggles the inline picker (the captured local is assigned before use).
            AspidGradientButton fixAll = null;
            fixAll = new AspidGradientButton(BuildFixAllLabel(group, expanded: false),
                    _ => ToggleGroupPicker(group, constraint, fixAll))
                .AddClass(GroupFixAllClass);
            // A migration card keeps its calm info tone end to end — the amber Fix all accent is the "broken" alarm.
            if (isMigration) fixAll.AddClass(GroupFixAllMigrateClass);
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

            if (isMigration)
            {
                // Migration is not a guess, so it replaces the heuristic Smart Fix row: one click bakes the rename
                // into every entry through the same confirm + diff preview + Undo flow as a picked fix, after which
                // the [MovedFrom] attribute can be deleted from the code.
                var migrate = new AspidGradientButton(
                        $"Migrate all ({group.Entries.Count}) → {migrationTarget.Name}",
                        _ => ApplyGroupFix(group, migrationTarget))
                    .AddClass(GroupMigrateClass);
                migrate.tooltip =
                    $"Every entry resolves to {migrationTarget.FullName} via its declared [MovedFrom] — Unity already " +
                    "migrates them in memory when the asset loads. Migrating rewrites the stored type name in the " +
                    "files so they match the code; the attribute can be removed once no file stores the old name.";
                card.AddChild(migrate);
            }
            else if (TryGetGroupSuggestion(group, constraint, out var suggestion))
            {
                // Reuse the shared label/detail builders so the Smart Fix copy never drifts from the inspector notice.
                var suggest = new AspidGradientButton(SerializeReferenceHelpers.GetSuggestionLabel(suggestion),
                        _ => ApplyGroupFix(group, suggestion.Type))
                    .AddClass(GroupSuggestClass);
                suggest.tooltip = SerializeReferenceHelpers.GetSuggestionDetail(suggestion);
                card.AddChild(suggest);
            }

            foreach (var entry in group.Entries)
                card.AddChild(BuildGroupEntryRow(entry));

            return card;
        }

        private static string BuildGroupCountText(ProjectGroup group)
        {
            var entries = group.Entries.Count;
            var files = group.FileCount;
            var entryText = entries == 1 ? "1 entry" : $"{entries} entries";
            var fileText = files == 1 ? "1 file" : $"{files} files";
            return $"{entryText} · {fileText}";
        }

        // The bulk-fix button's single label: "Fix all (N)" plus a trailing chevron that flips when the picker opens,
        // so the bulk action reads as one dropdown button rather than a split "Fix all" + "Fix ▼" pair.
        private static string BuildFixAllLabel(ProjectGroup group, bool expanded) =>
            $"Fix all ({group.Entries.Count})  {(expanded ? FixArrowExpanded : FixArrowCollapsed)}";

        // A single broken reference inside a group: its asset path and rid. Clicking pings the asset in the Project
        // window (read-only — the bulk Fix above is the only mutation in project mode).
        private VisualElement BuildGroupEntryRow(ProjectEntry entry)
        {
            var row = new VisualElement().AddClass(GroupEntryClass);

            var path = new Label(entry.AssetPath)
                .AddClass(GroupEntryPathClass);
            path.tooltip = entry.AssetPath;

            var rid = new Label($"rid {entry.Entry.Rid}")
                .AddClass(GroupEntryRidClass)
                .SetPickingMode(PickingMode.Ignore);

            row.AddChild(path).AddChild(rid);
            row.RegisterCallback<ClickEvent>(_ =>
            {
                var asset = AssetDatabase.LoadMainAssetAtPath(entry.AssetPath);
                if (asset is null) return;

                // Cross-link: jump to the asset's full Inspect graph; ping as a fallback when hosted standalone.
                if (OnInspectAsset is not null) OnInspectAsset(asset);
                else EditorGUIUtility.PingObject(asset);
            });

            return row;
        }

        // The group's bulk picker: one pick re-points every entry in the group. It expands inline below the Fix all
        // button just like the per-row picker, constrained to the group's intersected field type (or unconstrained
        // when the entries' declared types are mixed/unresolvable).
        private void ToggleGroupPicker(ProjectGroup group, Type constraint, AspidGradientButton button)
        {
            var wasOpen = _openPickerRow == button;
            ClosePicker();
            if (wasOpen) return;

            var view = BuildPickerView(constraint, assemblyQualifiedName =>
            {
                // <None> in the picker emits an empty name: the user wants the group cleared, not re-pointed at a type —
                // so null every entry out (drop the broken payload) instead of treating it as a no-op.
                if (string.IsNullOrEmpty(assemblyQualifiedName))
                {
                    ClearGroupToNull(group);
                    return;
                }

                var type = ResolveType(assemblyQualifiedName);
                if (type is not null) ApplyGroupFix(group, type);
            });

            OpenPickerBelow(button, view);
            button.Text = BuildFixAllLabel(group, expanded: true);
        }

        // Rewrites every entry in the group to newType, after a mandatory confirmation (file rewrites are not
        // undoable). Rewrites are batched per file so each affected asset is reimported exactly once, behind a
        // progress bar; a success summary then reports the count and a fresh project scan replaces the list.
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

            // Diff preview: show the exact YAML lines this rewrite will change (computed by the same scan the rewrite
            // applies, so the preview cannot lie) before confirming the file edit.
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

            // Re-sweep so the list reflects the new state, then surface the summary above it. The headline carries the
            // action + count; the body names both ends of the fix — the missing stored type and the type the entries
            // now point at (long, so they wrap below the divider) — plus any skipped note.
            var summaryTitle = rewritten == 1 ? "Rewrote 1 reference" : $"Rewrote {rewritten} references";
            var summaryBody = $"Replaced missing '{group.DisplayName}' with '{newType.FullName}'.";
            if (skipped > 0)
                summaryBody += $" Skipped {skipped} in open scene(s) or Prefab Mode.";

            // Undo re-points the same entries back to their original (now-missing) stored type — restoring the broken
            // state — via the same YAML rewrite. The data blocks were never touched on disk (only the type line moved),
            // so flipping the type back is a faithful revert. Captured into the summary's button below.
            var originalType = group.StoredType;
            var missingName = group.DisplayName;
            var appliedName = newType.FullName;
            void Undo(VisualElement receipt) => UndoGroupFix(entries, originalType, managedType, missingName, appliedName, receipt);

            if (_scanButton is not null) _scanButton.Text = RescanLabel;
            var groups = CollectProjectGroups(out var canceled);

            if (groups.Count == 0)
            {
                // The fix cleared the last broken type. Stay in the results region instead of swapping to the
                // "Project clean" hero, so the summary HelpBox below survives as the receipt for this rewrite — the
                // hero would hide it along with the whole _results subtree. The celebratory empty state is reserved
                // for an explicit Rescan (which the hint invites), so the user reads "what just happened" before the
                // view resets to clean.
                _list.Clear();
                ShowResults("No missing references", SerializeReferenceCanvasStyle.Success);
                _resultsHint.text =
                    "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";
            }
            else
            {
                RenderGroups(groups, canceled);
            }

            ShowSummary(summaryTitle, summaryBody, Undo);
        }

        // Clears every entry in the group to null. References whose asset is closed are nulled in the YAML directly
        // (TryNullReference: pointer → -2, broken payload dropped). References whose asset is open in Prefab Mode or a
        // loaded scene cannot be rewritten on disk (the open copy would clobber it on save), so they are nulled on the
        // live object in memory instead (TryClearMissingReferenceInMemory) and saved with the asset — until then they
        // keep showing in the audit. Reached by picking <None> in the group's Fix all picker; mirrors ApplyGroupFix's
        // confirm + receipt stack. NOT undoable: the broken payload is discarded, so the receipt carries no Undo button.
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
                RenderGroups(CollectProjectGroups(out var rescanCanceled), rescanCanceled);
                return;
            }

            var summaryTitle = cleared == 1 ? "Cleared 1 reference" : $"Cleared {cleared} references";
            var summaryBody = $"Set missing '{group.DisplayName}' to null.";
            if (clearedInMemory > 0)
                summaryBody += clearedInMemory == 1
                    ? " 1 was nulled in memory — save the asset to persist it (still listed until saved)."
                    : $" {clearedInMemory} were nulled in memory — save the assets to persist them (still listed until saved).";

            if (_scanButton is not null) _scanButton.Text = RescanLabel;
            var groups = CollectProjectGroups(out var canceled);

            if (groups.Count == 0)
            {
                // Same came-back-clean handling as ApplyGroupFix: stay in the results region so this receipt survives as
                // the record of the clear, rather than swapping to the "Project clean" hero which would hide it.
                _list.Clear();
                ShowResults("No missing references", SerializeReferenceCanvasStyle.Success);
                _resultsHint.text =
                    "Nothing left to repair. Rescan to sweep the project again and confirm it's clean.";
            }
            else
            {
                RenderGroups(groups, canceled);
            }

            // No Undo: clearing discards the broken payload (see ClearGroupToNull). The receipt is a plain record.
            ShowSummary(summaryTitle, summaryBody, onUndo: null);
        }

        // Splits a group's entries into those safe to rewrite on disk and those whose asset is open in Prefab Mode / a
        // loaded scene (which must be repaired in memory). Same writable test FilterWritableEntries uses, but it keeps
        // the open entries instead of only counting them, so the clear can null them on the live object.
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
                if (SerializeReferenceHelpers.TryClearMissingReferenceInMemory(entry.AssetPath, entry.Entry.Rid, storedType))
                    cleared++;

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

        // A compact "what gets cleared" list for the null-out confirmation: file + rid per entry, capped so the dialog
        // stays readable. Unlike the type-fix diff there is no before/after line — the whole entry is being dropped.
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

        // The skip filter both the fix and the undo share: a scene — or a prefab open in Prefab Mode — that is loaded in
        // the editor would race a file rewrite, since the in-memory copy wins on the next Ctrl+S and silently clobbers
        // the on-disk edit (the same hazard the per-property flow avoids in Prefab Mode by repairing in memory). Returns
        // the entries safe to write on disk; reports how many were held back.
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

        // An entry is safe to rewrite on disk only when its asset is not loaded in a scene or open in Prefab Mode — the
        // in-memory copy would otherwise win on the next save and clobber the file edit (see FilterWritableEntries).
        private static bool IsEntryWritable(ProjectEntry entry, string prefabStagePath)
        {
            var openInScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(entry.AssetPath).isLoaded;
            var openInPrefabMode = !string.IsNullOrEmpty(prefabStagePath) &&
                                   string.Equals(prefabStagePath, entry.AssetPath, StringComparison.Ordinal);

            return !openInScene && !openInPrefabMode;
        }

        // Rewrites every entry's stored type to targetType, batched per file behind a cancel-free progress bar.
        // StartAssetEditing defers each ImportAsset until StopAssetEditing, so the whole set reimports in one pass
        // instead of the editor churning once per file mid-loop. Returns how many entries were actually rewritten.
        // Shared by the forward fix (re-point to the chosen type) and Undo (re-point back to the missing type).
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

        // Reverts one bulk fix: re-points its entries back to the original (now-missing) stored type, after a
        // confirmation. Only the type line moves — the entries' data blocks are still on disk untouched — so this
        // restores the exact broken state the group had before the fix. The reverted references reappear as a fixable
        // group; only this fix's own receipt is dropped, so summaries for other still-applied fixes (and their Undo
        // buttons) survive — unlike a full Rescan, which would clear the whole stack.
        private void UndoGroupFix(IReadOnlyList<ProjectEntry> entries, ManagedTypeName originalType, ManagedTypeName appliedType, string missingName, string appliedName, VisualElement receipt)
        {
            // The asset may have been opened in a scene / Prefab Mode since the fix; re-point only the still-writable
            // entries, the same guard the forward fix applies.
            var writable = FilterWritableEntries(entries, out var skipped);

            // Only entries that STILL hold the type this receipt applied may be re-pointed: rids survive rewrites, so
            // the group can break again and be fixed to a DIFFERENT type while an older receipt sits on the stack —
            // blindly rewriting would destroy that newer fix. "Still holds what we applied" is exactly a rewrite
            // towards the applied type whose old line already equals its new line (a self-no-op).
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

            // Drop only this receipt — the other summaries describe fixes that are still applied, so they stay (and so
            // do their own Undo buttons). Then re-render the group list (RenderGroups rebuilds only _list, never
            // _summaries) so the reverted references reappear as a fixable group alongside the surviving receipts.
            receipt?.RemoveFromHierarchy();
            var groups = CollectProjectGroups(out var canceled);
            RenderGroups(groups, canceled);

            // The undo gets a receipt of its own, honest about the count it actually touched (BatchRewriteEntries can
            // come up short when a file changed between the check above and the write).
            var undoTitle = reverted == 1 ? "Reverted 1 reference" : $"Reverted {reverted} references";
            var undoBody = $"Re-pointed back to the missing '{missingName}'.";
            if (diverged > 0) undoBody += $" Left {diverged} alone (no longer '{appliedName}').";
            if (reverted < revertible.Count) undoBody += $" {revertible.Count - reverted} could not be rewritten.";
            ShowSummary(undoTitle, undoBody, null);
        }

        // Builds a compact old -> new line preview of the YAML the bulk fix will rewrite, using the same scan
        // (TryComputeRewrite) the rewrite applies, so the preview is exactly what gets written. Capped so the
        // confirmation stays readable.
        private static string BuildDiffPreview(List<ProjectEntry> entries, ManagedTypeName newType)
        {
            const int maxShown = 8;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Changes:");

            // Compute first, render second: an entry whose rewrite cannot be computed must neither vanish silently
            // nor inflate the "…and N more" remainder (which previously counted every unshown entry, computable or
            // not — overstating the tail and hiding the failures).
            var edits = new List<(ProjectEntry entry, RewriteEdit edit)>(entries.Count);
            foreach (var entry in entries)
                if (SerializeReferenceYamlEditor.TryComputeRewrite(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid, newType, out var edit))
                    edits.Add((entry, edit));

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

        // The group's Smart Fix suggestion: rank the stored type against the candidate pool (constrained to the
        // group's field type), keying the field-shape heuristic off the first entry's recorded field names. Surfaced
        // only when a candidate clears the confidence threshold. The quick-apply button hands the suggested type
        // straight to ApplyGroupFix, bypassing the picker — that is safe only because Rank enforces the constraint
        // internally (its pool is exactly the types the picker would offer), so the suggestion is always assignable.
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
            // card — reading as a dropdown spanning the card, with the entry rows shifting down beneath it. The ??
            // fallback keeps a sane target if the button is ever hosted outside a card.
            var card = anchor.parent;
            var container = card ?? _list;
            container.InsertChild(container.IndexOf(anchor) + 1, _openPicker);

            // The whole card becomes the active surface: it lights an accent frame, the header button welds to the
            // picker (its bottom corners square and its gap closes — see __group--picking) and the selector sheds its
            // own box to blend into the card (see __picker--attached), so header, selector and entry rows read as one
            // active card rather than a button stacked over a separate dropdown.
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
            // Flip the open button's chevron back to its resting ▼. ClosePicker has no group reference here, but only
            // the trailing glyph differs between the collapsed and expanded labels, so swapping it in place is enough.
            if (_openPickerRow is not null)
                _openPickerRow.Text = _openPickerRow.Text.Replace(FixArrowExpanded, FixArrowCollapsed);
            _openPickerCard?.RemoveClass(GroupPickingClass);

            _openPicker = null;
            _openPickerRow = null;
            _openPickerCard = null;
        }

        private static Type ResolveType(string assemblyQualifiedName) =>
            string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

        // The idle and both terminal states reuse one hero: the package icon (the snake) in the status colour, a
        // headline and a dimmed explanation. Rebuilt per call — the icon, accent and copy all differ between the idle
        // "not scanned" info state and the "Project clean" / "Scan canceled" terminal states.
        private void ShowEmptyState(bool success, string title, string message)
        {
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

        // The cold-index idle state shown until the user triggers the first scan: the Scan panel floats over an
        // info-toned hero — the snake icon, a headline and a dimmed prompt — mirroring the Asset References tab's
        // "No asset selected" hero, so an unscanned tab reads as a deliberate starting point rather than a blank
        // canvas. No results list yet (the project is unscanned, so "clean" cannot be claimed). ShowEmptyState paints
        // the info icon and tones the canvas Info; the button keeps its cold Scan Project label until ScanProject
        // relabels it.
        private void ShowIdle() => ShowEmptyState(
            success: false,
            title: "Project not scanned",
            message: "Run Scan Project to map every broken [SerializeReference] type across your assets — then repair each missing type in bulk.");

        // Reveals the results region with a header and tones the canvas. The tone is explicit per call site because the
        // region is reused for two opposite states: the missing-references sweep tones Warning (something to repair),
        // while the came-back-clean receipt (last broken type just fixed/cleared) tones Success — matching the
        // "Project clean" hero a fresh Rescan would show, instead of leaving a clean state on an amber backdrop.
        private void ShowResults(string headerText, Color tone)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);
            _resultsHeader.Text = headerText;
            OnCanvasTone?.Invoke(tone);
        }

        // Appends one receipt to the running stack rather than overwriting the previous: chaining a fix across several
        // groups leaves every earlier summary on screen, newest at the bottom (just above the list, where the lone
        // summary used to sit). The stack is reset only by ClearSummaries on the next fresh scan. The receipt carries a
        // right-pinned Undo button (the help box is a row whose text container flex-grows, so the button rides the
        // trailing edge) that reverts exactly this fix.
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

        // One broken reference located during a project scan: where it lives (asset path) and the orphaned entry.
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

        // All broken references that share one stored type, gathered across the whole project. The group resolves a
        // single picker constraint by intersecting the declared field types of its entries (per-file constraint maps
        // are cached so each asset is scanned once), falling back to typeof(object) when they disagree.
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

            // The common declared field type of every entry, or typeof(object) when they disagree or any is
            // unresolvable. Per-file constraint maps are built once and cached, so the intersection costs one scan per
            // distinct asset regardless of how many of the group's entries it holds.
            public Type ResolveConstraint() => ResolveConstraint(out _);

            // Overload reporting whether the typeof(object) fallback was caused specifically by the entries' declared
            // field types <i>disagreeing</i> (as opposed to one being unrecoverable). The bulk-fix confirmation warns
            // on this case, since the one chosen type may not fit every entry and the mismatched ones null on reimport.
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
