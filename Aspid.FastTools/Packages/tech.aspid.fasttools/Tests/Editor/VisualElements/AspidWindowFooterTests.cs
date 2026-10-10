using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// The footer row must keep the keys hint between the version and the GitHub link at any window width, and
    /// the hint and the version must use the readable text tone.
    /// </summary>
    [TestFixture]
    internal sealed class AspidWindowFooterTests
    {
        private const float Tolerance = 0.5f;

        private TestPanel _panel;
        private VisualElement _host;
        private AspidWindowFooter _footer;

        [SetUp]
        public void SetUp()
        {
            _panel = new TestPanel();

            _host = new VisualElement();
            _panel.Root.Add(_host);

            _footer = new AspidWindowFooter();
            _host.Add(_footer);
        }

        [TearDown]
        public void TearDown() => _panel.Dispose();

        [UnityTest]
        public IEnumerator KeysHint_AtAnyWidth_StaysBetweenVersionAndLink()
        {
            foreach (var width in new[] { 380f, 340f, 260f, 200f })
            {
                _host.style.width = width;
                yield return null;
                yield return null;

                var version = Element("version");
                var keys = Element("keys");
                var link = Element("link");

                var message = $"Width {width}: ";
                Assert.Greater(version.layout.width, 0f, message + "The version must keep its width.");
                Assert.Greater(link.layout.width, 0f, message + "The link must keep its width.");

                Assert.LessOrEqual(version.worldBound.xMax, keys.worldBound.xMin + Tolerance,
                    message + "The keys hint must not reach the version.");
                Assert.LessOrEqual(keys.worldBound.xMax, link.worldBound.xMin + Tolerance,
                    message + "The keys hint must not reach the link.");
                Assert.LessOrEqual(link.worldBound.xMax, _host.worldBound.xMax + Tolerance,
                    message + "The link must stay inside the window.");
            }
        }

        [UnityTest]
        public IEnumerator KeysHint_Hidden_KeepsVersionAndLinkApart()
        {
            _host.Clear();
            _footer = new AspidWindowFooter(showKeysHint: false);
            _host.Add(_footer);
            _host.style.width = 340f;
            yield return null;
            yield return null;

            Assert.IsNull(_footer.Q(className: ClassOf("keys")), "A footer without the hint must not build it.");
            Assert.LessOrEqual(Element("version").worldBound.xMax, Element("link").worldBound.xMin + Tolerance);
        }

        [UnityTest]
        public IEnumerator Hint_AndVersion_UseTheReadableTextTone()
        {
            yield return null;

            // The palette tokens sit on the footer, the root of the theme sheets.
            var tone = new CustomStyleProperty<Color>("--aspid-colors-text-dark");
            Assert.IsTrue(_footer.customStyle.TryGetValue(tone, out var expected),
                "The palette must define --aspid-colors-text-dark.");

            foreach (var name in new[] { "keys", "version" })
            {
                var element = Element(name);

                Assert.AreEqual(expected, element.resolvedStyle.color,
                    $"The {name} must use text-dark; text-darkness is below the readable contrast.");
            }
        }

        private static string ClassOf(string name) => "aspid-fasttools-window-footer__" + name;

        private Label Element(string name)
        {
            var element = _footer.Q<Label>(className: ClassOf(name));
            Assert.IsNotNull(element, $"The footer must have the {name} label.");
            return element;
        }
    }
}
