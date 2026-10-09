using System.Linq;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    // The gradient comes from one shared texture tinted per button, and a transparent gradient draws no image at all.
    // On the light palette the label keeps its color on hover.
    [TestFixture]
    internal sealed class AspidGradientButtonTests
    {
        private const string LabelClass = "aspid-fasttools-gradient-button__label";

        private static readonly Color Gradient = new(0.1f, 0.5f, 0.3f, 0.8f);
        private static readonly Color Accent = new(0.9f, 0.4f, 0.1f, 1f);

        private TestPanel _panel;

        [SetUp]
        public void SetUp() => _panel = new TestPanel();

        [TearDown]
        public void TearDown() => _panel.Dispose();

        [UnityTest]
        public IEnumerator TransparentGradient_DrawsNoImage()
        {
            var button = CreateButton(gradient: Color.clear);
            _panel.Root.Add(button);
            yield return null;

            Assert.AreEqual(StyleKeyword.Null, button.style.backgroundImage.keyword,
                "A transparent gradient must leave the background image unset.");
        }

        [UnityTest]
        public IEnumerator Gradient_TintsOneSharedTexture()
        {
            var first = CreateButton(Gradient);
            var second = CreateButton(new Color(0.7f, 0.2f, 0.2f, 0.5f));
            _panel.Root.Add(first);
            _panel.Root.Add(second);
            yield return null;

            var texture = first.style.backgroundImage.value.texture;
            Assert.IsNotNull(texture, "An opaque gradient must draw the fade texture.");
            Assert.AreSame(texture, second.style.backgroundImage.value.texture,
                "Every button must share the one fade texture.");
            Assert.AreEqual(Gradient, first.style.unityBackgroundImageTintColor.value,
                "The gradient color must tint the shared texture.");
        }

        [UnityTest]
        public IEnumerator Attach_DoesNotAllocateTexturesPerButton()
        {
            _panel.Root.Add(CreateButton(Gradient));
            yield return null;

            var before = CountFadeTextures();
            Assert.GreaterOrEqual(before, 1, "The shared fade texture must exist once a button has a gradient.");

            for (var i = 0; i < 10; i++)
                _panel.Root.Add(CreateButton(Gradient));

            yield return null;

            Assert.AreEqual(before, CountFadeTextures(),
                "Buttons must reuse the shared texture instead of creating their own.");
        }

        [UnityTest]
        public IEnumerator GradientChange_TogglesTheImage()
        {
            var button = CreateButton(Gradient);
            _panel.Root.Add(button);
            yield return null;

            button.Gradient = Color.clear;
            Assert.AreEqual(StyleKeyword.Null, button.style.backgroundImage.keyword);

            button.Gradient = Gradient;
            Assert.IsNotNull(button.style.backgroundImage.value.texture);
        }

        [UnityTest]
        public IEnumerator Hover_OnDarkPalette_PaintsTheLabelWithTheAccent()
        {
            var button = CreateButton(Gradient);
            _panel.Root.Add(button);
            yield return null;

            button.Highlighted = true;
            Assert.AreEqual(Accent, LabelOf(button).style.color.value);

            button.Highlighted = false;
            Assert.AreEqual(StyleKeyword.Null, LabelOf(button).style.color.keyword,
                "Leaving the button must give the label its stylesheet color back.");
        }

        [UnityTest]
        public IEnumerator Hover_OnLightPalette_KeepsTheLabelColor()
        {
            var host = new VisualElement().AddClass(AspidStyles.PaletteLightClass);
            _panel.Root.Add(host);

            var button = CreateButton(Gradient);
            host.Add(button);
            yield return null;

            button.Highlighted = true;
            Assert.AreEqual(StyleKeyword.Null, LabelOf(button).style.color.keyword,
                "The accent label would lose its contrast on the light palette.");
        }

        private static int CountFadeTextures() =>
            Resources.FindObjectsOfTypeAll<Texture2D>().Count(texture => texture.width == 256 && texture.height == 1);

        private static AspidGradientButton CreateButton(Color gradient) =>
            new("Text") { Gradient = gradient, Accent = Accent };

        private static Label LabelOf(AspidGradientButton button) =>
            button.Children().OfType<Label>().First(label => label.ClassListContains(LabelClass));
    }
}
