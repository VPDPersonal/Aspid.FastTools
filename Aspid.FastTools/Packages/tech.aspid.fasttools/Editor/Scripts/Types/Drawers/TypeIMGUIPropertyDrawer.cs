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

        private static readonly GUIContent _measureContent = new();

        // Re-tinted on every use so the cached style survives editor-theme changes.
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
                DrawMissingStripe(position: position);
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
                : TypeSelectorHelpers.GetTypeSelectorTitle(currentType,
                    assemblyQualifiedName: TypeSelectorHelpers.GetMissingDisplayName(property.stringValue));
            var captionStyle = EditorStyles.miniPullDown;
            if (isMissing)
            {
                captionStyle = GetMissingCaptionStyle();
                caption = FitCaptionFromLeft(style: captionStyle, text: caption, width: dropdownRect.width);
            }

            var captionContent = new GUIContent(text: caption,
                tooltip: isMissing ? GetMissingTooltip(storedName: property.stringValue) : null);

            if (EditorGUI.DropdownButton(position: dropdownRect, content: captionContent,
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
                DrawMissingNotice(rect: noticeRect, storedName: property.stringValue, suggestion: suggestion,
                    onFix: () => ShowSelector(property: property, rect: noticeRect, allow: allow, types: types, repair: true),
                    onSuggestion: picked => property.Persistent().SetStringAndApply(value: picked.AssemblyQualifiedName));
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
                    AdditionalTypes = GenericTypeResolver.GetClosableGenericDefinitions(types),
                    HideNoneOption = repair,
                    ExcludeEditorOnly = TypeSelectorHelpers.IsStoredInRuntimeObject(property),
                },
                currentAqn: repair || property.hasMultipleDifferentValues ? null : property.stringValue ?? string.Empty,
                onSelected: assemblyQualifiedName => persistent.SetStringAndApply(value: assemblyQualifiedName ?? string.Empty));
        }

        // The warning stripe on the left edge of a field whose stored type is missing.
        internal static void DrawMissingStripe(Rect position)
        {
            var stripeRect = EditorGUI.IndentedRect(source: position);
            EditorGUI.DrawRect(rect: new Rect(stripeRect.x, position.y + 2f, 2f, position.height - 4f),
                color: InspectorNoticeGUI.NoticeColor);
        }

        // Fix and the suggestion repair through the caller, which knows how its field stores a type.
        internal static void DrawMissingNotice(Rect rect, string storedName, Type suggestion, Action onFix,
            Action<Type> onSuggestion)
        {
            InspectorNoticeGUI.DrawNotice(rect: rect, message: "Missing type", actionText: "Fix",
                detail: TypeMissingRepair.GetDetail(storedName: storedName), onClick: onFix,
                suggestionText: suggestion is null ? null : $"→ {TypeSelectorHelpers.GetTypeSelectorTitle(suggestion)}",
                suggestionDetail: suggestion is null ? null : $"Replace the stored type name with {suggestion.AssemblyQualifiedName}.",
                onSuggestion: suggestion is null ? null : () => onSuggestion(suggestion));
        }

        internal static string GetMissingTooltip(string storedName) => $"Missing type: {storedName}";

        internal static GUIStyle GetMissingCaptionStyle()
        {
            _missingCaptionStyle ??= new GUIStyle(other: EditorStyles.miniPullDown);
            _missingCaptionStyle.normal.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.hover.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.active.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.focused.textColor = InspectorNoticeGUI.NoticeColor;
            return _missingCaptionStyle;
        }

        // IMGUI clips at the right edge, cutting the end of the name, so drop leading characters behind an ellipsis
        // instead: binary-searched for the smallest count that fits.
        internal static string FitCaptionFromLeft(GUIStyle style, string text, float width)
        {
            _measureContent.text = text;
            if (style.CalcSize(_measureContent).x <= width) return text;

            int low = 1, high = text.Length;
            while (low < high)
            {
                var mid = (low + high) / 2;
                _measureContent.text = "…" + text.Substring(mid);

                if (style.CalcSize(_measureContent).x <= width) high = mid;
                else low = mid + 1;
            }

            return "…" + text.Substring(low);
        }
    }
}
