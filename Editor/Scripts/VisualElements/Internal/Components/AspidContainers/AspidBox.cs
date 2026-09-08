using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    [UxmlElement(libraryPath = "Aspid/FastTools")]
    internal sealed partial class AspidBox : VisualElement
    {
        private const string StyleSheetPath = "UI/Components/Aspid-FastTools-AspidBox";

        private readonly ThemeStyle _theme;
        private readonly StatusStyle _status;

        [UxmlAttribute]
        public ThemeStyle.Type Theme
        {
            get => _theme.Value;
            set => _theme.SetValue(value);
        }

        [UxmlAttribute]
        public StatusStyle.Type Status
        {
            get => _status.Value;
            set => _status.SetValue(value);
        }

        public AspidBox()
            : this(AspidBoxPreset.Default) { }

        public AspidBox(AspidBoxPreset preset)
        {
            this.AddStyleSheetFromResources(StyleSheetPath)
                .AddClass(AspidStyles.BackgroundStyle)
                .AddClass(AspidStyles.BackgroundRoundedState);

            _theme = new ThemeStyle(this, preset.Theme);
            _status = new StatusStyle(this, preset.Status);
        }
    }
}
