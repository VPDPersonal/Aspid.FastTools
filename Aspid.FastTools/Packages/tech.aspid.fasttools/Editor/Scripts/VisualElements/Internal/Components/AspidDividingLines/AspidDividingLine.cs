using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
#if ASPID_UNITY_HAS_UXML_LIBRARY
    [UxmlElement(libraryPath = "Aspid/FastTools", visibility = LibraryVisibility.Hidden)]
#elif UNITY_6000_0_OR_NEWER
    [UxmlElement]
#endif
    internal sealed partial class AspidDividingLine : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidDividingLine";

        private readonly ThemeStyle _theme;
        private readonly StatusStyle _status;
        private readonly AspidDividingLineSizeStyle _size;
        private readonly AspidDividingLineDirectionStyle _direction;

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public ThemeStyle.Type Theme
        {
            get => _theme.Value;
            set => _theme.SetValue(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public StatusStyle.Type Status
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public AspidDividingLineSizeStyle.Type Size
        {
            get => _size.Value;
            set => _size.SetValue(value);
        }

#if UNITY_6000_0_OR_NEWER
        [UxmlAttribute]
#endif
        public AspidDividingLineDirectionStyle.Type Direction
        {
            get => _direction.Value;
            set => _direction.SetValue(value);
        }

        public AspidDividingLine()
            : this(AspidDividingLinePreset.Default) { }

        public AspidDividingLine(AspidDividingLinePreset preset)
        {
            this.AddStyleSheetFromResources(StyleSheetPath);

            _theme = new ThemeStyle(this, preset.Theme);
            _status = new StatusStyle(this, preset.Status);
            _size = new AspidDividingLineSizeStyle(this, preset.Size);
            _direction = new AspidDividingLineDirectionStyle(this, preset.Direction);
        }
    }
}
