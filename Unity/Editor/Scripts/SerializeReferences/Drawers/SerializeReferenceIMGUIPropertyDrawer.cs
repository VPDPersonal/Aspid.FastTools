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

                if (SerializeReferenceRequiredGate.IsViolation(property))
                    height += spacing + EditorGUIUtility.singleLineHeight;
            }

            if (property.managedReferenceValue is not null && property.isExpanded)
                height += GetChildrenHeight(property, spacing);

            return height;
        }

        public static void Draw(Rect position, GUIContent label, SerializedProperty property, params Type[] baseTypes)
        {
            // Auto-de-alias a freshly duplicated list element (Ctrl+D / Duplicate / list +): when this element shares its
            // rid with another element of the same array, the guard queues a swap to an independent clone on the next
            // editor tick (one Undo step) — never mutating the SerializedObject mid-draw. Cheap on the unchanged path
            // (size + rolling-hash gate), so it is safe to call from every IMGUI repaint.
            SerializeReferenceDuplicateGuard.Observe(property);

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var mixedTypes = SerializeReferenceHelpers.HasMixedTypes(property);
            var currentType = SerializeReferenceHelpers.GetCurrentType(property);
            var hasValue = currentType is not null && !mixedTypes;
            var fieldType = SerializeReferenceHelpers.GetFieldType(property);

            // Per-asset notices (missing type / shared reference / required) read/write a single backing asset, so they
            // are suppressed under a multi-object selection. Computed up front because whether the field shows any of
            // them decides whether it reserves the stripe gutter below.
            var noticesApply = !mixedTypes && SerializeReferenceHelpers.NoticesApply(property);
            var showMissing = noticesApply && SerializeReferenceHelpers.IsMissingType(property);
            var showShared = noticesApply && SerializeReferenceHelpers.HasSharedReference(property);
            var showRequired = noticesApply && SerializeReferenceRequiredGate.IsViolation(property);

            // The shared group's 1-based badge number (0 when not shared) drives BOTH the stripe colour and the notice,
            // so a badge's colour tracks its number — (1),(2),(3) always read as distinct colours — instead of a rid
            // hash that could alias two unrelated groups onto a similar hue.
            var sharedIndex = showShared ? SerializeReferenceHelpers.GetSharedReferenceIndex(property) : 0;

            // Reserve a left gutter (StripeGutter) for the status stripe ONLY on a field that shows one, by shifting the
            // body right — the bar then sits in the reserved space, mirroring the UIToolkit field's padding-left. A field
            // with no stripe keeps its natural position, so plain foldouts get no needless indent. Missing / required
            // fields keep the same gutter (so the bar stays put), but pull their arrow-less label + notice left by
            // FoldoutArrowIndent — onto the foldout-arrow spot — so they hug the bar instead of trailing to its right.
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

            var labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, line.height);
            if (hasValue)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, toggleOnLabelClick: true);
            }
            else
            {
                // A flat (missing / required) field has no foldout arrow, so pull its label left onto the arrow's spot —
                // hugging the stripe and lining up with a foldout sibling's arrow instead of trailing an empty slot.
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

            // Mixed types across the selection: show the standard "—" treatment on the dropdown and never the open-script
            // button (there is no single type to open). Picking a type still rewrites every target. The "—" caption is
            // what renders the dash here — DropdownButton has no mixed-value styling — but EditorGUI.showMixedValue is
            // still set/restored to mirror the UIToolkit side's mixed flag and to propagate to any nested IMGUI control.
            string missingTooltip = null;
            var caption = mixedTypes ? "—" : GetCaption(property, currentType, out missingTooltip);
            var previousMixed = EditorGUI.showMixedValue;
            EditorGUI.showMixedValue = mixedTypes;

            // Missing stored type: mirror the UIToolkit dropdown's --missing treatment — the caption tints the same
            // warning amber as the stripe and the notice, keeps its class name by trimming from the LEFT (IMGUI clips
            // at the right edge, which would cut exactly the informative tail of "<Missing Namespace.Class>"), and
            // carries the full identity — assembly included — on the hover tooltip.
            var captionStyle = EditorStyles.miniPullDown;
            if (missingTooltip is not null)
            {
                captionStyle = GetMissingCaptionStyle();
                caption = FitCaptionFromLeft(captionStyle, caption, dropdownRect.width);
            }

            // A resolved type hovers its full Namespace.Class, Assembly identity (the caption shows only the short
            // name); a missing one, the stored identity it can no longer load.
            var captionTooltip = mixedTypes
                ? "Mixed — the selected objects hold different types."
                : missingTooltip ?? TypeSelectorHelpers.GetTypeSelectorTooltip(currentType);

            if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(caption, captionTooltip),
                    FocusType.Passive, captionStyle))
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
                var hintRect = new Rect(body.x, y, body.width, EditorGUIUtility.singleLineHeight);
                DrawInfoNotice(
                    hintRect,
                    "Different types selected",
                    "The selected objects hold different managed-reference types, so their fields cannot be shown " +
                    "together.\nPick a type from the dropdown to set it on all of them, or select a single object " +
                    "to edit its own fields.");
                return;
            }

            // Indented content rect (of the gutter-shifted body): the header foldout arrow, label and child fields all
            // sit at this x (Unity shifts it right by EditorGUI.indentLevel), so anchoring the notices and the stripe
            // here keeps their offset from the foldout arrow the SAME at every nesting depth. GUI.Label/DrawRect ignore
            // indentLevel, so we apply it explicitly.
            var content = EditorGUI.IndentedRect(body);

            // Left status stripe spanning the whole field — header, notices and any expanded children — so a shared
            // group reads as one continuous colour down through its value's fields, mirroring the UIToolkit field's
            // full-height __stripe (top:0 bottom:0). The badge's per-index colour for a shared reference, else the
            // warning amber for a missing-type / required violation. Offset into the left gutter (StripeOffset) so it
            // clears the foldout arrow / label, and inset top and bottom (StripeInsetY) so adjacent stripes stay apart.
            {
                Color? stripeColor = null;
                if (showShared && sharedIndex > 0)
                    stripeColor = SerializeReferenceRidColor.ForIndex(sharedIndex);
                else if (showMissing || showRequired)
                    stripeColor = NoticeColor;

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

                // The Smart Fix suggestion rides the same row as a second clickable word ("· → Pistol?"): the highest
                // ranked existing type the renamed/moved reference most likely became. The ranking is cached per
                // (asset, rid), so this stays cheap across IMGUI's per-frame repaints. The candidate is pre-declared so
                // it stays definitely assigned even when the short-circuit skips the probe (canFix == false).
                SerializeReferenceRepairSuggestions.RepairCandidate suggestion = default;
                var hasSuggestion = canFix &&
                    SerializeReferenceHelpers.TryGetRepairSuggestion(property, baseTypes, out suggestion);

                DrawNotice(
                    noticeRect,
                    "Missing type",
                    canFix ? "Fix" : null,
                    canFix
                        ? $"Missing type: {typeName}.\nClick Fix to re-point this reference to an existing type, keeping its data."
                        : $"Missing type: {typeName}.\nOpen this asset from the Project window to repair it.",
                    canFix
                        ? () =>
                        {
                            // Anchor from the notice's top (yMin), not its bottom: ShowAsDropDown opens below the
                            // anchor rect, so a top-anchored one-line rect ends flush at the notice's bottom. Anchoring
                            // from yMax added the rect's own height on top, dropping the picker a full line lower.
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

                // Warning palette (not the dim info one): an unset required field is a problem to fix. The notice is
                // non-actionable — the header dropdown above is the implied fix.
                DrawRequiredNotice(noticeRect, message,
                    "This [SerializeReference] field is marked required but has no value.");
                y += EditorGUIUtility.singleLineHeight + spacing;
            }

            if (hasValue && property.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawChildren(property, body.x, body.width, spacing, ref y);
                EditorGUI.indentLevel--;
            }

            // Shared-reference notice sits at the very bottom of the field — under its nested properties — mirroring the
            // UIToolkit field, where the notice is a sibling placed after the foldout content. It is the only notice that
            // coexists with children (missing / required render no value, so no children), so only it moves down here.
            if (showShared)
            {
                // The badge's per-index colour tints the whole notice — leading swatch, message and Make-unique action —
                // and the full-height stripe, so aliased fields read as one colour and the "#n" badge and its colour
                // always agree (mirrors the UIToolkit --shared notice: no warning icon, since a shared reference is
                // attention, not an error).
                Color? indexColor = sharedIndex > 0 ? SerializeReferenceRidColor.ForIndex(sharedIndex) : null;

                // Where this member of the group was painted — when it is the member a sibling's message click just
                // revealed, the inspector scrolls to it here.
                SerializeReferenceSharedNavigation.RevealIfPending(property, position);

                // Pull the notice left by the foldout arrow's reserved width so its leading swatch lines up under the
                // header's arrow (the value is always a foldout here); widen to match so "Make unique" stays right-pinned.
                var noticeRect = new Rect(content.x - FoldoutArrowIndent, y,
                    content.width + FoldoutArrowIndent, EditorGUIUtility.singleLineHeight);
                var persistent = property.Persistent();

                // The badge uses "#" (not parentheses) so the number reads as a group id, not a member count; the
                // tooltip lists the group's other fields by display path, and clicking the message reveals them.
                DrawNotice(
                    noticeRect,
                    sharedIndex > 0 ? $"Shared reference #{sharedIndex}" : "Shared reference",
                    "Make unique",
                    SerializeReferenceHelpers.BuildSharedReferenceDetail(property),
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent),
                    ridColor: indexColor,
                    onMessageClick: () => SerializeReferenceSharedNavigation.NavigateFrom(persistent));

                y += EditorGUIUtility.singleLineHeight + spacing;

                // Group-navigation pulse: while a sibling's "Shared reference" message was just clicked, every other
                // drawn member of that group tints in the group colour, fading out. Painted over the full field rect
                // (header + children), mirroring the UIToolkit field's background pulse.
                if (SerializeReferenceSharedNavigation.TryGetFlashAlpha(property, out var flashAlpha) &&
                    indexColor.HasValue)
                {
                    var flashColor = indexColor.Value;
                    flashColor.a = flashAlpha;
                    EditorGUI.DrawRect(position, flashColor);
                }
            }
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
                filter: new TypeSelectorFilter
                {
                    Types = new[] { fieldType },
                    Predicate = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes),
                    AdditionalTypes = GenericTypeResolver.GetAssignableGenericDefinitions(fieldType, baseTypes),
                    ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                },
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)));

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

        private static void ShowContextMenu(SerializedProperty property, Type fieldType, Type[] baseTypes)
        {
            var persistent = property.Persistent();
            var filter = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes);
            var menu = new GenericMenu();

            // Copy reads the first target's value (Unity's convention for a multi-selection menu). Paste then applies an
            // independent instance PER target, so the pasted reference is never aliased across objects.
            menu.AddItem(new GUIContent("Copy Serialize Reference"), false,
                () => SerializeReferenceClipboard.Copy(persistent.managedReferenceValue));

            var pasteLabel = new GUIContent("Paste Serialize Reference");
            if (SerializeReferenceClipboard.CanPasteInto(fieldType, filter))
                menu.AddItem(pasteLabel, false, () => Paste(persistent));
            else
                menu.AddDisabledItem(pasteLabel);

            // Make-unique is a single-asset cross-reference operation; only offered (and only correct) for a single
            // target — under a multi-object selection the shared-reference notice is already suppressed.
            if (SerializeReferenceHelpers.NoticesApply(property) &&
                SerializeReferenceHelpers.HasSharedReference(property))
                menu.AddItem(new GUIContent("Make Unique Reference"), false,
                    () => SerializeReferenceHelpers.MakeReferenceUnique(persistent));

            // Find every asset/field using the current type, via the sr: Quick Search provider.
            var usagesType = SerializeReferenceHelpers.GetCurrentType(property);
            if (usagesType != null)
                menu.AddItem(new GUIContent($"Find Usages of {usagesType.Name}"), false,
                    () => SerializeReferenceUsageSearchProvider.OpenSearch(usagesType));

            // Link this field to an existing instance of the same object (inverse of Make Unique), single-target only.
            if (SerializeReferenceHelpers.NoticesApply(property))
                foreach (var candidate in SerializeReferenceLinker.CollectLinkCandidates(property))
                {
                    var path = candidate.Path;
                    menu.AddItem(new GUIContent($"Link to Existing/{candidate.Type.Name}  ({path})"), false,
                        () => SerializeReferenceLinker.LinkTo(persistent, path));
                }

            // Generate a new subclass of the field's type and assign it once it compiles.
            if (fieldType != null)
                menu.AddItem(new GUIContent("Create New Script…"), false, () =>
                {
                    if (!SerializeReferenceScriptCreator.TryCreateSubclassStub(fieldType, out _, out var fullTypeName)) return;

                    // Multi-object: enqueue one pending assignment PER target so every selected object gets the new type
                    // after the script compiles — each entry carries its own GlobalObjectId, so the (GlobalId, path)-keyed
                    // queue keeps them apart. Enqueuing only targetObject (singular) would leave objects 2..N untouched.
                    // Read from the persistent property: the transient `property` may be disposed by the time this
                    // deferred context-menu callback runs.
                    foreach (var target in persistent.serializedObject.targetObjects)
                        SerializeReferencePendingAssignment.Enqueue(target, persistent.propertyPath, fullTypeName);
                });

            // Save the current instance as a durable named template, and paste any assignable saved template.
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


        // A missing stored type also reports its full identity — assembly included — through missingTooltip (null
        // otherwise), which both feeds the dropdown's hover tooltip and flags the caption for the amber missing
        // treatment (see the DropdownButton call).
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

        // Amber caption for a missing stored type, mirroring the UIToolkit dropdown's --missing tint (NoticeColor is
        // the same warning amber the stripe and the notice text use). The colour is (re)assigned on every call, like
        // the notice styles below, so a cached style survives editor-theme changes.
        private static GUIStyle _missingCaptionStyle;

        private static GUIStyle GetMissingCaptionStyle()
        {
            _missingCaptionStyle ??= new GUIStyle(EditorStyles.miniPullDown);
            _missingCaptionStyle.normal.textColor = NoticeColor;
            _missingCaptionStyle.hover.textColor = NoticeColor;
            _missingCaptionStyle.active.textColor = NoticeColor;
            _missingCaptionStyle.focused.textColor = NoticeColor;
            return _missingCaptionStyle;
        }

        // IMGUI clips a too-long caption at its RIGHT edge, which would cut the class name — the informative tail of
        // "<Missing Namespace.Class>" — so mirror the UIToolkit side's start-ellipsis by hand: drop leading characters
        // (binary search on the measured width) until the rest fits behind a leading "…".
        private static readonly GUIContent MeasureContent = new();

        private static string FitCaptionFromLeft(GUIStyle style, string text, float width)
        {
            MeasureContent.text = text;
            if (style.CalcSize(MeasureContent).x <= width) return text;

            // low..high — candidate counts of dropped leading characters; find the smallest that fits.
            int low = 1, high = text.Length;
            while (low < high)
            {
                var mid = (low + high) / 2;
                MeasureContent.text = "…" + text.Substring(mid);

                if (style.CalcSize(MeasureContent).x <= width) high = mid;
                else low = mid + 1;
            }

            return "…" + text.Substring(low);
        }

        // Warning yellow mirrors the UIToolkit notice palette:
        // --aspid-colors-status-warning-text-light / -lightness.
        private static readonly Color NoticeColor = new(245f / 255f, 185f / 255f, 85f / 255f);
        private static readonly Color NoticeColorHover = new(255f / 255f, 235f / 255f, 175f / 255f);

        // How far the shared action's rid colour lightens toward white on hover — mirrors the UIToolkit notice's
        // ActionHoverLighten, the hover feedback in place of a static USS brighten (the rid colour is dynamic).
        private const float ActionHoverLighten = 0.35f;

        // Leading rid swatch size on the shared-reference notice — mirrors the UIToolkit __dot (8px), drawn as a filled
        // circle via GUI.DrawTexture's borderRadius.
        private const float DotSize = 8f;

        // The shared-reference notice sits under a foldout header, whose arrow reserves this much space to the left of
        // the value's inline label. Pull the notice's leading swatch back by it so the swatch lines up under the foldout
        // arrow rather than the label to its right.
        private const float FoldoutArrowIndent = 11f;

        // Left status stripe. StripeGutter is the left padding the whole field body is shifted by, reserving a clear
        // gutter for the stripe so it never rides on the foldout arrow (to its right) or a list drag handle (to its
        // left) — mirroring the UIToolkit field's padding-left. StripeOffset places the bar inside that gutter, measured
        // left from the indented content (so its gap from the arrow is the same at every nesting depth); keep it below
        // StripeGutter. StripeInsetY trims the top and bottom so adjacent full-height stripes (e.g. two shared list
        // elements) read as separate bars with a small gap instead of one merged line.
        private const float StripeGutter = 5f;
        private const float StripeWidth = 2f;
        private const float StripeOffset = 16f;
        private const float StripeInsetY = 2f;

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
        /// Draws a compact single-row notice, mirroring the UIToolkit <see cref="SerializeReferenceNotice"/>: a terse
        /// message, then a bold, right-pinned clickable action word (underlined; it lightens on hover) and — for a
        /// missing-type notice with a Smart Fix candidate — an optional trailing suggestion word ("· → Pistol?")
        /// clustered just after it at the right edge. The full <paramref name="detail"/> rides each segment's hover
        /// tooltip. Without <paramref name="ridColor"/> the row is a warning: a yellow triangle icon and the warning
        /// amber palette (missing-type / required). With <paramref name="ridColor"/> it is the shared-reference variant:
        /// no icon, a leading rid-coloured swatch, and the message plus action both tinted that per-rid colour — so
        /// aliased fields read as one colour and match at a glance. <paramref name="onMessageClick"/>, when given,
        /// makes the message itself clickable (link cursor + hover lighten, no underline — the action words keep that
        /// affordance) — the shared notice's "show me the other members of this group" segment.
        /// </summary>
        private static void DrawNotice(Rect rect, string message, string actionText, string detail, Action onClick,
            string suggestionText = null, string suggestionDetail = null, Action onSuggestion = null,
            Color? ridColor = null, Action onMessageClick = null)
        {
            var shared = ridColor.HasValue;
            var baseColor = shared ? ridColor.Value : NoticeColor;
            var hoverColor = shared ? Color.Lerp(baseColor, Color.white, ActionHoverLighten) : NoticeColorHover;

            _messageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _actionStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            _messageStyle.normal.textColor = baseColor;

            float messageX;
            if (shared)
            {
                // Shared-reference variant: no warning icon; lead the row with the rid-coloured swatch instead.
                DrawDot(rect.x, rect, baseColor);
                messageX = rect.x + DotSize + 6f;
            }
            else
            {
                const float iconSize = 16f;
                var iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
                GUI.Label(iconRect, EditorGUIUtility.IconContent("console.warnicon"));
                messageX = iconRect.xMax + 4f;
            }

            var messageContent = new GUIContent(message, detail);
            var messageWidth = _messageStyle.CalcSize(messageContent).x;
            var messageRect = new Rect(messageX, rect.y, messageWidth, rect.height);
            if (onMessageClick is not null)
            {
                // Clickable message (the shared notice's group navigation): link cursor and the same hover lighten as
                // the action beside it, mirroring the UIToolkit __message--navigable treatment.
                var messageHover = messageRect.Contains(Event.current.mousePosition);
                var messageColor = messageHover ? hoverColor : baseColor;
                _messageStyle.normal.textColor = messageColor;
                _messageStyle.hover.textColor = messageColor;

                EditorGUIUtility.AddCursorRect(messageRect, MouseCursor.Link);
                if (GUI.Button(messageRect, messageContent, _messageStyle)) onMessageClick();
            }
            else
            {
                // The style is shared across notices — reset the hover tint a clickable message may have left behind.
                _messageStyle.hover.textColor = baseColor;
                GUI.Label(messageRect, messageContent, _messageStyle);
            }

            if (string.IsNullOrEmpty(actionText) || onClick is null) return;

            // Right-align the action cluster (mirrors the UIToolkit margin-left:auto): measure the action and any
            // trailing Smart Fix suggestion, then pin them flush to the row's right edge — never overlapping the message.
            var actionContent = new GUIContent(actionText, detail);
            var actionWidth = _actionStyle.CalcSize(actionContent).x;

            var hasSuggestion = !string.IsNullOrEmpty(suggestionText) && onSuggestion is not null;
            var suggestionContent = hasSuggestion ? new GUIContent(suggestionText, suggestionDetail) : null;
            var suggestionWidth = hasSuggestion ? _actionStyle.CalcSize(suggestionContent).x : 0f;
            const float suggestionGap = 6f;

            var clusterWidth = actionWidth + (hasSuggestion ? suggestionGap + suggestionWidth : 0f);
            var actionX = Mathf.Max(messageRect.xMax + 6f, rect.xMax - clusterWidth);

            DrawLink(new Rect(actionX, rect.y, actionWidth, rect.height), actionContent, baseColor, hoverColor, onClick);

            if (hasSuggestion)
                DrawLink(new Rect(actionX + actionWidth + suggestionGap, rect.y, suggestionWidth, rect.height),
                    suggestionContent, baseColor, hoverColor, onSuggestion);
        }

        /// <summary>
        /// Draws the shared non-actionable "required" warning row (warning icon + yellow message). Reused by the string
        /// <see cref="Aspid.FastTools.Types.Editors.TypeIMGUIPropertyDrawer"/> path so its required notice matches the
        /// managed-reference one exactly.
        /// </summary>
        internal static void DrawRequiredNotice(Rect rect, string message, string detail) =>
            DrawNotice(rect, message, actionText: string.Empty, detail: detail, onClick: null);

        // Draws one bold, clickable, hover-tracking link word in the given rect — underlined, matching the UIToolkit
        // notice's <u> action treatment, so "this is a button" reads the same in both UIs — that lightens on hover.
        // Shared by the Fix action, the trailing Smart Fix suggestion and the shared-reference Make-unique action; the
        // caller supplies the resting and hover colours (warning amber, or the per-rid colour lightened toward white
        // for the shared notice).
        private static void DrawLink(Rect linkRect, GUIContent content, Color color, Color hoverColor, Action onClick)
        {
            var hover = linkRect.Contains(Event.current.mousePosition);
            var drawColor = hover ? hoverColor : color;
            _actionStyle.normal.textColor = drawColor;
            _actionStyle.hover.textColor = drawColor;

            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);

            // IMGUI rich text has no <u>, so the underline is a hand-drawn 1px line under the word.
            EditorGUI.DrawRect(new Rect(linkRect.x + 1f, linkRect.yMax - 3f, linkRect.width - 2f, 1f), drawColor);

            if (GUI.Button(linkRect, content, _actionStyle)) onClick();
        }

        // Draws the small rid-coloured swatch leading the shared-reference notice — its colour is shared with the tinted
        // message, action and header stripe, so aliased fields match at a glance. Drawn as a filled circle to mirror the
        // UIToolkit notice's rounded __dot: IMGUI has no circle primitive, so the 1×1 white texture is tinted and
        // rounded fully (borderRadius = half the size) via GUI.DrawTexture.
        private static void DrawDot(float x, Rect rect, Color color)
        {
            var dotRect = new Rect(x, rect.y + (rect.height - DotSize) * 0.5f, DotSize, DotSize);
            GUI.DrawTexture(dotRect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                alphaBlend: true, imageAspect: 0f, color: color, borderWidth: 0f, borderRadius: DotSize * 0.5f);
        }
    }
}
