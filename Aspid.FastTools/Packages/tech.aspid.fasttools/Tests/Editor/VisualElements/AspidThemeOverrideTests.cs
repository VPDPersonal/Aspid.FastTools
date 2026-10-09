using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// The theme override wins over the built-in palette and is applied live: an element that loads the Aspid palette
    /// follows a change of the override at once, whether it was already in a panel when it loaded the palette or
    /// joined one later. The settings surface offers the picker for it.
    /// </summary>
    [TestFixture]
    internal sealed class AspidThemeOverrideTests
    {
        private const string OverrideStyleSheetPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/AspidThemeOverrideProbe.uss";

        private TestPanel _panel;
        private StyleSheet _override;
        private StyleSheet _savedOverride;

        [SetUp]
        public void SetUp()
        {
            _savedOverride = AspidThemeSettings.OverrideStyleSheet;
            AspidThemeSettings.OverrideStyleSheet = null;

            _override = AssetDatabase.LoadAssetAtPath<StyleSheet>(OverrideStyleSheetPath);
            Assert.IsNotNull(_override, "The override style sheet must load.");

            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();
            AspidThemeSettings.OverrideStyleSheet = _savedOverride;
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
    }
}
