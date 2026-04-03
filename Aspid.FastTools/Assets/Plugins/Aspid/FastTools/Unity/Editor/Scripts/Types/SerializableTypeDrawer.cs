using System;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class SerializableTypeDrawer
    {
        private const string NoneOption = "<None>";
        private const string MissingOption = "<Missing>";
        
        private const string OpenButtonText = "Open";

        private const string StyleSheetPath = "Styles/Aspid-FastTools-SerializableType";
        private const string RootClass = "aspid-fasttools-serializable-type";
        private const string ButtonsClass = "aspid-fasttools-serializable-type-buttons";
        private const string OpenButtonClass = "aspid-fasttools-serializable-type-open-button";
        
        internal static void DrawIMGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label,
            Type[] types,
            bool allowAbstract,
            bool allowInterfaces)
        {
            const float openButtonWidth = 50f;
            
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
                    allowAbstract: allowAbstract,
                    allowInterface: allowInterfaces,
                    onSelected: assemblyQualifiedName =>
                    {
                        property.SetStringAndApply(assemblyQualifiedName ?? string.Empty);
                    });
            }

            if (!hasValidType) return;
            
            var openButtonRect = new Rect(dropdownRect.xMax + 2f, position.y, openButtonWidth, position.height);
            if (GUI.Button(openButtonRect, OpenButtonText))
                OpenScript(currentType);
        }
        
        internal static VisualElement DrawUIToolkit(
            SerializedProperty property,
            string label,
            Type[] types,
            bool allowAbstract,
            bool allowInterfaces)
        {
            var typeSelector = new VisualElement()
                .AddClass(RootClass)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddChild(new PropertyField(property).SetDisplay(DisplayStyle.None));

            var button = new Button()
                .SetText(GetCaption(property.stringValue))
                .SetTooltip(GetTooltip(property.stringValue));

            var propertyPath = property.propertyPath;
            var serializedObject = property.serializedObject;

            var openButton = new Button()
                .SetText(OpenButtonText)
                .SetDisplay(DisplayStyle.None)
                .AddClass(OpenButtonClass);

            button.clicked += () =>
            {
                var window = EditorWindow.focusedWindow;
                var worldBound = button.worldBound;
                var screenRect = new Rect(window.position.x + worldBound.xMin, window.position.y + worldBound.yMin, worldBound.width, worldBound.height);

                var current = serializedObject.FindProperty(propertyPath).stringValue ?? string.Empty;

                TypeSelectorWindow.Show(
                    screenRect: screenRect,
                    types: types,
                    currentAqn: current,
                    allowAbstract: allowAbstract,
                    allowInterface: allowInterfaces,
                    onSelected: assemblyQualifiedName =>
                    {
                        var currentProperty = serializedObject.FindProperty(propertyPath);
                        currentProperty.SetStringAndApply(assemblyQualifiedName ?? string.Empty);

                        button
                            .SetText(GetCaption(currentProperty.stringValue))
                            .SetTooltip(GetTooltip(currentProperty.stringValue));

                        UpdateOpenButtonVisibility(openButton, currentProperty.stringValue);
                    });
            };
            
            openButton.clicked += () =>
            {
                var currentProperty = serializedObject.FindProperty(propertyPath);
                var currentType = GetType(currentProperty.stringValue);
                
                if (currentType is not null)
                    OpenScript(currentType);
            };

            if (!string.IsNullOrEmpty(label))
            {
                typeSelector.AddChild(new Label(label));
            }

            var buttons = new VisualElement()
                .AddChild(button)
                .AddChild(openButton)
                .AddClass(ButtonsClass);
            
            UpdateOpenButtonVisibility(openButton, property.stringValue);
            return typeSelector.AddChild(buttons);
        }
        
        private static void UpdateOpenButtonVisibility(Button openButton, string assemblyQualifiedName)
        {
            var hasValidType = !string.IsNullOrWhiteSpace(assemblyQualifiedName) && GetType(assemblyQualifiedName) is not null;
            openButton.SetDisplay(hasValidType ? DisplayStyle.Flex : DisplayStyle.None);
        }
        
        private static void OpenScript(Type type)
        {
            var (monoScript, lineNumber) = type.FindMonoScriptWithLine();
            
            if (monoScript is not null)
                AssetDatabase.OpenAsset(monoScript, lineNumber);
        }

        private static string GetTooltip(string assemblyQualifiedName)
        {
            // TODO Aspid.FastTools – Add Tooltip for missing types
            var type = GetType(assemblyQualifiedName);
            return type is null ? string.Empty : type.FullName;
        }
        
        private static string GetCaption(string assemblyQualifiedName)
        {
            if (string.IsNullOrEmpty(assemblyQualifiedName)) return NoneOption;
            
            var type = GetType(assemblyQualifiedName);
            return type is null ? MissingOption : type.Name;
        }
        
        private static Type GetType(string assemblyQualifiedName) =>
            Type.GetType(assemblyQualifiedName, throwOnError: false);
    }
}