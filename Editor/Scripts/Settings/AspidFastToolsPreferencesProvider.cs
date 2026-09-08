using UnityEditor;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class AspidFastToolsPreferencesProvider
    {
        private const string SettingsPath = "Preferences/Aspid.FastTools";

        [SettingsProvider]
        public static SettingsProvider Create() => new(SettingsPath, SettingsScope.User)
        {
            label = "Aspid.FastTools",

            activateHandler = static (_, root) =>
                AspidSettingsUI.BuildProviderPage(root, AspidSettingsScope.User),

            keywords = new[]
            {
                "Aspid",
                "FastTools",
                "Type Selector",
                "Favorites",
                "Recent",
                "Breakage",
                "Welcome",
                "Dropdown",
            },
        };

        [SettingsProvider]
        public static SettingsProvider CreateSerializeReference() =>
            new(SettingsPath + "/SerializeReference", SettingsScope.User)
            {
                label = "SerializeReference",

                activateHandler = static (_, root) => AspidSettingsUI.BuildAreaProviderPage(
                    root,
                    title: "SerializeReference",
                    content => SerializeReferenceSettingsUI.BuildControls(content, AspidSettingsScope.User)),

                keywords = new[] { "Aspid", "FastTools", "Serialize", "Reference", "Breakage", "Repair" },
            };

        [SettingsProvider]
        public static SettingsProvider CreateTypeSelector() =>
            new(SettingsPath + "/Type Selector", SettingsScope.User)
            {
                label = "Type Selector",

                activateHandler = static (_, root) => AspidSettingsUI.BuildAreaProviderPage(
                    root,
                    title: "Type Selector",
                    TypeSelectorSettingsView.BuildControls),

                keywords = new[] { "Aspid", "FastTools", "Type Selector", "Favorites", "Recent", "Dropdown" },
            };

        [SettingsProvider]
        public static SettingsProvider CreateWelcome() =>
            new(SettingsPath + "/Welcome", SettingsScope.User)
            {
                label = "Welcome",

                activateHandler = static (_, root) => AspidSettingsUI.BuildAreaProviderPage(
                    root,
                    title: "Welcome",
                    WelcomeSettingsUI.BuildControls),

                keywords = new[] { "Aspid", "FastTools", "Welcome", "Auto", "Show" },
            };
    }
}
