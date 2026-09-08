using System;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class WelcomeSettings
    {
        public static event Action Changed;

        private const string AutoShowKeyPrefix = "Aspid.FastTools.Welcome.AutoShow.";

        private static string AutoShowKey => AutoShowKeyPrefix + PlayerSettings.productGUID;

        public static bool AutoShowEnabled
        {
            get => EditorPrefs.GetBool(AutoShowKey, true);
            set
            {
                if (AutoShowEnabled == value) return;
                EditorPrefs.SetBool(AutoShowKey, value);
                Changed?.Invoke();
            }
        }

        public static void ResetToDefaults() => AutoShowEnabled = true;
    }
}
