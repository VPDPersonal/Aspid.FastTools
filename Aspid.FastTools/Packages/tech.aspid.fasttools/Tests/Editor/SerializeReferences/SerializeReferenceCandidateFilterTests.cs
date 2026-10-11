using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Two candidates identical but for [Serializable] — which decides nothing for a managed reference and
    // everything for an ordinary field.
    [System.Serializable]
    internal sealed class SerializableCandidate { }

    [System.Serializable]
    internal struct SerializableStructCandidate { }

    internal sealed class PlainCandidate { }

    internal interface ICandidateContract { }

    internal abstract class AbstractCandidate { }

    [System.Serializable]
    internal sealed class OpenGenericCandidate<T> { }

    internal sealed class PlainOpenCandidate<T> { }

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

        [Test]
        public void UserSerializableStruct_IsAValidArgument() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgument(typeof(SerializableStructCandidate)));

        [TestCase(typeof(int))]
        [TestCase(typeof(char))]
        [TestCase(typeof(ulong))]
        [TestCase(typeof(string))]
        [TestCase(typeof(System.DayOfWeek))]
        [TestCase(typeof(UnityEngine.GameObject))]
        [TestCase(typeof(int[]))]
        [TestCase(typeof(string[]))]
        [TestCase(typeof(System.Collections.Generic.List<int>))]
        [TestCase(typeof(System.Collections.Generic.List<string>))]
        [TestCase(typeof(System.Collections.Generic.List<UnityEngine.Vector3>))]
        [TestCase(typeof(System.Collections.Generic.List<UnityEngine.Object>))]
        [TestCase(typeof(System.Collections.Generic.List<SerializableCandidate>))]
        [TestCase(typeof(SerializableStructCandidate[]))]
        public void WhatUnitySerializes_IsAValidArgument(System.Type type) =>
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgument(type),
                "Primitives, string, enums, objects, T[] and List<T> of these are real serialized fields — even " +
                "though the framework types among them carry [Serializable] only for .NET's own serializer.");

        [TestCase(typeof(System.DateTime))]
        [TestCase(typeof(System.DateTimeOffset))]
        [TestCase(typeof(System.TimeSpan))]
        [TestCase(typeof(System.Guid))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(System.Version))]
        [TestCase(typeof(System.Uri))]
        [TestCase(typeof(object))]
        [TestCase(typeof(System.IntPtr))]
        [TestCase(typeof(System.UIntPtr))]
        [TestCase(typeof(System.Collections.ArrayList))]
        [TestCase(typeof(System.Collections.BitArray))]
        [TestCase(typeof(System.Text.StringBuilder))]
        [TestCase(typeof(System.Collections.Generic.Dictionary<int, int>))]
        [TestCase(typeof(System.Collections.Generic.SortedDictionary<int, int>))]
        [TestCase(typeof(System.Collections.Generic.HashSet<int>))]
        [TestCase(typeof(System.Collections.Generic.Queue<int>))]
        [TestCase(typeof(System.Collections.Generic.Stack<int>))]
        [TestCase(typeof(System.Collections.Generic.LinkedList<int>))]
        [TestCase(typeof(System.Collections.ObjectModel.Collection<int>))]
        [TestCase(typeof(System.Collections.ObjectModel.ReadOnlyCollection<int>))]
        [TestCase(typeof(System.Collections.Generic.KeyValuePair<int, int>))]
        [TestCase(typeof(System.Tuple<int, int>))]
        [TestCase(typeof(System.ValueTuple<int, int>))]
        [TestCase(typeof(int?))]
        public void FrameworkTypeWithSerializable_IsRejected(System.Type type) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(type),
                "The framework marks these [Serializable] for .NET, but Unity writes none of their private fields, " +
                "so a field of this type silently loses its value.");

        [TestCase(typeof(int[,]))]
        [TestCase(typeof(int[][]))]
        [TestCase(typeof(SerializableCandidate[,]))]
        [TestCase(typeof(System.Collections.Generic.List<int[]>))]
        [TestCase(typeof(System.Collections.Generic.List<System.Collections.Generic.List<int>>))]
        [TestCase(typeof(System.DateTime[]))]
        [TestCase(typeof(PlainCandidate[]))]
        [TestCase(typeof(System.Collections.Generic.List<System.Guid>))]
        [TestCase(typeof(System.Collections.Generic.List<PlainCandidate>))]
        public void SequenceUnityDrops_IsRejected(System.Type type) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgument(type),
                "Unity keeps one level of T[] and List<T> and drops the field when T is not serialized.");

        [Test]
        public void ListDefinition_IsOfferedAsANestedArgument() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(
                typeof(System.Collections.Generic.List<>)),
                "List<T> is the one framework generic Unity serializes; its element is judged on its own page.");

        [TestCase(typeof(System.Collections.Generic.Dictionary<,>))]
        [TestCase(typeof(System.Collections.Generic.SortedDictionary<,>))]
        [TestCase(typeof(System.Collections.Generic.HashSet<>))]
        [TestCase(typeof(System.Collections.Generic.Queue<>))]
        [TestCase(typeof(System.Collections.Generic.Stack<>))]
        [TestCase(typeof(System.Collections.Generic.LinkedList<>))]
        [TestCase(typeof(System.Collections.ObjectModel.Collection<>))]
        [TestCase(typeof(System.Collections.Generic.KeyValuePair<,>))]
        [TestCase(typeof(System.Tuple<,>))]
        [TestCase(typeof(System.ValueTuple<,>))]
        [TestCase(typeof(System.Nullable<>))]
        [TestCase(typeof(PlainOpenCandidate<>))]
        public void GenericDefinitionUnityDrops_IsNotOffered(System.Type definition) =>
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(definition));

        [Test]
        public void ListDefinition_IsNotOfferedOnTheElementPageOfAList()
        {
            var list = typeof(System.Collections.Generic.List<>);

            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(building: list, definition: list),
                "Unity keeps one level of List<T>, so a list of lists could only end in the error of its closing.");
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(
                building: typeof(OpenGenericCandidate<>), definition: list));
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(
                building: list, definition: typeof(OpenGenericCandidate<>)));
            Assert.IsFalse(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(
                building: list, definition: typeof(System.Collections.Generic.HashSet<>)));
        }

        [Test]
        public void UserGenericDefinition_IsOffered() =>
            Assert.IsTrue(SerializeReferenceHelpers.IsValidGenericArgumentDefinition(typeof(OpenGenericCandidate<>)));
    }
}
