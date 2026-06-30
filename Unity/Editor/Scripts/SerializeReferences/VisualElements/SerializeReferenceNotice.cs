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
    /// multi-object "different types" hint that stands in for the suppressed child fields.
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

        // Info variant — a non-actionable, dim blue hint (e.g. the multi-object "different types" notice) rather than
        // the default actionable yellow warning. Swaps the icon and palette through the modifier class only.
        private const string InfoModifierClass = NoticeClass + "--info";

        private readonly VisualElement _icon;
        private readonly Label _message;
        private readonly Label _action;
        private readonly Label _suggestion;

        private Action _onAction;
        private Action _onSuggestion;
        private Color? _accentColor;

        public SerializeReferenceNotice()
        {
            // Base palette first (via the theme helper), then the feature sheet, then the block class.
            this.AddAspidThemeStyleSheets()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(NoticeClass);

            _icon = new VisualElement()
                .AddClass(IconClass)
                .SetPickingMode(PickingMode.Ignore);

            _message = new Label()
                .AddClass(MessageClass)
                .SetPickingMode(PickingMode.Ignore);

            _action = new Label().AddClass(ActionClass);
            _action.RegisterCallback<ClickEvent>(_ => _onAction?.Invoke());
            // The accent colour (when set) replaces the USS hover rule, which an inline colour would otherwise shadow —
            // so the hover-lighten effect is reproduced here for the accented case only.
            _action.RegisterCallback<PointerEnterEvent>(_ => ApplyActionAccent(hover: true));
            _action.RegisterCallback<PointerLeaveEvent>(_ => ApplyActionAccent(hover: false));

            _suggestion = new Label().AddClass(SuggestionClass);
            _suggestion.RegisterCallback<ClickEvent>(_ => _onSuggestion?.Invoke());

            this.AddChild(_icon)
                .AddChild(_message)
                .AddChild(_action)
                .AddChild(_suggestion);
        }

        /// <summary>
        /// Updates the notice content. The <paramref name="actionText"/> word is the only clickable part;
        /// pass an empty string to hide it (e.g. when the action is unavailable for unsaved targets). Setting the
        /// notice also clears any previously shown <see cref="SetSuggestion"/> segment.
        /// <paramref name="accentColor"/>, when given, tints the message and action text with that colour instead of
        /// the default warning yellow — used by the shared-reference notice to match its rid-colour stripe, so the
        /// text itself (not just the stripe) identifies which other fields share the same instance.
        /// </summary>
        public void Set(string message, string actionText, string detail, Action onAction, Color? accentColor = null)
        {
            EnableInClassList(InfoModifierClass, false);

            _message.text = message;
            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionText) && onAction is not null;
            _action.text = actionText;
            _action.SetDisplay(hasAction ? DisplayStyle.Flex : DisplayStyle.None);

            ApplyAccentColor(accentColor);

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

            _message.text = message;
            _onAction = null;
            _action.text = string.Empty;
            _action.SetDisplay(DisplayStyle.None);

            ApplyAccentColor(null);

            tooltip = detail;
            ClearSuggestion();
        }

        private void ApplyAccentColor(Color? color)
        {
            _accentColor = color;

            if (color.HasValue)
            {
                _message.style.color = color.Value;
                ApplyActionAccent(hover: false);
            }
            else
            {
                _message.style.color = StyleKeyword.Null;
                _action.style.color = StyleKeyword.Null;
                _action.style.borderBottomColor = StyleKeyword.Null;
            }
        }

        // Mirrors the USS :hover rule's lighten effect for the accented case, where an inline colour would otherwise
        // shadow that rule. A no-op while unaccented, so the default warning palette keeps its normal USS hover.
        private void ApplyActionAccent(bool hover)
        {
            if (!_accentColor.HasValue) return;

            var color = hover ? Color.Lerp(_accentColor.Value, Color.white, 0.35f) : _accentColor.Value;
            _action.style.color = color;
            _action.style.borderBottomColor = color;
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
            _suggestion.text = suggestionText;
            _suggestion.tooltip = detail;
            _suggestion.EnableInClassList(SuggestionVisibleClass, hasSuggestion);
        }

        private void ClearSuggestion()
        {
            _onSuggestion = null;
            _suggestion.text = string.Empty;
            _suggestion.tooltip = null;
            _suggestion.EnableInClassList(SuggestionVisibleClass, false);
        }
    }
}
