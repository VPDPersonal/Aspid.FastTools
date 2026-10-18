// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidStyles
    {
        public const string DefaultStyleSheet = "UI/Aspid-FastTools-Default-Dark";
        public const string LightStyleSheet = "UI/Aspid-FastTools-Default-Light";
        public const string VenomThemeStyleSheet = "UI/Themes/Aspid-FastTools-Theme-Venom";
        public const string UnityThemeStyleSheet = "UI/Themes/Aspid-FastTools-Theme-Unity";
        public const string BackgroundStyle = "aspid-fasttools-background";
        public const string BackgroundRoundedState = BackgroundStyle + "--rounded";
        public const string InspectorStyleClass = "aspid-fasttools-inspector-container";
        public const string SkinLightClass = "aspid-fasttools-skin--light";
        public const string WindowClass = "aspid-fasttools-window";
        public const string PaletteLightClass = "aspid-fasttools-palette--light";

        // A short action inside a card, such as Clear or Undo: a plain outlined Button. An action that spans the card
        // is an AspidGradientButton band instead.
        public const string ActionButtonClass = "aspid-fasttools-action-button";
        public const string ActionButtonDangerClass = ActionButtonClass + "--danger";
    }
}
