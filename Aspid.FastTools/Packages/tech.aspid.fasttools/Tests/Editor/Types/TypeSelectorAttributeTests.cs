using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the default of <see cref="TypeSelectorAttribute.Allow"/>. Every "name a type" context
    /// (a raw <c>string</c> picker or a <see cref="SerializableType"/>) offers abstract classes and
    /// interfaces unless the field opts out with <see cref="TypeAllow.None"/>, so the attribute must
    /// default to <see cref="TypeAllow.All"/> whichever constructor built it — and an explicit value
    /// must still win over that default.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorAttributeTests
    {
        [Test]
        public void Allow_DefaultsToAll_OnTheParameterlessConstructor()
        {
            var attribute = new TypeSelectorAttribute();
            Assert.AreEqual(TypeAllow.All, attribute.Allow);
        }

        [Test]
        public void Allow_DefaultsToAll_OnTheSingleTypeConstructor()
        {
            var attribute = new TypeSelectorAttribute(typeof(object));
            Assert.AreEqual(TypeAllow.All, attribute.Allow);
        }

        [Test]
        public void Allow_HonoursAnExplicitNone()
        {
            var attribute = new TypeSelectorAttribute(typeof(object)) { Allow = TypeAllow.None };
            Assert.AreEqual(TypeAllow.None, attribute.Allow);
        }
    }
}
