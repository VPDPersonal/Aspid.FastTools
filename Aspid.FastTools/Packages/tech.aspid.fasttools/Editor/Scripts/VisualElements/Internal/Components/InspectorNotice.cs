using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed class InspectorNotice : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-InspectorNotice";

        private const string NoticeClass = "aspid-fasttools-inspector-notice";
        private const string IconClass = NoticeClass + "__icon";
        private const string MessageClass = NoticeClass + "__message";
        private const string ActionClass = NoticeClass + "__action";
        private const string SuggestionClass = NoticeClass + "__suggestion";
        private const string SuggestionVisibleClass = SuggestionClass + "--visible";
        private const string SuggestionSeparatorClass = NoticeClass + "__suggestion-separator";
        private const string SuggestionSeparatorVisibleClass = SuggestionSeparatorClass + "--visible";

        private const string MessageNavigableClass = MessageClass + "--navigable";

        private const string DotClass = NoticeClass + "__dot";
        private const string DotVisibleClass = DotClass + "--visible";

        private const string InfoModifierClass = NoticeClass + "--info";

        private const string SharedModifierClass = NoticeClass + "--shared";

        private readonly Label _message;
        private readonly Label _action;
        private readonly Label _suggestion;
        private readonly Label _suggestionSeparator;
        private readonly VisualElement _dot;

        private Action _onAction;
        private Action _onSuggestion;
        private Action _onNavigate;

        private Color? _ridColor;
        private Color? _sharedColor;
        private bool _lightSkin = !EditorGUIUtility.isProSkin;

        public InspectorNotice()
        {
            this.AddAspidThemeStyleSheets()
                .AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(NoticeClass);

            // A switch of the editor skin restyles the panel; the inline text colour follows it without a refresh.
            RegisterCallback<CustomStyleResolvedEvent>(_ => SetSkin(lightSkin: !EditorGUIUtility.isProSkin));

            var icon = new VisualElement()
                .AddClass(IconClass)
                .SetPickingMode(PickingMode.Ignore);

            _message = new Label()
                .AddClass(MessageClass)
                .SetPickingMode(PickingMode.Ignore);
            _message.RegisterCallback<ClickEvent>(_ => _onNavigate?.Invoke());
            _message.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (_onNavigate is not null && _sharedColor.HasValue)
                    _message.style.color = SharedHoverColor(_sharedColor.Value);
            });
            _message.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (_sharedColor.HasValue) _message.style.color = _sharedColor.Value;
            });

            _action = new Label().AddClass(ActionClass);
            _action.RegisterCallback<ClickEvent>(_ => _onAction?.Invoke());
            _action.RegisterCallback<PointerEnterEvent>(_ =>
            {
                if (_sharedColor.HasValue) _action.style.color = SharedHoverColor(_sharedColor.Value);
            });
            _action.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (_sharedColor.HasValue) _action.style.color = _sharedColor.Value;
            });

            _suggestionSeparator = new Label("·").AddClass(SuggestionSeparatorClass);
            _suggestionSeparator.pickingMode = PickingMode.Ignore;

            _suggestion = new Label().AddClass(SuggestionClass);
            _suggestion.RegisterCallback<ClickEvent>(_ => _onSuggestion?.Invoke());

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

        public void Set(string message, string actionText, string detail, Action onAction, Color? dotColor = null,
            Action onNavigate = null)
        {
            EnableInClassList(InfoModifierClass, false);

            _message.text = message;
            _onAction = onAction;

            var hasAction = !string.IsNullOrEmpty(actionText) && onAction is not null;
            _action.text = Underline(actionText);
            _action.SetDisplay(hasAction ? DisplayStyle.Flex : DisplayStyle.None);

            _onNavigate = onNavigate;
            _message.SetPickingMode(onNavigate is not null ? PickingMode.Position : PickingMode.Ignore);
            _message.EnableInClassList(MessageNavigableClass, onNavigate is not null);

            ApplySharedColor(dotColor);

            EnableInClassList(SharedModifierClass, dotColor.HasValue);

            tooltip = detail;
            ClearSuggestion();
        }

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

        internal void SetSkin(bool lightSkin)
        {
            if (_lightSkin == lightSkin) return;

            _lightSkin = lightSkin;
            if (_ridColor.HasValue) ApplySharedColor(_ridColor);
        }

        // The dot keeps the given color; the text takes a variant that reads on the current skin.
        private void ApplySharedColor(Color? color)
        {
            _ridColor = color;
            _sharedColor = color.HasValue
                ? InspectorNoticeGUI.SharedTextColor(color.Value, lightSkin: _lightSkin)
                : null;

            if (color.HasValue)
            {
                _dot.EnableInClassList(DotVisibleClass, true);
                _dot.style.backgroundColor = color.Value;
                _message.style.color = _sharedColor.Value;
                _action.style.color = _sharedColor.Value;
            }
            else
            {
                _dot.EnableInClassList(DotVisibleClass, false);
                _dot.style.backgroundColor = StyleKeyword.Null;
                _message.style.color = StyleKeyword.Null;
                _action.style.color = StyleKeyword.Null;
            }
        }

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

        private Color SharedHoverColor(Color textColor) =>
            InspectorNoticeGUI.SharedHoverColor(textColor, lightSkin: _lightSkin);

        // USS has no text-decoration property; rich text supplies the action underline.
        private static string Underline(string text) =>
            string.IsNullOrEmpty(text) ? text : $"<u>{text}</u>";
    }
}
