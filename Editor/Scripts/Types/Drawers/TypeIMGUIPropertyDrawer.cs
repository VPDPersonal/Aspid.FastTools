using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeIMGUIPropertyDrawer
    {
        // IconContent picks the d_ variant on the dark skin itself.
        internal const string FolderClosedIconPath = "Folder Icon";
        internal const string FolderOpenedIconPath = "FolderOpened Icon";

        private static GUIStyle _missingCaptionStyle;

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
            if (TypeMissingRepair.IsMissing(property: property) ||
                (!property.hasMultipleDifferentValues && TypeSelectorRequiredGate.IsViolation(property)))
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
            var isMissing = TypeMissingRepair.IsMissing(property: property);
            var rowRect = position;
            rowRect.height = EditorGUIUtility.singleLineHeight;
            if (isMissing)
            {
                rowRect.xMin += 5f;
                var stripeRect = EditorGUI.IndentedRect(source: position);
                EditorGUI.DrawRect(rect: new Rect(stripeRect.x, position.y + 2f, 2f, position.height - 4f),
                    color: InspectorNoticeGUI.NoticeColor);
            }

            var isArrayElement = property.propertyPath.EndsWith("]");
            var openButtonSize = isArrayElement ? rowRect.height - 2 : rowRect.height;

            var fieldRect = string.IsNullOrWhiteSpace(label.text)
                ? rowRect
                : EditorGUI.PrefixLabel(rowRect, label);

            var dropdownRect = fieldRect;
            var currentType = TypeUtility.GetTypeOrNull(property.stringValue);
            var hasValidType = currentType is not null;

            if (hasValidType)
                dropdownRect.width -= openButtonSize + 1f;

            var caption = property.hasMultipleDifferentValues
                ? "—"
                : TypeSelectorHelpers.GetTypeSelectorTitle(currentType, property.stringValue);
            var captionStyle = isMissing ? GetMissingCaptionStyle() : EditorStyles.miniPullDown;

            if (EditorGUI.DropdownButton(position: dropdownRect, content: new GUIContent(caption),
                    focusType: FocusType.Passive, style: captionStyle))
                ShowSelector(property: property, rect: dropdownRect, allow: allow, types: types);

            if (hasValidType)
            {
                var openButtonRect = new Rect(dropdownRect.xMax + 1f, rowRect.y, openButtonSize, openButtonSize);
                DrawOpenScriptButton(openButtonRect, currentType);
            }

            var noticeRect = EditorGUI.IndentedRect(source: new Rect(rowRect.x,
                rowRect.yMax + EditorGUIUtility.standardVerticalSpacing, rowRect.width, EditorGUIUtility.singleLineHeight));

            if (isMissing)
            {
                var suggestion = TypeMissingRepair.GetSuggestion(storedName: property.stringValue, types: types,
                    allow: allow, excludeEditorOnly: TypeSelectorHelpers.IsStoredInRuntimeObject(property));
                InspectorNoticeGUI.DrawNotice(rect: noticeRect, message: "Missing type", actionText: "Fix",
                    detail: TypeMissingRepair.GetDetail(storedName: property.stringValue),
                    onClick: () => ShowSelector(property: property, rect: noticeRect, allow: allow, types: types, repair: true),
                    suggestionText: suggestion is null ? null : $"→ {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion)}",
                    suggestionDetail: suggestion is null ? null : $"Replace the stored type name with {suggestion.AssemblyQualifiedName}.",
                    onSuggestion: suggestion is null ? null : () => property.Persistent().SetStringAndApply(value: suggestion.AssemblyQualifiedName));
                return;
            }

            if (property.hasMultipleDifferentValues || !TypeSelectorRequiredGate.IsViolation(property)) return;

            InspectorNoticeGUI.DrawRequiredNotice(noticeRect, "Required type is not set",
                "This [TypeSelector] field is marked required but has no type.");
        }

        private static void ShowSelector(SerializedProperty property, Rect rect, TypeAllow allow, Type[] types,
            bool repair = false)
        {
            var persistent = property.Persistent();
            TypeSelectorWindow.Show(screenRect: GUIUtility.GUIToScreenRect(rect),
                filter: new TypeSelectorFilter
                {
                    Types = types,
                    Allow = allow,
                    HideNoneOption = repair,
                    ExcludeEditorOnly = TypeSelectorHelpers.IsStoredInRuntimeObject(property),
                },
                currentAqn: repair || property.hasMultipleDifferentValues ? null : property.stringValue ?? string.Empty,
                onSelected: assemblyQualifiedName => persistent.SetStringAndApply(value: assemblyQualifiedName ?? string.Empty));
        }

        private static GUIStyle GetMissingCaptionStyle()
        {
            _missingCaptionStyle ??= new GUIStyle(other: EditorStyles.miniPullDown);
            _missingCaptionStyle.normal.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.hover.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.active.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.focused.textColor = InspectorNoticeGUI.NoticeColor;
            return _missingCaptionStyle;
        }
    }
}
