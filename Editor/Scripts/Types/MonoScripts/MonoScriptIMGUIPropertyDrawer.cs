using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class MonoScriptIMGUIPropertyDrawer
    {
        internal static float GetHeight(SerializedProperty wrapperProperty)
        {
            var nameProperty = SerializableTypeUtility.GetBackingProperty(wrapperProperty);
            var height = EditorGUIUtility.singleLineHeight;

            if (TypeMissingRepair.IsMissingMonoScript(wrapperProperty: wrapperProperty) ||
                (!nameProperty.hasMultipleDifferentValues && TypeSelectorRequiredGate.IsViolation(nameProperty)))
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

            return height;
        }

        internal static void Draw(
            Rect position,
            GUIContent label,
            SerializedProperty wrapperProperty,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
        {
            var isMissing = TypeMissingRepair.IsMissingMonoScript(wrapperProperty: wrapperProperty);
            var rowRect = position;
            rowRect.height = EditorGUIUtility.singleLineHeight;
            if (isMissing)
            {
                rowRect.xMin += 5f;
                TypeIMGUIPropertyDrawer.DrawMissingStripe(position: position);
            }

            var isArrayElement = wrapperProperty.propertyPath.EndsWith("]");
            var openButtonSize = isArrayElement ? rowRect.height - 2 : rowRect.height;

            var fieldRect = string.IsNullOrWhiteSpace(label.text)
                ? rowRect
                : EditorGUI.PrefixLabel(rowRect, label);

            var dropdownRect = fieldRect;
            var currentType = SerializableMonoScriptUtility.GetCurrentType(wrapperProperty, out var assemblyQualifiedName);
            var hasValidType = currentType is not null;

            if (hasValidType)
                dropdownRect.width -= openButtonSize + 1f;

            HandleDrop(dropdownRect, wrapperProperty, allow, types);

            var caption = TypeSelectorHelpers.GetTypeSelectorTitle(currentType,
                assemblyQualifiedName: TypeSelectorHelpers.GetMissingDisplayName(assemblyQualifiedName));
            var captionStyle = EditorStyles.miniPullDown;
            if (isMissing)
            {
                captionStyle = TypeIMGUIPropertyDrawer.GetMissingCaptionStyle();
                caption = TypeIMGUIPropertyDrawer.FitCaptionFromLeft(style: captionStyle, text: caption, width: dropdownRect.width);
            }

            var captionContent = new GUIContent(text: caption,
                tooltip: isMissing ? TypeIMGUIPropertyDrawer.GetMissingTooltip(storedName: assemblyQualifiedName) : null);

            if (EditorGUI.DropdownButton(position: dropdownRect, content: captionContent,
                    focusType: FocusType.Passive, style: captionStyle))
                ShowSelector(wrapperProperty: wrapperProperty, rect: dropdownRect, allow: allow, types: types,
                    currentAqn: currentType?.AssemblyQualifiedName ?? assemblyQualifiedName);

            if (hasValidType)
            {
                var openButtonRect = new Rect(dropdownRect.xMax + 1f, rowRect.y, openButtonSize, openButtonSize);
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, currentType);
            }

            var noticeRect = EditorGUI.IndentedRect(source: new Rect(rowRect.x,
                rowRect.yMax + EditorGUIUtility.standardVerticalSpacing, rowRect.width, EditorGUIUtility.singleLineHeight));

            if (isMissing)
            {
                var suggestion = TypeMissingRepair.GetSuggestion(storedName: assemblyQualifiedName, types: types, allow: allow,
                    excludeEditorOnly: TypeSelectorHelpers.IsStoredInRuntimeObject(wrapperProperty),
                    predicate: SerializableMonoScriptUtility.HasScript);

                TypeIMGUIPropertyDrawer.DrawMissingNotice(rect: noticeRect, storedName: assemblyQualifiedName,
                    suggestion: suggestion,
                    onFix: () => ShowSelector(wrapperProperty: wrapperProperty, rect: noticeRect, allow: allow,
                        types: types, currentAqn: null, repair: true),
                    onSuggestion: picked => SerializableMonoScriptUtility.Assign(
                        wrapperProperty: wrapperProperty.Persistent(), type: picked));
                return;
            }

            var nameProperty = SerializableTypeUtility.GetBackingProperty(wrapperProperty);
            if (nameProperty.hasMultipleDifferentValues || !TypeSelectorRequiredGate.IsViolation(nameProperty)) return;

            InspectorNoticeGUI.DrawRequiredNotice(noticeRect, "Required type is not set",
                "This [TypeSelector] field is marked required but has no type.");
        }

        private static void ShowSelector(SerializedProperty wrapperProperty, Rect rect, TypeAllow allow, Type[] types,
            string currentAqn, bool repair = false)
        {
            var persistent = wrapperProperty.Persistent();

            TypeSelectorWindow.Show(
                screenRect: GUIUtility.GUIToScreenRect(rect),
                filter: CreateFilter(allow: allow, types: types,
                    excludeEditorOnly: TypeSelectorHelpers.IsStoredInRuntimeObject(persistent), hideNoneOption: repair),
                currentAqn: repair ? null : currentAqn,
                onSelected: picked => SerializableMonoScriptUtility.Assign(persistent, TypeUtility.GetTypeOrNull(picked)));
        }

        internal static TypeSelectorFilter CreateFilter(
            TypeAllow allow,
            Type[] types,
            bool excludeEditorOnly,
            bool hideNoneOption = false) => new()
        {
            Types = types,
            Allow = allow,
            Predicate = SerializableMonoScriptUtility.HasScript,
            HideNoneOption = hideNoneOption,
            ExcludeEditorOnly = excludeEditorOnly,
        };

        private static void HandleDrop(Rect rect, SerializedProperty wrapperProperty, TypeAllow allow, Type[] types)
        {
            var current = Event.current;
            if (current.type is not (EventType.DragUpdated or EventType.DragPerform)) return;
            if (!rect.Contains(current.mousePosition)) return;

            if (SerializableMonoScriptUtility.TryResolveDroppedType(types, allow, out var dropped))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                if (current.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    SerializableMonoScriptUtility.Assign(wrapperProperty, dropped);
                }
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            current.Use();
        }
    }
}
