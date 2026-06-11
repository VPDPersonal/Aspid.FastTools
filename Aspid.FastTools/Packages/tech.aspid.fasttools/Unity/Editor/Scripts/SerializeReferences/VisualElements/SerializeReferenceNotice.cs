using System;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

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
        private const string NoticeClass = "aspid-fasttools-serialize-reference-notice";
        private const string IconClass = NoticeClass + "__icon";
        private const string MessageClass = NoticeClass + "__message";
        private const string ActionClass = NoticeClass + "__action";
        private const string SuggestionClass = NoticeClass + "__suggestion";
        private const string RidChipClass = NoticeClass + "__rid-chip";

        // Info variant — a non-actionable, dim blue hint (e.g. the multi-object "different types" notice) rather than
        // the default actionable yellow warning. Swaps the icon and palette through the modifier class only.
        private const string InfoModifierClass = NoticeClass + "--info";

        private readonly Label _message;
        private readonly Label _action;
        private readonly Label _suggestion;
        private readonly VisualElement _ridChip;

        private Action _onAction;
        private Action _onSuggestion;

        public SerializeReferenceNotice()
        {
            this.AddClass(NoticeClass);

            var icon = new VisualElement()
                .AddClass(IconClass)
                .SetPickingMode(PickingMode.Ignore);

            _message = new Label()
                .AddClass(MessageClass)
                .SetPickingMode(PickingMode.Ignore);

            _action = new Label().AddClass(ActionClass);
            _action.RegisterCallback<ClickEvent>(_ => _onAction?.Invoke());

            _suggestion = new Label().AddClass(SuggestionClass);
            _suggestion.RegisterCallback<ClickEvent>(_ => _onSuggestion?.Invoke());

            // The rid chip is a small round dot appended after the action; its background colour is set
            // inline from code so the same rid always shows the same colour. Hidden by default.
            _ridChip = new VisualElement()
                .AddClass(RidChipClass)
                .SetPickingMode(PickingMode.Ignore);
            _ridChip.SetDisplay(DisplayStyle.None);

            this.AddChild(icon)
                .AddChild(_message)
                .AddChild(_action)
                .AddChild(_suggestion)
                .AddChild(_ridChip);
        }

        /// <summary>
        /// Updates the notice content. The <paramref name="actionText"/> word is the only clickable part;
        /// pass an empty string to hide it (e.g. when the action is unavailable for unsaved targets). Setting the
        /// notice also clears any previously shown <see cref="SetSuggestion"/> segment and any rid chip.
        /// </summary>
        public void Set(string message, string actionText, string detail, Action onAction)
        {
            EnableInClassList(InfoModifierClass, false);

            _message.text = message;
            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionText) && onAction is not null;
            _action.text = actionText;
            _action.SetDisplay(hasAction ? DisplayStyle.Flex : DisplayStyle.None);

            tooltip = detail;
            ClearSuggestion();
            ClearRidChip();
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

            tooltip = detail;
            ClearSuggestion();
            ClearRidChip();
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
            _suggestion.SetDisplay(hasSuggestion ? DisplayStyle.Flex : DisplayStyle.None);
        }

        /// <summary>
        /// Shows a small round colour chip after the action word, tinted with <paramref name="color"/>.
        /// Used by the shared-reference notice to make aliased fields identifiable by their rid colour.
        /// </summary>
        public void SetRidChip(Color color)
        {
            _ridChip.style.backgroundColor = color;
            _ridChip.SetDisplay(DisplayStyle.Flex);
        }

        private void ClearSuggestion()
        {
            _onSuggestion = null;
            _suggestion.text = string.Empty;
            _suggestion.tooltip = null;
            _suggestion.SetDisplay(DisplayStyle.None);
        }

        private void ClearRidChip()
        {
            _ridChip.style.backgroundColor = StyleKeyword.Null;
            _ridChip.SetDisplay(DisplayStyle.None);
        }
    }
}
