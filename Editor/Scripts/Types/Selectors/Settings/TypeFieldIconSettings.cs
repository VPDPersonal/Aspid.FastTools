using System;
using UnityEditor;
using UnityEditorInternal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    // The per-user "Type icons" switch: the icon left of the caption in every type dropdown of the Inspector.
    internal static class TypeFieldIconSettings
    {
        private const string EnabledKey = "Aspid.FastTools.Appearance.TypeIcons";

        internal static event Action Changed;

        internal static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, defaultValue: true);
            set
            {
                if (Enabled == value) return;
                EditorPrefs.SetBool(EnabledKey, value);
                Changed?.Invoke();

                // IMGUI drawers read the switch on their next repaint, which nothing else triggers.
                InternalEditorUtility.RepaintAllViews();
            }
        }

        internal static void ResetToDefaults() => Enabled = true;
    }
}
