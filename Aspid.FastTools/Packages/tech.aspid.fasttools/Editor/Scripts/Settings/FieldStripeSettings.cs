using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    // The per-user "Field stripes" switch: the status stripe left of [SerializeReference] and type fields in the
    // Inspector. Turning it off hides only the stripe; the notices under the fields stay.
    internal static class FieldStripeSettings
    {
        private const string EnabledKey = "Aspid.FastTools.Appearance.FieldStripes";

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

        // Shows the stripe only while the switch is on, and follows the switch while the stripe is in a panel.
        internal static T BindDisplay<T>(T stripe) where T : VisualElement
        {
            stripe.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                Changed -= Apply;
                Changed += Apply;
                Apply();
            });
            stripe.RegisterCallback<DetachFromPanelEvent>(_ => Changed -= Apply);
            Apply();

            return stripe;

            void Apply() => stripe.style.display = Enabled ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
