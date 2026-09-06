using System;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards the code-side contract of the wrappers: the implicit <c>SerializableType → Type</c> conversions
    /// (a <see langword="null"/> wrapper converts to <see langword="null"/> instead of throwing), the
    /// type-taking constructors and the stored-name accessor.
    /// </summary>
    [TestFixture]
    internal sealed class SerializableTypeTests
    {
        [Test]
        public void ImplicitConversion_NullWrapper_YieldsNull()
        {
            SerializableType wrapper = null;
            Type type = wrapper;

            Assert.IsNull(type);
        }

        [Test]
        public void ImplicitConversion_NullGenericWrapper_YieldsNull()
        {
            SerializableType<IComparable> wrapper = null;
            Type type = wrapper;

            Assert.IsNull(type);
        }

        [Test]
        public void EmptyWrapper_HasNoTypeAndAnEmptyName()
        {
            var wrapper = new SerializableType();

            Assert.IsNull(wrapper.Type);
            Assert.AreEqual(string.Empty, wrapper.AssemblyQualifiedName);
            Assert.AreEqual(string.Empty, wrapper.ToString());
        }

        [Test]
        public void Constructor_StoresTheTypeAndItsAssemblyQualifiedName()
        {
            var wrapper = new SerializableType(typeof(Exception));

            Assert.AreEqual(typeof(Exception), wrapper.Type);
            Assert.AreEqual(typeof(Exception).AssemblyQualifiedName, wrapper.AssemblyQualifiedName);
            Assert.AreEqual("Exception", wrapper.ToString());
        }

        [Test]
        public void Constructor_NullType_IsAnEmptyWrapper()
        {
            Assert.IsNull(new SerializableType(null).Type);
            Assert.IsNull(new SerializableType<Exception>(null).Type);
        }

        [Test]
        public void GenericConstructor_AcceptsAnAssignableType() =>
            Assert.AreEqual(typeof(ArgumentException), new SerializableType<Exception>(typeof(ArgumentException)).Type);

        [Test]
        public void GenericConstructor_RejectsAnUnrelatedType() =>
            Assert.Throws<ArgumentException>(() => new SerializableType<Exception>(typeof(string)));

        [Test]
        public void ConstrainedWrapper_IsASerializableType()
        {
            SerializableType wrapper = new SerializableType<IComparable>(typeof(int));

            Assert.AreEqual(typeof(IComparable), wrapper.BaseType, "BaseType must stay virtual through the base reference.");
            Assert.AreEqual(typeof(int), wrapper.Type);
            Assert.AreEqual(typeof(int), (Type)wrapper);
        }

        [Test]
        public void MonoScriptWrapper_IsNotASerializableType() =>
            Assert.IsFalse(typeof(SerializableType).IsAssignableFrom(typeof(SerializableMonoScript)),
                "The two families share only SerializableTypeBase: their serialized layouts differ.");
    }
}
