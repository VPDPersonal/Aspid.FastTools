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
    }
}
