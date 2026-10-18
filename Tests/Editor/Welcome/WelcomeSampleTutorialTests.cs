using NUnit.Framework;

namespace Aspid.FastTools.Editors.Tests
{
    // A sample description in package.json ends with "Tutorial: <url>"; the Welcome card shows the text and a link.
    [TestFixture]
    internal sealed class WelcomeSampleTutorialTests
    {
        private const string Url = "https://vpdpersonal.github.io/Aspid.FastTools/tutorials/types";

        [Test]
        public void SplitTutorialUrl_SeparatesTheTrailingLink()
        {
            var text = WelcomeView.SplitTutorialUrl($"An enemy spawner. Tutorial: {Url}", out var url);

            Assert.AreEqual("An enemy spawner.", text);
            Assert.AreEqual(Url, url);
        }

        [Test]
        public void SplitTutorialUrl_WithoutLink_KeepsTheDescription()
        {
            var text = WelcomeView.SplitTutorialUrl("An enemy spawner.", out var url);

            Assert.AreEqual("An enemy spawner.", text);
            Assert.IsNull(url);
        }

        [Test]
        public void SplitTutorialUrl_KeepsTextThatIsNotALink()
        {
            const string description = "Tutorial: see the site for more";

            var text = WelcomeView.SplitTutorialUrl(description, out var url);

            Assert.AreEqual(description, text);
            Assert.IsNull(url);
        }

        [Test]
        public void SplitTutorialUrl_WithOnlyALink_ReturnsEmptyText()
        {
            var text = WelcomeView.SplitTutorialUrl($"Tutorial: {Url}", out var url);

            Assert.IsEmpty(text);
            Assert.AreEqual(Url, url);
        }

        [Test]
        public void SplitTutorialUrl_WithoutDescription_ReturnsIt()
        {
            Assert.IsNull(WelcomeView.SplitTutorialUrl(null, out var url));
            Assert.IsNull(url);
            Assert.IsEmpty(WelcomeView.SplitTutorialUrl(string.Empty, out url));
            Assert.IsNull(url);
        }
    }
}
