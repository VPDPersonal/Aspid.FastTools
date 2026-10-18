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
            var nameProperty = SerializableTypeUtility.GetBackingProperty(wrapperProperty);
            var isRequired = !isMissing && !nameProperty.hasMultipleDifferentValues &&
                TypeSelectorRequiredGate.IsViolation(nameProperty);
            var rowRect = position;
            rowRect.height = EditorGUIUtility.singleLineHeight;
            if (isMissing || isRequired) TypeIMGUIPropertyDrawer.DrawWarningStripe(position: position);

            var isArrayElement = wrapperProperty.propertyPath.EndsWith("]");
            var openButtonSize = isArrayElement ? rowRect.height - 2 : rowRect.height;

            var fieldRect = string.IsNullOrWhiteSpace(label.text)
                ? rowRect
                : EditorGUI.PrefixLabel(rowRect, label);

            var dropdownRect = fieldRect;
            var currentType = SerializableMonoScriptUtility.GetCurrentType(wrapperProperty, out var assemblyQualifiedName);
            var isMixed = SerializableMonoScriptUtility.HasMultipleDifferentValues(wrapperProperty: wrapperProperty);
            var hasValidType = currentType is not null && !isMixed;

            if (hasValidType)
                dropdownRect.width -= openButtonSize + 1f;

            HandleDrop(dropdownRect, wrapperProperty, allow, types);

            var caption = isMixed
                ? "—"
                : TypeSelectorHelpers.GetTypeSelectorTitle(currentType,
                    assemblyQualifiedName: TypeSelectorHelpers.GetMissingDisplayName(assemblyQualifiedName));
            var captionStyle = EditorStyles.miniPullDown;
            if (isMissing)
            {
                captionStyle = TypeIMGUIPropertyDrawer.GetMissingCaptionStyle();
                caption = TypeIMGUIPropertyDrawer.FitCaptionFromLeft(style: captionStyle, text: caption, width: dropdownRect.width);
            }

            var captionContent = new GUIContent(text: caption,
                image: hasValidType ? TypeIMGUIPropertyDrawer.GetCaptionIcon(type: currentType) : null,
                tooltip: isMissing ? TypeIMGUIPropertyDrawer.GetMissingTooltip(storedName: assemblyQualifiedName) : null);

            if (TypeIMGUIPropertyDrawer.DrawDropdownButton(rect: dropdownRect, content: captionContent, style: captionStyle))
                ShowSelector(wrapperProperty: wrapperProperty, rect: dropdownRect, allow: allow, types: types,
                    currentAqn: isMixed ? null : currentType?.AssemblyQualifiedName ?? assemblyQualifiedName);

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
                        detached: new DetachedProperty(property: wrapperProperty), type: picked));
                return;
            }

            if (!isRequired) return;

            InspectorNoticeGUI.DrawRequiredNotice(noticeRect, "Required type is not set",
                "This [TypeSelector] field is marked required but has no type.");
        }

        private static void ShowSelector(SerializedProperty wrapperProperty, Rect rect, TypeAllow allow, Type[] types,
            string currentAqn, bool repair = false)
        {
            // The picker answers after this draw, and a cancelled one never answers: no copy is open until then.
            var detached = new DetachedProperty(property: wrapperProperty);

            TypeSelectorWindow.Show(
                screenRect: GUIUtility.GUIToScreenRect(rect),
                filter: CreateFilter(allow: allow, types: types,
                    excludeEditorOnly: TypeSelectorHelpers.IsStoredInRuntimeObject(property: wrapperProperty), hideNoneOption: repair),
                assemblyQualifiedName: repair ? null : currentAqn,
                onSelected: picked => SerializableMonoScriptUtility.Assign(
                    detached: detached, type: TypeUtility.GetTypeOrNull(assemblyQualifiedName: picked)));
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
            if (!GUI.enabled) return;

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
