#if !UNITY_6000_0_OR_NEWER
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidAnimatedTitle
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidAnimatedTitle, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _text = new() { name = "text" };
            private readonly UxmlColorAttributeDescription _color1 = new() { name = "color1" };
            private readonly UxmlColorAttributeDescription _color2 = new() { name = "color2" };
            private readonly UxmlColorAttributeDescription _color3 = new() { name = "color3" };
            private readonly UxmlFloatAttributeDescription _waveSpeed = new() { name = "wave-speed" };
            private readonly UxmlFloatAttributeDescription _colorSpeed = new() { name = "color-speed" };
            private readonly UxmlFloatAttributeDescription _waveStride = new() { name = "wave-stride" };
            private readonly UxmlFloatAttributeDescription _colorStride = new() { name = "color-stride" };
            private readonly UxmlFloatAttributeDescription _waveAmplitude = new() { name = "wave-amplitude" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidAnimatedTitle)ve;

                var text = element.Text;
                if (_text.TryGetValueFromBag(bag, cc: cc, value: ref text))
                    element.Text = text;

                var colorStride = element.ColorStride;
                if (_colorStride.TryGetValueFromBag(bag, cc: cc, value: ref colorStride))
                    element.ColorStride = colorStride;

                var colorSpeed = element.ColorSpeed;
                if (_colorSpeed.TryGetValueFromBag(bag, cc: cc, value: ref colorSpeed))
                    element.ColorSpeed = colorSpeed;

                var waveStride = element.WaveStride;
                if (_waveStride.TryGetValueFromBag(bag, cc: cc, value: ref waveStride))
                    element.WaveStride = waveStride;

                var waveSpeed = element.WaveSpeed;
                if (_waveSpeed.TryGetValueFromBag(bag, cc: cc, value: ref waveSpeed))
                    element.WaveSpeed = waveSpeed;

                var waveAmplitude = element.WaveAmplitude;
                if (_waveAmplitude.TryGetValueFromBag(bag, cc: cc, value: ref waveAmplitude))
                    element.WaveAmplitude = waveAmplitude;

                var color1 = element.Color1;
                if (_color1.TryGetValueFromBag(bag, cc: cc, value: ref color1))
                    element.Color1 = color1;

                var color2 = element.Color2;
                if (_color2.TryGetValueFromBag(bag, cc: cc, value: ref color2))
                    element.Color2 = color2;

                var color3 = element.Color3;
                if (_color3.TryGetValueFromBag(bag, cc: cc, value: ref color3))
                    element.Color3 = color3;
            }
        }
    }
}
#endif
