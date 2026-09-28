using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    // A label's text color follows its own LabelTheme, not the theme class of a container around it.
    [TestFixture]
    internal sealed class AspidLabelThemeTests
    {
        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
            _window.rootVisualElement.AddAspidThemeStyleSheets();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator DarkLabel_InsideLightBox_KeepsItsOwnTheme()
        {
            var standalone = CreateLabel(ThemeStyle.Type.Dark);
            var light = CreateLabel(ThemeStyle.Type.Light);
            var nested = CreateLabel(ThemeStyle.Type.Dark);

            _window.rootVisualElement.Add(standalone);
            _window.rootVisualElement.Add(light);
            _window.rootVisualElement.Add(new AspidBox(AspidBoxPreset.Default.SetTheme(ThemeStyle.Type.Light)).AddChild(nested));

            yield return null;

            Assert.AreNotEqual(TextColor(light), TextColor(standalone),
                "Precondition: the dark and light text colors must resolve to different values.");
            Assert.AreEqual(TextColor(standalone), TextColor(nested),
                "A dark label inside a light box must keep the dark text color.");
        }

        private static AspidLabel CreateLabel(ThemeStyle.Type theme) =>
            new("Title", AspidLabelPreset.Default.SetLabelTheme(theme));

        private static Color TextColor(AspidLabel label) =>
            label.Q<Label>().resolvedStyle.color;
    }
}
