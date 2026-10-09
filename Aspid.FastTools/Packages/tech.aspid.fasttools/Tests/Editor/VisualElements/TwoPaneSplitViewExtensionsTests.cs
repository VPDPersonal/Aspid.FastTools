using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class TwoPaneSplitViewExtensionsTests
    {
        [Test]
        public void Setters_ReturnTwoPaneSplitView()
        {
            var source = new TwoPaneSplitView();

            TwoPaneSplitView split = source
                .SetFixedPaneIndex(1)
                .SetFixedPaneInitialDimension(250f)
                .SetOrientation(TwoPaneSplitViewOrientation.Vertical);

            Assert.AreSame(source, split);
            Assert.AreEqual(1, split.fixedPaneIndex);
            Assert.AreEqual(250f, split.fixedPaneInitialDimension);
            Assert.AreEqual(TwoPaneSplitViewOrientation.Vertical, split.orientation);
        }

        [Test]
        public void Setters_AfterAddingBothPanes_KeepFixedPaneSettings()
        {
            var left = new VisualElement();
            var right = new VisualElement();

            var split = new TwoPaneSplitView()
                .AddChild(left)
                .AddChild(right)
                .SetFixedPaneIndex(1)
                .SetFixedPaneInitialDimension(120f);

            Assert.AreSame(right, split.fixedPane);
            Assert.AreSame(left, split.flexedPane);
            Assert.AreEqual(120f, split.fixedPaneInitialDimension);
        }
    }
}
