using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.Types.Editors;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.Settings
{
    // The per-user "Field stripes" switch under Appearance: it mirrors the store, and a stripe bound to it follows it
    // live, also after the stripe is detached and attached again.
    [TestFixture]
    internal sealed class FieldStripeSettingsTests
    {
        private bool _enabled;
        private TestPanel _panel;

        [SetUp]
        public void SetUp()
        {
            _enabled = FieldStripeSettings.Enabled;
            FieldStripeSettings.Enabled = true;
            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();
            FieldStripeSettings.Enabled = _enabled;
        }

        [UnityTest]
        public IEnumerator AppearanceSwitch_MirrorsTheStoreLive()
        {
            // A value change reaches its callbacks only inside a panel.
            var container = new VisualElement();
            _panel.Root.Add(container);
            AspidAppearanceSettingsUI.BuildControls(container);
            yield return null;

            var stripes = container.Q<AspidSwitch>(AspidAppearanceSettingsUI.FieldStripesName);
            Assert.IsNotNull(stripes, "BuildControls must emit the field stripes switch.");
            Assert.IsTrue(stripes.value, "The stripes are on by default.");

            FieldStripeSettings.Enabled = false;
            Assert.IsFalse(stripes.value, "The switch must mirror the store without a manual refresh.");

            stripes.value = true;
            Assert.IsTrue(FieldStripeSettings.Enabled, "The switch must write the store.");
        }

        [UnityTest]
        public IEnumerator TypeFieldStripe_FollowsTheSwitch_AlsoAfterReattach()
        {
            var stripe = TypeUIToolkitPropertyDrawer.CreateStripe();
            _panel.Root.Add(stripe);
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, stripe.style.display.value, "A stripe shows while the switch is on.");

            FieldStripeSettings.Enabled = false;
            Assert.AreEqual(DisplayStyle.None, stripe.style.display.value,
                "Turning the switch off must hide a stripe that is already open.");

            stripe.RemoveFromHierarchy();
            FieldStripeSettings.Enabled = true;
            Assert.AreEqual(DisplayStyle.None, stripe.style.display.value, "A stripe outside a panel must not follow the switch.");

            _panel.Root.Add(stripe);
            Assert.AreEqual(DisplayStyle.Flex, stripe.style.display.value, "A stripe back in a panel must pick the switch up.");
        }

        [Test]
        public void ResetToDefaults_TurnsTheStripesOn()
        {
            FieldStripeSettings.Enabled = false;
            FieldStripeSettings.ResetToDefaults();
            Assert.IsTrue(FieldStripeSettings.Enabled);
        }
    }
}
