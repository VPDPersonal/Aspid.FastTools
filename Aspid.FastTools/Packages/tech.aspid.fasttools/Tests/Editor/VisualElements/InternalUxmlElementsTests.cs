using System.Linq;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// Internal components are made for the package windows. A consumer project has no theme sheets for them, so in the
    /// UI Builder Library they would look unstyled; every non-public <c>[UxmlElement]</c> must stay hidden there.
    /// </summary>
    [TestFixture]
    internal sealed class InternalUxmlElementsTests
    {
        [Test]
        public void InternalUxmlElements_AreHiddenInLibrary()
        {
            var elements = typeof(AspidBox).Assembly.GetTypes()
                .Where(type => !type.IsVisible && type.GetCustomAttribute<UxmlElementAttribute>(inherit: false) is not null)
                .ToArray();

            Assert.IsNotEmpty(elements, "No internal UXML elements found; update this test.");

            foreach (var element in elements)
            {
                var attribute = element.GetCustomAttribute<UxmlElementAttribute>(inherit: false);
                Assert.AreEqual(LibraryVisibility.Hidden, attribute.visibility,
                    $"{element.Name} must set visibility = LibraryVisibility.Hidden.");
            }
        }
    }
}
