using System.Linq;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // Test-only generic hierarchy: a structure-constrained box and an unconstrained variant. 
    internal interface IResolverThing { }

    [System.Serializable]
    internal struct ResolverStruct : IResolverThing { }

    [System.Serializable]
    internal sealed class ResolverClass : IResolverThing { }

    internal sealed class ResolverNoDefaultCtor : IResolverThing
    {
        public ResolverNoDefaultCtor(int _) { }
    }

    internal sealed class StructBox<T>
        where T : struct, IResolverThing { }

    internal sealed class ClassBox<T>
        where T : class { }

    internal sealed class CtorBox<T>
        where T : new() { }

    internal sealed class OpenBox<T> { }

    // Inference shapes a field can present: a candidate binding one parameter from two arguments, the same
    // through a non-generic contract, and a candidate a view leaves underdetermined.
    internal interface IResolverConverter<TIn, TOut> { }

    internal interface IResolverStringConverter : IResolverConverter<string, string> { }

    internal sealed class ResolverSequence<T> : IResolverConverter<T, T> { }

    internal interface IResolverKeyed<TKey> { }

    internal sealed class ResolverPair<TKey, TValue> : IResolverKeyed<TKey> { }

    // One definition implemented twice with non-unifiable arguments — legal C#, and the shape that exposes any
    // dependence on the order Type.GetInterfaces() happens to return.
    internal interface IResolverThingOf<T> { }

    internal sealed class ResolverMulti<T> : IResolverThingOf<System.Collections.Generic.List<T>>, IResolverThingOf<int> { }

    /// <summary>
    /// Coverage for <see cref="GenericTypeResolver"/> — pure reflection logic that gates which closed generic types the
    /// picker may instantiate. A regression here lets the picker construct a managed reference Unity silently nulls.
    /// </summary>
    [TestFixture]
    internal sealed class GenericTypeResolverTests
    {
        [Test]
        public void SatisfiesSpecialConstraints_StructConstraint_AcceptsValueType_RejectsClass()
        {
            var parameter = typeof(StructBox<>).GetGenericArguments()[0];

            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverStruct)),
                "A value type must satisfy a 'struct' constraint.");
            Assert.IsFalse(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverClass)),
                "A reference type must not satisfy a 'struct' constraint.");
        }

        [Test]
        public void SatisfiesSpecialConstraints_ClassConstraint_AcceptsClass_RejectsValueType()
        {
            var parameter = typeof(ClassBox<>).GetGenericArguments()[0];

            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverClass)),
                "A reference type must satisfy a 'class' constraint.");
            Assert.IsFalse(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverStruct)),
                "A value type must not satisfy a 'class' constraint.");
        }

        [Test]
        public void SatisfiesSpecialConstraints_NewConstraint_RequiresAParameterlessConstructor()
        {
            var parameter = typeof(CtorBox<>).GetGenericArguments()[0];

            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverClass)),
                "A class with a public parameterless constructor must satisfy 'new()'.");
            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverStruct)),
                "A value type always satisfies 'new()'.");
            Assert.IsFalse(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverNoDefaultCtor)),
                "A class without a parameterless constructor must not satisfy 'new()'.");
        }

        [Test]
        public void GetConstraintBaseTypes_ReturnsExplicitConstraint()
        {
            var parameter = typeof(StructBox<>).GetGenericArguments()[0];
            CollectionAssert.Contains(GenericTypeResolver.GetConstraintBaseTypes(parameter), typeof(IResolverThing));
        }

        [Test]
        public void GetConstraintBaseTypes_Unconstrained_FallsBackToObject()
        {
            var parameter = typeof(OpenBox<>).GetGenericArguments()[0];
            CollectionAssert.AreEqual(new[] { typeof(object) }, GenericTypeResolver.GetConstraintBaseTypes(parameter));
        }

        [Test]
        public void TryConstruct_ValidArgument_ClosesType()
        {
            Assert.IsTrue(GenericTypeResolver.TryConstruct(
                typeof(StructBox<>), new[] { typeof(ResolverStruct) }, fieldTypes: null, out var closed, out var error));
            Assert.AreEqual(typeof(StructBox<ResolverStruct>), closed);
            Assert.IsNull(error);
        }

        [Test]
        public void TryConstruct_ConstraintViolated_FailsWithError()
        {
            Assert.IsFalse(GenericTypeResolver.TryConstruct(
                typeof(StructBox<>), new[] { typeof(ResolverClass) }, fieldTypes: null, out var closed, out var error));
            Assert.IsNull(closed);
            Assert.IsNotNull(error, "A violated struct constraint must report an error.");
        }

        [Test]
        public void TryConstruct_NotAssignableToField_Fails()
        {
            // OpenBox<int> does not implement IResolverThing, so it is rejected against that field type.
            Assert.IsFalse(GenericTypeResolver.TryConstruct(
                typeof(OpenBox<>), new[] { typeof(int) }, new[] { typeof(IResolverThing) }, out var closed, out var error));
            Assert.IsNull(closed);
            Assert.IsNotNull(error);
        }

        [Test]
        public void TryInferFromFieldType_ClosedGenericField_InfersArguments()
        {
            Assert.IsTrue(GenericTypeResolver.TryInferFromFieldType(typeof(OpenBox<int>), typeof(OpenBox<>), out var closed));
            Assert.AreEqual(typeof(OpenBox<int>), closed);
        }

        [Test]
        public void TryInferFromFieldType_FieldWithNoGenericView_Fails()
        {
            // IResolverThing is generic in no way at all — neither itself nor through a base or interface — so
            // there is nothing to unify against and the argument page is still needed.
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(typeof(IResolverThing), typeof(OpenBox<>), out var closed));
            Assert.IsNull(closed);
        }

        [Test]
        public void TryInferFromFieldType_UnrelatedDefinition_Fails()
        {
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(typeof(OpenBox<int>), typeof(StructBox<>), out var closed));
            Assert.IsNull(closed);
        }

        [Test]
        public void TryInferFromFieldType_InferredTypeNotAssignableToField_Fails()
        {
            // T binds to string, but ResolverSequence<string> does not implement the field's own contract —
            // inferring an argument must never produce a value the field cannot hold.
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverStringConverter), typeof(ResolverSequence<>), out var closed));

            Assert.IsNull(closed);
        }

        [Test]
        public void TryInferFromFieldType_OneParameterFromTwoArguments_InfersArguments()
        {
            // ResolverSequence<T> : IResolverConverter<T, T> — two arguments, one parameter. Copying the field's
            // arguments positionally cannot express this; unifying them can.
            Assert.IsTrue(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverConverter<string, string>), typeof(ResolverSequence<>), out var closed));

            Assert.AreEqual(typeof(ResolverSequence<string>), closed);
        }

        [Test]
        public void TryInferFromFieldType_ConflictingBindings_Fails()
        {
            // T would have to be both string and int at once.
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverConverter<string, int>), typeof(ResolverSequence<>), out var closed));

            Assert.IsNull(closed);
        }

        [Test]
        public void GetAssignableGenericDefinitions_DeterminedCandidate_IsOfferedClosed()
        {
            // Selecting this candidate never opens the argument page, so the row must not advertise parameters.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<string, string>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverSequence<string>),
                "A candidate the field fully determines must be offered closed.");
            CollectionAssert.DoesNotContain(offered, typeof(ResolverSequence<>),
                "…and its open definition must not be offered alongside it.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_UndeterminedCandidate_StaysOpen()
        {
            // IResolverKeyed<string> pins TKey but not TValue — the argument page is still the only way to finish.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverKeyed<string>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverPair<,>));
        }

        [Test]
        public void GetAssignableGenericDefinitions_ArgumentRejectedByFilter_StaysOpen()
        {
            // The filter is the argument page's own rule; closing a row must not slip an argument past it.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<string, string>), null, _ => false)
                .ToArray();

            CollectionAssert.DoesNotContain(offered, typeof(ResolverSequence<string>));
            CollectionAssert.Contains(offered, typeof(ResolverSequence<>),
                "A candidate whose inferred argument the filter rejects must keep offering the argument page.");
        }

        [Test]
        public void TryInferFromFieldType_UndeterminedParameter_Fails()
        {
            // IResolverKeyed<string> pins TKey but says nothing about TValue, so the argument page is still needed.
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverKeyed<string>), typeof(ResolverPair<,>), out var closed));

            Assert.IsNull(closed);
        }

        [Test]
        public void TryInferFromFieldType_ArgumentRejectedByFilter_Fails()
        {
            // Inference never shows the argument page, so the predicate that page applies to its candidates has to
            // hold here as well — otherwise a field shape that determines its arguments bypasses the rule entirely.
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverConverter<string, string>), typeof(ResolverSequence<>), out var closed,
                argumentFilter: argument => argument != typeof(string)));

            Assert.IsNull(closed);
        }

        [Test]
        public void TryInferFromFieldType_ArgumentAcceptedByFilter_InfersArguments()
        {
            Assert.IsTrue(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverConverter<string, string>), typeof(ResolverSequence<>), out var closed,
                argumentFilter: argument => argument == typeof(string)));

            Assert.AreEqual(typeof(ResolverSequence<string>), closed);
        }

        [Test]
        public void TryInferFromFieldType_DefinitionImplementedTwice_TriesEveryView()
        {
            // ResolverMulti<T> is known as IResolverThingOf<> twice; only one of those views binds T. Stopping at
            // whichever one reflection lists first would make this succeed or fail non-reproducibly.
            Assert.IsTrue(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverThingOf<System.Collections.Generic.List<string>>), typeof(ResolverMulti<>), out var closed));

            Assert.AreEqual(typeof(ResolverMulti<string>), closed);
        }

        [Test]
        public void IsAssignableToFieldTypes_ChecksEveryMeaningfulEntry()
        {
            Assert.IsTrue(GenericTypeResolver.IsAssignableToFieldTypes(typeof(ResolverClass), new[] { typeof(IResolverThing) }));
            Assert.IsFalse(GenericTypeResolver.IsAssignableToFieldTypes(typeof(OpenBox<int>), new[] { typeof(IResolverThing) }));
        }

        [Test]
        public void IsAssignableToFieldTypes_NullsAndObject_ImposeNoRestriction()
        {
            Assert.IsTrue(GenericTypeResolver.IsAssignableToFieldTypes(typeof(ResolverClass), fieldTypes: null));
            Assert.IsTrue(GenericTypeResolver.IsAssignableToFieldTypes(typeof(ResolverClass), new[] { null, typeof(object) }));
            Assert.IsFalse(GenericTypeResolver.IsAssignableToFieldTypes(null, fieldTypes: null),
                "No closed type can never pass the guard, whatever the field types.");
        }
    }
}
