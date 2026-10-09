using UnityEngine;
using NUnit.Framework;
using Aspid.FastTools.Editors;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// The "Shared reference" notice draws its text and link in the reference colour, which is tuned for the dark
    /// skin. On the light skin the dot keeps that colour, while the text and its hovered link must still read at 4.5:1
    /// against the inspector background.
    /// </summary>
    [TestFixture]
    internal sealed class InspectorNoticeSharedColorTests
    {
        // Unity's light-skin inspector background, --unity-colors-window-background.
        private static readonly Color LightSkinBackground = new Color32(200, 200, 200, 255);

        [Test]
        public void LightSkin_TextAndHoverRead()
        {
            for (var index = 1; index <= 64; index++)
            {
                var rid = SerializeReferenceRidColor.ForIndex(index);
                var text = InspectorNoticeGUI.SharedTextColor(rid, lightSkin: true);
                var hover = InspectorNoticeGUI.SharedHoverColor(text, lightSkin: true);

                Assert.GreaterOrEqual(Contrast(text, LightSkinBackground), 4.5f,
                    $"Shared reference #{index}: the text {text} must read at 4.5:1 on the light skin.");
                Assert.GreaterOrEqual(Contrast(hover, LightSkinBackground), 4.5f,
                    $"Shared reference #{index}: the hovered link {hover} must read at 4.5:1 on the light skin.");
            }
        }

        [Test]
        public void LightSkin_TextKeepsHue()
        {
            var rid = SerializeReferenceRidColor.ForIndex(3);
            Color.RGBToHSV(rid, out var ridHue, out _, out _);
            Color.RGBToHSV(InspectorNoticeGUI.SharedTextColor(rid, lightSkin: true), out var textHue, out _, out _);

            Assert.AreEqual(ridHue, textHue, 0.02f, "The darker text must keep the reference hue.");
        }

        [Test]
        public void DarkSkin_TextKeepsRidColor()
        {
            var rid = SerializeReferenceRidColor.ForIndex(3);

            Assert.AreEqual(rid, InspectorNoticeGUI.SharedTextColor(rid, lightSkin: false));
        }

        private static float Contrast(Color a, Color b)
        {
            var la = Luminance(a);
            var lb = Luminance(b);
            return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
        }

        private static float Luminance(Color color)
        {
            var linear = color.linear;
            return 0.2126f * linear.r + 0.7152f * linear.g + 0.0722f * linear.b;
        }
    }
}
