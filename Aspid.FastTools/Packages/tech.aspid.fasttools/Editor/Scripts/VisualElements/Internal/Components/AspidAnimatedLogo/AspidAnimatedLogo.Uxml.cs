#if !UNITY_6000_0_OR_NEWER
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidAnimatedLogo
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidAnimatedLogo, UxmlTraits>
        {
            public UxmlFactory() { }
        }

        internal new sealed class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription _image1 = new() { name = "image1" };
            private readonly UxmlStringAttributeDescription _image2 = new() { name = "image2" };
            private readonly UxmlStringAttributeDescription _image3 = new() { name = "image3" };
            private readonly UxmlFloatAttributeDescription _pulseSpeed = new() { name = "pulse-speed" };
            private readonly UxmlFloatAttributeDescription _pulseHoverAmplitude = new() { name = "pulse-hover-amplitude" };
            private readonly UxmlLongAttributeDescription _colorCycleIntervalMs = new() { name = "color-cycle-interval-ms" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag: bag, cc: cc);

                var element = (AspidAnimatedLogo)ve;

                var colorCycleIntervalMs = element.ColorCycleIntervalMs;
                if (_colorCycleIntervalMs.TryGetValueFromBag(bag, cc: cc, value: ref colorCycleIntervalMs))
                    element.ColorCycleIntervalMs = colorCycleIntervalMs;

                var pulseSpeed = element.PulseSpeed;
                if (_pulseSpeed.TryGetValueFromBag(bag, cc: cc, value: ref pulseSpeed))
                    element.PulseSpeed = pulseSpeed;

                var pulseHoverAmplitude = element.PulseHoverAmplitude;
                if (_pulseHoverAmplitude.TryGetValueFromBag(bag, cc: cc, value: ref pulseHoverAmplitude))
                    element.PulseHoverAmplitude = pulseHoverAmplitude;

                string image1 = null;
                if (_image1.TryGetValueFromBag(bag, cc: cc, value: ref image1))
                    element.Image1 = LoadTexture(image1, cc: cc);

                string image2 = null;
                if (_image2.TryGetValueFromBag(bag, cc: cc, value: ref image2))
                    element.Image2 = LoadTexture(image2, cc: cc);

                string image3 = null;
                if (_image3.TryGetValueFromBag(bag, cc: cc, value: ref image3))
                    element.Image3 = LoadTexture(image3, cc: cc);
            }

            private static Texture2D LoadTexture(string source, CreationContext cc)
            {
                if (string.IsNullOrEmpty(source)) return null;

                if (Uri.TryCreate(source, UriKind.Absolute, out var uri) && uri.Scheme == "project")
                    source = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
                else if (!source.StartsWith("Assets/", StringComparison.Ordinal) &&
                         !source.StartsWith("Packages/", StringComparison.Ordinal))
                {
                    var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(cc.visualTreeAsset));
                    if (string.IsNullOrEmpty(directory)) return null;

                    source = Path.Combine(directory, source).Replace('\\', '/');
                }

                return AssetDatabase.LoadAssetAtPath<Texture2D>(source);
            }
        }
    }
}
#endif
