using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Two candidates identical but for [Serializable] — which decides nothing for a managed reference and
    // everything for an ordinary field.
    [System.Serializable]
    internal sealed class SerializableCandidate { }

    internal sealed class PlainCandidate { }

    internal interface ICandidateContract { }

    internal abstract class AbstractCandidate { }

    [System.Serializable]
    internal sealed class OpenGenericCandidate<T> { }

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

        [TestCase(typeof(UnityEngine.Vector2))]
        [TestCase(typeof(UnityEngine.Vector3))]
        [TestCase(typeof(UnityEngine.Vector4))]
        [TestCase(typeof(UnityEngine.Vector2Int))]
        [TestCase(typeof(UnityEngine.Vector3Int))]
        [TestCase(typeof(UnityEngine.Quaternion))]
        [TestCase(typeof(UnityEngine.Matrix4x4))]
        [TestCase(typeof(UnityEngine.Color))]
        [TestCase(typeof(UnityEngine.Color32))]
        [TestCase(typeof(UnityEngine.Gradient))]
        [TestCase(typeof(UnityEngine.Rect))]
        [TestCase(typeof(UnityEngine.RectInt))]
        [TestCase(typeof(UnityEngine.Bounds))]
        [TestCase(typeof(UnityEngine.BoundsInt))]
        [TestCase(typeof(UnityEngine.LayerMask))]
        [TestCase(typeof(UnityEngine.AnimationCurve))]
        public void UnityNativeType_IsAValidArgument(System.Type type) =>
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgument(type),
                "Unity serializes this type natively, so it belongs in a serialized field — Type.IsSerializable " +
                "reports False for it because the engine writes the layout instead of .NET.");

        [TestCase(typeof(UnityEngine.Ray))]
        [TestCase(typeof(UnityEngine.Ray2D))]
        [TestCase(typeof(UnityEngine.Plane))]
        [TestCase(typeof(UnityEngine.RangeInt))]
        public void UnityTypeTheEngineDoesNotSerialize_StaysRejected(System.Type type) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(type),
                "Being a built-in Unity value type is not the criterion — being one Unity writes to a field is.");

        [TestCase(typeof(ICandidateContract))]
        [TestCase(typeof(AbstractCandidate))]
        [TestCase(typeof(OpenGenericCandidate<>))]
        [TestCase(typeof(System.Action))]
        [TestCase(typeof(PlainCandidate))]
        public void IneligibleArgument_IsRejected(System.Type type) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(type),
                "An interface, an abstract class, an open definition or a delegate cannot close a type parameter " +
                "at all; a class without [Serializable] would close it and then be dropped by Unity.");
    }
}
