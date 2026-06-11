using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// A compact, single-row warning notice for the <c>[TypeSelector]</c> drawer on <c>[SerializeReference]</c> fields: a small
    /// warning icon, a short yellow message and an underlined, clickable action word (e.g. <c>Fix</c>).
    /// The full explanation is surfaced on hover through the element's <see cref="VisualElement.tooltip"/>,
    /// so the inspector row stays terse while the detail is one hover away. Replaces the bulky
    /// <c>AspidHelpBox</c>-plus-button pair previously used for missing-type and shared-reference states.
    /// </summary>
    internal sealed class SerializeReferenceNotice : VisualElement
    {
        private const string NoticeClass = "aspid-fasttools-serialize-reference-notice";
        private const string IconClass = NoticeClass + "__icon";
        private const string MessageClass = NoticeClass + "__message";
        private const string ActionClass = NoticeClass + "__action";

        private readonly Label _message;
        private readonly Label _action;

        private Action _onAction;

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

            this.AddChild(icon)
                .AddChild(_message)
                .AddChild(_action);
        }

        /// <summary>
        /// Updates the notice content. The <paramref name="actionText"/> word is the only clickable part;
        /// pass an empty string to hide it (e.g. when the action is unavailable for unsaved targets).
        /// </summary>
        public void Set(string message, string actionText, string detail, Action onAction)
        {
            _message.text = message;
            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionText) && onAction is not null;
            _action.text = actionText;
            _action.SetDisplay(hasAction ? DisplayStyle.Flex : DisplayStyle.None);

            tooltip = detail;
        }
    }
}
