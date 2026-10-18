using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class AspidAppearanceSettingsUI
    {
        internal const string AnimatedBackgroundName = "animated-background";

        // The Appearance section and Preferences page: the theme, its override, then the animated background switch.
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
            SyncFromSettings(animated, () => AspidAnimatedDotsBackgroundSettings.Enabled);
            container.Add(animated);
        }

        private static void SyncFromSettings<TControl, TValue>(TControl control, Func<TValue> read)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            AspidSettingsUI.SyncFromSettings(
                control,
                read,
                handler => AspidAnimatedDotsBackgroundSettings.Changed += handler,
                handler => AspidAnimatedDotsBackgroundSettings.Changed -= handler);
        }
    }
}
