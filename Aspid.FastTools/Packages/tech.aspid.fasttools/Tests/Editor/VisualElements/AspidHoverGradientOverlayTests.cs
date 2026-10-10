using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    // The overlay's 16 ms timer runs only while the progress moves toward its target, so an idle button costs nothing.
    // A panel without a graphics device does not advance timers, so the tests call Tick themselves.
    [TestFixture]
    internal sealed class AspidHoverGradientOverlayTests
    {
        private const int MaxTicks = 500;

        private TestPanel _panel;
        private AspidHoverGradientOverlay _overlay;

        [SetUp]
        public void SetUp()
        {
            _panel = new TestPanel();
            _overlay = new AspidHoverGradientOverlay();
        }

        [TearDown]
        public void TearDown() => _panel.Dispose();

        [UnityTest]
        public IEnumerator Idle_DoesNotRunTheTimer()
        {
            _panel.Root.Add(_overlay);
            yield return null;

            Assert.IsFalse(_overlay.IsAnimating, "An overlay with nothing to animate must not run a timer.");

            _overlay.SetTarget(0f);
            Assert.IsFalse(_overlay.IsAnimating, "A target equal to the progress must not start the timer.");
        }

        [UnityTest]
        public IEnumerator SetTarget_RunsTheTimerUntilProgressLands()
        {
            _panel.Root.Add(_overlay);
            yield return null;

            _overlay.SetTarget(1f);
            Assert.IsTrue(_overlay.IsAnimating, "A new target must start the timer.");

            TickUntilIdle();
            Assert.IsFalse(_overlay.IsAnimating, "The timer must stop once the progress reached the target.");

            _overlay.SetTarget(0f);
            Assert.IsTrue(_overlay.IsAnimating, "Moving back must start the timer again.");

            TickUntilIdle();
            Assert.IsFalse(_overlay.IsAnimating, "The timer must stop after the fade-out too.");
        }

        [UnityTest]
        public IEnumerator SetTarget_BeforeAttach_StartsTheTimerOnAttach()
        {
            _overlay.SetTarget(1f);
            Assert.IsFalse(_overlay.IsAnimating, "An overlay outside a panel must not run the timer.");

            _panel.Root.Add(_overlay);
            yield return null;

            Assert.IsTrue(_overlay.IsAnimating, "A pending target must start the timer when the overlay attaches.");
        }

        [UnityTest]
        public IEnumerator Detach_StopsTheTimer_AndAttachResumesIt()
        {
            _panel.Root.Add(_overlay);
            yield return null;

            _overlay.SetTarget(1f);
            _overlay.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(_overlay.IsAnimating, "A detached overlay must stop its timer.");

            _panel.Root.Add(_overlay);
            yield return null;
            Assert.IsTrue(_overlay.IsAnimating, "An unfinished fade must resume on attach.");
        }

        private void TickUntilIdle()
        {
            var ticks = 0;

            while (_overlay.IsAnimating && ticks++ < MaxTicks)
                _overlay.Tick();

            Assert.Less(ticks, MaxTicks, "The progress must reach the target in a bounded number of ticks.");
        }
    }
}
