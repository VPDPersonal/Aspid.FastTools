using NUnit.Framework;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.VisualElements
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
