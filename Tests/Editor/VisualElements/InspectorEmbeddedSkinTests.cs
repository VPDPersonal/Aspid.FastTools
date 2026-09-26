using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Types.Editors;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// UI embedded in a regular Inspector must follow the editor skin: built-in icons resolve to the variant of the
    /// current skin and backgrounds come from Unity's theme instead of the dark Aspid palette.
    /// </summary>
    /// <remarks>
    /// The skin of a test run cannot be switched, so skin-driven checks assert against whichever skin is active and
    /// the folder icons are checked through the light-skin class directly.
    /// </remarks>
    [TestFixture]
    internal sealed class InspectorEmbeddedSkinTests
    {
        private const string EnumValuesStyleSheet = "UI/Enums/Aspid-FastTools-EnumValues";

        private EditorWindow _window;

        [SetUp]
        public void SetUp()
        {
            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
        }

        [UnityTest]
        public IEnumerator InspectorNotice_Icon_MatchesEditorSkin()
        {
            var notice = new InspectorNotice();
            notice.Set("Message", actionText: null, detail: null, onAction: null);
            _window.rootVisualElement.Add(notice);
            yield return null;

            AssertSkinIcon(notice.Q(className: "aspid-fasttools-inspector-notice__icon"), "console.warnicon");
        }

        [Test]
        public void ThemeStyleSheets_MarkLightSkin()
        {
            var host = new VisualElement().AddAspidThemeStyleSheets();

            Assert.AreEqual(!EditorGUIUtility.isProSkin, host.ClassListContains(AspidStyles.SkinLightClass));
        }

        [UnityTest]
        public IEnumerator TypeField_FolderIcon_DarkSkin() => AssertFolderIcon(lightSkin: false, "d_Folder Icon");

        [UnityTest]
        public IEnumerator TypeField_FolderIcon_LightSkin() => AssertFolderIcon(lightSkin: true, "Folder Icon");

        [UnityTest]
        public IEnumerator EnumValues_Background_FollowsEditorSkin()
        {
            var header = new VisualElement().AddClass("aspid-fasttools-enum-values__header");
            var container = new VisualElement().AddClass("aspid-fasttools-enum-values__container");

            _window.rootVisualElement
                .AddAspidThemeStyleSheets()
                .AddChild(new VisualElement()
                    .AddStyleSheetFromResources(EnumValuesStyleSheet)
                    .AddChild(header)
                    .AddChild(container));
            yield return null;

            AssertSkinBackground(header.resolvedStyle.backgroundColor, "header");
            AssertSkinBackground(container.resolvedStyle.backgroundColor, "container");
        }

        [UnityTest]
        public IEnumerator Switch_UnsetPaletteTokens_KeepSkinDefaults()
        {
            var toggle = new AspidSwitch("Switch");
            _window.rootVisualElement.AddAspidThemeStyleSheets().Add(toggle);
            yield return null;

            var handle = toggle.Q(className: BaseField<bool>.inputUssClassName)[0][0];
            var expected = EditorGUIUtility.isProSkin
                ? new Color(0.74f, 0.74f, 0.77f, 0.85f)
                : new Color(0.35f, 0.35f, 0.38f, 0.9f);

            var actual = handle.resolvedStyle.backgroundColor;
            Assert.AreEqual(expected.r, actual.r, 0.01f, "An unset switch token must keep the skin default handle.");
            Assert.AreEqual(expected.g, actual.g, 0.01f, "An unset switch token must keep the skin default handle.");
            Assert.AreEqual(expected.b, actual.b, 0.01f, "An unset switch token must keep the skin default handle.");
            Assert.AreEqual(expected.a, actual.a, 0.01f, "An unset switch token must keep the skin default handle.");
        }

        private IEnumerator AssertFolderIcon(bool lightSkin, string expected)
        {
            var field = new TypeField("Type");
            field.EnableInClassList(AspidStyles.SkinLightClass, lightSkin);
            _window.rootVisualElement.Add(field);
            yield return null;

            var texture = field.Q<Button>()[0].resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(texture, "The folder icon must resolve.");
            Assert.AreEqual(expected, texture.name);
        }

        private static void AssertSkinIcon(VisualElement icon, string iconName)
        {
            Assert.IsNotNull(icon);

            var texture = icon.resolvedStyle.backgroundImage.texture;
            Assert.IsNotNull(texture, $"The {iconName} icon must resolve.");
            StringAssert.Contains(iconName, texture.name);
            Assert.AreEqual(EditorGUIUtility.isProSkin, texture.name.StartsWith("d_"),
                $"'{texture.name}' must be the {(EditorGUIUtility.isProSkin ? "dark" : "light")} skin variant.");
        }

        private static void AssertSkinBackground(Color color, string part)
        {
            Assert.Greater(color.a, 0f, $"The {part} background must resolve.");

            var isDark = color.grayscale < 0.5f;
            Assert.AreEqual(EditorGUIUtility.isProSkin, isDark,
                $"The {part} background {color} must match the {(EditorGUIUtility.isProSkin ? "dark" : "light")} skin.");
        }
    }
}
