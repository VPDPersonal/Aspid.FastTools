using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class SliderExtensionsTests
    {
        [Test]
        public void Setters_ReturnSlider()
        {
            var source = new Slider();

            Slider slider = source
                .SetFill(true)
                .SetInverted(true)
                .SetPageSize(5f)
                .SetShowInputField(true)
                .SetDirection(SliderDirection.Vertical)
                .SetLowValue(-10f)
                .SetHighValue(10f);

            Assert.AreSame(source, slider);
            Assert.IsTrue(slider.fill);
            Assert.IsTrue(slider.inverted);
            Assert.AreEqual(5f, slider.pageSize);
            Assert.IsTrue(slider.showInputField);
            Assert.AreEqual(SliderDirection.Vertical, slider.direction);
            Assert.AreEqual(-10f, slider.lowValue);
            Assert.AreEqual(10f, slider.highValue);
        }

        [Test]
        public void Setters_ReturnSliderInt()
        {
            var source = new SliderInt();

            SliderInt slider = source
                .SetFill(true)
                .SetInverted(true)
                .SetPageSize(5f)
                .SetShowInputField(true)
                .SetDirection(SliderDirection.Vertical)
                .SetLowValue(-10)
                .SetHighValue(10);

            Assert.AreSame(source, slider);
            Assert.IsTrue(slider.fill);
            Assert.IsTrue(slider.inverted);
            Assert.AreEqual(5f, slider.pageSize);
            Assert.IsTrue(slider.showInputField);
            Assert.AreEqual(SliderDirection.Vertical, slider.direction);
            Assert.AreEqual(-10, slider.lowValue);
            Assert.AreEqual(10, slider.highValue);
        }

        [Test]
        public void SetValue_AfterRange_KeepsValue()
        {
            var slider = new Slider().SetHighValue(100f).SetValue(50f);

            Assert.AreEqual(50f, slider.value);
        }

        [Test]
        public void SetValue_BeforeRange_IsClampedToDefaultRange()
        {
            var slider = new Slider().SetValue(50f).SetHighValue(100f);

            Assert.AreEqual(10f, slider.value);
        }

        [Test]
        public void SetValue_AfterRange_KeepsValueOfSliderInt()
        {
            var slider = new SliderInt().SetHighValue(100).SetValue(50);

            Assert.AreEqual(50, slider.value);
        }

        [Test]
        public void SetValue_BeforeRange_IsClampedToDefaultRangeOfSliderInt()
        {
            var slider = new SliderInt().SetValue(50).SetHighValue(100);

            Assert.AreEqual(10, slider.value);
        }
    }
}
