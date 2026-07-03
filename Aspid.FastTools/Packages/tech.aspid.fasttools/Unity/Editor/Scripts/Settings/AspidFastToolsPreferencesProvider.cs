using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// The package's settings page at <c>Preferences → Aspid FastTools</c> — the per-user half of the settings,
    /// matching the page's <see cref="SettingsScope.User"/>: the References controls stored locally (breakage
    /// detection, the attribute-free dropdown), Type Selector, Appearance and Welcome. The team-wide controls live on
    /// the <c>Project Settings → Aspid FastTools → SerializeReference</c> page instead, and the window's Settings tab
    /// shows both scopes as the one full overview. <see cref="AspidSettingsUI.BuildProviderPage"/> composes the same
    /// branded page (dotted canvas, legend, sections, pinned reset footer) both Unity-native pages share; every
    /// surface mirrors the others live from one definition per control. Supersedes the old theme-only provider at
    /// this path; its controls now form the Appearance section.
    /// </summary>
    internal static class AspidFastToolsPreferencesProvider
    {
        private const string SettingsPath = "Preferences/Aspid FastTools";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(SettingsPath, SettingsScope.User)
            {
                label = "Aspid FastTools",
                activateHandler = static (_, root) => AspidSettingsUI.BuildProviderPage(root, AspidSettingsScope.User),
                keywords = new[]
                {
                    "Aspid", "FastTools", "Theme", "Style", "USS", "Color", "Palette", "Override",
                    "Type Selector", "Favorites", "Recent", "Breakage", "Welcome", "Dropdown",
                },
            };
        }
    }
}
