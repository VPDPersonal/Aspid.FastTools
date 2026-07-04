using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
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
                var filter = new TypeSelectorFilter
                {
                    Types = new[] { fieldInfo.DeclaringType },
                };

                TypeSelectorWindow.Show(
                    GUIUtility.GUIToScreenRect(dropdownRect),
                    filter,
                    currentType.AssemblyQualifiedName,
                    onSelected: aqn => ReplaceComponentScript(
                        persistent,
                        currentType,
                        string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, throwOnError: false)));
            }

            TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, currentType);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var currentType = property.serializedObject.targetObject.GetType();

            // The callback fires once the element may have outlived its source editor — closing over the
            // drawer-time property would read a disposed SerializedObject (the Persistent() contract).
            var persistent = property.Persistent();

            var field = new InspectorTypeField(label: null, defaultValue: currentType)
            {
                Types = new[] { fieldInfo.DeclaringType },
            };

            field.RegisterValueChangedCallback(evt =>
                ReplaceComponentScript(persistent, currentType, evt.newValue));

            return field;
        }

        /// <param name="property">Must own its <see cref="SerializedObject"/> (see <c>Persistent()</c>) —
        /// it is read one <see cref="EditorApplication.delayCall"/> tick later.</param>
        private static void ReplaceComponentScript(SerializedProperty property, Type oldType, Type newType)
        {
            if (newType is null || newType == oldType) return;

            var script = newType.FindMonoScript();

            if (script is null)
            {
                Debug.LogWarning($"[ComponentTypeSelector] MonoScript not found for type: {newType.AssemblyQualifiedName}");
                return;
            }

            EditorApplication.delayCall += () =>
                property.serializedObject.FindProperty("m_Script").SetObjectReferenceAndApply(script);
        }
    }
}
