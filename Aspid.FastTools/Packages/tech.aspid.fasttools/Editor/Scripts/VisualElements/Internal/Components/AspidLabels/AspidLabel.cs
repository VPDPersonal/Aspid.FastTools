using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
#if ASPID_UNITY_HAS_UXML_LIBRARY
    [UxmlElement(libraryPath = "Aspid/FastTools", visibility = LibraryVisibility.Hidden)]
#elif UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    internal sealed partial class AspidLabel : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidLabel";
        
        private readonly Label _label;
        private readonly AspidDividingLine _line;

        private readonly ThemeStyle _theme;
        private readonly StatusStyle _status;
        private readonly AspidLabelSizeStyle _size;
        private readonly AspidLabelFontStyle _fontStyle;

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public bool Selectable
        {
            get => _label.selection.isSelectable;
            set => _label.SetSelectable(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public ThemeStyle.Type LabelTheme
        {
            get => _theme.Value;
            set => _theme.SetValue(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public StatusStyle.Type LabelStatus
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public AspidLabelSizeStyle.Type LabelSize
        {
            get => _size.Value;
            set => _size.SetValue(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public FontStyle LabelFontStyle
        {
            get => _fontStyle.Value.value;
            set => _fontStyle.SetValue(new StyleEnum<FontStyle>(value));
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public ThemeStyle.Type LineTheme
        {
            get => _line.Theme;
            set => _line.Theme = value;
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public StatusStyle.Type LineStatus
        {
            get => _line.Status;
            set => _line.Status = value;
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public AspidDividingLineSizeStyle.Type LineSize
        {
            get => _line.Size;
            set => _line.Size = value;
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
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
            
            this.AddStyleSheetFromResources(StyleSheetPath)
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
