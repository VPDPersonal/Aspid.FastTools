#if !UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidGradientButton
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidGradientButton, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _text = new() { name = "text" };
            private readonly UxmlColorAttributeDescription _accent = new() { name = "accent" };
            private readonly UxmlColorAttributeDescription _gradient = new() { name = "gradient" };
            private readonly UxmlStringAttributeDescription _trailingText = new() { name = "trailing-text" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidGradientButton)ve;

                var text = element.Text;
                if (_text.TryGetValueFromBag(bag, cc: cc, value: ref text))
                    element.Text = text;

                var trailingText = element.TrailingText;
                if (_trailingText.TryGetValueFromBag(bag, cc: cc, value: ref trailingText))
                    element.TrailingText = trailingText;

                var gradient = element.Gradient;
                if (_gradient.TryGetValueFromBag(bag, cc: cc, value: ref gradient))
                    element.Gradient = gradient;

                var accent = element.Accent;
                if (_accent.TryGetValueFromBag(bag, cc: cc, value: ref accent))
                    element.Accent = accent;
            }
        }
    }
}
#endif
