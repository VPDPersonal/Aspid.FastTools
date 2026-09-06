using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A VisualElement that displays a styled help message with an optional title, a message-type icon, and status-
    // driven color accents. The icon and accents are USS-driven via the aspid-fasttools-help-box--{info|warning|error}
    // and aspid-fasttools-status--* classes; None hides the icon entirely.
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidHelpBox : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidHelpBox";
        private const string IconClass = "aspid-fasttools-help-box__icon";
        private const string IconHiddenClass = "aspid-fasttools-help-box__icon--hidden";
        private const string TextContainerClass = "aspid-fasttools-help-box__text-container";
        private const string TitleClass = "aspid-fasttools-help-box__title";
        private const string MessageTypeInfoClass = "aspid-fasttools-help-box--info";
        private const string MessageTypeWarningClass = "aspid-fasttools-help-box--warning";
        private const string MessageTypeErrorClass = "aspid-fasttools-help-box--error";

        private readonly VisualElement _imageElement;
        private readonly AspidLabel _messageElement;
        private readonly VisualElement _textContainer;
        private readonly AspidLabelPreset _titlePreset;

        private readonly StatusStyle _status;
        private AspidLabel _titleElement;
        private HelpBoxMessageType _messageType;

        [UxmlAttribute]
        public string Title
        {
            get => _titleElement is { parent: not null } ? _titleElement.Text : string.Empty;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    _titleElement?.RemoveFromHierarchy();
                    return;
                }

                if (_titleElement is not null && _titleElement.Text == value && _titleElement.parent is not null) return;

                if (_titleElement is null) _titleElement = new AspidLabel(value, _titlePreset).AddClass(TitleClass);
                else _titleElement.Text = value;

                if (_titleElement.parent is null) _textContainer.InsertChild(index: 0, _titleElement);
            }
        }

        [UxmlAttribute]
        public string Message
        {
            get => _messageElement.Text;
            set => _messageElement.Text = value;
        }

        [UxmlAttribute]
        public StatusStyle.Type Status
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

        [UxmlAttribute]
        public HelpBoxMessageType MessageType
        {
            get => _messageType;
            set
            {
                if (_messageType == value) return;

                this.RemoveClass(GetMessageTypeClass(_messageType));
                _messageType = value;
                this.AddClass(GetMessageTypeClass(value));

                if (value == HelpBoxMessageType.None) _imageElement.AddClass(IconHiddenClass);
                else _imageElement.RemoveClass(IconHiddenClass);
            }
        }

        public AspidHelpBox()
            : this(AspidHelpBoxPreset.Default) { }

        public AspidHelpBox(AspidHelpBoxPreset preset)
            : this(string.Empty, string.Empty, preset) { }

        public AspidHelpBox(string message, AspidHelpBoxPreset preset)
            : this(string.Empty, message, preset) { }

        public AspidHelpBox(string title, string message, AspidHelpBoxPreset preset)
        {
            this.AddStyleSheetsFromResource(StyleSheetPath);

            _titlePreset = preset.TitlePreset;
            _textContainer = new VisualElement().AddClass(TextContainerClass);
            _messageElement = new AspidLabel(message, preset.MessagePreset);
            _textContainer.AddChild(_messageElement);

            _imageElement = new VisualElement()
                .AddClass(IconClass)
                .AddClass(IconHiddenClass);

            this.AddChild(_imageElement)
                .AddChild(_textContainer);

            _status = new StatusStyle(this, preset.Status);
            MessageType = preset.MessageType;
            Title = title;
        }

        private static string GetMessageTypeClass(HelpBoxMessageType type) => type switch
        {
            HelpBoxMessageType.Info => MessageTypeInfoClass,
            HelpBoxMessageType.Warning => MessageTypeWarningClass,
            HelpBoxMessageType.Error => MessageTypeErrorClass,
            _ => string.Empty,
        };
    }
}
