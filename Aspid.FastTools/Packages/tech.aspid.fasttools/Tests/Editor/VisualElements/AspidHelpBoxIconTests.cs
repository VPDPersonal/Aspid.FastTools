using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// The help box icon follows the palette, not the editor skin: the dark palette keeps the dark-skin icon, and the
    /// light palette of an Aspid window puts the box on a light fill, so it takes the light-skin icon.
    /// </summary>
    [TestFixture]
    internal sealed class AspidHelpBoxIconTests
    {
        private static readonly HelpBoxMessageType[] Types =
        {
            HelpBoxMessageType.Info,
            HelpBoxMessageType.Warning,
            HelpBoxMessageType.Error,
        };

        private TestPanel _panel;

        [SetUp]
        public void SetUp() =>
            _panel = new TestPanel();

        [TearDown]
        public void TearDown() =>
            _panel.Dispose();

        [UnityTest]
        public IEnumerator DarkPalette_TakesDarkSkinIcon([ValueSource(nameof(Types))] HelpBoxMessageType type)
        {
            var box = AddBox(type, lightPalette: false);
            yield return null;

            StringAssert.StartsWith("d_", IconName(box), "The dark palette must keep the dark-skin icon.");
        }

        [UnityTest]
        public IEnumerator LightPalette_TakesLightSkinIcon([ValueSource(nameof(Types))] HelpBoxMessageType type)
        {
            var box = AddBox(type, lightPalette: true);
            yield return null;

            var name = IconName(box);
            Assert.IsFalse(name.StartsWith("d_"), $"The light palette must take the light-skin icon, not '{name}'.");
        }

        private AspidHelpBox AddBox(HelpBoxMessageType type, bool lightPalette)
        {
            var box = new AspidHelpBox("Message", AspidHelpBoxPreset.Default.SetMessageType(type));
            _panel.Root.Add(new VisualElement()
                .EnableClass(AspidStyles.PaletteLightClass, lightPalette)
                .AddChild(box));

            return box;
        }

        private static string IconName(AspidHelpBox box)
        {
            var texture = box.Q(className: "aspid-fasttools-help-box__icon").resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(texture, "The help box icon must resolve.");
            return texture.name;
        }
    }
}
