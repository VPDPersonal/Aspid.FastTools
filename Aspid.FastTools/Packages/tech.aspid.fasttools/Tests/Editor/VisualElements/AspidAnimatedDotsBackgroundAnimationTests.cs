using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    // The dots canvas runs its repaint timer only while it sits in a panel and the per-user "Animated background"
    // setting is on; with the setting off nothing keeps the editor repainting.
    [TestFixture]
    internal sealed class AspidAnimatedDotsBackgroundAnimationTests
    {
        private bool _enabled;
        private TestPanel _panel;

        [SetUp]
        public void SetUp()
        {
            _enabled = AspidAnimatedDotsBackgroundSettings.Enabled;
            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            _panel = new TestPanel();
        }

        [TearDown]
        public void TearDown()
        {
            _panel.Dispose();
            AspidAnimatedDotsBackgroundSettings.Enabled = _enabled;
        }

        [UnityTest]
        public IEnumerator Attached_WithSettingOn_Animates()
        {
            var canvas = new AspidAnimatedDotsBackground();
            Assert.IsFalse(canvas.IsAnimating, "A canvas outside a panel must not run the timer.");

            _panel.Root.Add(canvas);
            yield return null;

            Assert.IsTrue(canvas.IsAnimating);
        }

        [UnityTest]
        public IEnumerator Attached_WithSettingOff_StaysStill()
        {
            AspidAnimatedDotsBackgroundSettings.Enabled = false;

            var canvas = new AspidAnimatedDotsBackground();
            _panel.Root.Add(canvas);
            yield return null;

            Assert.IsFalse(canvas.IsAnimating, "With the setting off the timer must not start.");
        }

        [UnityTest]
        public IEnumerator SettingChange_StopsAndRestartsAttachedCanvas()
        {
            var canvas = new AspidAnimatedDotsBackground();
            _panel.Root.Add(canvas);
            yield return null;

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            Assert.IsFalse(canvas.IsAnimating, "Turning the setting off must stop an open canvas at once.");

            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsTrue(canvas.IsAnimating, "Turning the setting on must restart an open canvas at once.");
        }

        [UnityTest]
        public IEnumerator Detached_StopsAndIgnoresSettingChanges()
        {
            var canvas = new AspidAnimatedDotsBackground();
            _panel.Root.Add(canvas);
            yield return null;

            canvas.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(canvas.IsAnimating, "A detached canvas must stop its timer.");

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsFalse(canvas.IsAnimating, "A detached canvas must not follow the setting.");

            _panel.Root.Add(canvas);
            yield return null;
            Assert.IsTrue(canvas.IsAnimating, "Attaching again must restart the timer.");
        }

        [UnityTest]
        public IEnumerator SettingsSwitch_MirrorsTheStoreLive()
        {
            // A value change reaches its callbacks only inside a panel.
            var container = new VisualElement();
            _panel.Root.Add(container);
            AspidAppearanceSettingsUI.BuildControls(container);
            yield return null;

            var animated = container.Q<AspidSwitch>();
            Assert.IsNotNull(animated, "BuildControls must emit the animated background switch.");
            Assert.IsTrue(animated.value);

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            Assert.IsFalse(animated.value, "The switch must mirror the store without a manual refresh.");

            animated.value = true;
            Assert.IsTrue(AspidAnimatedDotsBackgroundSettings.Enabled, "The switch must write the store.");
        }
    }
}
