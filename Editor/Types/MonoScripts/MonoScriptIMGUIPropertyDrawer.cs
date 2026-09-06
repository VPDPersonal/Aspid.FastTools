using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // IMGUI rendering of a SerializableMonoScript / SerializableMonoScript<T> wrapper: the same dropdown and
    // open-script button as TypeIMGUIPropertyDrawer, but the candidate set is limited to types backed by a script asset,
    // a pick writes both the script reference and the type name, and a MonoScript can be dropped onto the row.
    internal static class MonoScriptIMGUIPropertyDrawer
    {
        internal static float GetHeight(SerializedProperty wrapperProperty) =>
            TypeIMGUIPropertyDrawer.GetHeight(SerializableTypeUtility.GetBackingProperty(wrapperProperty));

        internal static void Draw(
            Rect position,
            GUIContent label,
            SerializedProperty wrapperProperty,
            TypeAllow allow = TypeAllow.All,
            params Type[] types)
        {
            var rowRect = position;
            rowRect.height = EditorGUIUtility.singleLineHeight;

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

            var caption = TypeSelectorHelpers.GetTypeSelectorTitle(currentType, assemblyQualifiedName);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption), FocusType.Passive))
            {
                var persistent = wrapperProperty.Persistent();

                TypeSelectorWindow.Show(
                    screenRect: GUIUtility.GUIToScreenRect(dropdownRect),
                    filter: CreateFilter(allow, types),
                    currentAqn: currentType?.AssemblyQualifiedName ?? assemblyQualifiedName,
                    onSelected: picked => SerializableMonoScriptUtility.Assign(persistent, TypeUtility.GetTypeOrNull(picked)));
            }

            if (hasValidType)
            {
                var openButtonRect = new Rect(dropdownRect.xMax + 1f, rowRect.y, openButtonSize, openButtonSize);
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, currentType);
            }

            var nameProperty = SerializableTypeUtility.GetBackingProperty(wrapperProperty);
            if (!TypeSelectorRequiredGate.IsViolation(nameProperty)) return;

            var noticeRect = new Rect(position.x, rowRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width, EditorGUIUtility.singleLineHeight);

            InspectorNoticeGUI.DrawRequiredNotice(noticeRect, "Required type is not set",
                "This [TypeSelector] field is marked required but has no type.");
        }

        internal static TypeSelectorFilter CreateFilter(TypeAllow allow, Type[] types) => new()
        {
            Types = types,
            Allow = allow,
            Predicate = SerializableMonoScriptUtility.HasScript,
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
