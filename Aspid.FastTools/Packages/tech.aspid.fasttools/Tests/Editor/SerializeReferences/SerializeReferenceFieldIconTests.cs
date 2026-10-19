using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Aspid.FastTools.Types.Editors;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The type icon left of the caption: shown for a set reference only, and hidden live by the per-user
    // "Type icons" switch under Appearance, also after the field is detached and attached again.
    [TestFixture]
    [Category("Graphics")]
    internal sealed class SerializeReferenceFieldIconTests
    {
        private const string IconClass = "aspid-fasttools-serialize-reference__icon";

        private bool _enabled;
        private EditorWindow _window;
        private LinkerTestObject _first;
        private LinkerTestObject _second;

        [SetUp]
        public void SetUp()
        {
            _enabled = TypeFieldIconSettings.Enabled;
            TypeFieldIconSettings.Enabled = true;

            _window = ScriptableObject.CreateInstance<EditorWindow>();
            _window.ShowUtility();

            _first = ScriptableObject.CreateInstance<LinkerTestObject>();
            _second = ScriptableObject.CreateInstance<LinkerTestObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_window) Object.DestroyImmediate(_window);
            if (_first) Object.DestroyImmediate(_first);
            if (_second) Object.DestroyImmediate(_second);
            TypeFieldIconSettings.Enabled = _enabled;
        }

        [UnityTest]
        public IEnumerator Switch_HidesAndShowsTheIcon_AlsoAfterReattach()
        {
            _first.a = new TestSword();
            var field = AddField(_first);
            yield return null;
            var icon = field.Q<Image>(className: IconClass);

            Assert.AreEqual(TypeSelectorIconResolver.ResolveForType(typeof(TestSword)), icon.image);
            Assert.AreEqual(DisplayStyle.Flex, icon.style.display.value);

            TypeFieldIconSettings.Enabled = false;
            AssertHidden(icon, because: "Turning the switch off must apply to a field that is already open.");

            field.RemoveFromHierarchy();
            _window.rootVisualElement.Add(field);
            yield return null;

            TypeFieldIconSettings.Enabled = true;
            Assert.AreEqual(DisplayStyle.Flex, icon.style.display.value,
                "A re-attached field must still follow the switch.");
        }

        [UnityTest]
        public IEnumerator Icon_HiddenForAnEmptyReference()
        {
            var field = AddField(_first);
            yield return null;

            AssertHidden(field.Q<Image>(className: IconClass), because: "<None> has no type to show.");
        }

        [UnityTest]
        public IEnumerator Icon_HiddenForMixedTypes()
        {
            _first.a = new TestSword();
            _second.a = new SerializeReferenceFieldMixedValueTests.MixedValueShield();
            var field = AddField(_first, _second);
            yield return null;

            AssertHidden(field.Q<Image>(className: IconClass), because: "Mixed types show the dash only.");
        }

        private SerializeReferenceField AddField(params Object[] targets)
        {
            var field = new SerializeReferenceField("A", new SerializedObject(targets).FindProperty("a"));
            _window.rootVisualElement.Add(field);
            return field;
        }

        private static void AssertHidden(Image icon, string because)
        {
            Assert.IsNull(icon.image, because);
            Assert.AreEqual(DisplayStyle.None, icon.style.display.value, because);
        }
    }
}
