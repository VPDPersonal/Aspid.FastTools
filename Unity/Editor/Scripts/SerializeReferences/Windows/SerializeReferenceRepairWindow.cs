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
    internal sealed class SerializeReferenceRepairWindow : EditorWindow
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";

        private const string RootClass = "aspid-fasttools-repair-references";
        private const string BackgroundClass = RootClass + "__background";
        private const string ContentClass = RootClass + "__content";
        private const string CardClass = RootClass + "__card";
        private const string CardHeaderClass = RootClass + "__card-header";
        private const string FieldRowClass = RootClass + "__field-row";
        private const string AssetClass = RootClass + "__asset";
        private const string RescanClass = RootClass + "__rescan";
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

        // Project scan candidates: serialized text assets that can host managed references. Other extensions are
        // either binary or never carry [SerializeReference] data, so skipping them keeps the sweep fast.
        private static readonly string[] ScanExtensions = { ".prefab", ".asset", ".unity" };

        private Object _target;
        private ObjectField _assetField;
        private VisualElement _empty;
        private VisualElement _results;
        private AspidLabel _resultsHeader;
        private AspidHelpBox _summary;
        private Label _resultsHint;
        private VisualElement _list;
        private VisualElement _openPicker;
        private AspidGradientButton _openPickerRow;

        [MenuItem("Tools/Aspid 🐍/Repair Missing References FastTools", priority = 20)]
        private static void Open() => Open(Selection.activeObject as Object);

        public static void Open(Object target)
        {
            var window = GetWindow<SerializeReferenceRepairWindow>();
            window.titleContent = new GUIContent("Repair References");
            window.minSize = new Vector2(460f, 320f);
            window.SetTarget(target);
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(RootClass);

            // The animated dotted canvas absolutely fills the window (its own stylesheet positions it); the content
            // flows above it, mirroring the Welcome window so the dark Aspid components read against black instead of
            // the muddy default inspector grey.
            var background = new AspidAnimatedDotsBackground()
                .AddClass(BackgroundClass)
                .SetPickingMode(PickingMode.Ignore);

            // The asset picker sits in a Welcome-style card: an Aspid header with the signature green divider above
            // the full-width field, Rescan and Scan Project trailing it on the same row.
            var assetHeader = new AspidLabel("Asset", AspidLabelPreset.Default
                    .SetLabelTheme(ThemeStyle.Type.Lightness)
                    .SetLabelSize(AspidLabelSizeStyle.Type.H3)
                    .SetLineTheme(ThemeStyle.Type.Dark)
                    .SetLineStatus(StatusStyle.Type.Success))
                .AddClass(CardHeaderClass);

            _assetField = new ObjectField
            {
                objectType = typeof(Object),
                allowSceneObjects = false,
                value = _target,
            };
            _assetField.AddClass(AssetClass);
            _assetField.RegisterValueChangedCallback(evt => SetTarget(evt.newValue));

            var rescan = new AspidGradientButton("Rescan", _ => Rescan())
                .AddClass(RescanClass);

            var scanProject = new AspidGradientButton("Scan Project", _ => ScanProject())
                .AddClass(ScanProjectClass);

            var fieldRow = new VisualElement()
                .AddClass(FieldRowClass)
                .AddChild(_assetField)
                .AddChild(rescan)
                .AddChild(scanProject);

            var card = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(CardClass)
                .AddChild(assetHeader)
                .AddChild(fieldRow);

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
                .AddChild(card)
                .AddChild(_empty)
                .AddChild(_results);

            root.AddChild(background)
                .AddChild(content);

            Rescan();
        }

        private void SetTarget(Object target)
        {
            _target = target;
            // Open() retargets an already-open window, so the field must follow the new target —
            // without notifying, or the change callback would trigger a second scan.
            _assetField?.SetValueWithoutNotify(target);

            // Assigning an asset always returns to single-asset mode: the ObjectField is the single-asset entry point,
            // so a fresh pick replaces any project-wide grouped results with that asset's per-entry list.
            if (_list is not null) Rescan();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Single-asset mode
        // ---------------------------------------------------------------------------------------------------------

        private void Rescan()
        {
            if (_list is null) return;

            ClosePicker();
            HideSummary();
            _list.Clear();

            var assetPath = _target ? AssetDatabase.GetAssetPath(_target) : null;
            if (string.IsNullOrEmpty(assetPath))
            {
                ShowEmptyState(
                    success: false,
                    title: "No asset selected",
                    message: "Select a saved asset (a prefab or ScriptableObject) to scan for missing references, " +
                             "or press Scan Project to sweep the whole project at once.");
                return;
            }

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(assetPath, SerializeReferenceHelpers.StoredTypeResolves);
            if (missing.Count == 0)
            {
                ShowEmptyState(
                    success: true,
                    title: "All references intact",
                    message: "No missing managed references in this asset.");
                return;
            }

            ShowResults(missing.Count == 1 ? "1 missing reference" : $"{missing.Count} missing references");
            _resultsHint.text = "Pick a replacement type for each entry — Fix rewrites the stored type directly in the asset file.";

            // The declared field type backing each missing reference constrains the replacement list, so the picker
            // only offers types actually assignable to the field — re-pointing to an incompatible type would drop the
            // reference to null on the next import.
            var constraints = SerializeReferenceHelpers.BuildConstraintMap(assetPath);

            foreach (var entry in missing)
            {
                constraints.TryGetValue((entry.FileId, entry.Rid), out var constraint);
                _list.AddChild(BuildRow(assetPath, entry, constraint));
            }
        }

        // Each missing reference is a full-width gradient row (label = stored type, dimmed rid, trailing "Fix" cue),
        // the whole row acting as the Fix button — the same affordance the Welcome window uses for its sample list.
        private VisualElement BuildRow(string assetPath, MissingReferenceEntry entry, Type constraint)
        {
            var typeName = string.IsNullOrEmpty(entry.StoredType.Namespace)
                ? entry.StoredType.Class
                : $"{entry.StoredType.Namespace}.{entry.StoredType.Class}";

            AspidGradientButton row = null;
            row = new AspidGradientButton(entry.StoredType.Class, FixCollapsedText, _ => TogglePicker(assetPath, entry, constraint, row));
            row.AddClass(EntryClass);
            row.tooltip = typeName;

            var rid = new Label($"rid {entry.Rid}")
                .AddClass(EntryRidClass)
                .SetPickingMode(PickingMode.Ignore);

            // Slot the rid between the label (index 1) and the trailing "Fix" cue (index 2) so the flex-grown label
            // pushes it to the right edge alongside the action.
            row.InsertChild(2, rid);

            return row;
        }

        // The picker expands inline as an accordion panel right below the clicked row — the same selector view the
        // dropdown window hosts, boxed in this window's dark style instead of a floating grey popup. One panel at a
        // time; the row's trailing cue flips to ▲ while its panel is open and clicking the row again collapses it.
        private void TogglePicker(string assetPath, MissingReferenceEntry entry, Type constraint, AspidGradientButton row)
        {
            var wasOpen = _openPickerRow == row;
            ClosePicker();
            if (wasOpen) return;

            var view = BuildPickerView(constraint, assemblyQualifiedName => ApplyFix(assetPath, entry, assemblyQualifiedName));
            OpenPickerBelow(row, view);
            row.TrailingText = FixExpandedText;
        }

        private void ApplyFix(string assetPath, MissingReferenceEntry entry, string assemblyQualifiedName)
        {
            var type = ResolveType(assemblyQualifiedName);
            if (type is null) return;

            // An asset open in Prefab Mode holds a separate in-memory stage copy that does not refresh on reimport and
            // overwrites a file rewrite on save (the same hazard the per-property inspector flow sidesteps by repairing
            // in memory). Rewriting the file here would be silently discarded, so abort with an explanation instead.
            var prefabStagePath = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage()?.assetPath;
            if (!string.IsNullOrEmpty(prefabStagePath) &&
                string.Equals(prefabStagePath, assetPath, StringComparison.Ordinal))
            {
                EditorUtility.DisplayDialog(
                    "Repair Missing References",
                    "This asset is open in Prefab Mode, whose in-memory copy would overwrite a file rewrite on save.\n\n" +
                    "Close Prefab Mode and try again, or use the field's inline Fix in the Inspector, which repairs in memory.",
                    "OK");
                return;
            }

            if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, entry.FileId, entry.Rid, ManagedTypeName.FromType(type))) return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            SerializeReferenceRepairSuggestions.ClearCache();
            Rescan();
        }

        // ---------------------------------------------------------------------------------------------------------
        // Project mode
        // ---------------------------------------------------------------------------------------------------------

        // Sweeps every candidate text asset under Assets/, finds the missing references in each, and groups them by
        // their stored broken type. The sweep is the slow part (it parses every asset's YAML), so it runs behind a
        // cancelable progress bar; cancelling shows whatever was collected so far.
        private void ScanProject()
        {
            if (_list is null) return;

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

        // Enumerates the project's candidate assets and accumulates missing references grouped by stored type. Returns
        // the groups (sorted by descending entry count) and reports whether the user canceled the progress bar.
        private static List<ProjectGroup> CollectProjectGroups(out bool canceled)
        {
            canceled = false;
            var byType = new Dictionary<string, ProjectGroup>(StringComparer.Ordinal);

            var paths = AssetDatabase.GetAllAssetPaths()
                .Where(IsScanCandidate)
                .ToArray();

            try
            {
                for (var i = 0; i < paths.Length; i++)
                {
                    var path = paths[i];

                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Scanning Project",
                            $"{path}  ({i + 1}/{paths.Length})",
                            (float)i / paths.Length))
                    {
                        canceled = true;
                        break;
                    }

                    var missing = SerializeReferenceYamlEditor.FindMissingReferences(path, SerializeReferenceHelpers.StoredTypeResolves);
                    if (missing.Count == 0) continue;

                    foreach (var entry in missing)
                    {
                        var key = StoredTypeKey(entry.StoredType);
                        if (!byType.TryGetValue(key, out var group))
                        {
                            group = new ProjectGroup(entry.StoredType);
                            byType.Add(key, group);
                        }

                        group.Add(path, entry);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
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
                if (asset is not null) EditorGUIUtility.PingObject(asset);
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

            if (!EditorUtility.DisplayDialog(
                    "Repair Missing References",
                    $"Rewrite {entries.Count} reference(s) in {files} file(s) to '{newType.FullName}'?\n\n" +
                    "This edits the asset files directly and cannot be undone." + skippedNote + mixedNote,
                    "Rewrite",
                    "Cancel"))
                return;

            var managedType = ManagedTypeName.FromType(newType);
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

            // Anchor the panel directly after the row in its own parent so it reads as belonging to that row, whether
            // the row is a top-level single-asset entry (parent = _list) or a Fix all button nested in a group header.
            var parent = anchor.parent ?? _list;
            parent.InsertChild(parent.IndexOf(anchor) + 1, _openPicker);
            view.FocusSearch();
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

        private void ShowResults(string headerText)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);
            _resultsHeader.Text = headerText;
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

        private static bool IsScanCandidate(string path)
        {
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets/", StringComparison.Ordinal)) return false;

            foreach (var extension in ScanExtensions)
                if (path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        // Stable grouping key for a stored type identity (class + namespace + assembly). ManagedTypeName carries no
        // value equality, so the three fields are joined into a key string instead.
        private static string StoredTypeKey(ManagedTypeName type) =>
            $"{type.Assembly}|{type.Namespace}|{type.Class}";

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
