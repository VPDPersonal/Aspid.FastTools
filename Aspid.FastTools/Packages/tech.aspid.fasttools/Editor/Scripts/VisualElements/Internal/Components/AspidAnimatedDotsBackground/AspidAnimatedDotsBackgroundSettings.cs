using System;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // The per-user "Animated background" switch: it stops the dots background, and the title and logo of the Welcome tab.
    internal static class AspidAnimatedDotsBackgroundSettings
    {
        private const string EnabledKey = "Aspid.FastTools.Appearance.AnimatedBackground";

        public static event Action Changed;

        public static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set
            {
                if (Enabled == value) return;
                EditorPrefs.SetBool(EnabledKey, value);
                Changed?.Invoke();
            }
        }

        public static void ResetToDefaults() => Enabled = true;
    }
}
