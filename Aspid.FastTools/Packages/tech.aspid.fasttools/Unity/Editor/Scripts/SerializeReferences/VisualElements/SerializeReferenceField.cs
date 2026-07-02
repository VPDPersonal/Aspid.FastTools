using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Types;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.Types.Editors;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// UIToolkit field for a <c>[SerializeReference]</c> property: a foldout whose header carries an
    /// EnumField-style type dropdown (backed by <see cref="TypeSelectorWindow"/>) and an open-script button,
    /// whose content hosts the assigned instance's nested properties, and which surfaces a missing-type
    /// warning when the stored type can no longer be resolved.
    /// </summary>
    /// <remarks>
    /// Always bound to a managed-reference <see cref="SerializedProperty"/>; created by
    /// <see cref="Aspid.FastTools.Types.Editors.TypeSelectorPropertyDrawer"/>, not from UXML. The field keeps the live
    /// inspector property so child fields round-trip through Unity's binding (apply/Undo) and only rebuilds
    /// the nested properties when the assigned type actually changes.
    /// </remarks>
    internal sealed class SerializeReferenceField : VisualElement
    {
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";

        private const string BlockClass = "aspid-fasttools-serialize-reference";
        private const string EmptyClass = BlockClass + "--empty";
        private const string DropdownClass = BlockClass + "__dropdown";

        // Missing stored type: tints the caption the warning amber (coupling the broken value with the stripe and
        // the notice below) and flips its ellipsis to the start, so the class name — the informative tail of
        // "<Missing Namespace.Class>" — survives truncation instead of the namespace prefix.
        private const string DropdownMissingClass = DropdownClass + "--missing";

        // Wrapper that hosts the per-asset notices (missing / shared / required / mixed) as a sibling placed AFTER the
        // foldout's content, so an expanded field shows the shared notice BELOW its child fields — matching the IMGUI
        // drawer, which draws that notice after the children.
        private const string NoticesClass = BlockClass + "__notices";

        // A single 2 px stripe on the field's left edge flags it at a glance. The --active modifier gives it width; a
        // missing type paints it the warning amber from USS (--warning), while a shared reference paints it inline from
        // code in that reference's per-rid colour, matching the notice's swatch and tinted message.
        private const string StripeClass = BlockClass + "__stripe";
        private const string StripeActiveClass = StripeClass + "--active";
        private const string StripeWarningClass = StripeClass + "--warning";

        // The group-navigation pulse's overlay: an absolutely-positioned band spanning from the status stripe's line
        // to the field's right edge (the root's own background cannot reach the stripe, which hangs left of the root
        // in the gutter). Its colour is animated inline from code; see FlashSharedHighlight.
        private const string FlashClass = BlockClass + "__flash";

        // Root modifier toggled while the field shows a stripe (missing type / shared reference). It gates the foldout's
        // left padding — the stripe gutter — so a field with no stripe keeps its natural position instead of a needless
        // indent (matching the IMGUI drawer). A required-only notice carries no stripe, so it does not set this.
        private const string StripedClass = BlockClass + "--striped";

        // Applied to the header while a compatible MonoScript is dragged over the field.
        private const string DropTargetClass = BlockClass + "--drop-target";

        // Unity's mixed-value class — applied to the dropdown so the EnumField theme shows the standard "—" treatment
        // when the selected targets hold different managed-reference types under a multi-object selection.
        private const string MixedValueClass = "unity-base-field--show-mixed-value";

        // The caption Unity shows for a mixed (multiple-different-values) field.
        private const string MixedCaption = "—";

        // Unity's BaseField input class — applied to the dropdown's inner input so it picks up the
        // same flex/indent the EnumField theme rules target on a real field's visualInput.
        private const string BaseFieldInputClass = "unity-base-field__input";

        // Small gap kept between the value column and the dropdown's left edge.
        private const float DropdownGap = 2f;

        // The group-navigation pulse: how strongly a member of a shared group tints in its rid colour when the user
        // clicks another member's "Shared reference" message, how long the pulse lasts, and for what fraction of it
        // the tint holds at full strength before fading (an immediate fade read as a laggy flicker rather than a
        // "here it is" highlight). Mirrored by the IMGUI drawer's flash overlay so the pulse reads the same in both UIs.
        private const float FlashAlpha = 0.25f;
        private const int FlashDurationMs = 1600;
        private const float FlashHoldFraction = 0.35f;

        // A reveal that had to expand something first waits one beat — for the expansion's layout pass, and for a
        // just-expanded ListView to build its rows — before scrolling and pulsing, or ScrollTo would aim at a
        // zero-size target. Deeper nestings (a list inside a list) reveal one level per pass; a pass may also find
        // nothing new to expand while a previous pass's ListView is still building rows, so navigation keeps
        // re-checking until MaxRevealRetries passes are spent.
        private const long RevealDelayMs = 150;
        private const int MaxRevealRetries = 4;

        private readonly Foldout _foldout;
        private readonly TextElement _caption;
        private readonly VisualElement _dropdown;
        private readonly Button _openButton;
        private readonly VisualElement _content;
        private readonly VisualElement _notices;
        private readonly SerializedProperty _property;
        private readonly Type _fieldType;
        private readonly Type[] _baseTypes;
        private readonly Func<Type, bool> _filter;

        private SerializeReferenceNotice _missingNotice;
        private SerializeReferenceNotice _sharedNotice;
        private SerializeReferenceNotice _mixedNotice;
        private SerializeReferenceNotice _requiredNotice;
        private VisualElement _stripe;
        private VisualElement _flashOverlay;
        private Type _currentType;
        private bool _contentBuilt;
        private bool _mixedTypes;

        // Stripe inputs, written by the notice updates and consumed by UpdateStripe: a missing type paints the warning
        // amber stripe; a shared reference paints it in its per-rid colour (cached in _sharedColor).
        private bool _isMissing;
        private bool _isShared;
        private Color _sharedColor;
        private float _arrowInset = float.NaN;

        // Raised after any field reassigns its managed reference (Make unique, type pick, paste, link, drop, template,
        // missing-type fix). Every live field re-evaluates its shared-reference notice on it, since "shared" depends on
        // the other fields' rids and cannot be observed through value tracking (see the constructor subscription).
        private static event Action ManagedReferencesChanged;

        // Every field currently attached to a panel, so a shared notice's message click can find the other members of
        // its group (same target object + rid) and reveal them — scroll to the next one and pulse the whole group.
        // Registered/unregistered alongside the ManagedReferencesChanged subscription in the constructor.
        private static readonly List<SerializeReferenceField> LiveFields = new();

        // The per-group navigation cursor: the member the last click revealed, keyed by (target object, rid).
        // Advancing from the cursor — not from the clicked field — lets repeated clicks on the SAME notice walk the
        // whole group; advancing from the clicked field would recompute the same "next" forever and members two or
        // more steps away would stay unreachable from that notice.
        private static readonly Dictionary<(int target, long rid), string> NavigationCursor = new();

        public SerializeReferenceField(string label, SerializedProperty property, Type[] baseTypes = null)
        {
            _property = property;
            _fieldType = SerializeReferenceHelpers.GetFieldType(_property);
            _baseTypes = baseTypes;
            _filter = SerializeReferenceHelpers.BuildAssignableFilter(baseTypes);

            this.AddClass(BlockClass)
                .AddClass(PropertyField.ussClassName)
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddAspidThemeStyleSheets();

            _foldout = new Foldout();
            _foldout.RegisterValueChangedCallback(OnFoldoutToggled);
            _content = _foldout.contentContainer;

            // Host the per-asset notices as a sibling of the foldout's content, placed AFTER it, so the shared-reference
            // notice (the only one that coexists with child fields — missing / required / mixed render no children)
            // sits at the very bottom of the field, under its nested properties. As a sibling of the content container —
            // not inside it — the host carries no child indent and stays visible while the foldout is collapsed.
            _notices = new VisualElement().AddClass(NoticesClass);
            _foldout.hierarchy.Insert(_foldout.hierarchy.IndexOf(_content) + 1, _notices);

            _caption = new TextElement()
                .AddClass(EnumField.textUssClassName)
                .SetPickingMode(PickingMode.Ignore);

            // Mirror SerializableType's TypeField structure: an enum-field "root" wrapping a separate
            // "__input" child. Unity's theme indents the caption through descendant selectors
            // (".unity-enum-field .unity-enum-field__input"), which only match when the input is a
            // child of the field — collapsing both classes onto one element drops that indent.
            var dropdownInput = new VisualElement()
                .AddClass(BaseFieldInputClass)
                .AddClass(EnumField.inputUssClassName)
                .AddChild(_caption)
                .AddChild(new VisualElement()
                    .AddClass(EnumField.arrowUssClassName)
                    .SetPickingMode(PickingMode.Ignore));

            _dropdown = new VisualElement()
                .AddClass(EnumField.ussClassName)
                .AddClass(DropdownClass)
                .AddChild(dropdownInput);

            _dropdown.RegisterCallback<PointerDownEvent>(OnDropdownClicked);

            _openButton = new Button()
                .AddChild(new VisualElement())
                .AddClicked(() => SerializeReferenceHelpers.GetCurrentType(_property)?.OpenInScriptEditor());

            // Carry the foldout caption on the toggle's BaseField label and opt into Unity's
            // inspector field alignment so the label width tracks the value column exactly as
            // SerializableType does (see InspectorTypeField). The expand arrow stays on the far
            // left; the dropdown is then offset by the arrow width so it begins at the value column.
            var toggle = _foldout.Q<Toggle>();
            toggle.AddClass(BaseField<bool>.alignedFieldUssClassName);
            toggle.labelElement.AddClass(PropertyField.labelUssClassName);
            toggle.label = label;

            var arrow = toggle.Q(className: Foldout.inputUssClassName);
            toggle.Insert(0, arrow);
            arrow.RegisterCallback<GeometryChangedEvent>(OnArrowGeometryChanged);

            toggle.AddChild(_dropdown)
                .AddChild(_openButton);

            // Copy/Paste lives on the header only — child PropertyFields keep their own contextual menus.
            toggle.AddManipulator(new ContextualMenuManipulator(BuildContextMenu));

            // Dropping a MonoScript on the header assigns an instance of its class (when assignable).
            RegisterDragAndDrop(toggle);

            // When this field is a list element, replace the list's "+" with a picker-backed add (kills duplicate-last
            // rid aliasing at the source). Installed on attach, once per ListView.
            RegisterCallback<AttachToPanelEvent>(_ =>
                SerializeReferenceListAddBehavior.TryInstall(this, _property, _fieldType, _baseTypes));

            this.AddChild(_foldout);

            Refresh(forceRebuild: true);
            this.TrackPropertyValue(_property, _ => Refresh(forceRebuild: false));

            // "Shared" is a cross-field property — it depends on the OTHER fields' managed-reference ids — which no
            // value-tracking can observe: Make unique clones the reference to a NEW rid with the SAME data, so the
            // object's value hash is unchanged and TrackPropertyValue / TrackSerializedObjectValue never fire (this is
            // why the sibling's notice looked stale "sometimes" — whenever the shared instances held identical data). So
            // sibling fields are told explicitly: every reference-reassigning action raises ManagedReferencesChanged (see
            // ApplyReferenceChange), and each live field re-evaluates its notice on it.
            RegisterCallback<AttachToPanelEvent>(_ =>
            {
                // -= before += so a recycled ListView item (detach → re-attach) never double-subscribes.
                ManagedReferencesChanged -= OnManagedReferencesChanged;
                ManagedReferencesChanged += OnManagedReferencesChanged;
                // Undo/redo reverts the object outside every mutation path, so ManagedReferencesChanged never fires and
                // only the field whose OWN value changed gets a value-tracking callback — a sibling that re-gained (or
                // lost) an alias would stay stale until reselect. So each field also re-evaluates itself on undo/redo.
                Undo.undoRedoPerformed -= OnUndoRedo;
                Undo.undoRedoPerformed += OnUndoRedo;
                // Same Remove-then-Add guard for the group-navigation registry.
                LiveFields.Remove(this);
                LiveFields.Add(this);
            });
            RegisterCallback<DetachFromPanelEvent>(_ =>
            {
                ManagedReferencesChanged -= OnManagedReferencesChanged;
                Undo.undoRedoPerformed -= OnUndoRedo;
                LiveFields.Remove(this);
            });
        }

        // The arrow sits in-flow before the aligned label, so the label (and the dropdown that
        // follows it) overshoot the value column by the arrow's width. Pull the dropdown back by
        // that measured width so its left edge lands on the value column at any nesting depth.
        private void OnArrowGeometryChanged(GeometryChangedEvent evt)
        {
            var inset = ((VisualElement)evt.target).resolvedStyle.width;
            if (Mathf.Approximately(inset, _arrowInset)) return;

            _arrowInset = inset;
            _dropdown.style.marginLeft = DropdownGap - inset;
        }

        private void Refresh(bool forceRebuild)
        {
            // A saved-asset repair reimports the asset and invalidates this property's SerializedObject; the inspector
            // is rebuilt from a fresh selection instead, so a stale property here must no-op rather than throw.
            if (!IsPropertyAlive()) return;

            // Auto-de-alias a freshly duplicated list element (Ctrl+D / Duplicate / list +): when this element shares its
            // rid with another element of the same array, the guard queues a swap to an independent clone on the next
            // editor tick (one Undo step), which property tracking then re-renders. forceRebuild keeps this pass coherent.
            if (SerializeReferenceDuplicateGuard.Observe(_property))
                forceRebuild = true;

            var mixedTypes = SerializeReferenceHelpers.HasMixedTypes(_property);
            var currentType = SerializeReferenceHelpers.GetCurrentType(_property);
            var hasValue = currentType is not null;

            // A missing stored type keeps its full "<Missing Namespace.Class>" caption (the --missing USS rule
            // ellipsizes it from the LEFT so the class name survives truncation) with the complete identity —
            // assembly included — one hover away on the dropdown's tooltip.
            var missingType = !mixedTypes && !hasValue && SerializeReferenceHelpers.IsMissingType(_property)
                ? SerializeReferenceHelpers.GetMissingTypeName(_property)
                : default;

            // With mixed types the foldout would expose only the first target's children, which do not represent the
            // selection; collapse it and key off the dim "different types" hint instead of the per-instance fields.
            _caption.SetText(mixedTypes
                ? MixedCaption
                : TypeSelectorHelpers.GetTypeSelectorTitle(currentType, missingType.DisplayName));

            // The tooltip lives on the dropdown, not the caption — the caption ignores picking, so it can never be
            // the tooltip anchor. A resolved type hovers its full Namespace.Class, Assembly identity (the caption
            // shows only the short name); a missing one, the stored identity it can no longer load.
            _dropdown.tooltip = mixedTypes
                ? "Mixed — the selected objects hold different types."
                : hasValue
                    ? TypeSelectorHelpers.GetTypeSelectorTooltip(currentType)
                    : missingType.IsEmpty ? null : $"Missing type: {missingType.FullName}";

            _openButton.SetDisplay(hasValue && !mixedTypes ? DisplayStyle.Flex : DisplayStyle.None);

            _dropdown.EnableInClassList(DropdownMissingClass, !missingType.IsEmpty);
            _dropdown.EnableInClassList(MixedValueClass, mixedTypes);
            EnableInClassList(EmptyClass, !hasValue && !mixedTypes);
            _foldout.SetValueWithoutNotify(hasValue && !mixedTypes && _property.isExpanded);

            UpdateMissingBox();
            UpdateSharedBox();
            UpdateMixedBox(mixedTypes);
            UpdateRequiredBox();
            UpdateStripe();

            // A mixed selection never renders child fields — Unity's per-field multi-edit cannot merge fields of
            // different types — so the content is cleared and rebuilt only when the (shared) type actually changes.
            var rebuild = forceRebuild || !_contentBuilt || currentType != _currentType || mixedTypes != _mixedTypes;
            if (rebuild)
            {
                _currentType = currentType;
                _mixedTypes = mixedTypes;
                RebuildContent(hasValue && !mixedTypes);
            }
        }

        // The property's SerializedObject can be torn down out from under this field (e.g. a saved-asset repair
        // reimports the asset); probing the target object reports that without throwing on the dangling handle.
        private bool IsPropertyAlive()
        {
            try { return _property.serializedObject?.targetObject != null; }
            catch (Exception) { return false; }
        }

        private void RebuildContent(bool hasValue)
        {
            _content.Clear();
            _contentBuilt = true;
            if (!hasValue) return;

            var iterator = _property.Copy();
            var end = _property.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                enterChildren = false;

                var child = iterator.Copy();
                var field = new PropertyField(child);
                field.BindProperty(child);

                _content.Add(field);
            }
        }

        private void UpdateMixedBox(bool mixedTypes)
        {
            if (!mixedTypes)
            {
                _mixedNotice?.RemoveFromHierarchy();
                return;
            }

            _mixedNotice ??= new SerializeReferenceNotice();
            if (_mixedNotice.parent is null) _notices.AddChild(_mixedNotice);

            // Stands in for the per-instance child fields, which cannot be merged across different types. Keep it
            // terse and non-actionable: selecting a single object restores its own field, or picking a type from the
            // dropdown rewrites every target to that one type.
            _mixedNotice.SetInfo(
                message: "Different types selected",
                detail: "The selected objects hold different managed-reference types, so their fields cannot be shown " +
                        "together.\nPick a type from the dropdown to set it on all of them, or select a single object " +
                        "to edit its own fields.");
        }

        // A field marked [TypeSelector(Required = true)] but left empty shows a non-actionable notice (the dropdown right
        // above is the fix). Suppressed under a multi-object selection and when the type is missing (its own notice).
        private void UpdateRequiredBox()
        {
            if (!SerializeReferenceHelpers.NoticesApply(_property) || !SerializeReferenceRequiredGate.IsViolation(_property))
            {
                _requiredNotice?.RemoveFromHierarchy();
                return;
            }

            _requiredNotice ??= new SerializeReferenceNotice();
            if (_requiredNotice.parent is null) _notices.AddChild(_requiredNotice);

            var message = "Required reference is not set";

            // Warning palette (not the dim info one): an unset required field is a problem to fix, not a passive hint.
            // Non-actionable — the empty action word keeps the dropdown above as the implied fix.
            _requiredNotice.Set(
                message: message,
                actionText: string.Empty,
                detail: "This [SerializeReference] field is marked required but has no value. Pick a type from the dropdown.",
                onAction: null);
        }

        private void UpdateMissingBox()
        {
            // Missing-type detection reads the first target's backing asset YAML and its repair rewrites that one file,
            // so the notice is meaningless (and potentially misleading) under a multi-object selection — suppress it.
            // Cached for UpdateStripe so the YAML probe runs once per refresh.
            _isMissing = SerializeReferenceHelpers.NoticesApply(_property) &&
                         SerializeReferenceHelpers.IsMissingType(_property);

            if (!_isMissing)
            {
                _missingNotice?.RemoveFromHierarchy();
                return;
            }

            _missingNotice ??= new SerializeReferenceNotice();
            if (_missingNotice.parent is null) _notices.AddChild(_missingNotice);

            var typeName = SerializeReferenceHelpers.GetMissingTypeDisplayName(_property);

            // Offered for saved assets (YAML rewrite) and Prefab Mode objects (in-memory) — anything with a
            // resolvable backing document. Scene objects without one fall through to the read-only hint.
            var canFix = SerializeReferenceHelpers.TryGetRepairLocation(_property, out _, out _, out _);

            _missingNotice.Set(
                message: "Missing type",
                actionText: canFix ? "Fix" : string.Empty,
                detail: canFix
                    ? $"Missing type: {typeName}.\nClick Fix to re-point this reference to an existing type, keeping its data."
                    : $"Missing type: {typeName}.\nOpen this asset from the Project window to repair it.",
                onAction: OpenFixSelector);

            UpdateSuggestion(canFix);
        }

        // The Smart Fix suggestion rides the missing notice as a second clickable segment ("· → Pistol?"): the highest
        // ranked existing type the renamed/moved reference most likely became. It is offered only where the field can
        // be repaired at all; clicking it applies the candidate directly through the same repair path as a manual Fix.
        private void UpdateSuggestion(bool canFix)
        {
            if (!canFix ||
                !SerializeReferenceHelpers.TryGetRepairSuggestion(_property, _baseTypes, out var suggestion))
            {
                _missingNotice.SetSuggestion(string.Empty, null, null);
                return;
            }

            _missingNotice.SetSuggestion(
                suggestionText: SerializeReferenceHelpers.GetSuggestionLabel(suggestion),
                detail: SerializeReferenceHelpers.GetSuggestionDetail(suggestion),
                onSuggestion: () => ApplySuggestion(suggestion.Type));
        }

        private void ApplySuggestion(Type suggestedType)
        {
            if (SerializeReferenceHelpers.TryFixMissingType(_property, suggestedType))
                ApplyReferenceChange();
        }

        private void UpdateSharedBox()
        {
            // The shared-reference scan compares managedReferenceId only within the first target's SerializedObject and
            // Make-unique rewrites that one object, so neither generalises to a multi-object selection — suppress it.
            var rid = _property.managedReferenceId;
            _isShared = SerializeReferenceHelpers.NoticesApply(_property) &&
                        SerializeReferenceHelpers.HasSharedReference(_property) &&
                        rid >= 0;

            if (!_isShared)
            {
                _sharedNotice?.RemoveFromHierarchy();
                return;
            }

            // The badge number identifies the shared group: every field aliasing this rid shows the same "#n", so two
            // like-numbered fields are the same reference at a glance. "#" (not parentheses) so the number reads as a
            // group id rather than a member count. The colour is keyed to that badge number (not a rid hash), so
            // consecutive badges are maximally separated and the number and colour always agree.
            var index = SerializeReferenceHelpers.GetSharedReferenceIndex(_property);

            // The per-index colour is the field's shared-reference signal: it fills the notice's leading swatch, tints
            // its message, and (cached here for UpdateStripe) paints the left stripe — so the whole field reads in that
            // one colour and aliased fields match at a glance.
            var sharedColor = SerializeReferenceRidColor.ForIndex(index);
            _sharedColor = sharedColor;

            _sharedNotice ??= new SerializeReferenceNotice();
            if (_sharedNotice.parent is null) _notices.AddChild(_sharedNotice);

            // Passing the colour also flips the notice to its shared treatment (see SerializeReferenceNotice); the
            // tooltip lists the group's other fields by display path, and clicking the message reveals them.
            _sharedNotice.Set(
                message: index > 0 ? $"Shared reference #{index}" : "Shared reference",
                actionText: "Make unique",
                detail: SerializeReferenceHelpers.BuildSharedReferenceDetail(_property),
                onAction: MakeUnique,
                dotColor: sharedColor,
                onNavigate: NavigateToAliases);
        }

        // The shared notice's message click: reveal the WHOLE shared group — expand whatever hides any of its
        // members (collapsed parents, never-opened lists), scroll to the next member (cursor-based, so repeated
        // clicks walk the group) and pulse every member in the group colour — so "who else uses this?" is answered
        // by the inspector itself instead of a manual hunt for matching swatches.
        private void NavigateToAliases() => NavigateToAliases(MaxRevealRetries);

        private void NavigateToAliases(int retriesLeft)
        {
            if (!IsPropertyAlive()) return;

            var target = _property.serializedObject.targetObject;
            var rid = _property.managedReferenceId;

            // The group's canonical document order (shared with the IMGUI drawer) backs the cycling.
            var group = SerializeReferenceHelpers.GetSharedReferenceGroupPaths(_property);
            if (group.Count < 2) return;

            // The group members' live elements, keyed by path. Same panel only: with two inspectors on one object
            // every alias exists twice, and the reveal should stay inside the inspector the user clicked in.
            var live = new Dictionary<string, SerializeReferenceField>();
            foreach (var field in LiveFields)
            {
                if (ReferenceEquals(field, this) || field.panel != panel) continue;
                if (!field.IsPropertyAlive()) continue;
                if (field._property.serializedObject.targetObject != target) continue;
                if (field._property.managedReferenceId != rid) continue;
                live[field._property.propertyPath] = field;
            }

            // Reveal the WHOLE group, not just the scroll target: the pulse covers every member, so every member
            // must be on screen — expand the collapsed foldouts over the live ones, and the lists hiding the
            // missing ones (their elements do not exist until their ListView builds its rows).
            var selfPath = _property.propertyPath;
            var expandedSomething = false;
            var missing = 0;
            foreach (var path in group)
            {
                if (path == selfPath) continue;

                if (live.TryGetValue(path, out var member))
                {
                    expandedSomething |= ExpandAncestorFoldouts(member);
                }
                else
                {
                    missing++;
                    expandedSomething |= RevealPath(path);
                }
            }

            // Rows of a just-expanded list build asynchronously, so while members are still missing wait a beat and
            // re-run — a pass where nothing NEW could be expanded still waits, because the previous pass's rows may
            // still be building. Retries exhausted → proceed with the members that do exist.
            if (missing > 0 && retriesLeft > 0)
            {
                schedule.Execute(() => NavigateToAliases(retriesLeft - 1)).StartingIn(RevealDelayMs);
                return;
            }

            // The scroll target: the next member in document order after the group's cursor (the member the
            // previous click scrolled to), so repeated clicks on the same notice walk the whole group; the clicked
            // field itself is skipped, and members whose element never appeared fall through to the next one.
            var key = (target.GetInstanceID(), rid);
            var start = NavigationCursor.TryGetValue(key, out var cursor) ? IndexOf(group, cursor) : -1;
            if (start < 0) start = IndexOf(group, selfPath);

            SerializeReferenceField next = null;
            for (var step = 1; step <= group.Count && next is null; step++)
            {
                var candidate = group[(start + step) % group.Count];
                if (candidate != selfPath) live.TryGetValue(candidate, out next);
            }

            if (next is null) return;

            // Committing to this member: the next click continues the walk from here.
            NavigationCursor[key] = next._property.propertyPath;

            // Let the expansion's layout pass run before the scroll, so ScrollTo aims at settled positions.
            if (expandedSomething) next.schedule.Execute(() => Reveal(next, live)).StartingIn(RevealDelayMs);
            else Reveal(next, live);
        }

        // Expands every collapsed Foldout between a member's element and the panel root (nested reference foldouts,
        // list foldouts, [Serializable] container foldouts alike), so a member hidden inside collapsed parents gets
        // real geometry. Returns true when anything was actually expanded.
        private static bool ExpandAncestorFoldouts(VisualElement element)
        {
            var expanded = false;
            for (var ancestor = element.hierarchy.parent; ancestor is not null; ancestor = ancestor.hierarchy.parent)
            {
                if (ancestor is not Foldout { value: false } foldout) continue;
                foldout.value = true;
                expanded = true;
            }

            return expanded;
        }

        private static void Reveal(SerializeReferenceField next, Dictionary<string, SerializeReferenceField> members)
        {
            // Scroll the OUTERMOST ScrollView — the inspector's own scroller. The nearest ancestor is usually a
            // ListView's internal ScrollView, whose scrollable range is zero in the inspector (lists grow to fit),
            // so ScrollTo there clamps to a no-op and a list-element member never comes into view. The IMGUI twin
            // scrolls the window's root ScrollView for the same reason.
            ScrollView outermost = null;
            for (var ancestor = next.GetFirstAncestorOfType<ScrollView>();
                 ancestor is not null;
                 ancestor = ancestor.GetFirstAncestorOfType<ScrollView>())
                outermost = ancestor;

            outermost?.ScrollTo(next);
            foreach (var member in members.Values) member.FlashSharedHighlight();
        }

        private static int IndexOf(IReadOnlyList<string> paths, string path)
        {
            for (var i = 0; i < paths.Count; i++)
                if (paths[i] == path)
                    return i;

            return -1;
        }

        // Expands every foldout along the path to a group member whose element does not exist yet: managed-reference
        // ancestors through their live fields, list containers through the ListViews bound to the path's prefixes —
        // scoped to this editor's InspectorElement so another component's identically named list is left alone.
        // Returns true when something was actually expanded (i.e. a retry is worth scheduling).
        private bool RevealPath(string path)
        {
            var root = (VisualElement)GetFirstAncestorOfType<InspectorElement>() ?? panel?.visualTree;
            if (root is null) return false;

            var target = _property.serializedObject.targetObject;
            var expanded = false;

            for (var dot = path.IndexOf('.'); dot >= 0; dot = path.IndexOf('.', dot + 1))
            {
                var prefix = path[..dot];

                // A managed-reference ancestor → its live field's own foldout.
                foreach (var field in LiveFields)
                {
                    if (field.panel != panel || !field.IsPropertyAlive()) continue;
                    if (field._property.serializedObject.targetObject != target) continue;
                    if (field._property.propertyPath != prefix) continue;
                    if (field._foldout.value) continue;
                    field._foldout.value = true;
                    expanded = true;
                }

                // A list ancestor → the foldout header of the ListView bound to the prefix.
                root.Query<ListView>().Where(list => list.bindingPath == prefix).ForEach(list =>
                {
                    var header = list.Q<Foldout>();
                    if (header is null || header.value) return;
                    header.value = true;
                    expanded = true;
                });
            }

            return expanded;
        }

        // Briefly tints the whole field in its shared-reference colour — the "here it is" pulse the group-navigation
        // click uses to point at the other members of a shared group. The tint lives on a dedicated overlay spanning
        // from the status stripe's line to the field's right edge, so the pulse and the stripe read as one band
        // (the root's own background starts at the indented content edge and would leave the gutter dark).
        private void FlashSharedHighlight()
        {
            if (_flashOverlay is null)
            {
                _flashOverlay = new VisualElement()
                    .AddClass(FlashClass)
                    .SetPickingMode(PickingMode.Ignore);
                // First child, so the stripe and the content render over the tint.
                Insert(0, _flashOverlay);
            }

            // Stretch the band to the inspector's right edge: the field root ends a few px short of it (inspector
            // padding, field margins), which left a dark sliver after the pulse. Measured per flash — the gap varies
            // with nesting depth — and applied as a negative `right`, extending the overlay past the root.
            var inspectorRoot = (VisualElement)GetFirstAncestorOfType<InspectorElement>() ?? panel?.visualTree;
            if (inspectorRoot is not null)
            {
                var overhang = worldBound.xMax - inspectorRoot.worldBound.xMax;
                if (!float.IsNaN(overhang)) _flashOverlay.style.right = overhang;
            }

            var from = _sharedColor;
            from.a = FlashAlpha;

            _flashOverlay.experimental.animation
                .Start(from, Color.clear, FlashDurationMs,
                    static (element, color) => element.style.backgroundColor = color)
                .Ease(HoldThenFadeEasing)
                .OnCompleted(() => _flashOverlay.style.backgroundColor = StyleKeyword.Null);
        }

        // The pulse holds its full tint for the first FlashHoldFraction of its life and then fades out linearly —
        // matching the IMGUI overlay's curve.
        private static float HoldThenFadeEasing(float t) =>
            t < FlashHoldFraction ? 0f : (t - FlashHoldFraction) / (1f - FlashHoldFraction);

        // Picks the left-edge stripe from the inputs cached by the notice updates: a missing type paints it the warning
        // amber (from USS), a shared reference the cached per-rid colour (inline); a field that is both takes the
        // warning (the more urgent). Nothing else shows a stripe.
        private void UpdateStripe()
        {
            // Reserve the stripe gutter (the foldout's left padding, via --striped) only while a stripe is actually
            // shown, so a plain or required-only field keeps its natural position.
            EnableInClassList(StripedClass, _isMissing || _isShared);

            if (_isMissing) ApplyWarningStripe();
            else if (_isShared) ApplySharedStripe(_sharedColor);
            else RemoveStripe();
        }

        // Missing type: the fixed warning amber, from the palette via the --warning class (inline colour cleared).
        private void ApplyWarningStripe()
        {
            EnsureStripe();
            _stripe.style.backgroundColor = StyleKeyword.Null;
            _stripe.EnableInClassList(StripeWarningClass, true);
            _stripe.EnableInClassList(StripeActiveClass, true);
        }

        // Shared reference: the reference's per-rid colour, set inline (unique per reference) so the stripe matches the
        // notice's swatch and tinted message down the field's left edge.
        private void ApplySharedStripe(Color color)
        {
            EnsureStripe();
            _stripe.EnableInClassList(StripeWarningClass, false);
            _stripe.style.backgroundColor = color;
            _stripe.EnableInClassList(StripeActiveClass, true);
        }

        private void EnsureStripe()
        {
            if (_stripe is not null) return;

            _stripe = new VisualElement()
                .AddClass(StripeClass)
                .SetPickingMode(PickingMode.Ignore);
            // Insert as the first child so it renders behind everything else.
            Insert(0, _stripe);
        }

        private void RemoveStripe()
        {
            if (_stripe is null) return;
            _stripe.EnableInClassList(StripeActiveClass, false);
            _stripe.EnableInClassList(StripeWarningClass, false);
            _stripe.style.backgroundColor = StyleKeyword.Null;
        }

        private void OpenFixSelector()
        {
            var window = EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;
            if (!window) return;

            // Anchor the picker to the notice itself (top-left + its own size) so ShowAsDropDown opens flush below the
            // notice's bottom edge. Passing yMax as the anchor top double-counted the height and dropped the picker a
            // full notice-row lower, leaving a visible gap — mirror the dropdown path, which anchors from yMin.
            var bound = _missingNotice.worldBound;
            var screenRect = new Rect(
                window.position.x + bound.xMin,
                window.position.y + bound.yMin,
                bound.width,
                bound.height);

            SerializeReferenceHelpers.ShowFixTypeSelector(_property, screenRect, () => ApplyReferenceChange(), _baseTypes);
        }

        private void OnFoldoutToggled(ChangeEvent<bool> evt)
        {
            if (evt.target != _foldout) return;
            _property.isExpanded = evt.newValue;
        }

        private void OnDropdownClicked(PointerDownEvent evt)
        {
            if (evt.button is not 0) return;

            var window = EditorWindow.focusedWindow != null
                ? EditorWindow.focusedWindow
                : EditorWindow.mouseOverWindow;

            if (!window) return;

            // Under mixed types there is no single "current" type to pre-highlight in the picker — open it unselected.
            var currentType = SerializeReferenceHelpers.HasMixedTypes(_property)
                ? null
                : SerializeReferenceHelpers.GetCurrentType(_property);
            var screenRect = GetScreenRect();

            TypeSelectorWindow.Show(
                screenRect: screenRect,
                filter: new TypeSelectorFilter
                {
                    Types = new[] { _fieldType },
                    Predicate = _filter,
                    AdditionalTypes = GenericTypeResolver.GetAssignableGenericDefinitions(_fieldType, _baseTypes),
                    ArgumentFilter = SerializeReferenceHelpers.IsValidGenericArgument,
                },
                currentAqn: currentType?.AssemblyQualifiedName ?? string.Empty,
                onSelected: assemblyQualifiedName => Apply(string.IsNullOrEmpty(assemblyQualifiedName)
                    ? null
                    : Type.GetType(assemblyQualifiedName, throwOnError: false)));

            evt.StopPropagation();
            return;

            void Apply(Type type)
            {
                // Multi-object: each target gets its OWN instance, created from that target's previous value, so the
                // managed reference is never aliased across objects; <None> clears all. One Undo step covers them all.
                if (SerializeReferenceHelpers.IsEditingMultipleObjects(_property))
                {
                    SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                        _property,
                        previous => SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));

                    // After a per-target pick the selection shares the new type, so the live foldout drives expansion;
                    // set it on the live property (the per-target writes went through disposed SerializedObjects).
                    _property.isExpanded = type is not null;
                }
                else
                {
                    var previous = _property.managedReferenceValue;
                    _property.SetManagedReferenceAndApply(SerializeReferenceHelpers.CreateInstancePreservingData(type, previous));
                    _property.isExpanded = type is not null;
                }

                ApplyReferenceChange();
            }

            Rect GetScreenRect() => new(
                window.position.x + _dropdown.worldBound.xMin,
                window.position.y + _dropdown.worldBound.yMin,
                _dropdown.worldBound.width,
                _dropdown.worldBound.height);
        }

        private void BuildContextMenu(ContextualMenuPopulateEvent evt)
        {
            // Copy reads the first target's value (Unity's own convention for a multi-selection menu). Paste then
            // applies an independent instance PER target, so the pasted reference is never aliased across objects.
            evt.menu.AppendAction("Copy Serialize Reference",
                _ => SerializeReferenceClipboard.Copy(_property.managedReferenceValue));

            var canPaste = SerializeReferenceClipboard.CanPasteInto(_fieldType, _filter);

            evt.menu.AppendAction("Paste Serialize Reference",
                _ => PasteFromClipboard(),
                canPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);

            // Make-unique is a single-asset cross-reference operation; it is only offered (and only correct) for a
            // single target — under a multi-object selection the shared-reference notice is already suppressed.
            if (SerializeReferenceHelpers.NoticesApply(_property) &&
                SerializeReferenceHelpers.HasSharedReference(_property))
                evt.menu.AppendAction("Make Unique Reference", _ => MakeUnique());

            // Find every asset/field using the current type, via the sr: Quick Search provider.
            var usagesType = SerializeReferenceHelpers.GetCurrentType(_property);
            if (usagesType != null)
                evt.menu.AppendAction($"Find Usages of {usagesType.Name}",
                    _ => SerializeReferenceUsageSearchProvider.OpenSearch(usagesType));

            // Link this field to an existing instance of the same object (the inverse of Make Unique), single-target only.
            if (SerializeReferenceHelpers.NoticesApply(_property))
                foreach (var candidate in SerializeReferenceLinker.CollectLinkCandidates(_property))
                {
                    var path = candidate.Path;
                    evt.menu.AppendAction($"Link to Existing/{candidate.Type.Name}  ({path})", _ => LinkToExisting(path));
                }

            // Generate a new subclass of the field's type and assign it once it compiles.
            if (_fieldType != null)
                evt.menu.AppendAction("Create New Script…", _ => CreateNewScript());

            // Save the current instance as a durable named template, and paste any assignable saved template.
            if (usagesType != null)
                evt.menu.AppendAction("Save as Template…", _ => SaveAsTemplate(usagesType));

            foreach (var template in SerializeReferenceTemplates.LoadResolved())
            {
                if (_fieldType != null && !_fieldType.IsAssignableFrom(template.Type)) continue;
                if (!_filter(template.Type)) continue;
                var name = template.Name;
                evt.menu.AppendAction($"Paste Template/{name}", _ => ApplyTemplate(name));
            }
        }

        private void RegisterDragAndDrop(VisualElement target)
        {
            target.RegisterCallback<DragEnterEvent>(_ => UpdateDrag(target));
            target.RegisterCallback<DragUpdatedEvent>(_ => UpdateDrag(target));
            target.RegisterCallback<DragLeaveEvent>(_ => target.RemoveFromClassList(DropTargetClass));
            target.RegisterCallback<DragPerformEvent>(_ => PerformDrop(target));
            target.RegisterCallback<DetachFromPanelEvent>(_ => target.RemoveFromClassList(DropTargetClass));
        }

        private void UpdateDrag(VisualElement target)
        {
            if (SerializeReferenceDropHandler.TryResolveDroppedType(_fieldType, _baseTypes, out _))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                target.AddToClassList(DropTargetClass);
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                target.RemoveFromClassList(DropTargetClass);
            }
        }

        private void PerformDrop(VisualElement target)
        {
            target.RemoveFromClassList(DropTargetClass);
            if (!SerializeReferenceDropHandler.TryResolveDroppedType(_fieldType, _baseTypes, out var type)) return;

            DragAndDrop.AcceptDrag();
            SerializeReferenceDropHandler.Assign(_property, type);
            ApplyReferenceChange();
        }

        private void LinkToExisting(string sourcePath)
        {
            if (SerializeReferenceLinker.LinkTo(_property, sourcePath)) ApplyReferenceChange();
        }

        private void CreateNewScript()
        {
            if (!SerializeReferenceScriptCreator.TryCreateSubclassStub(_fieldType, out _, out var fullTypeName)) return;

            // Multi-object: enqueue one pending assignment PER target so every selected object gets the new type after
            // the script compiles — each entry carries its own GlobalObjectId, so the (GlobalId, path)-keyed queue
            // keeps them apart. Enqueuing only targetObject (singular) would leave objects 2..N untouched.
            foreach (var target in _property.serializedObject.targetObjects)
                SerializeReferencePendingAssignment.Enqueue(target, _property.propertyPath, fullTypeName);
        }

        private void SaveAsTemplate(Type type)
        {
            var value = _property.managedReferenceValue;
            if (value is null) return;

            SerializeReferenceNamePrompt.Show("Save Template", SerializeReferenceTemplates.SuggestName(type),
                name => SerializeReferenceTemplates.SaveConfirmed(name, value));
        }

        private void ApplyTemplate(string name)
        {
            if (SerializeReferenceHelpers.IsEditingMultipleObjects(_property))
            {
                SerializeReferenceHelpers.ApplyManagedReferencePerTarget(_property, _ => SerializeReferenceTemplates.CreateInstance(name));
            }
            else
            {
                var instance = SerializeReferenceTemplates.CreateInstance(name);
                if (instance is null) return;
                _property.SetManagedReferenceAndApply(instance);
            }

            ApplyReferenceChange();
        }

        private void MakeUnique()
        {
            SerializeReferenceHelpers.MakeReferenceUnique(_property);
            ApplyReferenceChange();
        }

        // Refreshes this field after it reassigned its managed reference, then notifies sibling fields so they can re-
        // evaluate their own shared-reference notice (see ManagedReferencesChanged).
        private void ApplyReferenceChange()
        {
            // MakeReferenceUnique (and the other mutations) apply through a throwaway "persistent" SerializedObject, so
            // this field's LIVE object is left stale. Pull the change in, then drop the per-frame alias memo (keyed by
            // frame + object instance, not content, so it survives the Update) — otherwise the re-query below AND the
            // siblings read the pre-mutation snapshot and still report the just-broken alias as shared.
            _property.serializedObject.Update();
            SerializeReferenceHelpers.InvalidateSharedReferenceCache();
            Refresh(forceRebuild: true);
            ManagedReferencesChanged?.Invoke();
        }

        // A field somewhere reassigned its managed reference; this field may have started or stopped sharing a rid with
        // it, so its notice is re-evaluated. Pull the mutation into this field's live object first (a sibling may hold a
        // separate SerializedObject instance) so the re-query sees the current ids. Guarded — the property may be gone.
        private void OnManagedReferencesChanged()
        {
            if (!IsPropertyAlive()) return;
            _property.serializedObject.Update();
            Refresh(forceRebuild: false);
        }

        // Undo/redo changed the object; re-evaluate this field's notice with fresh data — the reverted managed reference
        // may have re-aliased or un-aliased this field. The per-frame alias memo is dropped ONCE globally (the static
        // hook in SerializeReferenceHelpers runs before any field handler — it subscribed at domain load), so N live
        // fields rebuild it a single time instead of each field's invalidation discarding the previous rebuild.
        private void OnUndoRedo()
        {
            if (!IsPropertyAlive()) return;
            _property.serializedObject.Update();
            Refresh(forceRebuild: false);
        }

        private void PasteFromClipboard()
        {
            // Multi-object: rebuild a fresh instance from the clipboard for EACH target so no two objects share the same
            // managed reference (CreateInstance already returns an independent instance per call). One Undo step covers all.
            if (SerializeReferenceHelpers.IsEditingMultipleObjects(_property))
            {
                SerializeReferenceHelpers.ApplyManagedReferencePerTarget(
                    _property,
                    _ => SerializeReferenceClipboard.CreateInstance());

                // All targets now share the pasted type, so the live foldout drives expansion; set it on the live
                // property (the per-target writes went through disposed SerializedObjects). Type is null only on an
                // empty-reference paste, which collapses the foldout — matching the single-object branch.
                _property.isExpanded = SerializeReferenceClipboard.Type is not null;
            }
            else
            {
                var value = SerializeReferenceClipboard.CreateInstance();
                _property.SetManagedReferenceAndApply(value);
                _property.isExpanded = value is not null;
            }

            ApplyReferenceChange();
        }
    }
}
