using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using UnityEditor.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class SerializableTypeDrawer
    {
        private const string OpenButtonIconPath = "Icons/open_button_icon";
        
        internal static void DrawIMGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            Type[] types,
            TypeAllow allow)
        {
            var openButtonWidth = position.height;

            if (!string.IsNullOrWhiteSpace(label.text))
            {
                EditorGUI.LabelField(position, label);
                position.x += EditorGUIUtility.labelWidth;
                position.width -= EditorGUIUtility.labelWidth;
            }

            var dropdownRect = position;
            var currentType = GetType(property.stringValue);
            var hasValidType = currentType is not null;
            
            if (hasValidType)
                dropdownRect.width -= openButtonWidth + 2f;
                
            var caption = GetCaption(property.stringValue);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption), FocusType.Passive))
            {
                var current = property.stringValue ?? string.Empty;
                var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(dropdownRect.x, dropdownRect.y));
                var screenRect = new Rect(screenPosition.x, screenPosition.y, dropdownRect.width, dropdownRect.height);
                
                TypeSelectorWindow.Show(
                    screenRect: screenRect,
                    types: types,
                    currentAqn: current,
                    allow: allow,
                    onSelected: assemblyQualifiedName =>
                    {
                        property.SetStringAndApply(assemblyQualifiedName ?? string.Empty);
                    });
            }

            if (!hasValidType) return;
            
            var openButtonRect = new Rect(dropdownRect.xMax + 2f, position.y, openButtonWidth, position.height);
            if (GUI.Button(openButtonRect, new GUIContent(Resources.Load<Texture2D>(OpenButtonIconPath))))
                OpenScript(currentType);
        }
        
        internal static VisualElement DrawUIToolkit(
            SerializedProperty property,
            string label,
            Type[] types,
            TypeAllow allow)
        {
            var field = new TypeSelectorField(string.IsNullOrEmpty(label) ? null : label, property) 
                {
                    Types = types,
                    Allow = allow, 
                }
                .AddClass(PropertyField.ussClassName)
                .AddClass(TypeSelectorField.alignedFieldUssClassName);

            field.labelElement.AddClass(PropertyField.labelUssClassName);
            return field;
        }

        private static void OpenScript(Type type)
        {
            var (monoScript, lineNumber) = type.FindMonoScriptWithLine();

            if (monoScript is not null)
                AssetDatabase.OpenAsset(monoScript, lineNumber);
        }

        private static string GetCaption(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName))
                return Constants.NoneOption;

            var type = GetType(assemblyQualifiedName);
            return type is null ? Constants.MissingOption : type.Name;
        }

        private static Type GetType(string assemblyQualifiedName) =>
            Type.GetType(assemblyQualifiedName, throwOnError: false);
    }
}
