using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    internal sealed class TextElementExtensionsTests
    {
        [Test]
        public void SetTextSelf_ReturnsElementForChaining()
        {
            var button = new Button();

            var result = button
                .SetTextSelf("Apply")
                .SetTooltip("Applies the changes");

            Assert.AreSame(button, result);
            Assert.AreEqual("Apply", result.text);
        }
    }
}
