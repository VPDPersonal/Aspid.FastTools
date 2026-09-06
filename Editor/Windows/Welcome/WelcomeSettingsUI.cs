using System;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    // The Welcome auto-show switch, defined once so the window's Settings tab and the Preferences page render the
    // same control.
    internal static class WelcomeSettingsUI
    {
        public static void BuildControls(VisualElement container)
        {
            var autoShow = new AspidSwitch("Auto-show Welcome")
            {
                value = WelcomeSettings.AutoShowEnabled,
                tooltip = "Open the Welcome tab automatically after the package is installed or updated.\n"
                    + "Turning it off suppresses every future auto-show; Tools → Aspid 🐍 → FastTools → Welcome keeps working.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            autoShow.WithScopeStripe(AspidSettingsUI.UserScopeClass);
            autoShow.RegisterValueChangedCallback(evt => WelcomeSettings.AutoShowEnabled = evt.newValue);
            SyncFromSettings(autoShow, () => WelcomeSettings.AutoShowEnabled);
            container.Add(autoShow);
        }

        private static void SyncFromSettings<TControl, TValue>(TControl control, Func<TValue> read)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            AspidSettingsUI.SyncFromSettings(
                control,
                read,
                handler => WelcomeSettings.Changed += handler,
                handler => WelcomeSettings.Changed -= handler);
        }
    }
}
