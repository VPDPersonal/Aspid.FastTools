using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceIMGUIPropertyDrawer
    {
        private static readonly GUIContent _measureContent = new();

        // Re-tinted on every use so the cached style survives editor-theme changes.
        private static GUIStyle _missingCaptionStyle;

        // Bold twin of the foldout style for a field overridden on a prefab instance; rebuilt on a skin change so its
        // arrow textures follow the theme.
        private static GUIStyle _overriddenFoldoutStyle;
        private static bool _overriddenFoldoutStylePro;

        // Space the foldout arrow reserves left of the label; notices pull back by it to line up under the arrow.
        private const float FoldoutArrowIndent = 11f;

        // Left status stripe: the gutter shifts the label of a field with an arrow right to clear room for the bar (the
        // dropdown and the values of the children keep their column), the offset places the bar in the arrow's spot
        // measured from the indented field (so the gap is depth-independent), and the inset keeps adjacent full-height
        // stripes from merging.
        private const float StripeGutter = 5f;
        private const float StripeWidth = 2f;
        private const float StripeOffset = 11f;
        private const float StripeInsetY = 2f;

        static SerializeReferenceIMGUIPropertyDrawer() { }

        public static float GetHeight(SerializedProperty property) => GetHeight(property, depth: 0);

        // depth counts the managed-reference levels between this row and the field the drawer was invoked on; see
        // SerializeReferenceNesting.
        internal static float GetHeight(SerializedProperty property, int depth)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUIUtility.singleLineHeight;

            if (SerializeReferenceHelpers.HasMixedTypes(property))
                return height + spacing + EditorGUIUtility.singleLineHeight;

            if (SerializeReferenceHelpers.NoticesApply(property))
            {
                if (SerializeReferenceHelpers.IsMissingType(property))
                    height += spacing + EditorGUIUtility.singleLineHeight;

                if (SerializeReferenceHelpers.HasSharedReference(property))
                    height += spacing + EditorGUIUtility.singleLineHeight;

                if (TypeSelectorRequiredGate.IsViolation(property))
                    height += spacing + EditorGUIUtility.singleLineHeight;
            }

            if (property.managedReferenceValue is not null && property.isExpanded)
                height += GetChildrenHeight(property, spacing, depth);

            return height;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, params Type[] baseTypes) =>
            Draw(position, label, property, depth: 0, baseTypes);

        internal static void Draw(Rect position, GUIContent label, SerializedProperty property, int depth, Type[] baseTypes)
        {
            SerializeReferenceDuplicateGuard.Observe(property);

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var mixedTypes = SerializeReferenceHelpers.HasMixedTypes(property);
            var currentType = SerializeReferenceHelpers.GetCurrentType(property);
            var hasValue = currentType is not null && !mixedTypes;
            var fieldType = SerializeReferenceHelpers.GetFieldType(property);

            var noticesApply = !mixedTypes && SerializeReferenceHelpers.NoticesApply(property);
            var showMissing = noticesApply && SerializeReferenceHelpers.IsMissingType(property);
            var showShared = noticesApply && SerializeReferenceHelpers.HasSharedReference(property);
            var showRequired = noticesApply && TypeSelectorRequiredGate.IsViolation(property);

            // The 1-based badge number (0 when not shared) drives both the stripe color and the notice, so a badge's
            // color tracks its number instead of a rid hash that could alias two groups onto one hue.
            var sharedIndex = showShared ? SerializeReferenceHelpers.GetSharedReferenceIndex(property) : 0;

            // A missing or required field is empty and has no arrow, so its stripe takes the arrow's spot and its label
            // stays where a filled field puts it. Only a shared one needs the gutter, and only while stripes are on.
            var showStripe = FieldStripeSettings.Enabled;
            var gutter = showShared && showStripe ? StripeGutter : 0f;
            var body = new Rect(position.x + gutter, position.y, position.width - gutter, position.height);

            var line = new Rect(body.x, body.y, body.width, EditorGUIUtility.singleLineHeight);

            var contextEvent = Event.current;
            if (contextEvent.type == EventType.ContextClick && line.Contains(contextEvent.mousePosition))
            {
                ShowContextMenu(property, fieldType, baseTypes);
                contextEvent.Use();
            }

            // A disabled field takes no drop, as Unity's own fields.
            if (GUI.enabled &&
                (contextEvent.type == EventType.DragUpdated || contextEvent.type == EventType.DragPerform) &&
                line.Contains(contextEvent.mousePosition))
            {
                if (SerializeReferenceDropHandler.TryResolveDroppedType(property, fieldType, baseTypes, out var droppedType))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    if (contextEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        SerializeReferenceDropHandler.Assign(property, droppedType);
                        contextEvent.Use();
                        return; // re-layout on the next repaint with the new value
                    }
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }

            var expandable = hasValue && SerializeReferenceNesting.HasVisibleChildren(property);

            // Unity draws a prefab instance override in bold; its own fields do it, this header has to do it by hand.
            var overridden = SerializeReferencePrefabOverride.IsOverridden(property);

            var labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth - gutter, line.height);
            if (expandable)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, toggleOnLabelClick: true,
                    overridden ? GetOverriddenFoldoutStyle() : EditorStyles.foldout);
            }
            else
            {
                EditorGUI.LabelField(labelRect, label, overridden ? EditorStyles.boldLabel : EditorStyles.label);
            }

            // The gutter moves only the label: the dropdown starts where the dropdowns of the fields around start, as in
            // the UI Toolkit field.
            var dropdownX = position.x + EditorGUIUtility.labelWidth + 2f;
            var dropdownRect = new Rect(dropdownX, line.y, line.xMax - dropdownX, line.height);

            var openRect = Rect.zero;
            if (hasValue)
            {
                var openSize = line.height;
                openRect = new Rect(dropdownRect.xMax - openSize, dropdownRect.y, openSize, openSize);
                dropdownRect.width -= openSize + 1f;
            }

            // DropdownButton has no mixed-value styling, so the "—" caption stands in; the flag is still set so it
            // propagates to any nested IMGUI control.
            string missingTooltip = null;
            var caption = mixedTypes ? "—" : GetCaption(property, currentType, out missingTooltip);
            var previousMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedTypes;

            // Amber caption, trimmed from the left: IMGUI clips at the right edge, cutting the class-name tail.
            var captionStyle = EditorStyles.miniPullDown;
            if (missingTooltip is not null)
            {
                captionStyle = GetMissingCaptionStyle();
                caption = FitCaptionFromLeft(captionStyle, caption, dropdownRect.width);
            }

            var captionTooltip = mixedTypes
                ? "Mixed — the selected objects hold different types."
                : missingTooltip ?? TypeSelectorHelpers.GetTypeSelectorTooltip(currentType);

            var captionIcon = hasValue ? TypeIMGUIPropertyDrawer.GetCaptionIcon(currentType) : null;

            if (TypeIMGUIPropertyDrawer.DrawDropdownButton(dropdownRect, new GUIContent(caption, captionIcon, captionTooltip),
                    captionStyle))
            {
                ShowSelector(property, fieldType, baseTypes, mixedTypes ? null : currentType, dropdownRect);
            }

            EditorGUI.showMixedValue = previousMixed;

            if (hasValue)
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openRect, currentType);

            var y = line.yMax + spacing;

            if (mixedTypes)
            {
                var hintRect = new Rect(body.x, y, body.width, EditorGUIUtility.singleLineHeight);
                InspectorNoticeGUI.DrawInfoNotice(
                    hintRect,
                    "Different types selected",
                    "The selected objects hold different managed-reference types, so their fields cannot be shown " +
                    "together.\nPick a type from the dropdown to set it on all of them, or select a single object " +
                    "to edit its own fields.");
                return;
            }

            // GUI.Label and DrawRect ignore indentLevel, so it is applied explicitly — that keeps the offset from the
            // foldout arrow the same at every nesting depth.
            var content = EditorGUI.IndentedRect(body);

            {
                Color? stripeColor = null;
                if (showShared && sharedIndex > 0)
                    stripeColor = SerializeReferenceRidColor.ForIndex(sharedIndex);
                else if (showMissing || showRequired)
                    stripeColor = InspectorNoticeGUI.NoticeColor;

                if (showStripe && stripeColor.HasValue)
                    EditorGUI.DrawRect(
                        new Rect(EditorGUI.IndentedRect(position).x - StripeOffset, position.y + StripeInsetY,
                            StripeWidth, position.height - 2f * StripeInsetY),
                        stripeColor.Value);
            }

            if (showMissing)
            {
                var noticeRect = new Rect(content.x, y, content.width, EditorGUIUtility.singleLineHeight);
                var typeName = SerializeReferenceHelpers.GetMissingTypeDisplayName(property);
                var canFix = SerializeReferenceHelpers.TryGetRepairLocation(property, out _, out _, out _);

                // The ranking is cached per (asset, rid), so the suggestion stays cheap across per-frame repaints.
                SerializeReferenceRepairSuggestions.RepairCandidate suggestion = default;
                var hasSuggestion = canFix &&
                    SerializeReferenceHelpers.TryGetRepairSuggestion(property, baseTypes, out suggestion);

                InspectorNoticeGUI.DrawNotice(
                    noticeRect,
                    "Missing type",
                    canFix ? "Fix" : null,
                    canFix
                        ? $"Missing type: {typeName}.\nClick Fix to re-point this reference to an existing type, keeping its data."
                        : $"Missing type: {typeName}.\n{SerializeReferenceHelpers.GetMissingTypeRepairHint(property)}",
                    canFix
                        ? () =>
                        {
                            // Anchored at the notice's top: ShowAsDropDown opens below the rect, so yMax would drop
                            // the picker a line lower.
                            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(noticeRect.x, noticeRect.y));
                            var screenRect = new Rect(screenPosition.x, screenPosition.y, noticeRect.width, EditorGUIUtility.singleLineHeight);
                            SerializeReferenceHelpers.ShowFixTypeSelector(property, screenRect, null, baseTypes, detach: true);
                        }
                        : null,
                    hasSuggestion ? SerializeReferenceHelpers.GetSuggestionLabel(suggestion) : null,
                    hasSuggestion ? SerializeReferenceHelpers.GetSuggestionDetail(suggestion) : null,
                    hasSuggestion
                        ? () => new DetachedProperty(property).Use(
                            persistent => SerializeReferenceHelpers.TryFixMissingType(persistent, suggestion.Type))
                        : null);

                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (showRequired)
            {
                var noticeRect = new Rect(content.x, y, content.width, EditorGUIUtility.singleLineHeight);
                var message = "Required reference is not set";

                InspectorNoticeGUI.DrawRequiredNotice(noticeRect, message,
                    "This [SerializeReference] field is marked required but has no value.");
                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (expandable && property.isExpanded)
            {
                // The children keep the gutter in their labels only: a label column narrower by it puts their values
                // back in the column of the fields around. Without a gutter the width stays automatic.
                var labelWidth = EditorGUIUtility.labelWidth;
                if (gutter > 0f) EditorGUIUtility.labelWidth = labelWidth - gutter;

                EditorGUI.indentLevel++;
                try
                {
                    DrawChildren(property, body.x, body.width, spacing, ref y, depth);
                }
                finally
                {
                    // A child that leaves the GUI early (a picker opening) must not leave the narrower column behind.
                    EditorGUI.indentLevel--;
                    if (gutter > 0f) EditorGUIUtility.labelWidth = labelWidth;
                }
            }

            if (showShared)
            {
                // One color across notice and stripe, so aliased fields read as a group. No warning icon: this is
                // attention, not an error.
                Color? indexColor = sharedIndex > 0 ? SerializeReferenceRidColor.ForIndex(sharedIndex) : null;

                SerializeReferenceSharedNavigation.RevealIfPending(property, position);

                // Pulled left by the arrow's width so the swatch lines up under it, and widened to match so
                // "Make unique" stays right-pinned.
                var noticeRect = new Rect(content.x - FoldoutArrowIndent, y,
                    content.width + FoldoutArrowIndent, EditorGUIUtility.singleLineHeight);

                // Navigation needs the live property: expansion state is cached per SerializedObject, so the
                // ancestor isExpanded writes must go through the inspector's own.
                InspectorNoticeGUI.DrawNotice(
                    noticeRect,
                    sharedIndex > 0 ? $"Shared reference #{sharedIndex}" : "Shared reference",
                    "Make unique",
                    SerializeReferenceHelpers.BuildSharedReferenceDetail(property),
                    () => SerializeReferenceHelpers.MakeReferenceUnique(property),
                    ridColor: indexColor,
                    onMessageClick: () => SerializeReferenceSharedNavigation.NavigateFrom(property));

                // Group-navigation pulse, painted from the stripe's line so both read as one band. Its right edge is
                // the inspector's for a root-level field and the box border inside a list. A path that crosses an
                // array element without a pushed limit is a row of Unity's own ReorderableList, whose rect is inset
                // by Defaults.padding — adding that back lands the band on the box frame instead of past it.
                if (SerializeReferenceSharedNavigation.TryGetFlashAlpha(property, out var flashAlpha) &&
                    indexColor.HasValue)
                {
                    var flashColor = indexColor.Value;
                    flashColor.a = flashAlpha;
                    var flashX = content.x - StripeOffset;
                    var rowLimit = SerializeReferenceIMGUIList.CurrentElementRightLimit;
                    if (float.IsNaN(rowLimit) && property.propertyPath.Contains(".Array.data["))
                        rowLimit = position.xMax + UnityEditorInternal.ReorderableList.Defaults.padding;
                    var flashXMax = float.IsNaN(rowLimit)
                        ? Mathf.Max(position.xMax, EditorGUIUtility.currentViewWidth)
                        : rowLimit;
                    EditorGUI.DrawRect(
                        new Rect(flashX, position.y, flashXMax - flashX, position.height), flashColor);
                }
            }
        }

        private static void DrawChildren(SerializedProperty property, float x, float width, float spacing, ref float y, int depth)
        {
            foreach (var iterator in property.VisibleChildren())
            {
                var child = iterator.Copy();

                // Unity ships no picker, so PropertyField would draw a nested reference with no way to choose a type
                // and a list whose "+" appends elements nothing can fill. Use this package's own header instead.
                if (SerializeReferenceNesting.DrawsOwnHeader(child, depth))
                {
                    // Drawing the child here skips Unity's decorator drawers, so [Header] and [Space] are drawn above it.
                    var decorators = SerializeReferenceDecorators.For(child);
                    var decoratorsHeight = GetDecoratorsHeight(decorators);
                    DrawDecorators(new Rect(x, y, width, decoratorsHeight), decorators);

                    var nestedHeight = NestedHeight(child, depth);
                    var nestedRect = new Rect(x, y + decoratorsHeight, width, nestedHeight);
                    var content = new GUIContent(child.displayName, decorators.Tooltip);

                    if (child.isArray)
                    {
                        SerializeReferenceIMGUIList.Draw(nestedRect, child, content,
                            SerializeReferenceHelpers.GetArrayElementType(child), Array.Empty<Type>(), depth + 1);
                    }
                    else
                    {
                        Draw(nestedRect, content, child, depth + 1, Array.Empty<Type>());
                    }

                    y += decoratorsHeight + nestedHeight + spacing;
                    continue;
                }

                var height = EditorGUI.GetPropertyHeight(iterator, includeChildren: true);
                EditorGUI.PropertyField(new Rect(x, y, width, height), iterator, includeChildren: true);
                y += height + spacing;
            }
        }

        private static float GetChildrenHeight(SerializedProperty property, float spacing, int depth)
        {
            var height = 0f;
            foreach (var iterator in property.VisibleChildren())
                height += ChildHeight(iterator.Copy(), depth) + spacing;

            return height;
        }

        // Measured exactly as DrawChildren draws, or the reserved space and the painted rows drift apart.
        private static float ChildHeight(SerializedProperty child, int depth)
        {
            if (!SerializeReferenceNesting.DrawsOwnHeader(child, depth))
                return EditorGUI.GetPropertyHeight(child, includeChildren: true);

            return GetDecoratorsHeight(SerializeReferenceDecorators.For(child)) + NestedHeight(child, depth);
        }

        private static float NestedHeight(SerializedProperty child, int depth) =>
            child.isArray
                ? SerializeReferenceIMGUIList.GetHeight(child, new GUIContent(child.displayName),
                    SerializeReferenceHelpers.GetArrayElementType(child), Array.Empty<Type>(), depth + 1)
                : GetHeight(child, depth + 1);

        private static float GetDecoratorsHeight(SerializeReferenceDecorators decorators)
        {
            var height = 0f;

            foreach (var decorator in decorators.Items)
                height += GetDecoratorHeight(decorator);

            return height;
        }

        private static float GetDecoratorHeight(PropertyAttribute decorator)
        {
            if (decorator is SpaceAttribute space) return space.height;
            if (decorator is not HeaderAttribute header) return 0f;

            // Same as Unity's HeaderDrawer: one and a half lines, plus the height of each further line of text.
            var lines = 1;
            if (header.header is not null)
            {
                foreach (var character in header.header)
                {
                    if (character == '\n') lines++;
                }
            }

            var height = EditorGUIUtility.singleLineHeight * 1.5f;
            if (lines == 1) return height;

            _measureContent.text = header.header;
            var lineHeight = EditorStyles.boldLabel.CalcHeight(_measureContent, width: 1f) / lines;
            return height + lineHeight * (lines - 1);
        }

        private static void DrawDecorators(Rect position, SerializeReferenceDecorators decorators)
        {
            foreach (var decorator in decorators.Items)
            {
                position.height = GetDecoratorHeight(decorator);

                if (decorator is HeaderAttribute header)
                {
                    var labelRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 0.5f,
                        position.width, position.height - EditorGUIUtility.singleLineHeight * 0.5f);

                    GUI.Label(EditorGUI.IndentedRect(labelRect), header.header, EditorStyles.boldLabel);
                }

                position.y += position.height;
            }
        }

        private static void ShowSelector(SerializedProperty property, Type fieldType, Type[] baseTypes, Type currentType, Rect dropdownRect)
        {
            // The picker answers after this draw: the write goes through a SerializedObject opened only then, and a
            // copy of the inspector's property receives the expansion.
            var detached = new DetachedProperty(property);
            var view = property.Copy();
            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(dropdownRect.x, dropdownRect.y));
            var screenRect = new Rect(screenPosition.x, screenPosition.y, dropdownRect.width, dropdownRect.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                filter: SerializeReferenceHelpers.BuildPickerFilter(fieldType, baseTypes,
                    excludeEditorOnly: TypeSelectorHelpers.IsStoredInRuntimeObject(property)),
                assemblyQualifiedName: SerializeReferenceHelpers.GetSelectorCurrentAqn(property, currentType),
                onSelected: assemblyQualifiedName =>
                {
                    var type = string.IsNullOrEmpty(assemblyQualifiedName)
                        ? null
                        : Type.GetType(assemblyQualifiedName, throwOnError: false);

                    detached.Use(persistent => SerializeReferenceWriter.SetType(persistent, type, view));
                });
        }

        private static void ShowContextMenu(SerializedProperty property, Type fieldType, Type[] baseTypes)
        {
            // Each item works on its own copy, opened when the item is clicked, so a closed menu holds nothing.
            var detached = new DetachedProperty(property);
            var view = property.Copy();
            var filter = SerializeReferenceWriter.WithHoldCheck(property, SerializeReferenceHelpers.BuildAssignableFilter(baseTypes));
            var menu = new GenericMenu();

            if (SerializeReferencePrefabOverride.IsOverridden(property))
            {
                menu.AddItem(new GUIContent("Revert"), false, () =>
                {
                    detached.Use(SerializeReferencePrefabOverride.Revert);
                    SerializeReferenceHelpers.InvalidateReferenceMemos();
                });

                if (SerializeReferencePrefabOverride.TryGetApplyTarget(property, out var applyPath))
                {
                    menu.AddItem(new GUIContent(SerializeReferencePrefabOverride.GetApplyLabel(applyPath)), false, () =>
                    {
                        detached.Use(persistent => SerializeReferencePrefabOverride.Apply(persistent, applyPath));
                        SerializeReferenceHelpers.InvalidateReferenceMemos();
                    });
                }

                menu.AddSeparator(string.Empty);
            }

            // Copy reads the first target's value, Unity's convention; paste applies an independent instance per
            // target so the result is never aliased.
            menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                () => detached.Use(persistent => SerializeReferenceClipboard.Copy(persistent.managedReferenceValue)));

            var pasteLabel = new GUIContent("Paste Serialize Reference");
            if (SerializeReferenceClipboard.CanPasteInto(fieldType, filter))
                menu.AddItem(pasteLabel, false, () => detached.Use(persistent => SerializeReferenceWriter.SetValue(persistent,
                    SerializeReferenceClipboard.Type, SerializeReferenceClipboard.CreateInstance, view)));
            else
                menu.AddDisabledItem(pasteLabel);

            // A single-asset cross-reference operation, so it is only correct for a single target.
            if (SerializeReferenceHelpers.NoticesApply(property) &&
                SerializeReferenceHelpers.HasSharedReference(property))
                menu.AddItem(new GUIContent("Make Unique Reference"), false,
                    () => detached.Use(SerializeReferenceHelpers.MakeReferenceUniqueOnCopy));

            var usagesType = SerializeReferenceHelpers.GetCurrentType(property);
            if (usagesType != null)
            {
                menu.AddItem(new GUIContent(SerializeReferenceHelpers.GetFindUsagesMenuLabel(usagesType)), false,
                    () => SerializeReferenceUsageSearchProvider.OpenSearch(usagesType));
            }

            if (SerializeReferenceHelpers.NoticesApply(property))
            {
                foreach (var candidate in SerializeReferenceLinker.CollectLinkCandidates(property, filter: filter))
                {
                    var path = candidate.Path;
                    var label = SerializeReferenceHelpers.GetLinkToExistingMenuLabel(candidate.Type, path);
                    menu.AddItem(new GUIContent(label), false,
                        () => detached.Use(persistent => SerializeReferenceLinker.LinkTo(persistent, path, view)));
                }
            }

            if (fieldType != null)
            {
                menu.AddItem(new GUIContent("Create New Script…"), false, () =>
                {
                    if (!SerializeReferenceScriptCreator.TryCreateSubclassStub(fieldType, out _, out var fullTypeName)) return;

                    // One pending assignment per target — targetObject alone would leave objects 2..N untouched.
                    foreach (var target in detached.Targets)
                        SerializeReferencePendingAssignment.Enqueue(target, detached.Path, fullTypeName);
                });
            }

            if (usagesType != null)
            {
                menu.AddItem(new GUIContent("Save as Template…"), false, () => detached.Use(persistent =>
                {
                    var value = persistent.managedReferenceValue;
                    SerializeReferenceNamePrompt.Show("Save Template",
                        SerializeReferenceTemplates.SuggestName(usagesType),
                        name => SerializeReferenceTemplates.SaveConfirmed(name, value));
                }));
            }

            var hasTemplates = false;
            foreach (var template in SerializeReferenceTemplates.LoadResolved())
            {
                if (fieldType != null && !fieldType.IsAssignableFrom(template.Type)) continue;
                if (!filter(template.Type)) continue;
                var name = template.Name;
                var templateType = template.Type;
                menu.AddItem(new GUIContent($"Paste Template/{name}"), false, () => detached.Use(persistent =>
                    SerializeReferenceWriter.SetValue(persistent, templateType, () => SerializeReferenceTemplates.CreateInstance(name), view)));
                hasTemplates = true;
            }

            var missingTemplates = SerializeReferenceTemplates.UnresolvedNames().Count;
            if (missingTemplates > 0)
            {
                if (hasTemplates) menu.AddSeparator("Paste Template/");
                menu.AddItem(new GUIContent($"Paste Template/Remove Missing ({missingTemplates})…"), false,
                    SerializeReferenceTemplates.RemoveUnresolvedConfirmed);
            }

            menu.ShowAsContext();
        }

        private static string GetCaption(SerializedProperty property, Type currentType, out string missingTooltip)
        {
            missingTooltip = null;

            if (currentType is not null)
                return TypeSelectorHelpers.GetTypeSelectorTitle(currentType);

            var missingType = SerializeReferenceHelpers.IsMissingType(property)
                ? SerializeReferenceHelpers.GetMissingTypeName(property)
                : default;

            if (!missingType.IsEmpty)
                missingTooltip = $"Missing type: {missingType.FullName}";

            return TypeSelectorHelpers.GetTypeSelectorTitle(null, missingType.DisplayName);
        }

        private static GUIStyle GetOverriddenFoldoutStyle()
        {
            if (_overriddenFoldoutStyle is null || _overriddenFoldoutStylePro != EditorGUIUtility.isProSkin)
            {
                _overriddenFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                _overriddenFoldoutStylePro = EditorGUIUtility.isProSkin;
            }

            return _overriddenFoldoutStyle;
        }

        private static GUIStyle GetMissingCaptionStyle()
        {
            _missingCaptionStyle ??= new GUIStyle(EditorStyles.miniPullDown);
            _missingCaptionStyle.normal.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.hover.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.active.textColor = InspectorNoticeGUI.NoticeColor;
            _missingCaptionStyle.focused.textColor = InspectorNoticeGUI.NoticeColor;
            return _missingCaptionStyle;
        }

        // IMGUI clips at the right edge, cutting the informative class-name tail, so drop leading characters behind
        // an ellipsis instead — binary-searched for the smallest count that fits.
        private static string FitCaptionFromLeft(GUIStyle style, string text, float width)
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
