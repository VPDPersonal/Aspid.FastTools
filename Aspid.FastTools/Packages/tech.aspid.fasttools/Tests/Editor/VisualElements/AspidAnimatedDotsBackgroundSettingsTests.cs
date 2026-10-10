using NUnit.Framework;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    [TestFixture]
    internal sealed class AspidAnimatedDotsBackgroundSettingsTests
    {
        private bool _enabled;

        [SetUp]
        public void SetUp() =>
            _enabled = AspidAnimatedDotsBackgroundSettings.Enabled;

        [TearDown]
        public void TearDown() =>
            AspidAnimatedDotsBackgroundSettings.Enabled = _enabled;

        [Test]
        public void Setter_RaisesChanged_OnlyOnRealChange()
        {
            AspidAnimatedDotsBackgroundSettings.Enabled = true;

            var fired = 0;

            AspidAnimatedDotsBackgroundSettings.Changed += Handler;
            try
            {
                AspidAnimatedDotsBackgroundSettings.Enabled = true;
                Assert.AreEqual(0, fired, "Re-assigning the same value must not raise Changed.");

                AspidAnimatedDotsBackgroundSettings.Enabled = false;
                Assert.AreEqual(1, fired, "A genuine change must raise Changed exactly once.");
            }
            finally
            {
                AspidAnimatedDotsBackgroundSettings.Changed -= Handler;
            }

            void Handler() => fired++;
        }

        [Test]
        public void ResetToDefaults_TurnsTheAnimationOn()
        {
            AspidAnimatedDotsBackgroundSettings.Enabled = false;
            AspidAnimatedDotsBackgroundSettings.ResetToDefaults();
            Assert.IsTrue(AspidAnimatedDotsBackgroundSettings.Enabled, "The reset must restore the animation to on.");
        }
    }
}
