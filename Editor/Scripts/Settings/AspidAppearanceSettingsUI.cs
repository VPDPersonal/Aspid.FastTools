using UnityEngine.UIElements;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class AspidAppearanceSettingsUI
    {
        internal const string AnimatedBackgroundName = "animated-background";
        internal const string TypeIconsName = "type-icons";

        // The Appearance section and Preferences page: the theme override, the animated background switch, then the type icons switch.
        public static void BuildControls(VisualElement container)
        {
            AspidThemeSettingsUI.BuildControls(container);

            var animated = new AspidSwitch("Animated background")
            {
                name = AnimatedBackgroundName,
                value = AspidAnimatedDotsBackgroundSettings.Enabled,
                tooltip = "Animate the dots behind the FastTools window and its settings pages, "
                    + "and the title and logo on the Welcome tab.\n"
                    + "Turning it off stops their timers.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            animated.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            animated.RegisterValueChangedCallback(evt => AspidAnimatedDotsBackgroundSettings.Enabled = evt.newValue);
            AspidSettingsUI.SyncFromSettings(
                animated,
                () => AspidAnimatedDotsBackgroundSettings.Enabled,
                handler => AspidAnimatedDotsBackgroundSettings.Changed += handler,
                handler => AspidAnimatedDotsBackgroundSettings.Changed -= handler);
            container.Add(animated);

            var typeIcons = new AspidSwitch("Type icons")
            {
                name = TypeIconsName,
                value = TypeFieldIconSettings.Enabled,
                tooltip = "Show the type's icon left of its name in type dropdowns: [TypeSelector], "
                    + "SerializableType, SerializableMonoScript, [SerializeReference] and TypeField.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            typeIcons.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            typeIcons.RegisterValueChangedCallback(evt => TypeFieldIconSettings.Enabled = evt.newValue);
            AspidSettingsUI.SyncFromSettings(
                typeIcons,
                () => TypeFieldIconSettings.Enabled,
                handler => TypeFieldIconSettings.Changed += handler,
                handler => TypeFieldIconSettings.Changed -= handler);
            container.Add(typeIcons);
        }
    }
}
