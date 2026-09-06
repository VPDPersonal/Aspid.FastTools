using System;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A compact single-row notice for the drawer on [SerializeReference] fields: a warning icon, a short amber
    // message and an underlined clickable action word, with an optional Smart Fix segment that applies the best
    // ranked repair candidate in one click. Every segment's detail rides its hover tooltip, so the inspector row
    // stays terse.
    //
    // The info variant re-tints the row to a dim, non-actionable palette for the multi-object "different types"
    // hint. A shared-reference call passes a rid color and flips the row to a calm link treatment — leading swatch,
    // message and action all in that color, no icon — since a shared reference is attention, not an error. There the
    // message is a third clickable segment that reveals the other members of the group.
    internal sealed class InspectorNotice : VisualElement
    {
        // Shared by the [SerializeReference] and [TypeSelector] drawers, so it loads its own stylesheet.
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-InspectorNotice";

        // Own BEM block (a reusable notice, not an element of any field block).
        private const string NoticeClass = "aspid-fasttools-inspector-notice";
        private const string IconClass = NoticeClass + "__icon";
        private const string MessageClass = NoticeClass + "__message";
        private const string ActionClass = NoticeClass + "__action";
        private const string SuggestionClass = NoticeClass + "__suggestion";
        private const string SuggestionVisibleClass = SuggestionClass + "--visible";
        private const string SuggestionSeparatorClass = NoticeClass + "__suggestion-separator";
        private const string SuggestionSeparatorVisibleClass = SuggestionSeparatorClass + "--visible";

        // Marks the message as the shared notice's click-to-navigate segment (link cursor via USS); its hover lighten
        // comes from code, since the rid color is dynamic.
        private const string MessageNavigableClass = MessageClass + "--navigable";

        // Leading rid swatch — a small color-coded circle at the head of the shared-reference row; its per-rid
        // color is shared inline with the message text and the field's left stripe.
        private const string DotClass = NoticeClass + "__dot";
        private const string DotVisibleClass = DotClass + "--visible";

        // Info variant — a non-actionable, dim blue hint rather than the default actionable yellow warning.
        private const string InfoModifierClass = NoticeClass + "--info";

        // Shared-reference variant — added whenever the notice carries a rid swatch (dotColor). Softens the warning
        // amber to a calm "linked" treatment (no icon, action pinned right); the per-rid color itself is set inline.
        private const string SharedModifierClass = NoticeClass + "--shared";

        // How far the shared action's color is lightened toward white on hover — the hover feedback (in place of an
        // underline), since the rid color is dynamic and cannot be brightened from a static USS rule.
        private const float ActionHoverLighten = 0.35f;

        private readonly Label _message;
        private readonly Label _action;
        private readonly Label _suggestion;
        private readonly Label _suggestionSeparator;
        private readonly VisualElement _dot;

        private Action _onAction;
        private Action _onSuggestion;
        private Action _onNavigate;

        // The shared notice's resting rid color (null when not a shared notice): the hover handlers brighten from
        // and restore to it; the missing-type action keeps its USS hover instead.
        private Color? _sharedColor;

        public InspectorNotice()
        {
            // Base palette first (via the theme helper), then the feature sheet, then the block class.
            this.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(NoticeClass);

            var icon = new VisualElement()
                .AddClass(IconClass)
                .SetPickingMode(PickingMode.Ignore);

            // Ignores picking by default; the shared notice re-enables it (see Set) so a message click reveals
            // the group's other members.
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
            // Shared-notice hover: brighten the rid color (the missing-type action has no _sharedColor and keeps its
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

            // Only ever shown on the shared-reference notice, so it can lead the row unconditionally — matching
            // swatches line up down the left edge.
            _dot = new VisualElement()
                .AddClass(DotClass)
                .SetPickingMode(PickingMode.Ignore);

            this.AddChild(_dot)
                .AddChild(icon)
                .AddChild(_message)
                .AddChild(_action)
                .AddChild(_suggestionSeparator)
                .AddChild(_suggestion);
        }

        // An empty actionText hides the action word; setting the notice also clears any suggestion segment.
        // dotColor marks this as the shared-reference notice, leading the row with a swatch in that color and
        // switching to the calm link treatment, so aliased fields match at a glance. onNavigate makes the message
        // itself clickable — the shared notice's "show me the other members" affordance.
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

        // Configures the notice as a dim, non-actionable info hint: an info icon, dim text and no clickable segments.
        // Used for the multi-object "different types" notice, which only explains why the per-instance child fields are
        // hidden and offers nothing to click.
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

        // Applies (or, with null, clears) the per-rid color. Unique per reference, so set inline: it fills the
        // swatch and tints the message and action (cached in _sharedColor for the hover handlers); clearing
        // reverts to the USS palette.
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

        // Shows (or, with an empty suggestionText, hides) the trailing Smart Fix suggestion segment —
        // a second underlined clickable word that applies the best ranked repair candidate. Its own
        // detail tooltip carries the suggestion reason and the full type name.
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

        // Rich-text <u>, since USS has no text-decoration — the underline means "this is a button", leaving color
        // free to carry group identity. The IMGUI drawer draws the matching underline by hand.
        private static string Underline(string text) =>
            string.IsNullOrEmpty(text) ? text : $"<u>{text}</u>";
    }
}
