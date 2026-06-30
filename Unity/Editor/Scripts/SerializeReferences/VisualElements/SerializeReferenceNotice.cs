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

        // Trailing rid dot — the only per-rid coloured element; the rest of the row keeps the warning palette.
        private const string DotClass = NoticeClass + "__dot";
        private const string DotVisibleClass = DotClass + "--visible";

        // Info variant — a non-actionable, dim blue hint (e.g. the multi-object "different types" notice) rather than
        // the default actionable yellow warning. Swaps the icon and palette through the modifier class only.
        private const string InfoModifierClass = NoticeClass + "--info";

        private readonly VisualElement _icon;
        private readonly Label _message;
        private readonly Label _action;
        private readonly Label _suggestion;
        private readonly VisualElement _dot;

        private Action _onAction;
        private Action _onSuggestion;

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

            _suggestion = new Label().AddClass(SuggestionClass);
            _suggestion.RegisterCallback<ClickEvent>(_ => _onSuggestion?.Invoke());

            // Trailing rid dot: a small colour-coded circle after the action word; its colour is set inline from code.
            _dot = new VisualElement()
                .AddClass(DotClass)
                .SetPickingMode(PickingMode.Ignore);

            this.AddChild(_icon)
                .AddChild(_message)
                .AddChild(_action)
                .AddChild(_suggestion)
                .AddChild(_dot);
        }

        /// <summary>
        /// Updates the notice content. The <paramref name="actionText"/> word is the only clickable part;
        /// pass an empty string to hide it (e.g. when the action is unavailable for unsaved targets). Setting the
        /// notice also clears any previously shown <see cref="SetSuggestion"/> segment.
        /// <paramref name="dotColor"/>, when given, shows a small colour-coded dot trailing the action word in that
        /// colour — used by the shared-reference notice to carry its per-rid colour, so aliased fields can be matched at
        /// a glance while the row's text and left stripe stay the warning palette. Omit it (the missing-type notice) to
        /// hide the dot.
        /// </summary>
        public void Set(string message, string actionText, string detail, Action onAction, Color? dotColor = null)
        {
            EnableInClassList(InfoModifierClass, false);

            _message.text = message;
            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionText) && onAction is not null;
            _action.text = actionText;
            _action.SetDisplay(hasAction ? DisplayStyle.Flex : DisplayStyle.None);

            ApplyDotColor(dotColor);

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

            ApplyDotColor(null);

            tooltip = detail;
            ClearSuggestion();
        }

        // Shows (with a colour) or hides the trailing rid dot — the only element tinted per-rid. The colour is set inline
        // since it is unique per reference; the --visible modifier reveals the dot, and clearing it leaves the USS default.
        private void ApplyDotColor(Color? color)
        {
            if (color.HasValue)
            {
                _dot.EnableInClassList(DotVisibleClass, true);
                _dot.style.backgroundColor = color.Value;
            }
            else
            {
                _dot.EnableInClassList(DotVisibleClass, false);
                _dot.style.backgroundColor = StyleKeyword.Null;
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
