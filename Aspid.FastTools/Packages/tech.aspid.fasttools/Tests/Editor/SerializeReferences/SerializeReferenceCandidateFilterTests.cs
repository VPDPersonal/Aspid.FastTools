using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Two candidates identical but for [Serializable] — which decides nothing for a managed reference and
    // everything for an ordinary field.
    [System.Serializable]
    internal sealed class SerializableCandidate { }

    internal sealed class PlainCandidate { }

    /// <summary>
    /// Coverage for <see cref="SerializeReferenceHelpers.IsAssignableManagedReference"/> and the argument filter
    /// beside it. The two answer different questions, and only the argument filter turns on
    /// <see cref="System.SerializableAttribute"/>: a managed reference is stored in the asset's <c>references</c>
    /// registry by type identity, while a generic argument lands in an ordinary field Unity skips without it.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceCandidateFilterTests
    {
        [Test]
        public void SerializableClass_IsACandidate() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsAssignableManagedReference(typeof(SerializableCandidate)));

        [Test]
        public void ClassWithoutSerializable_IsStillACandidate() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsAssignableManagedReference(typeof(PlainCandidate)),
                "Unity serializes a managed reference through the asset's reference registry, which needs no " +
                "[Serializable] on the concrete type — refusing to offer it would hide a legal choice.");

        [TestCase(typeof(string))]
        [TestCase(typeof(System.Action))]
        [TestCase(typeof(UnityEngine.ScriptableObject))]
        public void StructurallyIneligibleTypes_StayRejected(System.Type type) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsAssignableManagedReference(type));

        [Test]
        public void GenericArgumentWithoutSerializable_IsRejected() =>
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(typeof(PlainCandidate)),
                "A generic argument becomes an ordinary serialized field, which Unity drops without [Serializable].");

        [Test]
        public void SerializableGenericArgument_IsAccepted() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgument(typeof(SerializableCandidate)));
    }
}
