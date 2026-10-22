using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Object = UnityEngine.Object;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    // The status stripe left of a shared reference, and the gutter it makes, follow the per-user "Field stripes" switch
    // under Appearance: a field built with the switch off has neither, and an open field drops and restores both live.
    [TestFixture]
    internal sealed class SerializeReferenceFieldStripeTests
    {
        private const string StripedClass = "aspid-fasttools-serialize-reference--striped";
        private const string StripeClass = "aspid-fasttools-serialize-reference__stripe";
        private const string StripeActiveClass = StripeClass + "--active";

        // A runtime panel, not a shown EditorWindow: only the former updates without a graphics device (-nographics).
        private const string PanelSettingsPath =
            "Packages/tech.aspid.fasttools/Tests/Editor/VisualElements/TestPanel/AspidFastToolsTestPanelSettings.asset";

        private bool _enabled;
        private GameObject _host;
        private PanelSettings _settings;
        private LinkerTestObject _object;

        [SetUp]
        public void SetUp()
        {
            _enabled = FieldStripeSettings.Enabled;
            FieldStripeSettings.Enabled = true;

            _object = ScriptableObject.CreateInstance<LinkerTestObject>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_host) Object.DestroyImmediate(_host);
            if (_settings) Object.DestroyImmediate(_settings);
            if (_object) Object.DestroyImmediate(_object);
            FieldStripeSettings.Enabled = _enabled;
        }

        [Test]
        public void SharedField_BuiltWithTheSwitchOn_HasTheStripe() =>
            AssertStriped(CreateSharedField(), expected: true, because: "A shared reference must draw its stripe.");

        [Test]
        public void SharedField_BuiltWithTheSwitchOff_HasNoStripe()
        {
            FieldStripeSettings.Enabled = false;

            AssertStriped(CreateSharedField(), expected: false,
                because: "With the switch off a field must draw neither the stripe nor its gutter.");
        }

        [UnityTest]
        public IEnumerator OpenSharedField_FollowsTheSwitch()
        {
            var field = CreateSharedField();
            CreatePanelRoot().Add(field);
            yield return null;

            FieldStripeSettings.Enabled = false;
            AssertStriped(field, expected: false, because: "Turning the switch off must apply to a field that is already open.");

            FieldStripeSettings.Enabled = true;
            AssertStriped(field, expected: true, because: "Turning it back on must bring the stripe back.");
        }

        // Two fields on one rid make field 'a' a shared reference that has a value.
        private SerializeReferenceField CreateSharedField()
        {
            var serialized = new SerializedObject(_object);
            serialized.FindProperty("a").managedReferenceValue = new TestSword { damage = 7 };
            serialized.ApplyModifiedProperties();

            Assert.IsTrue(SerializeReferenceLinker.LinkTo(serialized.FindProperty("b"), "a"));
            serialized.Update();

            return new SerializeReferenceField("A", serialized.FindProperty("a"));
        }

        private VisualElement CreatePanelRoot()
        {
            _settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath));
            _settings.hideFlags = HideFlags.HideAndDontSave;

            _host = new GameObject(nameof(SerializeReferenceFieldStripeTests)) { hideFlags = HideFlags.HideAndDontSave };
            var document = _host.AddComponent<UIDocument>();
            document.panelSettings = _settings;

            return document.rootVisualElement;
        }

        private static void AssertStriped(VisualElement field, bool expected, string because)
        {
            Assert.AreEqual(expected, field.ClassListContains(StripedClass), because);

            var stripe = field.Q(className: StripeClass);
            Assert.AreEqual(expected, stripe is not null && stripe.ClassListContains(StripeActiveClass), because);
        }
    }
}
