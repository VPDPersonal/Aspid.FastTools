using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    // The title and the logo of the Welcome tab tick only while they sit in a panel and the per-user
    // "Animated background" setting is on, like the dots canvas; with the setting off nothing keeps them ticking.
    [TestFixture]
    internal sealed class AspidWelcomeAnimationTests
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
        public IEnumerator Title_Attached_WithSettingOn_Animates()
        {
            var title = new AspidAnimatedTitle("Title");
            Assert.IsFalse(title.IsAnimating, "A title outside a panel must not run the timer.");

            _panel.Root.Add(title);
            yield return null;

            Assert.IsTrue(title.IsAnimating);
        }

        [UnityTest]
        public IEnumerator Title_Attached_WithSettingOff_StaysStill()
        {
            AspidAnimatedDotsBackgroundSettings.Enabled = false;

            var title = new AspidAnimatedTitle("Title");
            _panel.Root.Add(title);
            yield return null;

            Assert.IsFalse(title.IsAnimating, "With the setting off the timer must not start.");
        }

        [UnityTest]
        public IEnumerator Title_SettingChange_StopsAndRestartsAttachedTitle()
        {
            var title = new AspidAnimatedTitle("Title");
            _panel.Root.Add(title);
            yield return null;

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            Assert.IsFalse(title.IsAnimating, "Turning the setting off must stop an open title at once.");

            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsTrue(title.IsAnimating, "Turning the setting on must restart an open title at once.");
        }

        [UnityTest]
        public IEnumerator Title_Detached_StopsAndIgnoresSettingChanges()
        {
            var title = new AspidAnimatedTitle("Title");
            _panel.Root.Add(title);
            yield return null;

            title.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(title.IsAnimating, "A detached title must stop its timer.");

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsFalse(title.IsAnimating, "A detached title must not follow the setting.");

            _panel.Root.Add(title);
            yield return null;
            Assert.IsTrue(title.IsAnimating, "Attaching again must restart the timer.");
        }

        [UnityTest]
        public IEnumerator Logo_Attached_WithSettingOn_Pulses()
        {
            var logo = new AspidAnimatedLogo();
            Assert.IsFalse(logo.IsPulsing, "A logo outside a panel must not run the timer.");

            _panel.Root.Add(logo);
            yield return null;

            Assert.IsTrue(logo.IsPulsing);
        }

        [UnityTest]
        public IEnumerator Logo_Attached_WithSettingOff_StaysStill()
        {
            AspidAnimatedDotsBackgroundSettings.Enabled = false;

            var logo = new AspidAnimatedLogo();
            _panel.Root.Add(logo);
            yield return null;

            Assert.IsFalse(logo.IsPulsing, "With the setting off the timer must not start.");
        }

        [UnityTest]
        public IEnumerator Logo_SettingChange_StopsAndRestartsAttachedLogo()
        {
            var logo = new AspidAnimatedLogo();
            _panel.Root.Add(logo);
            yield return null;

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            Assert.IsFalse(logo.IsPulsing, "Turning the setting off must stop an open logo at once.");

            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsTrue(logo.IsPulsing, "Turning the setting on must restart an open logo at once.");
        }

        [UnityTest]
        public IEnumerator Logo_SettingOff_ReturnsToItsNaturalSize()
        {
            var logo = new AspidAnimatedLogo();
            _panel.Root.Add(logo);
            yield return null;

            // The pulse writes this inline scale on every tick, so a stopped pulse would leave it behind.
            logo.style.scale = new Scale(new Vector3(1.04f, 1.04f, 1f));

            AspidAnimatedDotsBackgroundSettings.Enabled = false;

            Assert.AreEqual(StyleKeyword.Null, logo.style.scale.keyword,
                "A logo with the animation off must drop the scale of its last frame.");
        }

        [UnityTest]
        public IEnumerator Logo_Detached_StopsAndIgnoresSettingChanges()
        {
            var logo = new AspidAnimatedLogo();
            _panel.Root.Add(logo);
            yield return null;

            logo.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(logo.IsPulsing, "A detached logo must stop its timer.");

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsFalse(logo.IsPulsing, "A detached logo must not follow the setting.");

            _panel.Root.Add(logo);
            yield return null;
            Assert.IsTrue(logo.IsPulsing, "Attaching again must restart the timer.");
        }

        [UnityTest]
        public IEnumerator Logo_HoverColorCycle_FollowsTheSetting()
        {
            var logo = new AspidAnimatedLogo();
            _panel.Root.Add(logo);
            yield return null;

            Send<PointerEnterEvent>(logo);
            Assert.IsTrue(logo.IsCyclingColors, "Hovering the logo with the setting on must cycle its colors.");

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            Assert.IsFalse(logo.IsCyclingColors, "Turning the setting off must stop the cycle of a hovered logo.");

            AspidAnimatedDotsBackgroundSettings.Enabled = true;
            Assert.IsTrue(logo.IsCyclingColors, "Turning the setting on must restart the cycle of a hovered logo.");

            Send<PointerLeaveEvent>(logo);
            Assert.IsFalse(logo.IsCyclingColors, "Leaving the logo must stop the cycle.");

            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            Send<PointerEnterEvent>(logo);
            Assert.IsFalse(logo.IsCyclingColors, "Hovering the logo with the setting off must not cycle its colors.");
        }

        [UnityTest]
        public IEnumerator WelcomeView_TitleAndLogo_FollowTheSetting()
        {
            AspidAnimatedDotsBackgroundSettings.Enabled = false;

            // The window root declares the palette; the view inherits it.
            _panel.Root.AddAspidThemeStyleSheets();

            var view = new WelcomeView();
            _panel.Root.Add(view);
            yield return null;

            var title = view.Q<AspidAnimatedTitle>();
            var logo = view.Q<AspidAnimatedLogo>();
            Assert.IsNotNull(title, "The Welcome template must hold the title.");
            Assert.IsNotNull(logo, "The Welcome template must hold the logo.");

            Assert.IsFalse(title.IsAnimating, "The Welcome title must stay still with the setting off.");
            Assert.IsFalse(logo.IsPulsing, "The Welcome logo must stay still with the setting off.");

            AspidAnimatedDotsBackgroundSettings.Enabled = true;

            Assert.IsTrue(title.IsAnimating, "The Welcome title must animate again with the setting on.");
            Assert.IsTrue(logo.IsPulsing, "The Welcome logo must animate again with the setting on.");
        }

        private static void Send<TEvent>(VisualElement target)
            where TEvent : EventBase<TEvent>, new()
        {
            using var evt = EventBase<TEvent>.GetPooled();
            evt.target = target;
            target.SendEvent(evt);
        }
    }
}
