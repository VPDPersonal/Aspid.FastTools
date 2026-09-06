using System;
using UnityEditor;
using UnityEngine;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // IMGUI rendering for the [TypeSelector] drawer on a [SerializeReference] field: a foldout-and-dropdown header
    // row, the notices, and the nested properties of the assigned instance.
    internal static class SerializeReferenceIMGUIPropertyDrawer
    {
        private static readonly GUIContent _measureContent = new();

        // Re-tinted on every use so the cached style survives editor-theme changes.
        private static GUIStyle _missingCaptionStyle;

        // Space the foldout arrow reserves left of the label; notices pull back by it to line up under the arrow.
        private const float FoldoutArrowIndent = 11f;

        // Left status stripe: the gutter shifts the field body right to clear room for the bar, the offset places the
        // bar inside it measured from the indented content (so the gap is depth-independent), and the inset keeps
        // adjacent full-height stripes from merging.
        private const float StripeGutter = 5f;
        private const float StripeWidth = 2f;
        private const float StripeOffset = 16f;
        private const float StripeInsetY = 2f;

        static SerializeReferenceIMGUIPropertyDrawer() { }

        public static float GetHeight(SerializedProperty property) => GetHeight(property, depth: 0);

        // depth counts the managed-reference levels between this row and the field the drawer was invoked on; see
        // SerializeReferenceNesting.
        internal static float GetHeight(SerializedProperty property, int depth)
        {
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var height = EditorGUIUtility.singleLineHeight;

            // Per-instance child fields cannot be merged, so mixed types get the dropdown and a one-line hint only.
            if (SerializeReferenceHelpers.HasMixedTypes(property))
                return height + spacing + EditorGUIUtility.singleLineHeight;

            // Per-asset notices are suppressed under a multi-object selection.
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
            // On a rid collision the guard queues a de-alias for the next tick, never mutating mid-draw. Cheap on the
            // unchanged path, so safe to call from every repaint.
            SerializeReferenceDuplicateGuard.Observe(property);

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var mixedTypes = SerializeReferenceHelpers.HasMixedTypes(property);
            var currentType = SerializeReferenceHelpers.GetCurrentType(property);
            var hasValue = currentType is not null && !mixedTypes;
            var fieldType = SerializeReferenceHelpers.GetFieldType(property);

            // Computed up front: showing any notice decides whether the field reserves the stripe gutter.
            var noticesApply = !mixedTypes && SerializeReferenceHelpers.NoticesApply(property);
            var showMissing = noticesApply && SerializeReferenceHelpers.IsMissingType(property);
            var showShared = noticesApply && SerializeReferenceHelpers.HasSharedReference(property);
            var showRequired = noticesApply && TypeSelectorRequiredGate.IsViolation(property);

            // The 1-based badge number (0 when not shared) drives both the stripe color and the notice, so a badge's
            // color tracks its number instead of a rid hash that could alias two groups onto one hue.
            var sharedIndex = showShared ? SerializeReferenceHelpers.GetSharedReferenceIndex(property) : 0;

            // Only a field that shows a stripe reserves the gutter. Missing / required fields keep it but pull their
            // arrow-less label and notice left onto the foldout-arrow spot.
            var flat = showMissing || showRequired;
            var gutter = showMissing || showShared || showRequired ? StripeGutter : 0f;
            var body = new Rect(position.x + gutter, position.y, position.width - gutter, position.height);

            var line = new Rect(body.x, body.y, body.width, EditorGUIUtility.singleLineHeight);

            var contextEvent = Event.current;
            if (contextEvent.type == EventType.ContextClick && line.Contains(contextEvent.mousePosition))
            {
                ShowContextMenu(property, fieldType, baseTypes);
                contextEvent.Use();
            }

            // Dropping a MonoScript on the header row assigns an instance of its class (when assignable).
            if ((contextEvent.type == EventType.DragUpdated || contextEvent.type == EventType.DragPerform) &&
                line.Contains(contextEvent.mousePosition))
            {
                if (SerializeReferenceDropHandler.TryResolveDroppedType(fieldType, baseTypes, out var droppedType))
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

            // A type with no serialized fields is drawn flat: an arrow would promise content that never appears.
            var expandable = hasValue && SerializeReferenceNesting.HasVisibleChildren(property);

            var labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
            if (expandable)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, toggleOnLabelClick: true);
            }
            else
            {
                // No foldout arrow, so pull the label left onto its spot.
                var labelPull = flat ? FoldoutArrowIndent : 0f;
                EditorGUI.LabelField(new Rect(labelRect.x - labelPull, labelRect.y,
                    labelRect.width + labelPull, labelRect.height), label);
            }

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

            // The caption shows the short name, so the tooltip carries the full identity.
            var captionTooltip = mixedTypes
                ? "Mixed — the selected objects hold different types."
                : missingTooltip ?? TypeSelectorHelpers.GetTypeSelectorTooltip(currentType);

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption, captionTooltip),
                    FocusType.Passive, captionStyle))
            {
                // No single current type under mixed types, so the picker opens unselected.
                ShowSelector(property, fieldType, baseTypes, mixedTypes ? null : currentType, dropdownRect);
            }

            EditorGUI.showMixedValue = previousMixed;

            if (hasValue)
                TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openRect, currentType);

            var y = line.yMax + spacing;

            // One dim info line stands in for the child fields, which cannot be merged.
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

            // Full-height status stripe: the badge's per-index color when shared, else the warning amber.
            {
                Color? stripeColor = null;
                if (showShared && sharedIndex > 0)
                    stripeColor = SerializeReferenceRidColor.ForIndex(sharedIndex);
                else if (showMissing || showRequired)
                    stripeColor = InspectorNoticeGUI.NoticeColor;

                if (stripeColor.HasValue)
                    EditorGUI.DrawRect(
                        new Rect(content.x - StripeOffset, position.y + StripeInsetY,
                            StripeWidth, position.height - 2f * StripeInsetY),
                        stripeColor.Value);
            }

            if (showMissing)
            {
                // Flat field (no arrow): pull the notice left onto the arrow's spot so it lines up with the label above.
                var noticeRect = new Rect(content.x - FoldoutArrowIndent, y,
                    content.width + FoldoutArrowIndent, EditorGUIUtility.singleLineHeight);
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
                        : $"Missing type: {typeName}.\nOpen this asset from the Project window to repair it.",
                    canFix
                        ? () =>
                        {
                            // Anchored at the notice's top: ShowAsDropDown opens below the rect, so yMax would drop
                            // the picker a line lower.
                            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(noticeRect.x, noticeRect.y));
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

            // A required-but-empty reference shows a non-actionable notice; the header dropdown above is the fix.
            if (showRequired)
            {
                // Flat field (no arrow): pull the notice left onto the arrow's spot so it lines up with the label above.
                var noticeRect = new Rect(content.x - FoldoutArrowIndent, y,
                    content.width + FoldoutArrowIndent, EditorGUIUtility.singleLineHeight);
                var message = "Required reference is not set";

                InspectorNoticeGUI.DrawRequiredNotice(noticeRect, message,
                    "This [SerializeReference] field is marked required but has no value.");
                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (expandable && property.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawChildren(property, body.x, body.width, spacing, ref y, depth);
                EditorGUI.indentLevel--;
            }

            // The only notice that coexists with children, so it alone sits under the nested properties.
            if (showShared)
            {
                // One color across notice and stripe, so aliased fields read as a group. No warning icon: this is
                // attention, not an error.
                Color? indexColor = sharedIndex > 0 ? SerializeReferenceRidColor.ForIndex(sharedIndex) : null;

                // When this member is the one a sibling's message click just revealed, scroll the inspector to it.
                SerializeReferenceSharedNavigation.RevealIfPending(property, position);

                // Pulled left by the arrow's width so the swatch lines up under it, and widened to match so
                // "Make unique" stays right-pinned.
                var noticeRect = new Rect(content.x - FoldoutArrowIndent, y,
                    content.width + FoldoutArrowIndent, EditorGUIUtility.singleLineHeight);
                var persistent = property.Persistent();

                // Navigation needs the live property: expansion state is cached per SerializedObject, so the
                // ancestor isExpanded writes must go through the inspector's own.
                InspectorNoticeGUI.DrawNotice(
                    noticeRect,
                    sharedIndex > 0 ? $"Shared reference #{sharedIndex}" : "Shared reference",
                    "Make unique",
                    SerializeReferenceHelpers.BuildSharedReferenceDetail(property),
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent),
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
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                var child = iterator.Copy();

                // Unity ships no picker, so PropertyField would draw a nested reference with no way to choose a type
                // and a list whose "+" appends elements nothing can fill. Use this package's own header instead.
                if (SerializeReferenceNesting.DrawsOwnHeader(child, depth))
                {
                    var nestedHeight = ChildHeight(child, depth);
                    var nestedRect = new Rect(x, y, width, nestedHeight);
                    var content = new GUIContent(child.displayName);

                    if (child.isArray)
                    {
                        SerializeReferenceIMGUIList.Draw(nestedRect, child, content,
                            SerializeReferenceHelpers.GetArrayElementType(child), Array.Empty<Type>(), depth + 1);
                    }
                    else
                    {
                        Draw(nestedRect, content, child, depth + 1, Array.Empty<Type>());
                    }

                    y += nestedHeight + spacing;
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
            var iterator = property.Copy();
            var end = property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;
                height += ChildHeight(iterator.Copy(), depth) + spacing;
            }

            return height;
        }

        // Measured exactly as DrawChildren draws, or the reserved space and the painted rows drift apart.
        private static float ChildHeight(SerializedProperty child, int depth)
        {
            if (!SerializeReferenceNesting.DrawsOwnHeader(child, depth))
                return EditorGUI.GetPropertyHeight(child, includeChildren: true);

            return child.isArray
                ? SerializeReferenceIMGUIList.GetHeight(child, new GUIContent(child.displayName),
                    SerializeReferenceHelpers.GetArrayElementType(child), Array.Empty<Type>(), depth + 1)
                : GetHeight(child, depth + 1);
        }

        private static void ShowSelector(SerializedProperty property, Type fieldType, Type[] baseTypes, Type currentType, Rect dropdownRect)
        {
            var persistent = property.Persistent();
            var screenPosition = GUIUtility.GUIToScreenPoint(new Vector2(dropdownRect.x, dropdownRect.y));
            var screenRect = new Rect(screenPosition.x, screenPosition.y, dropdownRect.width, dropdownRect.height);

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                filter: new TypeSelectorFilter
                {
                    Types = new[] { fieldType },
                    Predicate = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes),
                    AdditionalTypes = GenericTypeResolver.GetAssignableGenericDefinitions(fieldType, baseTypes, SerializeReferenceHelpers.IsAcceptableGenericArgument),
                    ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                    InferredArgumentFilter = SerializeReferenceHelpers.IsAcceptableGenericArgument,
                },
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)));

            return;

            void Apply(Type type)
            {
                // Each target gets its own instance built from that target's previous value, so the reference is
                // never aliased across objects. One Undo step covers them all.
                if (SerializeReferenceHelpers.IsEditingMultipleObjects(persistent))
                {
                    SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                        persistent,
                        previous => SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));

                    // The per-target writes went through disposed SerializedObjects, so set expansion here.
                    persistent.isExpanded = type is not null;
                    return;
                }

                var single = persistent.managedReferenceValue;
                persistent.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, single));
                persistent.isExpanded = type is not null;
            }
        }

        private static void ShowContextMenu(SerializedProperty property, Type fieldType, Type[] baseTypes)
        {
            var persistent = property.Persistent();
            var filter = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes);
            var menu = new GenericMenu();

            // Copy reads the first target's value, Unity's convention; paste applies an independent instance per
            // target so the result is never aliased.
            menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                () => SerializeReferenceClipboard.Copy(persistent.managedReferenceValue));

            var pasteLabel = new GUIContent("Paste Serialize Reference");
            if (SerializeReferenceClipboard.CanPasteInto(fieldType, filter))
                menu.AddItem(pasteLabel, false, () => Paste(persistent));
            else
                menu.AddDisabledItem(pasteLabel);

            // A single-asset cross-reference operation, so it is only correct for a single target.
            if (SerializeReferenceHelpers.NoticesApply(property) &&
                SerializeReferenceHelpers.HasSharedReference(property))
                menu.AddItem(new GUIContent("Make Unique Reference"), false,
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent));

            var usagesType = SerializeReferenceHelpers.GetCurrentType(property);
            if (usagesType != null)
            {
                menu.AddItem(new GUIContent($"Find Usages of {usagesType.Name}"), false,
                    () => SerializeReferenceUsageSearchProvider.OpenSearch(usagesType));
            }

            // The inverse of Make Unique: point this field at an existing instance in the same object.
            if (SerializeReferenceHelpers.NoticesApply(property))
            {
                foreach (var candidate in SerializeReferenceLinker.CollectLinkCandidates(property))
                {
                    var path = candidate.Path;
                    menu.AddItem(new GUIContent($"Link to Existing/{candidate.Type.Name}  ({path})"), false,
                        () => SerializeReferenceLinker.LinkTo(persistent, path));
                }
            }

            if (fieldType != null)
            {
                menu.AddItem(new GUIContent("Create New Script…"), false, () =>
                {
                    if (!SerializeReferenceScriptCreator.TryCreateSubclassStub(fieldType, out _, out var fullTypeName)) return;

                    // One pending assignment per target — targetObject alone would leave objects 2..N untouched. The
                    // transient property may be disposed by the time this deferred callback runs.
                    foreach (var target in persistent.serializedObject.targetObjects)
                        SerializeReferencePendingAssignment.Enqueue(target, persistent.propertyPath, fullTypeName);
                });
            }

            if (usagesType != null)
            {
                var value = persistent.managedReferenceValue;
                menu.AddItem(new GUIContent("Save as Template…"), false,
                    () => SerializeReferenceNamePrompt.Show("Save Template",
                        SerializeReferenceTemplates.SuggestName(usagesType),
                        name => SerializeReferenceTemplates.SaveConfirmed(name, value)));
            }

            foreach (var template in SerializeReferenceTemplates.LoadResolved())
            {
                if (fieldType != null && !fieldType.IsAssignableFrom(template.Type)) continue;
                if (!filter(template.Type)) continue;
                var name = template.Name;
                menu.AddItem(new GUIContent($"Paste Template/{name}"), false, () => ApplyTemplate(persistent, name));
            }

            menu.ShowAsContext();
            return;

            void Paste(SerializedProperty target)
            {
                if (SerializeReferenceHelpers.IsEditingMultipleObjects(target))
                {
                    SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                        target,
                        _ => SerializeReferenceClipboard.CreateInstance());

                    // The per-target writes went through disposed SerializedObjects, so set expansion here. A null
                    // clipboard type is an empty-reference paste, which collapses.
                    target.isExpanded = SerializeReferenceClipboard.Type is not null;
                    return;
                }

                var value = SerializeReferenceClipboard.CreateInstance();
                target.SetManagedReferenceAndApply(value);
                target.isExpanded = value is not null;
            }
        }

        // Applies a saved template to the property (an independent instance per target on a multi-object selection).
        private static void ApplyTemplate(SerializedProperty property, string name)
        {
            var persistent = property.Persistent();

            if (SerializeReferenceHelpers.IsEditingMultipleObjects(persistent))
            {
                SerializeReferenceHelpers.ApplyManagedReferencePerTarget(persistent, _ => SerializeReferenceTemplates.CreateInstance(name));
                persistent.isExpanded = true;
                return;
            }

            var instance = SerializeReferenceTemplates.CreateInstance(name);
            if (instance is null) return;

            persistent.SetManagedReferenceAndApply(instance);
            persistent.isExpanded = true;
        }

        // A non-null missingTooltip both feeds the hover tooltip and flags the caption for the amber treatment.
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
