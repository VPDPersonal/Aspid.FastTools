using System.IO;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Aspid.FastTools.Editors.Tests
{
    // The Welcome sheet must work wherever the package sits and keep its text readable on the light skin.
    [TestFixture]
    internal sealed class WelcomeViewStyleTests
    {
        private const string TemplatePath = "UI/Windows/Welcome/Aspid-FastTools-Welcome-View";
        private const string WelcomeStyleSheet = "UI/Windows/Welcome/Aspid-FastTools-Welcome";
        private const string WindowStyleSheet = "UI/SerializeReferences/Aspid-FastTools-SerializeReference-Window";

        private const string HeroClass = "aspid-fasttools-welcome__hero";
        private const string ToastClass = "aspid-fasttools-welcome__toast";
        private const string ToastVisibleClass = "aspid-fasttools-welcome__toast--visible";
        private const string CanvasClass = "aspid-fasttools-serialize-reference-window__background";

        private TestPanel _panel;

        [SetUp]
        public void SetUp() =>
            _panel = new TestPanel();

        [TearDown]
        public void TearDown() =>
            _panel.Dispose();

        [Test]
        public void Template_HasNoLinksBoundToTheAssetPath()
        {
            // A project:// link names the asset path; with the package in Assets every link logs a warning on import.
            var template = Resources.Load<VisualTreeAsset>(TemplatePath);
            Assert.IsNotNull(template, "The Welcome template must load.");

            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(template);
            var package = PackageInfo.FindForAssetPath(assetPath);
            var relativePath = assetPath[(package.assetPath.Length + 1)..];

            var text = File.ReadAllText(Path.Combine(package.resolvedPath, relativePath));

            StringAssert.DoesNotContain("project://", text);
        }

        [Test]
        public void View_AddsDefaultDarkBeforeTheWelcomeSheet()
        {
            // The Welcome card rules tie with Default-Dark's AspidBox rules on specificity; order breaks the tie.
            var view = new WelcomeView();

            var defaultDark = IndexOf(view, Resources.Load<StyleSheet>(AspidStyles.DefaultStyleSheet));
            var welcome = IndexOf(view, Resources.Load<StyleSheet>(WelcomeStyleSheet));
            var defaultLight = IndexOf(view, Resources.Load<StyleSheet>(AspidStyles.LightStyleSheet));

            Assert.GreaterOrEqual(defaultDark, 0, "The view must carry Default-Dark.");
            Assert.Greater(welcome, defaultDark, "The Welcome sheet must come after Default-Dark.");
            Assert.Greater(defaultLight, welcome, "The light palette must come after the Welcome sheet.");
        }

        [UnityTest]
        public IEnumerator Logo_TakesItsLayersFromTheStyleSheet()
        {
            // The window root declares the palette; the view inherits it.
            _panel.Root.AddAspidThemeStyleSheets();

            var view = new WelcomeView();
            _panel.Root.Add(view);
            yield return null;

            var logo = view.Q<AspidAnimatedLogo>();
            Assert.IsNotNull(logo, "The Welcome template must hold the logo.");
            Assert.IsNotNull(logo.Image1, "The first logo layer must resolve from the style sheet.");
            Assert.IsNotNull(logo.Image2, "The second logo layer must resolve from the style sheet.");
            Assert.IsNotNull(logo.Image3, "The third logo layer must resolve from the style sheet.");
            Assert.AreNotEqual(logo.Image1, logo.Image2);
            Assert.AreNotEqual(logo.Image2, logo.Image3);
        }

        [UnityTest]
        public IEnumerator Title_LightSkin_StaysReadable()
        {
            UseSkin(light: true);
            var title = AddTitle();
            var canvas = AddCanvas();
            yield return null;

            var background = canvas.resolvedStyle.backgroundColor;

            // 3:1 is the WCAG minimum for large text; the title is 36 px bold.
            Assert.GreaterOrEqual(Contrast(title.Color1, background), 3f, "The first title tone must be readable on the light canvas.");
            Assert.GreaterOrEqual(Contrast(title.Color2, background), 3f, "The second title tone must be readable on the light canvas.");
            Assert.GreaterOrEqual(Contrast(title.Color3, background), 3f, "The third title tone must be readable on the light canvas.");
        }

        [UnityTest]
        public IEnumerator Title_DarkSkin_KeepsRisingBrightness()
        {
            UseSkin(light: false);
            var title = AddTitle();
            yield return null;

            Assert.Less(Luminance(title.Color1), Luminance(title.Color2), "The dark skin must keep its tones from dim to bright.");
            Assert.Less(Luminance(title.Color2), Luminance(title.Color3), "The dark skin must keep its tones from dim to bright.");
        }

        [UnityTest]
        public IEnumerator Toast_LightSkin_StaysReadable()
        {
            UseSkin(light: true);
            var toast = AddToast();
            var canvas = AddCanvas();
            yield return null;

            var opacity = toast.resolvedStyle.opacity;
            var background = canvas.resolvedStyle.backgroundColor;
            var text = Blend(toast.resolvedStyle.color, background, opacity);
            var fill = Blend(toast.resolvedStyle.backgroundColor, background, opacity);

            Assert.GreaterOrEqual(Contrast(text, fill), 4.5f, "The toast text must pass AA contrast on the light skin.");
        }

        [UnityTest]
        public IEnumerator Toast_DarkSkin_StaysTranslucent()
        {
            UseSkin(light: false);
            var toast = AddToast();
            yield return null;

            Assert.AreEqual(0.75f, toast.resolvedStyle.opacity, 0.001f, "The dark skin keeps the translucent toast.");
        }

        // Mirrors WelcomeView: Default-Dark, the Welcome sheet, then the light palette. The skin classes are set by
        // hand because the theme extension would reset them to the real editor skin on every restyle.
        private void UseSkin(bool light)
        {
            _panel.Root
                .AddStyleSheetFromResources(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetFromResources(WelcomeStyleSheet)
                .AddStyleSheetFromResources(AspidStyles.LightStyleSheet)
                .AddStyleSheetFromResources(WindowStyleSheet);

            if (light)
                _panel.Root.AddClass(AspidStyles.SkinLightClass).AddClass(AspidStyles.PaletteLightClass);
        }

        private AspidAnimatedTitle AddTitle()
        {
            var title = new AspidAnimatedTitle("Welcome");
            _panel.Root.Add(new VisualElement().AddClass(HeroClass).AddChild(title));
            return title;
        }

        private Label AddToast()
        {
            var toast = new Label("Imported").AddClass(ToastClass).AddClass(ToastVisibleClass);
            _panel.Root.Add(toast);
            return toast;
        }

        private VisualElement AddCanvas()
        {
            var canvas = new VisualElement().AddClass(CanvasClass);
            _panel.Root.Add(canvas);
            return canvas;
        }

        private static int IndexOf(VisualElement element, StyleSheet sheet)
        {
            for (var i = 0; i < element.styleSheets.count; i++)
            {
                if (element.styleSheets[i] == sheet) return i;
            }

            return -1;
        }

        private static Color Blend(Color color, Color background, float opacity) =>
            Color.Lerp(background, new Color(color.r, color.g, color.b, 1f), color.a * opacity);

        private static float Contrast(Color first, Color second)
        {
            var a = Luminance(first);
            var b = Luminance(second);
            return (Mathf.Max(a, b) + 0.05f) / (Mathf.Min(a, b) + 0.05f);
        }

        // WCAG relative luminance of an sRGB colour.
        private static float Luminance(Color color) =>
            0.2126f * Linear(color.r) + 0.7152f * Linear(color.g) + 0.0722f * Linear(color.b);

        private static float Linear(float channel) => channel <= 0.03928f
            ? channel / 12.92f
            : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
    }
}
