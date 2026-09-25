using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    internal sealed class IMGUIContainerExtensionsTests
    {
        [Test]
        public void MarkDirtyLayoutSelf_ReturnsContainerForChaining()
        {
            var container = new IMGUIContainer();

            var result = container
                .MarkDirtyLayoutSelf()
                .SetCullingEnabled(true);

            Assert.AreSame(container, result);
            Assert.IsTrue(result.cullingEnabled);
        }
    }
}
