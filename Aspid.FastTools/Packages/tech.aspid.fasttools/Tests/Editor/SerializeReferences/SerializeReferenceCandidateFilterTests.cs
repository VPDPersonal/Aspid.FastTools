using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Two candidates identical but for the attribute Unity requires to serialize a managed reference.
    [System.Serializable]
    internal sealed class SerializableCandidate { }

    internal sealed class PlainCandidate { }

    /// <summary>
    /// Coverage for <see cref="SerializeReferenceHelpers.IsAssignableManagedReference"/>. Offering a type Unity
    /// cannot serialize is worse than offering nothing: the value appears to be set and is gone after the next
    /// reload, so the structural checks are not enough on their own.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceCandidateFilterTests
    {
        [Test]
        public void SerializableClass_IsACandidate() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsAssignableManagedReference(typeof(SerializableCandidate)));

        [Test]
        public void ClassWithoutSerializable_IsRejected() =>
            Assert.IsFalse(SerializeReferenceHelpers.IsAssignableManagedReference(typeof(PlainCandidate)),
                "Unity drops a managed reference whose type is not [Serializable], so the picker must not offer it.");

        [TestCase(typeof(string))]
        [TestCase(typeof(System.Action))]
        [TestCase(typeof(UnityEngine.ScriptableObject))]
        public void StructurallyIneligibleTypes_StayRejected(System.Type type) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsAssignableManagedReference(type));
    }
}
