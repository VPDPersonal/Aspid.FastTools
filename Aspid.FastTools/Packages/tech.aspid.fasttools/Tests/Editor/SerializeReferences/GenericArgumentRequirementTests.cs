using System;
using UnityEngine;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Where a type parameter lands decides what its argument has to be. The fixtures below are the same class
    // written over and over, differing only in that.
#pragma warning disable CS0169, CS0649
    internal interface IRequirementItem<T> { }

    // The case the whole walk exists for: T is only ever behind a managed reference, so nothing of it is stored.
    internal sealed class ReferenceOnly<T>
    {
        [SerializeReference] private IRequirementItem<T>[] _items;
    }

    internal sealed class ByValue<T>
    {
        [SerializeField] private T _value;
    }

    internal sealed class PublicByValue<T>
    {
        public T Value;
    }

    internal sealed class ByValueList<T>
    {
        [SerializeField] private System.Collections.Generic.List<T> _values;
    }

    internal sealed class ByValueArray<T>
    {
        [SerializeField] private T[] _values;
    }

    // Every field shape Unity leaves alone.
    internal sealed class NeverSerialized<T>
    {
        public static T Shared;
        [NonSerialized] public T Ignored;
        public readonly T Fixed;
        private T _hidden;
    }

    [Serializable]
    internal class SerializableBox<T>
    {
        public T Value;
    }

    internal sealed class NestedInBox<T>
    {
        [SerializeField] private SerializableBox<T> _box;
    }

    // No [Serializable]: Unity drops the field whole, so nothing inside it is ever written.
    internal class PlainBox<T>
    {
        public T Value;
    }

    internal sealed class NestedInPlainBox<T>
    {
        [SerializeField] private PlainBox<T> _box;
    }

    [Serializable]
    internal class SelfReferencing<T>
    {
        public SelfReferencing<T> Next;
        [SerializeReference] private IRequirementItem<T> _item;
    }

    internal sealed class NestedInSelfReferencing<T>
    {
        [SerializeField] private SelfReferencing<T> _node;
    }

    internal class ByValueBase<T>
    {
        [SerializeField] private T _value;
    }

    internal sealed class InheritsAByValueField<T> : ByValueBase<T> { }
#pragma warning restore CS0169, CS0649

    /// <summary>
    /// Coverage for <see cref="GenericArgumentRequirement"/> and the contextual argument filter beside it. Getting
    /// this wrong in one direction hides usable candidates from the picker; in the other it offers a candidate whose
    /// data Unity silently drops, which is why the walk is written to err only the first way.
    /// </summary>
    [TestFixture]
    internal sealed class GenericArgumentRequirementTests
    {
        [TestCase(typeof(ByValue<>))]
        [TestCase(typeof(PublicByValue<>))]
        [TestCase(typeof(ByValueList<>))]
        [TestCase(typeof(ByValueArray<>))]
        [TestCase(typeof(NestedInBox<>))]
        [TestCase(typeof(InheritsAByValueField<>))]
        public void ParameterStoredByValue_RequiresASerializableArgument(Type definition)
        {
            var parameter = ParameterOf(definition);

            Assert.IsTrue(GenericArgumentRequirement.RequiresSerializableArgument(definition, parameter));
            Assert.IsFalse(SerializeReferenceHelpers.IsAcceptableGenericArgument(definition, parameter, typeof(PlainCandidate)),
                "The closed type would carry a field Unity cannot write, so the argument has to be refused.");
            Assert.IsTrue(SerializeReferenceHelpers.IsAcceptableGenericArgument(definition, parameter, typeof(Vector2)));
        }

        [TestCase(typeof(ReferenceOnly<>))]
        [TestCase(typeof(NeverSerialized<>))]
        [TestCase(typeof(NestedInPlainBox<>))]
        public void ParameterNeverStoredByValue_ImposesNoRequirement(Type definition)
        {
            var parameter = ParameterOf(definition);

            Assert.IsFalse(GenericArgumentRequirement.RequiresSerializableArgument(definition, parameter));
            Assert.IsTrue(SerializeReferenceHelpers.IsAcceptableGenericArgument(definition, parameter, typeof(PlainCandidate)),
                "Nothing of the argument is stored, so demanding that Unity could store it refuses a usable type.");
            Assert.IsTrue(SerializeReferenceHelpers.IsAcceptableGenericArgument(definition, parameter, typeof(Ray)),
                "…including a Unity value type the engine does not serialize.");
        }

        [Test]
        public void SelfReferencingType_TerminatesAndImposesNoRequirement()
        {
            // SelfReferencing<T> holds a field of its own type, so a walk without a visited set never returns.
            var definition = typeof(NestedInSelfReferencing<>);
            Assert.IsFalse(GenericArgumentRequirement.RequiresSerializableArgument(definition, ParameterOf(definition)));
        }

        [Test]
        public void UnwalkableDefinition_KeepsTheRequirement()
        {
            // The walk proves an absence; with nothing to walk there is no proof, and the strict rule stands.
            Assert.IsTrue(GenericArgumentRequirement.RequiresSerializableArgument(null, null));
            Assert.IsTrue(GenericArgumentRequirement.RequiresSerializableArgument(
                typeof(ReferenceOnly<int>), typeof(ReferenceOnly<>).GetGenericArguments()[0]),
                "A closed type is not a definition whose parameters can be located — no proof, no relaxation.");
        }

        [TestCase(typeof(SerializableBox<>))]
        [TestCase(null)]
        public void StructurallyImpossibleArgument_IsRejectedWithoutAnyRequirement(Type argument)
        {
            // MakeGenericType refuses these outright, so no reasoning about storage can rescue them.
            var definition = typeof(ReferenceOnly<>);
            Assert.IsFalse(SerializeReferenceHelpers.IsAcceptableGenericArgument(definition, ParameterOf(definition), argument));
        }

        private static Type ParameterOf(Type definition) => definition.GetGenericArguments()[0];
    }
}
