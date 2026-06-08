using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.UIElements;
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
        private Object _target;
        private Label _status;
        private VisualElement _list;

        [MenuItem("Tools/Aspid FastTools/Repair Missing References")]
        private static void Open()
        {
            var window = GetWindow<SerializeReferenceRepairWindow>();
            window.titleContent = new GUIContent("Repair References");
            window.minSize = new Vector2(440f, 220f);
            window.SetTarget(Selection.activeObject as Object);
            window.Show();
        }

        public static void Open(Object target)
        {
            var window = GetWindow<SerializeReferenceRepairWindow>();
            window.titleContent = new GUIContent("Repair References");
            window.minSize = new Vector2(440f, 220f);
            window.SetTarget(target);
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.AddStyleSheetsFromResource(AspidStyles.DefaultStyleSheet)
                .SetPadding(8f);

            var header = new VisualElement()
                .SetFlexDirection(FlexDirection.Row);

            var assetField = new ObjectField("Asset")
            {
                objectType = typeof(Object),
                allowSceneObjects = false,
                value = _target
            };
            assetField.style.flexGrow = 1f;
            assetField.RegisterValueChangedCallback(evt => SetTarget(evt.newValue));

            var rescan = new Button(Rescan) { text = "Rescan" };

            header.AddChild(assetField).AddChild(rescan);
            root.AddChild(header);

            _status = new Label().SetMarginTop(6f);
            _list = new VisualElement().SetMarginTop(4f);

            root.AddChild(_status).AddChild(_list);

            Rescan();
        }

        private void SetTarget(Object target)
        {
            _target = target;
            if (_list is not null) Rescan();
        }

        private void Rescan()
        {
            if (_list is null) return;

            _list.Clear();

            var assetPath = _target ? AssetDatabase.GetAssetPath(_target) : null;
            if (string.IsNullOrEmpty(assetPath))
            {
                _status.text = "Select a saved asset (a prefab or ScriptableObject) to scan for missing references.";
                return;
            }

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(assetPath, SerializeReferenceHelpers.StoredTypeResolves);
            if (missing.Count == 0)
            {
                _status.text = "No missing managed references in this asset.";
                return;
            }

            _status.text = missing.Count == 1
                ? "1 missing reference — pick a replacement type:"
                : $"{missing.Count} missing references — pick a replacement type for each:";

            foreach (var entry in missing)
                _list.AddChild(BuildRow(assetPath, entry));
        }

        private VisualElement BuildRow(string assetPath, MissingReferenceEntry entry)
        {
            var row = new VisualElement()
                .SetFlexDirection(FlexDirection.Row)
                .SetMarginTop(2f);

            var typeName = string.IsNullOrEmpty(entry.StoredType.Namespace)
                ? entry.StoredType.Class
                : $"{entry.StoredType.Namespace}.{entry.StoredType.Class}";

            var label = new Label($"{typeName}   (rid {entry.Rid})");
            label.style.flexGrow = 1f;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;

            Button fix = null;
            fix = new Button(() => ShowPicker(assetPath, entry, fix)) { text = "Fix" };

            return row.AddChild(label).AddChild(fix);
        }

        private void ShowPicker(string assetPath, MissingReferenceEntry entry, VisualElement anchor)
        {
            var bound = anchor.worldBound;
            var screenRect = new Rect(
                position.x + bound.xMin,
                position.y + bound.yMax,
                Mathf.Max(bound.width, 260f),
                bound.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: new[] { typeof(object) },
                currentAqn: string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName =>
                {
                    var type = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    if (type is null) return;

                    if (SerializeReferenceYamlEditor.TryRewriteType(assetPath, entry.FileId, entry.Rid, ManagedTypeName.FromType(type)))
                    {
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        Rescan();
                    }
                },
                filter: SerializeReferenceHelpers.IsAssignableManagedReference,
                additionalTypes: null,
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument);
        }
    }
}
