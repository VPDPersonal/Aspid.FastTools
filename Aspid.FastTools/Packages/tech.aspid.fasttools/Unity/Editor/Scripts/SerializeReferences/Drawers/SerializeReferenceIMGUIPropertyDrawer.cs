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

            // Mixed types across the selection: the per-instance child fields cannot be merged, so only the dropdown
            // and a single-line "different types" hint are drawn — never the children or the per-asset notices.
            if (SerializeReferenceHelpers.HasMixedTypes(property))
                return height + spacing + EditorGUIUtility.singleLineHeight;

            // Per-asset notices are suppressed under a multi-object selection (each reads/writes a single backing asset).
            if (SerializeReferenceHelpers.NoticesApply(property))
            {
                if (SerializeReferenceHelpers.IsMissingType(property))
                    height += spacing + EditorGUIUtility.singleLineHeight;

                if (SerializeReferenceHelpers.HasSharedReference(property))
                    height += spacing + EditorGUIUtility.singleLineHeight;
            }

            if (property.managedReferenceValue is not null && property.isExpanded)
                height += GetChildrenHeight(property, spacing);

            return height;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, params Type[] baseTypes)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var mixedTypes = SerializeReferenceHelpers.HasMixedTypes(property);
            var currentType = SerializeReferenceHelpers.GetCurrentType(property);
            var hasValue = currentType is not null && !mixedTypes;
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

            // Mixed types across the selection: show the standard "—" treatment on the dropdown and never the open-script
            // button (there is no single type to open). Picking a type still rewrites every target. The "—" caption is
            // what renders the dash here — DropdownButton has no mixed-value styling — but EditorGUI.showMixedValue is
            // still set/restored to mirror the UIToolkit side's mixed flag and to propagate to any nested IMGUI control.
            var caption = mixedTypes ? "—" : GetCaption(property, currentType);
            var previousMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedTypes;
            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption,
                    mixedTypes ? "Mixed — the selected objects hold different types." : null), FocusType.Passive))
                // Under mixed types there is no single "current" type to pre-highlight — open the picker unselected.
                ShowSelector(property, fieldType, baseTypes, mixedTypes ? null : currentType, dropdownRect);
            EditorGUI.showMixedValue = previousMixed;

            if (hasValue)
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openRect, currentType);

            var y = line.yMax + spacing;

            // Mixed types: stand in for the per-instance child fields (which cannot be merged) with a single dim info
            // line, and skip the per-asset notices entirely.
            if (mixedTypes)
            {
                var hintRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                DrawInfoNotice(
                    hintRect,
                    "Different types selected",
                    "The selected objects hold different managed-reference types, so their fields cannot be shown " +
                    "together.\nPick a type from the dropdown to set it on all of them, or select a single object " +
                    "to edit its own fields.");
                return;
            }

            // Per-asset notices read/write a single backing asset, so they are suppressed under a multi-object selection.
            var noticesApply = SerializeReferenceHelpers.NoticesApply(property);

            if (noticesApply && SerializeReferenceHelpers.IsMissingType(property))
            {
                var noticeRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                var typeName = SerializeReferenceHelpers.GetMissingTypeDisplayName(property);
                var canFix = SerializeReferenceHelpers.TryGetRepairLocation(property, out _, out _, out _);

                // The Smart Fix suggestion rides the same row as a second clickable word ("· → Pistol?"): the highest
                // ranked existing type the renamed/moved reference most likely became. The ranking is cached per
                // (asset, rid), so this stays cheap across IMGUI's per-frame repaints. The candidate is pre-declared so
                // it stays definitely assigned even when the short-circuit skips the probe (canFix == false).
                SerializeReferenceRepairSuggestions.RepairCandidate suggestion = default;
                var hasSuggestion = canFix &&
                    SerializeReferenceHelpers.TryGetRepairSuggestion(property, baseTypes, out suggestion);

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
                        : null,
                    hasSuggestion ? SerializeReferenceHelpers.GetSuggestionLabel(suggestion) : null,
                    hasSuggestion ? SerializeReferenceHelpers.GetSuggestionDetail(suggestion) : null,
                    hasSuggestion
                        ? () => SerializeReferenceHelpers.TryFixMissingType(property.Persistent(), suggestion.Type)
                        : null);

                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (noticesApply && SerializeReferenceHelpers.HasSharedReference(property))
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
                // Multi-object: each target gets its OWN instance, created from that target's previous value, so the
                // managed reference is never aliased across objects; <None> clears all. One Undo step covers them all.
                if (SerializeReferenceHelpers.IsEditingMultipleObjects(persistent))
                {
                    SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                        persistent,
                        previous => SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));

                    // All targets now share the new type, so the live foldout drives expansion; set it on the
                    // persistent property (the per-target writes went through disposed SerializedObjects).
                    persistent.isExpanded = type is not null;
                    return;
                }

                var single = persistent.managedReferenceValue;
                persistent.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, single));
                persistent.isExpanded = type is not null;
            }
        }

        private static void ShowContextMenu(SerializedProperty property, Type fieldType)
        {
            var persistent = property.Persistent();
            var menu = new GenericMenu();

            // Copy reads the first target's value (Unity's convention for a multi-selection menu). Paste then applies an
            // independent instance PER target, so the pasted reference is never aliased across objects.
            menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                () => SerializeReferenceClipboard.Copy(persistent.managedReferenceValue));

            var pasteLabel = new GUIContent("Paste Serialize Reference");
            if (SerializeReferenceClipboard.CanPasteInto(fieldType))
                menu.AddItem(pasteLabel, false, () => Paste(persistent));
            else
                menu.AddDisabledItem(pasteLabel);

            // Make-unique is a single-asset cross-reference operation; only offered (and only correct) for a single
            // target — under a multi-object selection the shared-reference notice is already suppressed.
            if (SerializeReferenceHelpers.NoticesApply(property) &&
                SerializeReferenceHelpers.HasSharedReference(property))
                menu.AddItem(new GUIContent("Make Unique Reference"), false,
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent));

            menu.ShowAsContext();

            void Paste(SerializedProperty target)
            {
                if (SerializeReferenceHelpers.IsEditingMultipleObjects(target))
                {
                    SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                        target,
                        _ => SerializeReferenceClipboard.CreateInstance());

                    // All targets now share the pasted type, so the live foldout drives expansion; set it on the
                    // persistent property (the per-target writes went through disposed SerializedObjects). A null
                    // clipboard type is an empty-reference paste, which collapses — matching the single-object branch.
                    target.isExpanded = SerializeReferenceClipboard.Type is not null;
                    return;
                }

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

        // Dim grey for the non-actionable mixed-types info hint, mirroring the UIToolkit info notice's --aspid-colors-text-dark.
        private static readonly Color InfoNoticeColor = new(150f / 255f, 150f / 255f, 150f / 255f);

        private static GUIStyle _messageStyle;
        private static GUIStyle _actionStyle;
        private static GUIStyle _infoMessageStyle;

        /// <summary>
        /// Draws a compact single-row, non-actionable info hint: a small info icon and a terse dim message. Used for the
        /// multi-object "different types" notice that stands in for the suppressed child fields, mirroring the UIToolkit
        /// <see cref="SerializeReferenceNotice"/> info variant. The full <paramref name="detail"/> rides the hover tooltip.
        /// </summary>
        private static void DrawInfoNotice(Rect rect, string message, string detail)
        {
            _infoMessageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _infoMessageStyle.normal.textColor = InfoNoticeColor;

            const float iconSize = 16f;
            var iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            GUI.Label(iconRect, EditorGUIUtility.IconContent("console.infoicon"));

            var messageContent = new GUIContent(message, detail);
            var messageRect = new Rect(iconRect.xMax + 4f, rect.y, rect.xMax - iconRect.xMax - 4f, rect.height);
            GUI.Label(messageRect, messageContent, _infoMessageStyle);
        }

        /// <summary>
        /// Draws a compact single-row warning: a small warning icon, a terse yellow message, an optional underlined,
        /// clickable action word and — for a missing-type notice with a Smart Fix candidate — an optional trailing
        /// suggestion word ("· → Pistol?"). The full <paramref name="detail"/> rides each segment's hover tooltip,
        /// mirroring the UIToolkit <see cref="SerializeReferenceNotice"/>.
        /// </summary>
        private static void DrawNotice(Rect rect, string message, string actionText, string detail, Action onClick,
            string suggestionText = null, string suggestionDetail = null, Action onSuggestion = null)
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

            var actionEnd = DrawLink(messageRect.xMax + 4f, rect, actionText, detail, onClick);

            if (!string.IsNullOrEmpty(suggestionText) && onSuggestion is not null)
                DrawLink(actionEnd + 6f, rect, suggestionText, suggestionDetail, onSuggestion);
        }

        // Draws one underlined, clickable, hover-tracking link word at x and returns its right edge, so the caller can
        // lay the next segment out after it. Shared by the Fix action and the trailing Smart Fix suggestion.
        private static float DrawLink(float x, Rect rect, string text, string detail, Action onClick)
        {
            var content = new GUIContent(text, detail);
            var width = _actionStyle.CalcSize(content).x;
            var linkRect = new Rect(x, rect.y, width, rect.height);

            var hover = linkRect.Contains(Event.current.mousePosition);
            var color = hover ? NoticeColorHover : NoticeColor;
            _actionStyle.normal.textColor = color;
            _actionStyle.hover.textColor = color;

            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);
            var clicked = GUI.Button(linkRect, content, _actionStyle);

            // Underline the word — IMGUI styles have no text-decoration, so draw the rule manually.
            var underline = new Rect(linkRect.x, linkRect.center.y + EditorGUIUtility.singleLineHeight * 0.35f, width, 1f);
            EditorGUI.DrawRect(underline, color);

            if (clicked) onClick();
            return linkRect.xMax;
        }
    }
}
