using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    [CustomPropertyDrawer(typeof(ComponentTypeSelector))]
    internal sealed class ComponentTypeSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currentType = property.serializedObject.targetObject.GetType();
            var rowHeight = EditorGUIUtility.singleLineHeight;

            var dropdownRect = new Rect(position.x, position.y, position.width - rowHeight - 2f, rowHeight);
            var openButtonRect = new Rect(dropdownRect.xMax + 2f, position.y, rowHeight, rowHeight);

            if (EditorGUI.DropdownButton(dropdownRect,
                    new GUIContent(TypeSelectorHelpers.GetTypeSelectorTitle(currentType)), FocusType.Passive))
            {
                var persistent = property.Persistent();

                TypeSelectorWindow.Show(
                    GUIUtility.GUIToScreenRect(dropdownRect),
                    CreateFilter(),
                    currentType.AssemblyQualifiedName,
                    onSelected: aqn => ReplaceComponentScript(persistent, currentType, TypeUtility.GetTypeOrNull(aqn)));
            }

            TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, currentType);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var currentType = property.serializedObject.targetObject.GetType();
            var persistent = property.Persistent();
            var filter = CreateFilter();

            var field = new InspectorTypeField(label: null, defaultValue: currentType)
            {
                Types = filter.Types,
                Allow = filter.Allow,
                HideNoneOption = filter.HideNoneOption,
            };

            field.RegisterValueChangedCallback(evt =>
            {
                // The swap is applied on the next editor tick (see ReplaceComponentScript); until then — and for good
                // when no script is found — the caption must keep showing the type the object actually is.
                if (!ReplaceComponentScript(persistent, currentType, evt.newValue))
                    field.SetValueWithoutNotify(currentType);
            });

            field.RegisterCallback<AttachToPanelEvent>(_ => HideScriptField(field));

            return field;
        }

        // Only concrete subtypes of the class declaring the marker can back a script asset, and "no script" is not a
        // state a component can be in, so the <None> row is left out.
        private TypeSelectorFilter CreateFilter() => new()
        {
            Types = new[] { fieldInfo.DeclaringType },
            Allow = TypeAllow.None,
            HideNoneOption = true,
        };

        private static void HideScriptField(VisualElement field)
        {
            var inspector = field.GetFirstAncestorOfType<InspectorElement>();
            if (inspector is null) return;

            inspector.Query<PropertyField>()
                .Where(propertyField => propertyField.bindingPath == "m_Script")
                .ForEach(propertyField => propertyField.style.display = DisplayStyle.None);
        }

        // Returns whether a swap was scheduled. A null or unchanged type is a no-op; a type without a script of its own
        // is refused with a warning.
        private static bool ReplaceComponentScript(SerializedProperty property, Type oldType, Type newType)
        {
            if (newType is null || newType == oldType) return false;

            var script = newType.FindMonoScript();

            // FindMonoScript answers with the file a type is DECLARED in, which for a nested type is the declaring
            // type's file. m_Script must name the script whose own class is the component, so a script reporting a
            // different class is a miss: writing it would silently swap the component for another class.
            if (script is null || script.GetClass() != newType)
            {
                Debug.LogWarning($"[ComponentTypeSelector] MonoScript not found for type: {newType.AssemblyQualifiedName}");
                return false;
            }

            // Deferred: the swap rebuilds the inspector, which must not happen from inside the current GUI/event pass.
            EditorApplication.delayCall += () =>
                property.serializedObject.FindProperty("m_Script").SetObjectReferenceAndApply(script);

            return true;
        }
    }
}
