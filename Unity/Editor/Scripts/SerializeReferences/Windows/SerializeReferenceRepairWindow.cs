using System;
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
    /// Asset-level repair tool for missing <c>[SerializeReference]</c> types. Unlike the per-field <b>Fix</b> button,
    /// this scans the whole asset file and lists every orphaned managed reference — at any nesting depth and on any
    /// child object — so references the Inspector cannot navigate to (nested values, child-object components,
    /// anything Unity has dropped to <c>&lt;None&gt;</c>) can still be re-pointed. Each entry is fixed by rewriting
    /// its stored type directly in the YAML, so it never needs Prefab Mode.
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
        private const string ScrollClass = RootClass + "__scroll";
        private const string EntryClass = RootClass + "__entry";
        private const string EntryRidClass = RootClass + "__entry-rid";
        private const string PickerClass = RootClass + "__picker";

        private const string FixCollapsedText = "Fix  ▼";
        private const string FixExpandedText = "Fix  ▲";

        private Object _target;
        private ObjectField _assetField;
        private VisualElement _empty;
        private VisualElement _results;
        private AspidLabel _resultsHeader;
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
            // the full-width field, Rescan trailing it on the same row.
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

            var fieldRow = new VisualElement()
                .AddClass(FieldRowClass)
                .AddChild(_assetField)
                .AddChild(rescan);

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

            var resultsHint = new Label("Pick a replacement type for each entry — Fix rewrites the stored type directly in the asset file.")
                .AddClass(ResultsHintClass);

            _list = new VisualElement();

            var scroll = new ScrollView().AddClass(ScrollClass);
            scroll.AddChild(_list);

            _results = new VisualElement()
                .AddClass(ResultsClass)
                .AddChild(_resultsHeader)
                .AddChild(resultsHint)
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
            if (_list is not null) Rescan();
        }

        private void Rescan()
        {
            if (_list is null) return;

            ClosePicker();
            _list.Clear();

            var assetPath = _target ? AssetDatabase.GetAssetPath(_target) : null;
            if (string.IsNullOrEmpty(assetPath))
            {
                ShowEmptyState(
                    success: false,
                    title: "No asset selected",
                    message: "Select a saved asset (a prefab or ScriptableObject) to scan for missing references.");
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

            ShowResults(missing.Count);

            // The declared field type backing each missing reference constrains the replacement list, so the picker
            // only offers types actually assignable to the field — re-pointing to an incompatible type would drop the
            // reference to null on the next import.
            var constraints = BuildConstraintMap(assetPath);

            foreach (var entry in missing)
            {
                constraints.TryGetValue((entry.FileId, entry.Rid), out var constraint);
                _list.AddChild(BuildRow(assetPath, entry, constraint));
            }
        }

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

        private void ShowResults(int count)
        {
            _empty.AddClass(EmptyHiddenClass);
            _results.RemoveClass(ResultsHiddenClass);

            _resultsHeader.Text = count == 1
                ? "1 missing reference"
                : $"{count} missing references";
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

            var baseType = constraint ?? typeof(object);

            var view = new TypeSelectorView(
                types: new[] { baseType },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => ApplyFix(assetPath, entry, assemblyQualifiedName),
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: constraint is null ? null : GenericTypeResolver.GetAssignableGenericDefinitions(constraint),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument,
                onDismiss: ClosePicker);

            _openPicker = new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Darkness))
                .AddClass(PickerClass)
                .AddChild(view);

            _openPickerRow = row;
            row.TrailingText = FixExpandedText;

            _list.InsertChild(_list.IndexOf(row) + 1, _openPicker);
            view.FocusSearch();
        }

        private void ClosePicker()
        {
            _openPicker?.RemoveFromHierarchy();
            if (_openPickerRow is not null) _openPickerRow.TrailingText = FixCollapsedText;

            _openPicker = null;
            _openPickerRow = null;
        }

        private void ApplyFix(string assetPath, MissingReferenceEntry entry, string assemblyQualifiedName)
        {
            var type = string.IsNullOrEmpty(assemblyQualifiedName)
                ? null
                : Type.GetType(assemblyQualifiedName, throwOnError: false);

            if (type is null) return;
            if (!SerializeReferenceYamlEditor.TryRewriteType(assetPath, entry.FileId, entry.Rid, ManagedTypeName.FromType(type))) return;

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            Rescan();
        }

        // Maps every managed reference in the asset to the declared field type that holds it, keyed by the object
        // document and the reference's RefIds id. A missing reference reads back null through the serialization API
        // but its field still reports the declared element type via managedReferenceFieldTypename, and the orphaned
        // rid survives in the YAML — so the two together recover the constraint for the picker. References nested
        // inside a missing parent are unreachable here (the parent is null) and simply fall back to an unconstrained
        // picker, as do orphaned rids no field points at.
        private static Dictionary<(long fileId, long rid), Type> BuildConstraintMap(string assetPath)
        {
            var map = new Dictionary<(long, long), Type>();

            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (obj == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out _, out var fileId)) continue;

                using var serialized = new SerializedObject(obj);
                var iterator = serialized.GetIterator();
                if (!iterator.Next(enterChildren: true)) continue;

                do
                {
                    if (iterator.propertyType != SerializedPropertyType.ManagedReference) continue;

                    var fieldType = SerializeReferenceHelpers.GetFieldType(iterator);
                    if (fieldType is null || fieldType == typeof(object)) continue;

                    long rid;
                    if (iterator.managedReferenceValue is not null)
                        rid = iterator.managedReferenceId;
                    else if (!SerializeReferenceYamlEditor.TryReadReferenceId(assetPath, fileId, iterator.propertyPath, out rid))
                        continue;

                    map[(fileId, rid)] = fieldType;
                }
                while (iterator.Next(enterChildren: true));
            }

            return map;
        }
    }
}
