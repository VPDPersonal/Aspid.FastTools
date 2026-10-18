using System;
using System.IO;
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
    /// A theme of the project replaces the built-in theme over the palettes and is applied live: an element that loads the
    /// Aspid palette follows a change of the theme at once, whether it was already in a panel when it loaded the palette
    /// or joined one later. A built-in theme sits after both palettes, its light block wins over the light palette, and
    /// Venom recolors the accent but not the success status. The settings surface offers one Theme dropdown that lists
    /// the built-in themes, then the labelled sheets of the project, and makes a theme from a copy of the current one.
    /// </summary>
    [TestFixture]
    internal sealed class AspidThemeTests
    {
        private const string ProbeFolder = "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements";
        private const string ProbePath = ProbeFolder + "/AspidThemeProbe.uss";
        private const string CreatedThemePath = "Assets/AspidThemeTests-Created.uss";

        private TestPanel _panel;
        private StyleSheet _probe;
        private StyleSheet _savedCustom;
        private AspidTheme _savedTheme;

        [SetUp]
        public void SetUp()
        {
            _savedCustom = AspidThemeSettings.CustomTheme;
            _savedTheme = AspidThemeSettings.Theme;
            AspidThemeSettings.Theme = AspidTheme.Default;

            _probe = AssetDatabase.LoadAssetAtPath<StyleSheet>(ProbePath);
            Assert.IsNotNull(_probe, "The probe theme must load.");

            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();

            // Picking the built-in theme clears the custom one, so the custom theme is restored after it.
            AspidThemeSettings.Theme = _savedTheme;
            if (_savedCustom != null) AspidThemeSettings.CustomTheme = _savedCustom;
        }

        [UnityTest]
        public IEnumerator ElementInPanel_FollowsCustomThemeChange()
        {
            yield return null;

            var element = new VisualElement();
            _panel.Root.Add(element);
            Assert.IsNotNull(element.panel, "The element must be in a panel before it loads the palette.");

            element.AddAspidThemeStyleSheets();
            Assert.IsFalse(element.styleSheets.Contains(_probe));

            AspidThemeSettings.CustomTheme = _probe;
            Assert.IsTrue(element.styleSheets.Contains(_probe), "A picked theme must reach an element already in a panel.");

            AspidThemeSettings.CustomTheme = null;
            Assert.IsFalse(element.styleSheets.Contains(_probe), "A cleared theme must leave an element already in a panel.");
        }

        [UnityTest]
        public IEnumerator ElementAttachedLater_FollowsCustomThemeChange()
        {
            yield return null;

            var element = new VisualElement().AddAspidThemeStyleSheets();
            _panel.Root.Add(element);

            AspidThemeSettings.CustomTheme = _probe;
            Assert.IsTrue(element.styleSheets.Contains(_probe), "A picked theme must reach an element that joined a panel later.");
        }

        [UnityTest]
        public IEnumerator ElementLeavingPanel_StopsFollowingTheme()
        {
            yield return null;

            var element = new VisualElement();
            _panel.Root.Add(element);
            element.AddAspidThemeStyleSheets();

            element.RemoveFromHierarchy();
            AspidThemeSettings.CustomTheme = _probe;
            Assert.IsFalse(element.styleSheets.Contains(_probe), "An element outside a panel must not subscribe to the theme.");

            _panel.Root.Add(element);
            Assert.IsTrue(element.styleSheets.Contains(_probe), "An element back in a panel must pick the current theme up.");

            AspidThemeSettings.CustomTheme = null;
            Assert.IsFalse(element.styleSheets.Contains(_probe), "An element back in a panel must follow the theme again.");
        }

        [UnityTest]
        public IEnumerator CustomTheme_WinsOverBuiltInPalette_AndLeavesWithIt()
        {
            yield return null;

            var host = _panel.Root.AddAspidThemeStyleSheets();
            var box = new VisualElement()
                .AddClass("aspid-fasttools-background")
                .AddClass("aspid-fasttools-theme--dark");
            host.Add(box);
            yield return null;

            var builtIn = box.resolvedStyle.backgroundColor;
            var custom = new Color(1f / 255f, 2f / 255f, 3f / 255f);
            Assert.AreNotEqual(custom, builtIn, "The built-in palette must not already use the probe color.");

            AspidThemeSettings.CustomTheme = _probe;
            yield return null;
            Assert.AreEqual(custom, box.resolvedStyle.backgroundColor, "The theme must win over the built-in palette.");

            AspidThemeSettings.CustomTheme = null;
            yield return null;
            Assert.AreEqual(builtIn, box.resolvedStyle.backgroundColor, "Clearing the theme must bring the palette back.");
        }

        [UnityTest]
        public IEnumerator CustomTheme_WinsOverLightPalette()
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

            host.AddStyleSheet(_probe);
            yield return null;
            var custom = new Color(1f / 255f, 2f / 255f, 3f / 255f);
            Assert.AreEqual(custom, box.resolvedStyle.backgroundColor, "The theme must win over the light palette.");
        }

        [UnityTest]
        public IEnumerator ThemeSheet_IsAddedAfterBothPalettes_AndCustomReplacesBuiltIn()
        {
            yield return null;

            AspidThemeSettings.Theme = AspidTheme.Venom;
            var host = _panel.Root.AddAspidThemeStyleSheets();

            var dark = Resources.Load<StyleSheet>(AspidStyles.DefaultStyleSheet);
            var light = Resources.Load<StyleSheet>(AspidStyles.LightStyleSheet);
            var venom = Resources.Load<StyleSheet>(AspidStyles.VenomThemeStyleSheet);
            CollectionAssert.AreEqual(
                new[] { dark, light, venom },
                StyleSheets(host),
                "The theme must come after both palettes, so it wins their ties.");

            AspidThemeSettings.CustomTheme = _probe;
            CollectionAssert.AreEqual(
                new[] { dark, light, _probe },
                StyleSheets(host),
                "A theme of the project must take the place of the built-in theme.");

            AspidThemeSettings.Theme = AspidTheme.Default;
            CollectionAssert.AreEqual(
                new[] { dark, light },
                StyleSheets(host),
                "Default must leave only the palettes.");
        }

        [Test]
        public void PickingBuiltInTheme_LeavesCustomTheme()
        {
            AspidThemeSettings.CustomTheme = _probe;
            Assert.AreEqual(_probe, AspidThemeSettings.CustomTheme);

            var changes = 0;
            void OnChanged() => changes++;
            AspidThemeSettings.Changed += OnChanged;

            try
            {
                // The built-in theme is already Default, so only the custom theme changes.
                AspidThemeSettings.Theme = AspidTheme.Default;
            }
            finally
            {
                AspidThemeSettings.Changed -= OnChanged;
            }

            Assert.IsNull(AspidThemeSettings.CustomTheme, "Picking a built-in theme must leave the custom one.");
            Assert.AreEqual(1, changes, "Leaving the custom theme must raise one change.");
        }

        [Test]
        public void SettingsSurface_OffersThemeDropdown_BetweenTypeSelectorAndWelcome()
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

            Assert.IsNotNull(container.Q<AspidThemeField>(), "The Appearance section must offer the Theme dropdown.");
            Assert.IsNull(container.Q<ObjectField>(), "A theme of the project is picked in the dropdown, not in an object field.");
        }

        [Test]
        public void SettingsSurface_SharedScope_HasNoThemeDropdown()
        {
            var container = new VisualElement();
            AspidSettingsUI.BuildSurfaceContent(container, AspidSettingsScope.Shared);

            Assert.IsNull(container.Q<AspidThemeField>(), "The theme is a per-user setting.");
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
        public IEnumerator Venom_TakesTheBlueAccent_AndLeavesSuccessGreen()
        {
            yield return null;

            var host = _panel.Root.AddAspidThemeStyleSheets();
            yield return null;
            var success = Token(host, "--aspid-colors-status-success-text-dark");
            Assert.AreEqual(Rgb(92, 180, 107), Token(host, "--aspid-colors-accent-text-dark"), "Default keeps the green accent.");

            AspidThemeSettings.Theme = AspidTheme.Venom;
            yield return null;
            Assert.AreEqual(Rgb(78, 165, 218), Token(host, "--aspid-colors-accent-text-dark"), "Venom must take the blue accent.");
            Assert.AreEqual(success, Token(host, "--aspid-colors-status-success-text-dark"), "The success status must stay green.");
        }

        [UnityTest]
        public IEnumerator BuildControls_LiveSyncsThemeFieldFromSettings()
        {
            yield return null;

            var container = new VisualElement();
            _panel.Root.Add(container);
            AspidThemeSettingsUI.BuildControls(container);

            var field = container.Q<AspidThemeField>();
            Assert.IsNotNull(field, "BuildControls must emit the Theme dropdown.");
            Assert.AreEqual("Default", field.value);

            AspidThemeSettings.Theme = AspidTheme.Venom;
            Assert.AreEqual("Venom", field.value, "The dropdown must mirror the built-in theme live.");

            AspidThemeSettings.CustomTheme = _probe;
            Assert.AreEqual(_probe.name, field.value, "The dropdown must name the theme of the project.");

            AspidThemeSettings.Theme = AspidTheme.Default;
            Assert.AreEqual("Default", field.value, "The dropdown must follow the pick of a built-in theme.");
        }

        [Test]
        public void ThemeField_Step_WalksTheThemesWithoutWrapping()
        {
            var field = new AspidThemeField("Theme", createTheme: null);

            field.Step(-1);
            Assert.AreEqual(AspidTheme.Default, AspidThemeSettings.Theme, "Stepping back from the first theme must keep it.");

            field.Step(+1);
            Assert.AreEqual(AspidTheme.Venom, AspidThemeSettings.Theme);

            field.Step(+1);
            Assert.AreEqual(AspidTheme.Unity, AspidThemeSettings.Theme);

            // A current sheet outside Assets is listed last, so the arrows stop on it.
            AspidThemeSettings.CustomTheme = _probe;
            field.Step(+1);
            Assert.AreEqual(_probe, AspidThemeSettings.CustomTheme, "Stepping on from the last theme must keep it.");

            field.Step(-1);
            Assert.IsNull(AspidThemeSettings.CustomTheme, "Stepping back from a custom theme must reach a built-in one.");
        }

        [Test]
        public void FindCustomThemes_ListsLabelledSheets_OnlyInTheGivenFolders()
        {
            CollectionAssert.Contains(AspidThemeSettings.FindCustomThemes(ProbeFolder), _probe,
                "A sheet with the theme label must be listed.");
            CollectionAssert.DoesNotContain(AspidThemeSettings.FindCustomThemes(), _probe,
                "The dropdown must list only the sheets in Assets, not those of packages.");
        }

        [Test]
        public void CurrentThemeSource_CopiesTheCurrentTheme()
        {
            var source = AspidThemeSettingsUI.GetCurrentThemeSource();
            StringAssert.Contains("made from Default", source);
            StringAssert.Contains("--aspid-colors-accent-text-dark", source);

            AspidThemeSettings.Theme = AspidTheme.Venom;
            source = AspidThemeSettingsUI.GetCurrentThemeSource();
            StringAssert.Contains("made from Venom", source);
            StringAssert.Contains("rgb(78, 165, 218)", source, "A copy of Venom must carry its blue accent.");

            AspidThemeSettings.CustomTheme = _probe;
            Assert.AreEqual(
                File.ReadAllText(FileUtil.GetPhysicalPath(ProbePath)),
                AspidThemeSettingsUI.GetCurrentThemeSource(),
                "A theme of the project must be copied as it is.");
        }

        [UnityTest]
        public IEnumerator CreateTheme_WritesALabelledCopy_AndPicksIt()
        {
            yield return null;

            AspidThemeSettings.Theme = AspidTheme.Venom;

            try
            {
                var sheet = AspidThemeSettingsUI.CreateTheme(CreatedThemePath);
                Assert.IsNotNull(sheet, "The new theme must import as a style sheet.");

                CollectionAssert.Contains(AssetDatabase.GetLabels(sheet), AspidThemeSettings.CustomThemeLabel,
                    "The new theme must carry the theme label.");
                CollectionAssert.Contains(AspidThemeSettings.FindCustomThemes(), sheet,
                    "The new theme must be listed in the dropdown.");
                Assert.AreEqual(sheet, AspidThemeSettings.CustomTheme, "The new theme must be picked.");

                var host = _panel.Root.AddAspidThemeStyleSheets();
                yield return null;
                Assert.AreEqual(Rgb(78, 165, 218), Token(host, "--aspid-colors-accent-text-dark"),
                    "A copy of Venom must look like Venom.");
            }
            finally
            {
                AspidThemeSettings.CustomTheme = null;
                AssetDatabase.DeleteAsset(CreatedThemePath);
            }
        }

        [Test]
        public void DeletingThePickedTheme_RaisesAChange_AndFallsBackToTheBuiltIn()
        {
            AspidThemeSettings.Theme = AspidTheme.Venom;
            var sheet = AspidThemeSettingsUI.CreateTheme(CreatedThemePath);
            Assert.AreEqual(sheet, AspidThemeSettings.CustomTheme);

            var changes = 0;
            void OnChanged() => changes++;
            AspidThemeSettings.Changed += OnChanged;

            try
            {
                AssetDatabase.DeleteAsset(CreatedThemePath);
            }
            finally
            {
                AspidThemeSettings.Changed -= OnChanged;
                AspidThemeSettings.CustomTheme = null;
            }

            Assert.Greater(changes, 0, "Deleting the picked sheet must tell the windows and the dropdown.");
            Assert.AreEqual(AspidTheme.Venom, AspidThemeSettings.Theme, "The built-in theme must stay for the fallback.");
        }

        [Test]
        public void CreateTheme_OverAnExistingSheet_KeepsItsLabels()
        {
            try
            {
                File.WriteAllText(CreatedThemePath, ":root {}\n");
                AssetDatabase.ImportAsset(CreatedThemePath);
                AssetDatabase.SetLabels(AssetDatabase.LoadAssetAtPath<StyleSheet>(CreatedThemePath), new[] { "Keep" });

                var sheet = AspidThemeSettingsUI.CreateTheme(CreatedThemePath);
                CollectionAssert.IsSupersetOf(AssetDatabase.GetLabels(sheet),
                    new[] { "Keep", AspidThemeSettings.CustomThemeLabel },
                    "The theme label must join the labels the sheet had.");
            }
            finally
            {
                AspidThemeSettings.CustomTheme = null;
                AssetDatabase.DeleteAsset(CreatedThemePath);
            }
        }

        private static List<StyleSheet> StyleSheets(VisualElement element)
        {
            var sheets = new List<StyleSheet>();
            for (var index = 0; index < element.styleSheets.count; index++)
                sheets.Add(element.styleSheets[index]);

            return sheets;
        }

        private static Color Rgb(int r, int g, int b) => new(r / 255f, g / 255f, b / 255f);

        private static Color Token(VisualElement element, string name)
        {
            Assert.IsTrue(element.customStyle.TryGetValue(new CustomStyleProperty<Color>(name), out var color),
                $"The palette must declare {name}.");
            return color;
        }
    }
}
