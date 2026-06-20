using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeIMGUIPropertyDrawer
    {
        private const string FolderClosedIconPath = "d_Folder Icon";
        private const string FolderOpenedIconPath = "d_FolderOpened Icon";
        
        public static void DrawOpenScriptButton(Rect rect, Type type)
        {
            var clicked = GUI.Button(rect, GUIContent.none);

            if (Event.current.type == EventType.Repaint)
            {
                var isHover = rect.Contains(Event.current.mousePosition);
                var icon = EditorGUIUtility.IconContent(isHover ? FolderOpenedIconPath : FolderClosedIconPath).image;
               
                GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit);
            }

            if (clicked) type.OpenInScriptEditor();
        }

        /// <summary>Row height: one line, plus a second line for the required-notice when the field is in violation.</summary>
        internal static float GetHeight(SerializedProperty property)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (SerializeReferenceRequiredGate.IsViolation(property))
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            return height;
        }

        internal static void Draw(
            Rect position,
            GUIContent label,
            SerializedProperty property,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
        {
            // The field is always a single line; a required-notice (when shown) sits on its own row below.
            var rowRect = position;
            rowRect.height = EditorGUIUtility.singleLineHeight;

            var isArray = property.propertyPath.EndsWith("]");
            var openButtonSize = isArray ? rowRect.height - 2 : rowRect.height;

            var fieldRect = rowRect;
            if (!string.IsNullOrWhiteSpace(label.text))
            {
                EditorGUI.LabelField(rowRect, label);
                fieldRect.x += EditorGUIUtility.labelWidth;
                fieldRect.width -= EditorGUIUtility.labelWidth;
            }

            var dropdownRect = fieldRect;
            var currentType = GetType(property.stringValue);
            var hasValidType = currentType is not null;

            if (hasValidType)
                dropdownRect.width -= openButtonSize + 1f;

            var caption = GetCaption(property.stringValue);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption), FocusType.Passive))
            {
                var persistent = property.Persistent();
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
                        persistent.SetStringAndApply(assemblyQualifiedName ?? string.Empty);
                    });
            }

            if (hasValidType)
            {
                var openButtonRect = new Rect(dropdownRect.xMax + 1f, rowRect.y, openButtonSize, openButtonSize);
                DrawOpenScriptButton(openButtonRect, currentType);
            }

            // A [TypeSelector(Required = true)] string left empty shows a non-actionable warning below the row, matching
            // the managed-reference required notice; the dropdown above is the implied fix.
            if (!SerializeReferenceRequiredGate.IsViolation(property)) return;

            SerializeReferenceRequiredGate.TryGetRequired(property, out var selector);
            var message = string.IsNullOrEmpty(selector?.RequiredMessage) ? "Required type is not set" : selector.RequiredMessage;
            var noticeRect = new Rect(position.x, rowRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);
            SerializeReferenceIMGUIPropertyDrawer.DrawRequiredNotice(noticeRect, message,
                "This [TypeSelector] field is marked required but has no type.");
        }

        private static string GetCaption(string assemblyQualifiedName)
        {
            var type = GetType(assemblyQualifiedName);
            return TypeSelectorHelpers.GetTypeSelectorTitle(type, assemblyQualifiedName);
        }

        private static Type GetType(string assemblyQualifiedName) =>
            Type.GetType(assemblyQualifiedName, throwOnError: false);
    }
}
