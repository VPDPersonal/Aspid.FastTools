using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class AspidAppearanceSettingsUI
    {
        public static void BuildControls(VisualElement container)
        {
            var animated = new AspidSwitch("Animated background")
            {
                value = AspidAnimatedDotsBackgroundSettings.Enabled,
                tooltip = "Animate the dots behind the FastTools window and its settings pages.\n"
                    + "Turning it off stops the repaint timer, so an open window no longer keeps the editor busy.\n"
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
