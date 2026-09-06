using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A VisualElement label with Aspid theming, status accents, font size control, and an optional AspidDividingLine
    // beneath the text. Theme and status can be driven by USS custom properties or set explicitly in code.
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidLabel : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidLabel";
        
        private readonly Label _label;
        private readonly AspidDividingLine _line;

        private readonly ThemeStyle _theme;
        private readonly StatusStyle _status;
        private readonly AspidLabelSizeStyle _size;
        private readonly AspidLabelFontStyle _fontStyle;

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        [UxmlAttribute]
        public bool Selectable
        {
            get => _label.selection.isSelectable;
            set => _label.SetIsSelectable(value);
        }

        [UxmlAttribute]
        public ThemeStyle.Type LabelTheme
        {
            get => _theme.Value;
            set => _theme.SetValue(value);
        }

        [UxmlAttribute]
        public StatusStyle.Type LabelStatus
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

        [UxmlAttribute]
        public AspidLabelSizeStyle.Type LabelSize
        {
            get => _size.Value;
            set => _size.SetValue(value);
        }

        [UxmlAttribute]
        public FontStyle LabelFontStyle
        {
            get => _fontStyle.Value.value;
            set => _fontStyle.SetValue(new StyleEnum<FontStyle>(value));
        }

        [UxmlAttribute]
        public ThemeStyle.Type LineTheme
        {
            get => _line.Theme;
            set => _line.Theme = value;
        }

        [UxmlAttribute]
        public StatusStyle.Type LineStatus
        {
            get => _line.Status;
            set => _line.Status = value;
        }

        [UxmlAttribute]
        public AspidDividingLineSizeStyle.Type LineSize
        {
            get => _line.Size;
            set => _line.Size = value;
        }

        [UxmlAttribute]
        public AspidDividingLineDirectionStyle.Type LineDirection
        {
            get => _line.Direction;
            set => _line.Direction = value;
        }

        public AspidLabel()
            : this(string.Empty) { }

        public AspidLabel(AspidLabelPreset preset)
            : this(string.Empty, preset) { }

        public AspidLabel(string text)
            : this(text, AspidLabelPreset.Default) { }

        public AspidLabel(string text, AspidLabelPreset preset)
        {
            _label = new Label(text);
            _line = new AspidDividingLine(preset.Line);
            
            this.AddStyleSheetsFromResource(StyleSheetPath)
                .AddChild(_label)
                .AddChild(_line);

            Selectable = preset.Selectable;
            _theme = new ThemeStyle(this, preset.Theme);
            _status = new StatusStyle(this, preset.Status);
            _size = new AspidLabelSizeStyle(this, preset.Size);
            _fontStyle = new AspidLabelFontStyle(this, preset.FontStyle);
        }
    }
}
