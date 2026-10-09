using UnityEngine;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal.Tests;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class ScrollViewExtensionsTests
    {
        [Test]
        public void Setters_ReturnScrollView()
        {
            var source = new ScrollView();

            ScrollView scroll = source
                .SetMode(ScrollViewMode.VerticalAndHorizontal)
                .SetHorizontalScrollerVisibility(ScrollerVisibility.AlwaysVisible)
                .SetVerticalScrollerVisibility(ScrollerVisibility.Hidden)
                .SetHorizontalPageSize(25f)
                .SetVerticalPageSize(40f)
                .SetMouseWheelScrollSize(60f)
                .SetScrollDecelerationRate(0.5f)
                .SetElasticity(0.25f)
                .SetElasticAnimationIntervalMs(32)
                .SetTouchScrollBehavior(ScrollView.TouchScrollBehavior.Elastic)
                .SetNestedInteractionKind(ScrollView.NestedInteractionKind.ForwardScrolling);

            Assert.AreSame(source, scroll);
            Assert.AreEqual(ScrollViewMode.VerticalAndHorizontal, scroll.mode);
            Assert.AreEqual(ScrollerVisibility.AlwaysVisible, scroll.horizontalScrollerVisibility);
            Assert.AreEqual(ScrollerVisibility.Hidden, scroll.verticalScrollerVisibility);
            Assert.AreEqual(25f, scroll.horizontalPageSize);
            Assert.AreEqual(40f, scroll.verticalPageSize);
            Assert.AreEqual(60f, scroll.mouseWheelScrollSize);
            Assert.AreEqual(0.5f, scroll.scrollDecelerationRate);
            Assert.AreEqual(0.25f, scroll.elasticity);
            Assert.AreEqual(32, scroll.elasticAnimationIntervalMs);
            Assert.AreEqual(ScrollView.TouchScrollBehavior.Elastic, scroll.touchScrollBehavior);
            Assert.AreEqual(ScrollView.NestedInteractionKind.ForwardScrolling, scroll.nestedInteractionKind);
        }

        [Test]
        public void SetElasticity_And_SetScrollDecelerationRate_ClampNegativeValuesToZero()
        {
            var scroll = new ScrollView()
                .SetElasticity(-1f)
                .SetScrollDecelerationRate(-1f);

            Assert.AreEqual(0f, scroll.elasticity);
            Assert.AreEqual(0f, scroll.scrollDecelerationRate);
        }

        [UnityTest]
        public IEnumerator SetScrollOffset_MovesLaidOutContent()
        {
            using var panel = new TestPanel();
            var content = new VisualElement()
                .SetWidth(100)
                .SetHeight(400);
            var scroll = new ScrollView()
                .SetWidth(100)
                .SetHeight(100)
                .AddChild(content);
            panel.Root.Add(scroll);
            yield return null;
            yield return null;

            var result = scroll.SetScrollOffset(new Vector2(0f, 40f));

            Assert.AreSame(scroll, result);
            Assert.AreEqual(new Vector2(0f, 40f), scroll.scrollOffset);

            scroll.SetScrollOffset(new Vector2(0f, 1000f));

            Assert.Less(scroll.scrollOffset.y, 1000f, "The scrollers limit the offset to the scrollable range.");
        }
    }
}
