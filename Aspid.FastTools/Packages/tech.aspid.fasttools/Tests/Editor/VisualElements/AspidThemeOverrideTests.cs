using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// The theme override wins over the built-in palette and is applied live: an element that loads the Aspid palette
    /// follows a change of the override at once, whether it was already in a panel when it loaded the palette or
    /// joined one later. A built-in theme sits between the palettes and the override, and its light block wins over
    /// the light palette. The settings surface offers the theme dropdown and the picker for the override.
    /// </summary>
    [TestFixture]
    internal sealed class AspidThemeOverrideTests
    {
        private const string OverrideStyleSheetPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/AspidThemeOverrideProbe.uss";

        private TestPanel _panel;
        private StyleSheet _override;
        private StyleSheet _savedOverride;
        private AspidTheme _savedTheme;

        [SetUp]
        public void SetUp()
        {
            _savedOverride = AspidThemeSettings.OverrideStyleSheet;
            _savedTheme = AspidThemeSettings.Theme;
            AspidThemeSettings.OverrideStyleSheet = null;
            AspidThemeSettings.Theme = AspidTheme.Default;

            _override = AssetDatabase.LoadAssetAtPath<StyleSheet>(OverrideStyleSheetPath);
            Assert.IsNotNull(_override, "The override style sheet must load.");

            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();
            AspidThemeSettings.OverrideStyleSheet = _savedOverride;
            AspidThemeSettings.Theme = _savedTheme;
        }

        [UnityTest]
        public IEnumerator ElementInPanel_FollowsOverrideChange()
        {
            yield return null;

            var element = new VisualElement();
            _panel.Root.Add(element);
            Assert.IsNotNull(element.panel, "The element must be in a panel before it loads the palette.");

            element.AddAspidThemeStyleSheets();
            Assert.IsFalse(element.styleSheets.Contains(_override));

            AspidThemeSettings.OverrideStyleSheet = _override;
            Assert.IsTrue(element.styleSheets.Contains(_override), "A set override must reach an element already in a panel.");

            AspidThemeSettings.OverrideStyleSheet = null;
            Assert.IsFalse(element.styleSheets.Contains(_override), "A cleared override must leave an element already in a panel.");
        }

        [UnityTest]
        public IEnumerator ElementAttachedLater_FollowsOverrideChange()
        {
            yield return null;

            var element = new VisualElement().AddAspidThemeStyleSheets();
            _panel.Root.Add(element);

            AspidThemeSettings.OverrideStyleSheet = _override;
            Assert.IsTrue(element.styleSheets.Contains(_override), "A set override must reach an element that joined a panel later.");
        }

        [UnityTest]
        public IEnumerator ElementLeavingPanel_StopsFollowingOverride()
        {
            yield return null;

            var element = new VisualElement();
            _panel.Root.Add(element);
            element.AddAspidThemeStyleSheets();

            element.RemoveFromHierarchy();
            AspidThemeSettings.OverrideStyleSheet = _override;
            Assert.IsFalse(element.styleSheets.Contains(_override), "An element outside a panel must not subscribe to the override.");

            _panel.Root.Add(element);
            Assert.IsTrue(element.styleSheets.Contains(_override), "An element back in a panel must pick the current override up.");

            AspidThemeSettings.OverrideStyleSheet = null;
            Assert.IsFalse(element.styleSheets.Contains(_override), "An element back in a panel must follow the override again.");
        }

        [UnityTest]
        public IEnumerator Override_WinsOverBuiltInPalette_AndLeavesWithIt()
        {
            yield return null;

            var host = _panel.Root.AddAspidThemeStyleSheets();
            var box = new VisualElement()
                .AddClass("aspid-fasttools-background")
                .AddClass("aspid-fasttools-theme--dark");
            host.Add(box);
            yield return null;

            var builtIn = box.resolvedStyle.backgroundColor;
            var overridden = new Color(1f / 255f, 2f / 255f, 3f / 255f);
            Assert.AreNotEqual(overridden, builtIn, "The built-in palette must not already use the override color.");

            AspidThemeSettings.OverrideStyleSheet = _override;
            yield return null;
            Assert.AreEqual(overridden, box.resolvedStyle.backgroundColor, "The override must win over the built-in palette.");

            AspidThemeSettings.OverrideStyleSheet = null;
            yield return null;
            Assert.AreEqual(builtIn, box.resolvedStyle.backgroundColor, "Clearing the override must bring the palette back.");
        }

        [UnityTest]
        public IEnumerator Override_WinsOverLightPalette()
        {
            yield return null;

            // The light class is set by hand: AddAspidThemeStyleSheets would remove it again on a dark editor skin.
            // The palettes tie in specificity (:root and a class), so the sheet order must decide.
            var host = _panel.Root
                .AddStyleSheetFromResources(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetFromResources(AspidStyles.LightStyleSheet);

            var box = new VisualElement()
                .AddClass("aspid-fasttools-background")
                .AddClass("aspid-fasttools-theme--dark");
            host.Add(box);
            yield return null;
            var dark = box.resolvedStyle.backgroundColor;

            host.AddClass(AspidStyles.PaletteLightClass);
            yield return null;
            Assert.AreNotEqual(dark, box.resolvedStyle.backgroundColor, "The light class must switch the palette.");

            host.AddStyleSheet(_override);
            yield return null;
            var overridden = new Color(1f / 255f, 2f / 255f, 3f / 255f);
            Assert.AreEqual(overridden, box.resolvedStyle.backgroundColor, "The override must win over the light palette.");
        }

        [UnityTest]
        public IEnumerator OverrideSheet_IsAddedAfterBothPalettes()
        {
            yield return null;

            AspidThemeSettings.OverrideStyleSheet = _override;
            var host = _panel.Root.AddAspidThemeStyleSheets();

            var sheets = new List<StyleSheet>();
            for (var index = 0; index < host.styleSheets.count; index++)
                sheets.Add(host.styleSheets[index]);

            var dark = Resources.Load<StyleSheet>(AspidStyles.DefaultStyleSheet);
            var light = Resources.Load<StyleSheet>(AspidStyles.LightStyleSheet);
            CollectionAssert.AreEqual(
                new[] { dark, light, _override },
                sheets,
                "The override must come after both palettes, so it wins their ties.");
        }

        [Test]
        public void SettingsSurface_OffersThemeOverride_BetweenTypeSelectorAndWelcome()
        {
            var container = new VisualElement();
            AspidSettingsUI.BuildSurfaceContent(container, AspidSettingsScope.User);

            var titles = container
                .Query<Label>(className: AspidSettingsUI.SectionTitleClass)
                .ToList()
                .Select(label => label.text)
                .ToList();

            CollectionAssert.AreEqual(
                new[] { "SerializeReference", "Type Selector", "Appearance", "Welcome" },
                titles,
                "The user scope must offer the Appearance section.");

            var field = container.Q<ObjectField>();
            Assert.IsNotNull(field, "The Appearance section must offer the theme override field.");
            Assert.AreEqual(typeof(StyleSheet), field.objectType);
        }

        [Test]
        public void SettingsSurface_SharedScope_HasNoThemeOverride()
        {
            var container = new VisualElement();
            AspidSettingsUI.BuildSurfaceContent(container, AspidSettingsScope.Shared);

            Assert.IsNull(container.Q<ObjectField>(), "The theme override is a per-user setting.");
        }

        [UnityTest]
        public IEnumerator BuildControls_LiveSyncsFieldFromSettings()
        {
            yield return null;

            // A field outside a panel sets its value without notifying, so the container joins the panel.
            var container = new VisualElement();
            _panel.Root.Add(container);
            AspidThemeSettingsUI.BuildControls(container);

            var field = container.Q<ObjectField>();
            Assert.IsNotNull(field, "BuildControls must emit the theme override field.");
            Assert.IsNull(field.value);

            // A change of the store (as another surface makes) must reach the field without a manual refresh.
            AspidThemeSettings.OverrideStyleSheet = _override;
            Assert.AreEqual(_override, field.value, "The field must mirror the settings live.");

            field.value = null;
            Assert.IsNull(AspidThemeSettings.OverrideStyleSheet, "Clearing the field must clear the override.");
        }

        [Test]
        public void EveryBuiltInTheme_LoadsItsSheet()
        {
            foreach (AspidTheme theme in Enum.GetValues(typeof(AspidTheme)))
            {
                AspidThemeSettings.Theme = theme;

                if (theme is AspidTheme.Default) Assert.IsNull(AspidThemeSettings.ThemeStyleSheet, "Default adds no sheet.");
                else Assert.IsNotNull(AspidThemeSettings.ThemeStyleSheet, $"The {theme} theme must load its sheet.");
            }
        }

        [UnityTest]
        public IEnumerator ThemeSheet_IsAddedBetweenPalettesAndOverride()
        {
            yield return null;

            AspidThemeSettings.Theme = AspidTheme.Venom;
            AspidThemeSettings.OverrideStyleSheet = _override;
            var host = _panel.Root.AddAspidThemeStyleSheets();

            var dark = Resources.Load<StyleSheet>(AspidStyles.DefaultStyleSheet);
            var light = Resources.Load<StyleSheet>(AspidStyles.LightStyleSheet);
            var venom = Resources.Load<StyleSheet>(AspidStyles.VenomThemeStyleSheet);
            CollectionAssert.AreEqual(
                new[] { dark, light, venom, _override },
                StyleSheets(host),
                "The theme must win the ties of both palettes and lose those of the override.");

            // A change of the theme must keep the override last.
            AspidThemeSettings.Theme = AspidTheme.Graphite;
            var graphite = Resources.Load<StyleSheet>(AspidStyles.GraphiteThemeStyleSheet);
            CollectionAssert.AreEqual(
                new[] { dark, light, graphite, _override },
                StyleSheets(host),
                "A new theme must replace the old one and stay under the override.");
        }

        [UnityTest]
        public IEnumerator Theme_WinsOverBuiltInPalette_AndLeavesWithIt()
        {
            yield return null;

            var host = _panel.Root.AddAspidThemeStyleSheets();
            var box = new VisualElement()
                .AddClass("aspid-fasttools-background")
                .AddClass("aspid-fasttools-theme--dark");
            host.Add(box);
            yield return null;
            var builtIn = box.resolvedStyle.backgroundColor;

            AspidThemeSettings.Theme = AspidTheme.Venom;
            yield return null;
            Assert.AreEqual(Rgb(30, 34, 44), box.resolvedStyle.backgroundColor, "The theme must win over the built-in palette.");

            AspidThemeSettings.Theme = AspidTheme.Default;
            yield return null;
            Assert.AreEqual(builtIn, box.resolvedStyle.backgroundColor, "Default must bring the built-in palette back.");
        }

        [UnityTest]
        public IEnumerator ThemeLightBlock_WinsOverLightPalette()
        {
            yield return null;

            // The light class is set by hand: AddAspidThemeStyleSheets would remove it again on a dark editor skin.
            var host = _panel.Root
                .AddStyleSheetFromResources(AspidStyles.DefaultStyleSheet)
                .AddStyleSheetFromResources(AspidStyles.LightStyleSheet)
                .AddStyleSheetFromResources(AspidStyles.VenomThemeStyleSheet)
                .AddClass(AspidStyles.PaletteLightClass);

            var box = new VisualElement()
                .AddClass("aspid-fasttools-background")
                .AddClass("aspid-fasttools-theme--dark");
            host.Add(box);
            yield return null;

            Assert.AreEqual(Rgb(237, 238, 241), box.resolvedStyle.backgroundColor,
                "The light block of the theme must win over both the light palette and the theme's own :root.");
        }

        [UnityTest]
        public IEnumerator BuildControls_LiveSyncsThemeFieldFromSettings()
        {
            yield return null;

            var container = new VisualElement();
            _panel.Root.Add(container);
            AspidThemeSettingsUI.BuildControls(container);

            var field = container.Q<EnumField>();
            Assert.IsNotNull(field, "BuildControls must emit the theme dropdown.");
            Assert.AreEqual(AspidTheme.Default, field.value);

            AspidThemeSettings.Theme = AspidTheme.Graphite;
            Assert.AreEqual(AspidTheme.Graphite, field.value, "The dropdown must mirror the settings live.");

            field.value = AspidTheme.Venom;
            Assert.AreEqual(AspidTheme.Venom, AspidThemeSettings.Theme, "Picking a theme must store it.");
        }

        private static List<StyleSheet> StyleSheets(VisualElement element)
        {
            var sheets = new List<StyleSheet>();
            for (var index = 0; index < element.styleSheets.count; index++)
                sheets.Add(element.styleSheets[index]);

            return sheets;
        }

        private static Color Rgb(int r, int g, int b) => new(r / 255f, g / 255f, b / 255f);
    }
}
