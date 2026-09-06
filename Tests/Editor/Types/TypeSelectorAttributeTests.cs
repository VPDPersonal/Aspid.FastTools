using System;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the defaults of <see cref="TypeSelectorAttribute"/>: <see cref="TypeSelectorAttribute.Allow"/> must be
    /// <see cref="TypeAllow.All"/> so every "name a type" context offers abstract classes and interfaces unless the
    /// field opts out, and <see cref="TypeSelectorAttribute.AssemblyQualifiedNames"/> must never hold an entry that
    /// cannot supply a constraint (a null type, a blank name).
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorAttributeTests
    {
        [Test]
        public void Allow_DefaultsToAll()
        {
            var attribute = new TypeSelectorAttribute();
            Assert.AreEqual(TypeAllow.All, attribute.Allow);
        }

        [Test]
        public void Unconstrained_HasNoNames()
        {
            Assert.IsEmpty(new TypeSelectorAttribute().AssemblyQualifiedNames);
            Assert.IsEmpty(new TypeSelectorAttribute((Type[])null).AssemblyQualifiedNames);
            Assert.IsEmpty(new TypeSelectorAttribute((string[])null).AssemblyQualifiedNames);
        }

        [Test]
        public void TypeConstructor_StoresAssemblyQualifiedNames_SkippingNulls()
        {
            var attribute = new TypeSelectorAttribute(typeof(IDisposable), null, typeof(Exception));

            CollectionAssert.AreEqual(
                new[] { typeof(IDisposable).AssemblyQualifiedName, typeof(Exception).AssemblyQualifiedName },
                attribute.AssemblyQualifiedNames);
        }

        [Test]
        public void StringConstructor_SkipsBlankNames()
        {
            var attribute = new TypeSelectorAttribute("_member", "  ", null, "Some.Type, Some");

            CollectionAssert.AreEqual(new[] { "_member", "Some.Type, Some" }, attribute.AssemblyQualifiedNames);
        }
    }
}
