using System;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// A compact, single-row warning notice for the <c>[TypeSelector]</c> drawer on <c>[SerializeReference]</c> fields: a small
    /// warning icon, a short yellow message and an underlined, clickable action word (e.g. <c>Fix</c>). A missing-type
    /// notice can carry an optional second clickable segment — the <b>Smart Fix</b> suggestion (e.g. <c>· → Pistol?</c>) —
    /// that applies the best ranked repair candidate in one click. The full explanation is surfaced on hover through each
    /// segment's <see cref="VisualElement.tooltip"/>, so the inspector row stays terse while the detail is one hover away.
    /// Replaces the bulky <c>AspidHelpBox</c>-plus-button pair previously used for missing-type and shared-reference states.
    /// An <see cref="SetInfo"/> variant re-tints the row to a dim, non-actionable info palette — used for the
    /// multi-object "different types" hint that stands in for the suppressed child fields. A shared-reference call (one
    /// that passes a rid <c>dotColor</c>) instead flips the row to a calm link treatment — the leading swatch, the
    /// message and the Make-unique action all tinted in that per-rid colour, with no icon — since a shared reference is
    /// attention, not an error. On that shared row the message itself is a third clickable segment (see
    /// <c>onNavigate</c>): it reveals the other members of the group, so "who else uses this?" is one click away.
    /// </summary>
    internal sealed class SerializeReferenceNotice : VisualElement
    {
        // Self-contained component (reused on the [SerializeReference] field AND the string [TypeSelector(Required)]
        // path), so it loads its own stylesheet rather than depending on a host having added it.
        private const string StyleSheetPath = "UI/SerializeReferences/Aspid-FastTools-SerializeReference";

        // Own BEM block (a reusable notice, not an element of the serialize-reference field block).
        private const string NoticeClass = "aspid-fasttools-reference-notice";
        private const string IconClass = NoticeClass + "__icon";
        private const string MessageClass = NoticeClass + "__message";
        private const string ActionClass = NoticeClass + "__action";
        private const string SuggestionClass = NoticeClass + "__suggestion";
        private const string SuggestionVisibleClass = SuggestionClass + "--visible";
        private const string SuggestionSeparatorClass = NoticeClass + "__suggestion-separator";
        private const string SuggestionSeparatorVisibleClass = SuggestionSeparatorClass + "--visible";

        // Marks the message as the shared notice's click-to-navigate segment (link cursor via USS); its hover lighten
        // comes from code, since the rid colour is dynamic.
        private const string MessageNavigableClass = MessageClass + "--navigable";

        // Leading rid swatch — a small colour-coded circle at the head of the shared-reference row. Its per-rid colour
        // is shared inline with the message text and the field's left stripe, so the whole field reads in one colour
        // and aliased fields match at a glance.
        private const string DotClass = NoticeClass + "__dot";
        private const string DotVisibleClass = DotClass + "--visible";

        // Info variant — a non-actionable, dim blue hint (e.g. the multi-object "different types" notice) rather than
        // the default actionable yellow warning. Swaps the icon and palette through the modifier class only.
        private const string InfoModifierClass = NoticeClass + "--info";

        // Shared-reference variant — added whenever the notice carries a rid swatch (dotColor). Softens the row from the
        // warning amber to a calm "these are linked" treatment (no icon; swatch, message and Make-unique action all in
        // the rid colour, the action pinned right), so a shared reference reads as attention rather than an error.
        // Applied through the modifier class only; the per-rid colour itself is set inline (see ApplySharedColor).
        private const string SharedModifierClass = NoticeClass + "--shared";

        // How far the shared action's colour is lightened toward white on hover — the hover feedback (in place of an
        // underline), since the rid colour is dynamic and cannot be brightened from a static USS rule.
        private const float ActionHoverLighten = 0.35f;

        private readonly VisualElement _icon;
        private readonly Label _message;
        private readonly Label _action;
        private readonly Label _suggestion;
        private readonly Label _suggestionSeparator;
        private readonly VisualElement _dot;

        private Action _onAction;
        private Action _onSuggestion;
        private Action _onNavigate;

        // The shared notice's resting rid colour (null when not a shared notice), so the hover handlers on the action
        // and the navigable message know whether to brighten and what to restore to; the missing-type action keeps its
        // USS hover instead.
        private Color? _sharedColor;

        public SerializeReferenceNotice()
        {
            // Base palette first (via the theme helper), then the feature sheet, then the block class.
            this.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(NoticeClass);

            _icon = new VisualElement()
                .AddClass(IconClass)
                .SetPickingMode(PickingMode.Ignore);

            // Ignores picking by default; the shared notice re-enables it (see Set) so a click on the message reveals
            // the other members of the group, with the same code-driven hover lighten as the action beside it.
            _message = new Label()
                .AddClass(MessageClass)
                .SetPickingMode(PickingMode.Ignore);
            _message.RegisterCallback<ClickEvent>(_ => _onNavigate?.Invoke());
            _message.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (_onNavigate is not null && _sharedColor.HasValue)
                    _message.style.color = Color.Lerp(_sharedColor.Value, Color.white, ActionHoverLighten);
            });
            _message.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (_sharedColor.HasValue) _message.style.color = _sharedColor.Value;
            });

            _action = new Label().AddClass(ActionClass);
            _action.RegisterCallback<ClickEvent>(_ => _onAction?.Invoke());
            // Shared-notice hover: brighten the rid colour (the missing-type action has no _sharedColor and keeps its
            // USS hover instead).
            _action.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (_sharedColor.HasValue) _action.style.color = Color.Lerp(_sharedColor.Value, Color.white, ActionHoverLighten);
            });
            _action.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (_sharedColor.HasValue) _action.style.color = _sharedColor.Value;
            });

            // The "·" between the Fix action and the Smart Fix suggestion is DECORATION, not part of either action:
            // its own element, never underlined, never clickable, so the underline keeps meaning "this is a button".
            _suggestionSeparator = new Label("·").AddClass(SuggestionSeparatorClass);
            _suggestionSeparator.pickingMode = PickingMode.Ignore;

            _suggestion = new Label().AddClass(SuggestionClass);
            _suggestion.RegisterCallback<ClickEvent>(_ => _onSuggestion?.Invoke());

            // Leading rid swatch: a small colour-coded circle before the icon; its colour is set inline from code. Only
            // ever shown on the shared-reference notice (the sole caller passing a dotColor), so it can lead the row
            // unconditionally — matching swatches then line up down the left edge and read as one group.
            _dot = new VisualElement()
                .AddClass(DotClass)
                .SetPickingMode(PickingMode.Ignore);

            this.AddChild(_dot)
                .AddChild(_icon)
                .AddChild(_message)
                .AddChild(_action)
                .AddChild(_suggestionSeparator)
                .AddChild(_suggestion);
        }

        /// <summary>
        /// Updates the notice content. The <paramref name="actionText"/> word is the primary clickable part;
        /// pass an empty string to hide it (e.g. when the action is unavailable for unsaved targets). Setting the
        /// notice also clears any previously shown <see cref="SetSuggestion"/> segment.
        /// <paramref name="dotColor"/>, when given, marks this as the shared-reference notice: it leads the row with a
        /// small colour-coded swatch in that colour and switches to the calm link treatment, so aliased fields can be
        /// matched at a glance by their lined-up swatches. Omit it (the missing-type / required notices) to keep the
        /// default warning palette and hide the swatch.
        /// <paramref name="onNavigate"/>, when given, makes the message itself clickable (link cursor + hover lighten) —
        /// the shared notice's "show me the other members of this group" affordance.
        /// </summary>
        public void Set(string message, string actionText, string detail, Action onAction, Color? dotColor = null,
            Action onNavigate = null)
        {
            EnableInClassList(InfoModifierClass, false);

            _message.text = message;
            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionText) && onAction is not null;
            _action.text = Underline(actionText);
            _action.SetDisplay(hasAction ? DisplayStyle.Flex : DisplayStyle.None);

            // A navigable message picks up pointer events (the default is Ignore so a plain notice never eats clicks).
            _onNavigate = onNavigate;
            _message.SetPickingMode(onNavigate is not null ? PickingMode.Position : PickingMode.Ignore);
            _message.EnableInClassList(MessageNavigableClass, onNavigate is not null);

            ApplySharedColor(dotColor);

            // A rid swatch is unique to the shared-reference notice; it also flips the row to the calm link treatment.
            EnableInClassList(SharedModifierClass, dotColor.HasValue);

            tooltip = detail;
            ClearSuggestion();
        }

        /// <summary>
        /// Configures the notice as a dim, non-actionable info hint: an info icon, dim text and no clickable segments.
        /// Used for the multi-object "different types" notice, which only explains why the per-instance child fields are
        /// hidden and offers nothing to click.
        /// </summary>
        public void SetInfo(string message, string detail)
        {
            EnableInClassList(InfoModifierClass, true);
            EnableInClassList(SharedModifierClass, false);

            _message.text = message;
            _onAction = null;
            _action.text = string.Empty;
            _action.SetDisplay(DisplayStyle.None);

            _onNavigate = null;
            _message.SetPickingMode(PickingMode.Ignore);
            _message.EnableInClassList(MessageNavigableClass, false);

            ApplySharedColor(null);

            tooltip = detail;
            ClearSuggestion();
        }

        // Applies (or, with null, clears) the shared reference's per-rid colour. It is unique per reference, so it is set
        // inline from code: it fills the leading swatch, tints the "Shared reference" message AND the "Make unique"
        // action (cached in _sharedColor for the hover-lighten handlers), and the field paints its left stripe the same
        // colour, so the whole row reads in the rid's colour. The --visible modifier reveals the swatch; clearing
        // reverts to the USS palette (the missing-type / required warning amber).
        private void ApplySharedColor(Color? color)
        {
            _sharedColor = color;

            if (color.HasValue)
            {
                _dot.EnableInClassList(DotVisibleClass, true);
                _dot.style.backgroundColor = color.Value;
                _message.style.color = color.Value;
                _action.style.color = color.Value;
            }
            else
            {
                _dot.EnableInClassList(DotVisibleClass, false);
                _dot.style.backgroundColor = StyleKeyword.Null;
                _message.style.color = StyleKeyword.Null;
                _action.style.color = StyleKeyword.Null;
            }
        }

        /// <summary>
        /// Shows (or, with an empty <paramref name="suggestionText"/>, hides) the trailing Smart Fix suggestion segment —
        /// a second underlined clickable word that applies the best ranked repair candidate. Its own
        /// <paramref name="detail"/> tooltip carries the suggestion reason and the full type name.
        /// </summary>
        public void SetSuggestion(string suggestionText, string detail, Action onSuggestion)
        {
            _onSuggestion = onSuggestion;

            var hasSuggestion = !string.IsNullOrEmpty(suggestionText) && onSuggestion is not null;
            _suggestion.text = Underline(suggestionText);
            _suggestion.tooltip = detail;
            _suggestion.EnableInClassList(SuggestionVisibleClass, hasSuggestion);
            _suggestionSeparator.EnableInClassList(SuggestionSeparatorVisibleClass, hasSuggestion);
        }

        private void ClearSuggestion()
        {
            _onSuggestion = null;
            _suggestion.text = string.Empty;
            _suggestion.tooltip = null;
            _suggestion.EnableInClassList(SuggestionVisibleClass, false);
            _suggestionSeparator.EnableInClassList(SuggestionSeparatorVisibleClass, false);
        }

        // Every clickable action word is underlined (rich-text <u>, since USS has no text-decoration) so "this is a
        // button" reads the same on the Fix, Smart Fix and Make-unique segments — colour alone stays free to carry
        // group identity on the shared notice. The IMGUI drawer draws the matching underline by hand.
        private static string Underline(string text) =>
            string.IsNullOrEmpty(text) ? text : $"<u>{text}</u>";
    }
}
