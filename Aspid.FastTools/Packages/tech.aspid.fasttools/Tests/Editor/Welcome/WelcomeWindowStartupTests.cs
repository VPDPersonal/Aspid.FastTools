using NUnit.Framework;

namespace Aspid.FastTools.Editors.Tests
{
    /// <summary>
    /// Coverage for the decision behind the Welcome window's auto-show: it opens only for an interactive Editor
    /// whose user left auto-show on, has not seen the window for this package version and has no FastTools window open.
    /// </summary>
    [TestFixture]
    internal sealed class WelcomeWindowStartupTests
    {
        // isBatchMode, autoShowEnabled, hasBeenSeen, hasOpenWindow
        [TestCase(false, true, false, false, true, TestName = "Interactive Editor, auto-show on, not seen, no window")]
        [TestCase(true, true, false, false, false, TestName = "Batch mode never shows")]
        [TestCase(false, false, false, false, false, TestName = "Auto-show off never shows")]
        [TestCase(false, true, true, false, false, TestName = "Already seen never shows")]
        [TestCase(false, true, false, true, false, TestName = "An open FastTools window never shows")]
        [TestCase(true, false, true, true, false, TestName = "Every blocker together")]
        public void ShouldShow_Case(bool isBatchMode, bool autoShowEnabled, bool hasBeenSeen, bool hasOpenWindow, bool expected)
        {
            var actual = WelcomeWindowStartup.ShouldShow(
                isBatchMode: isBatchMode,
                autoShowEnabled: autoShowEnabled,
                hasBeenSeen: hasBeenSeen,
                hasOpenWindow: hasOpenWindow);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ShouldShow_IsTrueForExactlyOneCombination()
        {
            var shown = 0;

            for (var mask = 0; mask < 16; mask++)
            {
                var show = WelcomeWindowStartup.ShouldShow(
                    isBatchMode: (mask & 1) != 0,
                    autoShowEnabled: (mask & 2) != 0,
                    hasBeenSeen: (mask & 4) != 0,
                    hasOpenWindow: (mask & 8) != 0);

                if (show) shown++;
            }

            Assert.AreEqual(1, shown, "Only the interactive, enabled, unseen, no-window state may open the Welcome window.");
        }
    }
}
