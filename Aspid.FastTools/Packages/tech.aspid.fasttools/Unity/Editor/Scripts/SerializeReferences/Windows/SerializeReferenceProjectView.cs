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
        private const string SummaryClass = RootClass + "__summary";
        private const string SummaryHiddenClass = SummaryClass + "--hidden";
        private const string ScrollClass = RootClass + "__scroll";
        private const string EntryClass = RootClass + "__entry";
        private const string EntryRidClass = RootClass + "__entry-rid";
        private const string PickerClass = RootClass + "__picker";

        private const string GroupClass = RootClass + "__group";
        private const string GroupHeaderRowClass = RootClass + "__group-header-row";
        private const string GroupHeaderClass = RootClass + "__group-header";
        private const string GroupCountClass = RootClass + "__group-count";
        private const string GroupActionsClass = RootClass + "__group-actions";
        private const string GroupFixAllClass = RootClass + "__group-fix-all";
        private const string GroupSuggestClass = RootClass + "__group-suggest";
        private const string GroupEntryClass = RootClass + "__group-entry";
        private const string GroupEntryPathClass = RootClass + "__group-entry-path";
        private const string GroupEntryRidClass = RootClass + "__group-entry-rid";

        private const string FixCollapsedText = "Fix  ▼";
        private const string FixExpandedText = "Fix  ▲";

        // The scan button's label adapts to the index state: a deliberate "Scan Project" call-to-action while the
        // index is cold (the first build is expensive), a quiet "Rescan" refresh once a scan has warmed it.
        private const string ScanLabel = "Scan Project";
        private const string RescanLabel = "Rescan";

        private VisualElement _empty;
        private VisualElement _results;
        private AspidLabel _resultsHeader;
        private AspidHelpBox _summary;
        private Label _resultsHint;
        private VisualElement _list;
        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;
        private AspidGradientButton _scanButton;

        /// <summary>Jump from a project-audit result row to that asset's Inspect graph. Wired by the host window.</summary>
        public Action<Object> OnInspectAsset;

        /// <summary>Reports this view's state-tone to the host window, which owns the shared dotted canvas. Wired by the window.</summary>
        public Action<Color> OnCanvasTone;

        public SerializeReferenceProjectView()
        {
            var root = this;
            style.flexGrow = 1;
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            // A full-width translucent header panel gives the Scan Project action a purposeful home, stacked: a title
            // and a one-line description of what the audit does, then a full-width Scan Project button below. The
            // dotted canvas (owned by the host window) reads through the panel's semi-transparent fill. The title is
            // phrased around the action rather than repeating the tab's "Project Audit" name.
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

            // A success help-box reporting how many references the last bulk fix rewrote; hidden until a Fix all runs.
            _summary = new AspidHelpBox(AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Info))
                .AddClass(SummaryClass)
                .AddClass(SummaryHiddenClass);
            _summary.Status = StatusStyle.Type.Success;

            _list = new VisualElement();

            var scroll = new ScrollView().AddClass(ScrollClass);
            scroll.AddChild(_list);

            _results = new VisualElement()
                .AddClass(ResultsClass)
                .AddChild(_resultsHeader)
                .AddChild(_resultsHint)
                .AddChild(_summary)
                .AddChild(scroll);

            var content = new VisualElement()
                .AddClass(ContentClass)
                .AddChild(panel)
                .AddChild(_empty)
                .AddChild(_results);

            root.AddChild(content);
        }

        // The tab-switch entry point. The project sweep's first pass is cold-built per session — the static usage
        // index does not survive domain reloads — and parses every candidate asset's YAML behind a blocking bar,
        // which is slow on large projects. So a plain switch to the Project Audit tab only auto-shows results when the
        // index is already warm (a near-free in-memory filter); while cold it shows just the Scan panel over the idle
        // canvas and waits for a deliberate Scan Project click, rather than freezing the editor on the tab switch. The
        // breakage-notification deep-link bypasses this gate and forces a scan (the host window calls ScanProject
        // directly), because the user opened it specifically to see the broken references.
        public void Initialize()
        {
            if (SerializeReferenceTypeUsageIndex.IsWarm) ScanProject();
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

            // A scan always warms the index, so the button is a refresh affordance from here on — whether this is the
            // first cold scan the user triggered or a warm rescan.
            if (_scanButton is not null) _scanButton.Text = RescanLabel;

            ClosePicker();
            HideSummary();
            _list.Clear();

            var groups = CollectProjectGroups(out var canceled);

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
            ShowResults(entryCount == 1 ? "1 missing reference" : $"{entryCount} missing references");
            _resultsHint.text = canceled
                ? "Scan canceled — showing partial results. Each group is a broken type; Fix all re-points every entry across every file at once."
                : "Each group is a broken stored type. Fix all picks one replacement and re-points every entry across every affected file at once.";

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

        // A broken-type group card: a header (display name + entry/file counts), a Fix all bulk action, an optional
        // Smart Fix quick-apply, then one ping-only row per entry. Entries are intentionally not individually fixable
        // here — the per-row Fix affordance is reserved for single-asset mode, where the whole row is the button;
        // adding a second per-entry action under a bulk-fix group would fight that layout, so project mode is
        // group-level only (a precise per-asset fix is one ObjectField pick away).
        private VisualElement BuildGroupCard(ProjectGroup group)
        {
            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(GroupClass);

            var constraint = group.ResolveConstraint();
            var displayName = group.DisplayName;

            var header = new AspidLabel(group.StoredType.Class, AspidLabelPreset.Default
                    .SetLabelStatus(StatusStyle.Type.Warning)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H5)
                    .SetLineSize(AspidDividingLineSizeStyle.Type.None))
                .AddClass(GroupHeaderClass);
            header.tooltip = constraint == typeof(object)
                ? $"{displayName}\nMixed or unresolvable field types — the picker is unconstrained (any managed-reference type)."
                : $"{displayName}\nConstrained to {constraint.FullName}.";

            var count = new Label(BuildGroupCountText(group))
                .AddClass(GroupCountClass)
                .SetPickingMode(PickingMode.Ignore);

            var actions = new VisualElement().AddClass(GroupActionsClass);

            AspidGradientButton fixAll = null;
            fixAll = new AspidGradientButton($"Fix all ({group.Entries.Count})", FixCollapsedText,
                    _ => ToggleGroupPicker(group, constraint, fixAll))
                .AddClass(GroupFixAllClass);
            actions.AddChild(fixAll);

            if (TryGetGroupSuggestion(group, constraint, out var suggestion))
            {
                // Reuse the shared label/detail builders so the Smart Fix copy never drifts from the inspector notice.
                var suggest = new AspidGradientButton(SerializeReferenceHelpers.GetSuggestionLabel(suggestion),
                        _ => ApplyGroupFix(group, suggestion.Type))
                    .AddClass(GroupSuggestClass);
                suggest.tooltip = SerializeReferenceHelpers.GetSuggestionDetail(suggestion);
                actions.AddChild(suggest);
            }

            var headerRow = new VisualElement()
                .AddClass(GroupHeaderRowClass)
                .AddChild(header)
                .AddChild(count)
                .AddChild(actions);

            card.AddChild(headerRow);

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
                var type = ResolveType(assemblyQualifiedName);
                if (type is not null) ApplyGroupFix(group, type);
            });

            OpenPickerBelow(button, view);
            button.TrailingText = FixExpandedText;
        }

        // Rewrites every entry in the group to newType, after a mandatory confirmation (file rewrites are not
        // undoable). Rewrites are batched per file so each affected asset is reimported exactly once, behind a
        // progress bar; a success summary then reports the count and a fresh project scan replaces the list.
        private void ApplyGroupFix(ProjectGroup group, Type newType)
        {
            if (newType is null) return;
            ClosePicker();

            // A scene — or a prefab open in Prefab Mode — that is loaded in the editor would race the rewrite: the
            // in-memory copy wins on the next Ctrl+S and silently clobbers the file fix (same hazard the per-property
            // flow avoids in Prefab Mode by repairing in memory). Such entries are skipped here; close the scene /
            // Prefab Mode and rescan to include them.
            var prefabStagePath = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;
            var entries = new List<ProjectEntry>(group.Entries.Count);
            var skipped = 0;
            foreach (var entry in group.Entries)
            {
                var openInScene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(entry.AssetPath).isLoaded;
                var openInPrefabMode = !string.IsNullOrEmpty(prefabStagePath) &&
                                       string.Equals(prefabStagePath, entry.AssetPath, StringComparison.Ordinal);

                if (openInScene || openInPrefabMode) skipped++;
                else entries.Add(entry);
            }

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
            // applies, so the preview cannot lie) before confirming an irreversible file edit.
            var diff = BuildDiffPreview(entries, managedType);

            if (!EditorUtility.DisplayDialog(
                    "Repair Missing References",
                    $"Rewrite {entries.Count} reference(s) in {files} file(s) to '{newType.FullName}'?\n\n" +
                    diff +
                    "This edits the asset files directly and cannot be undone." + skippedNote + mixedNote,
                    "Rewrite",
                    "Cancel"))
                return;
            var byFile = entries
                .GroupBy(entry => entry.AssetPath, StringComparer.Ordinal)
                .ToArray();

            var rewritten = 0;

            // Batch the per-file reimports: StartAssetEditing defers every ImportAsset until StopAssetEditing, so the
            // whole group reimports in one pass instead of the editor churning once per file mid-loop.
            AssetDatabase.StartAssetEditing();
            try
            {
                for (var i = 0; i < byFile.Length; i++)
                {
                    var file = byFile[i];
                    EditorUtility.DisplayProgressBar(
                        "Repairing References",
                        $"{file.Key}  ({i + 1}/{byFile.Length})",
                        (float)i / byFile.Length);

                    var changed = false;
                    foreach (var entry in file)
                    {
                        if (!SerializeReferenceYamlEditor.TryRewriteType(file.Key, entry.Entry.FileId, entry.Entry.Rid, managedType))
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

            SerializeReferenceRepairSuggestions.ClearCache();

            // Re-sweep so the list reflects the new state, then surface the summary above it.
            ScanProject();
            var summary = rewritten == 1
                ? $"Rewrote 1 reference to {newType.FullName}."
                : $"Rewrote {rewritten} references to {newType.FullName}.";
            if (skipped > 0)
                summary += $" Skipped {skipped} in open scene(s) or Prefab Mode.";
            ShowSummary(summary);
        }

        // Builds a compact old -> new line preview of the YAML the bulk fix will rewrite, using the same scan
        // (TryComputeRewrite) the rewrite applies, so the preview is exactly what gets written. Capped so the
        // confirmation stays readable.
        private static string BuildDiffPreview(List<ProjectEntry> entries, ManagedTypeName newType)
        {
            const int maxShown = 8;
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Changes:");

            var shown = 0;
            foreach (var entry in entries)
            {
                if (shown >= maxShown)
                {
                    builder.AppendLine($"  …and {entries.Count - shown} more");
                    break;
                }

                if (!SerializeReferenceYamlEditor.TryComputeRewrite(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid, newType, out var edit))
                    continue;

                builder.AppendLine($"  {System.IO.Path.GetFileName(entry.AssetPath)} (rid {entry.Entry.Rid}):");
                builder.AppendLine($"    - {edit.OldLine.Trim()}");
                builder.AppendLine($"    + {edit.NewLine.Trim()}");
                shown++;
            }

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
                types: new[] { baseType },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: onSelected,
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: baseType == typeof(object) ? null : GenericTypeResolver.GetAssignableGenericDefinitions(baseType),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument,
                onDismiss: ClosePicker);
        }

        private void OpenPickerBelow(AspidGradientButton anchor, TypeSelectorView view)
        {
            _openPicker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(PickerClass)
                .AddChild(view);

            _openPickerRow = anchor;

            // The Fix all button lives in the right-aligned actions cluster of the group's header row, so anchoring
            // the picker directly after the button would cram the selector into that narrow right-hand column (it read
            // as a side panel). Walk up to the group card and drop the picker full-width right below the whole header
            // row instead, so it reads as a dropdown spanning the card, with the entry rows shifting down beneath it.
            // The ?? fallback keeps a sane target if the actions → header-row → card nesting ever changes.
            var headerRow = anchor.parent?.parent;   // anchor → actions → header row
            var card = headerRow?.parent;            // header row → card
            var container = card ?? anchor.parent ?? _list;
            var after = card is not null ? headerRow : anchor;
            container.InsertChild(container.IndexOf(after) + 1, _openPicker);

            view.FocusPicker();
        }

        private void ClosePicker()
        {
            _openPicker?.RemoveFromHierarchy();
            if (_openPickerRow is not null) _openPickerRow.TrailingText = FixCollapsedText;

            _openPicker = null;
            _openPickerRow = null;
        }

        private static Type ResolveType(string assemblyQualifiedName) =>
            string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

        // Both terminal states reuse one hero: the package icon in the status colour, a headline and a dimmed
        // explanation. Rebuilt per scan — the icon, accent and copy all differ between the two states.
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

        // The cold-index idle state shown until the user triggers the first scan: just the Scan panel floating over
        // the info-toned canvas, with neither the results list nor a terminal hero (the project is unscanned, so
        // "clean" cannot be claimed yet). The button keeps its cold Scan Project label until ScanProject relabels it.
        private void ShowIdle()
        {
            _results.AddClass(ResultsHiddenClass);
            _empty.AddClass(EmptyHiddenClass);
            OnCanvasTone?.Invoke(SerializeReferenceCanvasStyle.Info);
        }

        private void ShowResults(string headerText)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);
            _resultsHeader.Text = headerText;
            OnCanvasTone?.Invoke(SerializeReferenceCanvasStyle.Warning);
        }

        private void ShowSummary(string message)
        {
            _summary.Message = message;
            _summary.RemoveClass(SummaryHiddenClass);
        }

        private void HideSummary() => _summary?.AddClass(SummaryHiddenClass);

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

            public string DisplayName => string.IsNullOrEmpty(StoredType.Namespace)
                ? StoredType.Class
                : $"{StoredType.Namespace}.{StoredType.Class}";

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
