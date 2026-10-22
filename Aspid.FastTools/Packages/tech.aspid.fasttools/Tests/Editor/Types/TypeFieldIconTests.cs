using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;
using Aspid.FastTools.Tests.Editors.VisualElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.Types
{
    // The type icon left of the caption: shown for a resolved type only, and hidden live by the per-user
    // "Type icons" switch under Appearance.
    [TestFixture]
    internal sealed class TypeFieldIconTests
    {
        private const string IconClass = "aspid-fasttools-type-field__icon";
        private const string MissingName = "Missing.Namespace.GoneType, Missing.Assembly";

        private bool _enabled;
        private TestPanel _panel;

        [SetUp]
        public void SetUp()
        {
            _enabled = TypeFieldIconSettings.Enabled;
            TypeFieldIconSettings.Enabled = true;
            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();
            TypeFieldIconSettings.Enabled = _enabled;
        }

        [UnityTest]
        public IEnumerator Icon_ShowsOnlyForAResolvedType()
        {
            var field = AddField();
            yield return null;
            var icon = field.Q<Image>(className: IconClass);
            AssertHidden(icon, because: "<None> has no type to show.");

            field.SetValueWithoutNotify(typeof(Camera));
            Assert.AreEqual(TypeSelectorIconResolver.ResolveForType(typeof(Camera)), icon.image);
            Assert.AreEqual(DisplayStyle.Flex, icon.style.display.value);

            field.SetValueFromAssemblyQualifiedNameWithoutNotify(assemblyQualifiedName: MissingName);
            AssertHidden(icon, because: "A missing type has no icon; its amber caption says it is missing.");

            field.SetValueWithoutNotify(typeof(Camera));
            field.showMixedValue = true;
            AssertHidden(icon, because: "Mixed values show the dash only.");
        }

        [UnityTest]
        public IEnumerator Switch_HidesAndShowsTheIconOfAnOpenField()
        {
            var field = AddField();
            field.SetValueWithoutNotify(typeof(Camera));
            yield return null;
            var icon = field.Q<Image>(className: IconClass);

            TypeFieldIconSettings.Enabled = false;
            AssertHidden(icon, because: "Turning the switch off must apply to a field that is already open.");

            TypeFieldIconSettings.Enabled = true;
            Assert.AreEqual(DisplayStyle.Flex, icon.style.display.value, "Turning it back on must bring the icon back.");
        }

        [UnityTest]
        public IEnumerator AppearanceSwitch_MirrorsTheStoreLive()
        {
            // A value change reaches its callbacks only inside a panel.
            var container = new VisualElement();
            _panel.Root.Add(container);
            AspidAppearanceSettingsUI.BuildControls(container);
            yield return null;

            var typeIcons = container.Q<AspidSwitch>(AspidAppearanceSettingsUI.TypeIconsName);
            Assert.IsNotNull(typeIcons, "BuildControls must emit the type icons switch.");
            Assert.IsTrue(typeIcons.value, "The icons are on by default.");

            TypeFieldIconSettings.Enabled = false;
            Assert.IsFalse(typeIcons.value, "The switch must mirror the store without a manual refresh.");

            typeIcons.value = true;
            Assert.IsTrue(TypeFieldIconSettings.Enabled, "The switch must write the store.");
        }

        private TypeField AddField()
        {
            var field = new TypeField();
            _panel.Root.Add(field);
            return field;
        }

        private static void AssertHidden(Image icon, string because)
        {
            Assert.IsNull(icon.image, because);
            Assert.AreEqual(DisplayStyle.None, icon.style.display.value, because);
        }
    }
}
