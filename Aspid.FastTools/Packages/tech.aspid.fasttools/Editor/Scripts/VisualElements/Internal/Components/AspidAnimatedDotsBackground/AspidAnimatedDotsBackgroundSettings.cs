using System;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
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
