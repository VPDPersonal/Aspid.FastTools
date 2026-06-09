using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// IMGUI rendering for the <c>[TypeSelector]</c> drawer on a <c>[SerializeReference]</c> field: a
    /// foldout-and-dropdown header row, an optional missing-type warning, and the nested properties of the
    /// assigned instance. The optional base types narrow the candidate list below the declared field type.
    /// </summary>
    internal static class SerializeReferenceIMGUIPropertyDrawer
    {
        public static float GetHeight(SerializedProperty property)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUIUtility.singleLineHeight;

            if (SerializeReferenceHelpers.IsMissingType(property))
                height += spacing + EditorGUIUtility.singleLineHeight;

            if (SerializeReferenceHelpers.HasSharedReference(property))
                height += spacing + EditorGUIUtility.singleLineHeight;

            if (property.managedReferenceValue is not null && property.isExpanded)
                height += GetChildrenHeight(property, spacing);

            return height;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, params Type[] baseTypes)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var currentType = SerializeReferenceHelpers.GetCurrentType(property);
            var hasValue = currentType is not null;
            var fieldType = SerializeReferenceHelpers.GetFieldType(property);

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var contextEvent = Event.current;
            if (contextEvent.type == EventType.ContextClick && line.Contains(contextEvent.mousePosition))
            {
                ShowContextMenu(property, fieldType);
                contextEvent.Use();
            }

            var labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
            if (hasValue) property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, toggleOnLabelClick: true);
            else EditorGUI.LabelField(labelRect, label);

            var dropdownRect = new Rect(
                line.x + EditorGUIUtility.labelWidth + 2f,
                line.y,
                line.width - EditorGUIUtility.labelWidth - 2f,
                line.height);

            var openRect = Rect.zero;
            if (hasValue)
            {
                var openSize = line.height;
                openRect = new Rect(dropdownRect.xMax - openSize, dropdownRect.y, openSize, openSize);
                dropdownRect.width -= openSize + 1f;
            }

            var caption = GetCaption(property, currentType);
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption), FocusType.Passive))
                ShowSelector(property, fieldType, baseTypes, currentType, dropdownRect);

            if (hasValue)
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openRect, currentType);

            var y = line.yMax + spacing;

            if (SerializeReferenceHelpers.IsMissingType(property))
            {
                var noticeRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                var typeName = SerializeReferenceHelpers.GetMissingTypeDisplayName(property);
                var canFix = SerializeReferenceHelpers.TryGetRepairLocation(property, out _, out _, out _);

                DrawNotice(
                    noticeRect,
                    "Missing type —",
                    canFix ? "Fix" : null,
                    canFix
                        ? $"Missing type: {typeName}.\nClick Fix to re-point this reference to an existing type, keeping its data."
                        : $"Missing type: {typeName}.\nOpen this asset from the Project window to repair it.",
                    canFix
                        ? () =>
                        {
                            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(noticeRect.x, noticeRect.yMax));
                            var screenRect = new Rect(screenPosition.x, screenPosition.y, noticeRect.width, EditorGUIUtility.singleLineHeight);
                            SerializeReferenceHelpers.ShowFixTypeSelector(property.Persistent(), screenRect, null, baseTypes);
                        }
                        : null);

                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (SerializeReferenceHelpers.HasSharedReference(property))
            {
                var noticeRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                var persistent = property.Persistent();

                DrawNotice(
                    noticeRect,
                    "Shared reference —",
                    "Make unique",
                    "This reference is shared with another field — editing one changes both.\n" +
                    "Click Make unique to give this field its own independent copy.",
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent));

                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (!hasValue || !property.isExpanded) return;

            EditorGUI.indentLevel++;
            DrawChildren(property, position.x, position.width, spacing, ref y);
            EditorGUI.indentLevel--;
        }

        private static void DrawChildren(SerializedProperty property, float x, float width, float spacing, ref float y)
        {
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                var height = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                EditorGUI.PropertyField(new Rect(x, y, width, height), iterator, includeChildren: true);
                y += height + spacing;
            }
        }

        private static float GetChildrenHeight(SerializedProperty property, float spacing)
        {
            var height = 0f;
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                height += EditorGUI.GetPropertyHeight(iterator, includeChildren: true) + spacing;
            }

            return height;
        }

        private static void ShowSelector(SerializedProperty property, Type fieldType, Type[] baseTypes, Type currentType, Rect dropdownRect)
        {
            var persistent = property.Persistent();
            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(dropdownRect.x, dropdownRect.y));
            var screenRect = new Rect(screenPosition.x, screenPosition.y, dropdownRect.width, dropdownRect.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                types: new[] { fieldType },
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                allow: TypeAllow.None,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)),
                filter: SerializeReferenceHelpers.BuildAssignableFilter(baseTypes),
                additionalTypes: GenericTypeResolver.GetAssignableGenericDefinitions(fieldType, baseTypes),
                argumentFilter: SerializeReferenceHelpers.IsValidGenericArgument);

            return;

            void Apply(Type type)
            {
                var previous = persistent.managedReferenceValue;
                persistent.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                persistent.isExpanded = type is not null;
            }
        }

        private static void ShowContextMenu(SerializedProperty property, Type fieldType)
        {
            var persistent = property.Persistent();
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                () => SerializeReferenceClipboard.Copy(persistent.managedReferenceValue));

            var pasteLabel = new GUIContent("Paste Serialize Reference");
            if (SerializeReferenceClipboard.CanPasteInto(fieldType))
                menu.AddItem(pasteLabel, false, () => Paste(persistent));
            else
                menu.AddDisabledItem(pasteLabel);

            if (SerializeReferenceHelpers.HasSharedReference(property))
                menu.AddItem(new GUIContent("Make Unique Reference"), false,
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent));

            menu.ShowAsContext();

            void Paste(SerializedProperty target)
            {
                var value = SerializeReferenceClipboard.CreateInstance();
                target.SetManagedReferenceAndApply(value);
                target.isExpanded = value is not null;
            }
        }


        private static string GetCaption(SerializedProperty property, Type currentType)
        {
            if (currentType is not null)
                return TypeSelectorHelpers.GetTypeSelectorTitle(currentType);

            var missingName = SerializeReferenceHelpers.IsMissingType(property)
                ? SerializeReferenceHelpers.GetMissingTypeDisplayName(property)
                : null;

            return TypeSelectorHelpers.GetTypeSelectorTitle(null, missingName);
        }

        // Warning yellow mirrors the UIToolkit notice palette:
        // --aspid-colors-status-warning-text-light / -lightness.
        private static readonly Color NoticeColor = new(245f / 255f, 185f / 255f, 85f / 255f);
        private static readonly Color NoticeColorHover = new(255f / 255f, 235f / 255f, 175f / 255f);

        private static GUIStyle _messageStyle;
        private static GUIStyle _actionStyle;

        /// <summary>
        /// Draws a compact single-row warning: a small warning icon, a terse yellow message and an
        /// optional underlined, clickable action word. The full <paramref name="detail"/> rides the
        /// hover tooltip, mirroring the UIToolkit <see cref="SerializeReferenceNotice"/>.
        /// </summary>
        private static void DrawNotice(Rect rect, string message, string actionText, string detail, Action onClick)
        {
            _messageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _actionStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            _messageStyle.normal.textColor = NoticeColor;

            const float iconSize = 16f;
            var iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            GUI.Label(iconRect, EditorGUIUtility.IconContent("console.warnicon"));

            var messageContent = new GUIContent(message, detail);
            var messageWidth = _messageStyle.CalcSize(messageContent).x;
            var messageRect = new Rect(iconRect.xMax + 4f, rect.y, messageWidth, rect.height);
            GUI.Label(messageRect, messageContent, _messageStyle);

            if (string.IsNullOrEmpty(actionText) || onClick is null) return;

            var actionContent = new GUIContent(actionText, detail);
            var actionWidth = _actionStyle.CalcSize(actionContent).x;
            var actionRect = new Rect(messageRect.xMax + 4f, rect.y, actionWidth, rect.height);

            var hover = actionRect.Contains(Event.current.mousePosition);
            var actionColor = hover ? NoticeColorHover : NoticeColor;
            _actionStyle.normal.textColor = actionColor;
            _actionStyle.hover.textColor = actionColor;

            EditorGUIUtility.AddCursorRect(actionRect, MouseCursor.Link);
            var clicked = GUI.Button(actionRect, actionContent, _actionStyle);

            // Underline the action word — IMGUI styles have no text-decoration, so draw the rule manually.
            var underline = new Rect(actionRect.x, actionRect.center.y + EditorGUIUtility.singleLineHeight * 0.35f, actionWidth, 1f);
            EditorGUI.DrawRect(underline, actionColor);

            if (clicked) onClick();
        }
    }
}
