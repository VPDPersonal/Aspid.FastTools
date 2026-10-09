using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class VisualElementStyleExtensionsTests
    {
        [Test]
        public void SetPadding_PositionalArguments_SetTopAndRight()
        {
            var element = new VisualElement().SetPadding(8, 4);

            Assert.AreEqual(8f, element.style.paddingTop.value.value);
            Assert.AreEqual(4f, element.style.paddingRight.value.value);
            Assert.AreEqual(StyleKeyword.Null, element.style.paddingBottom.keyword);
            Assert.AreEqual(StyleKeyword.Null, element.style.paddingLeft.keyword);
        }

        [Test]
        public void SetPadding_OnStyle_PositionalArgumentsSetTopAndRight()
        {
            var element = new VisualElement();

            element.style.SetPadding(8, 4);

            Assert.AreEqual(8f, element.style.paddingTop.value.value);
            Assert.AreEqual(4f, element.style.paddingRight.value.value);
            Assert.AreEqual(StyleKeyword.Null, element.style.paddingBottom.keyword);
            Assert.AreEqual(StyleKeyword.Null, element.style.paddingLeft.keyword);
        }

        [Test]
        public void SetPaddingYAndX_SetPairs()
        {
            var element = new VisualElement().SetPaddingY(8).SetPaddingX(4);

            Assert.AreEqual(8f, element.style.paddingTop.value.value);
            Assert.AreEqual(8f, element.style.paddingBottom.value.value);
            Assert.AreEqual(4f, element.style.paddingLeft.value.value);
            Assert.AreEqual(4f, element.style.paddingRight.value.value);
        }

        [Test]
        public void SetMargin_PositionalArguments_SetTopAndRight()
        {
            var element = new VisualElement().SetMargin(8, 4);

            Assert.AreEqual(8f, element.style.marginTop.value.value);
            Assert.AreEqual(4f, element.style.marginRight.value.value);
            Assert.AreEqual(StyleKeyword.Null, element.style.marginBottom.keyword);
            Assert.AreEqual(StyleKeyword.Null, element.style.marginLeft.keyword);
        }

        [Test]
        public void SetBorderWidth_PositionalArguments_SetTopAndRight()
        {
            var element = new VisualElement().SetBorderWidth(8, 4);

            Assert.AreEqual(8f, element.style.borderTopWidth.value);
            Assert.AreEqual(4f, element.style.borderRightWidth.value);
            Assert.AreEqual(StyleKeyword.Null, element.style.borderBottomWidth.keyword);
            Assert.AreEqual(StyleKeyword.Null, element.style.borderLeftWidth.keyword);
        }

        [Test]
        public void SetBorderColor_PositionalArguments_SetTopAndRight()
        {
            var element = new VisualElement().SetBorderColor(Color.red, Color.blue);

            Assert.AreEqual(Color.red, element.style.borderTopColor.value);
            Assert.AreEqual(Color.blue, element.style.borderRightColor.value);
            Assert.AreEqual(StyleKeyword.Null, element.style.borderBottomColor.keyword);
            Assert.AreEqual(StyleKeyword.Null, element.style.borderLeftColor.keyword);
        }
    }
}
