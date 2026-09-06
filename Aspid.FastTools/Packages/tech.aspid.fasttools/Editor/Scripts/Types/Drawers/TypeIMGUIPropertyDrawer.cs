using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using Aspid.FastTools.SerializeReferences.Editors;

using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeIMGUIPropertyDrawer
    {
        private const string FolderClosedIconPath = "d_Folder Icon";
        private const string FolderOpenedIconPath = "d_FolderOpened Icon";

        internal static void DrawOpenScriptButton(Rect rect, Type type)
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

        internal static float GetHeight(SerializedProperty property)
        {
            var height = EditorGUIUtility.singleLineHeight;
            if (TypeSelectorRequiredGate.IsViolation(property))
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
            var rowRect = position;
            rowRect.height = EditorGUIUtility.singleLineHeight;

            var isArrayElement = property.propertyPath.EndsWith("]");
            var openButtonSize = isArrayElement ? rowRect.height - 2 : rowRect.height;

            // PrefixLabel honors the indent level and hands back the value column, exactly like a built-in field.
            var fieldRect = string.IsNullOrWhiteSpace(label.text)
                ? rowRect
                : EditorGUI.PrefixLabel(rowRect, label);

            var dropdownRect = fieldRect;
            var currentType = TypeUtility.GetTypeOrNull(property.stringValue);
            var hasValidType = currentType is not null;

            if (hasValidType)
                dropdownRect.width -= openButtonSize + 1f;

            var caption = TypeSelectorHelpers.GetTypeSelectorTitle(currentType, property.stringValue);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption), FocusType.Passive))
            {
                var persistent = property.Persistent();

                var filter = new TypeSelectorFilter
                {
                    Types = types,
                    Allow = allow,
                };

                TypeSelectorWindow.Show(
                    screenRect: GUIUtility.GUIToScreenRect(dropdownRect),
                    filter: filter,
                    currentAqn: property.stringValue ?? string.Empty,
                    onSelected: assemblyQualifiedName => persistent.SetStringAndApply(assemblyQualifiedName ?? string.Empty));
            }

            if (hasValidType)
            {
                var openButtonRect = new Rect(dropdownRect.xMax + 1f, rowRect.y, openButtonSize, openButtonSize);
                DrawOpenScriptButton(openButtonRect, currentType);
            }

            if (!TypeSelectorRequiredGate.IsViolation(property)) return;

            const string message = "Required type is not set";
            var noticeRect = new Rect(position.x, rowRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);

            InspectorNoticeGUI.DrawRequiredNotice(noticeRect, message,
                "This [TypeSelector] field is marked required but has no type.");
        }
    }
}
