#if !UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidAnimatedDotsBackground
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidAnimatedDotsBackground, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlColorAttributeDescription _color1 = new() { name = "color1" };
            private readonly UxmlColorAttributeDescription _color2 = new() { name = "color2" };
            private readonly UxmlColorAttributeDescription _color3 = new() { name = "color3" };
            private readonly UxmlFloatAttributeDescription _dotRadius = new() { name = "dot-radius" };
            private readonly UxmlFloatAttributeDescription _dotSpacing = new() { name = "dot-spacing" };
            private readonly UxmlEnumAttributeDescription<StatusStyle.Type> _status = new() { name = "status" };
            private readonly UxmlFloatAttributeDescription _scaleReferenceSize = new() { name = "scale-reference-size" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidAnimatedDotsBackground)ve;

                var status = element.Status;
                if (_status.TryGetValueFromBag(bag, cc: cc, value: ref status))
                    element.Status = status;

                var color1 = element.Color1;
                if (_color1.TryGetValueFromBag(bag, cc: cc, value: ref color1))
                    element.Color1 = color1;

                var color2 = element.Color2;
                if (_color2.TryGetValueFromBag(bag, cc: cc, value: ref color2))
                    element.Color2 = color2;

                var color3 = element.Color3;
                if (_color3.TryGetValueFromBag(bag, cc: cc, value: ref color3))
                    element.Color3 = color3;

                var dotRadius = element.DotRadius;
                if (_dotRadius.TryGetValueFromBag(bag, cc: cc, value: ref dotRadius))
                    element.DotRadius = dotRadius;

                var dotSpacing = element.DotSpacing;
                if (_dotSpacing.TryGetValueFromBag(bag, cc: cc, value: ref dotSpacing))
                    element.DotSpacing = dotSpacing;

                var scaleReferenceSize = element.ScaleReferenceSize;
                if (_scaleReferenceSize.TryGetValueFromBag(bag, cc: cc, value: ref scaleReferenceSize))
                    element.ScaleReferenceSize = scaleReferenceSize;
            }
        }
    }
}
#endif
