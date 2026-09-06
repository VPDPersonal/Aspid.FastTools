using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // A VisualElement container with an Aspid background plus theme and status-accent support. The theme (--aspid-
    // fasttools-prop-theme) and status (--aspid-fasttools-prop-status) can be driven by USS custom properties or set
    // explicitly in code.
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
            this.AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass(AspidStyles.BackgroundStyle)
                .AddClass(AspidStyles.BackgroundRoundedState);

            _theme = new ThemeStyle(this, preset.Theme);
            _status = new StatusStyle(this, preset.Status);
        }
    }
}
