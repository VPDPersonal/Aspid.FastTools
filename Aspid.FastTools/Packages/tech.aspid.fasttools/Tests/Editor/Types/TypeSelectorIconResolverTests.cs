using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="TypeSelectorIconResolver"/>: where a <see cref="TypeSelectorDisplayAttribute.Icon"/>
    /// name is looked up.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorIconResolverTests
    {
        // A texture in Tests/Editor/Resources.
        private const string ResourcesIcon = "AspidFastToolsTestIcon";

        [Test]
        public void Resolve_ResourcesIconWithoutSlash_LoadsWithoutAConsoleError()
        {
            // Unity's built-in lookup logs "Unable to load the icon" for a name it does not know, and the test
            // framework fails a test on an unexpected error log.
            var texture = TypeSelectorIconResolver.Resolve(ResourcesIcon);

            Assert.IsNotNull(texture);
            Assert.AreEqual(ResourcesIcon, texture.name);
        }

        [Test]
        public void Resolve_BuiltInIconName_StillLoads() =>
            Assert.IsNotNull(TypeSelectorIconResolver.Resolve("d_cs Script Icon"));
    }
}
