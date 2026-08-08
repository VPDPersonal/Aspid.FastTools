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

    // A candidate that fixes one of the definition's arguments itself: it is an IResolverConverter<,> like any
    // other, yet no TFrom turns it into an IResolverConverter<float, float>.
    internal sealed class ResolverToString<TFrom> : IResolverConverter<TFrom, string> { }

    internal interface IResolverKeyed<TKey> { }

    internal sealed class ResolverPair<TKey, TValue> : IResolverKeyed<TKey> { }

    // Same shape as ResolverPair, except the key it implements is pinned and cannot follow the field.
    internal sealed class ResolverIntKeyed<TValue> : IResolverKeyed<int> { }

    // Variance is part of assignability, so a candidate naming a wider argument than the field is still a
    // candidate — but only across a reference conversion.
    internal interface IResolverVariant<in TIn, out TOut> { }

    internal sealed class ResolverFromObject<T> : IResolverVariant<object, T> { }

    // The variant twin of ResolverSequence: one parameter answering for both positions of a variant definition.
    internal sealed class ResolverVariantSequence<T> : IResolverVariant<T, T> { }

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
        public void GetAssignableGenericDefinitions_CandidateFixingAnArgument_IsNotOffered()
        {
            // ResolverToString<TFrom> : IResolverConverter<TFrom, string> matches the field's definition and nothing
            // else — TOut is string whatever TFrom becomes. Offering it would put a dead row in the picker: the
            // argument page opens and then refuses every choice made on it.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<float, float>), null)
                .ToArray();

            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverToString<>)),
                "A candidate that cannot close to the field under any argument must not be offered at all.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_CandidateFixingAnArgumentTheFieldAgreesWith_IsStillOffered()
        {
            // The same candidate against a field whose second argument is the one it fixes: rejecting it here would
            // trade the dead row for a missing one.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<float, string>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverToString<float>));
        }

        [Test]
        public void GetAssignableGenericDefinitions_DeterminedCandidate_SurvivesTheArgumentComparison()
        {
            // ResolverSequence<T> : IResolverConverter<T, T> does close to this field — comparing arguments must
            // bind T from both positions rather than reject the second as a repeat.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<float, float>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverSequence<float>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverSequence<>));
        }

        [Test]
        public void GetAssignableGenericDefinitions_PartiallyDeterminedCandidate_SurvivesTheArgumentComparison()
        {
            // ResolverPair<TKey, TValue> : IResolverKeyed<TKey> binds TKey and leaves TValue free — the argument
            // comparison has to tolerate that partial binding, or it deletes the rows the argument page exists for.
            // ResolverIntKeyed<TValue> has the same free parameter but pins the key to a type the field does not
            // name, so no choice of TValue can save it.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverKeyed<string>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverPair<,>));
            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverIntKeyed<>)),
                "A free parameter cannot rescue a candidate whose fixed argument already mismatches.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_VariantPosition_KeepsAReferenceConvertibleCandidate()
        {
            // IResolverVariant<in TIn, out TOut>: ResolverFromObject<string> really is an
            // IResolverVariant<string, string>, so comparing arguments by identity alone would drop a usable row.
            Assert.IsTrue(typeof(IResolverVariant<string, string>).IsAssignableFrom(typeof(ResolverFromObject<string>)),
                "Sanity: the CLR accepts this candidate, so the picker must offer it.");

            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverVariant<string, string>), null)
                .ToArray();

            Assert.IsTrue(OffersAnyFormOf(offered, typeof(ResolverFromObject<>)));
        }

        [Test]
        public void GetAssignableGenericDefinitions_VariantPositionOnAValueType_DropsTheCandidate()
        {
            // Variance is only applied over a reference conversion, so int boxing to object buys the candidate
            // nothing here — the same candidate the previous test keeps is a dead row against this field.
            Assert.IsFalse(typeof(IResolverVariant<int, int>).IsAssignableFrom(typeof(ResolverFromObject<int>)),
                "Sanity: the CLR refuses the value-type conversion, so the picker must not offer it.");

            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverVariant<int, int>), null)
                .ToArray();

            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverFromObject<>)));
        }

        [Test]
        public void GetAssignableGenericDefinitions_UnityNativeArgument_IsOfferedClosed()
        {
            // End-to-end with the real argument filter, on the shape the picker reported: a Vector2 converter field
            // determines T, but the filter used to refuse Vector2 for lacking [Serializable], so the row fell back to
            // its open definition — and the argument page refused Vector2 as well, leaving nothing to pick.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<UnityEngine.Vector2, UnityEngine.Vector2>),
                    null, SerializeReferences.Editors.SerializeReferenceHelpers.IsValidGenericArgument)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverSequence<UnityEngine.Vector2>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverSequence<>),
                "…and its open definition must not be offered beside it.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_ValueTypePinningAVariantPosition_DropsTheCandidate()
        {
            // ResolverVariantSequence<T> : IResolverVariant<T, T>. The field's float pins T — variance buys a value
            // type nothing — and a T of float can never be the string the covariant position then asks for. The
            // verdict has to hold whichever of the two positions is looked at first.
            Assert.IsFalse(typeof(IResolverVariant<float, string>).IsAssignableFrom(typeof(ResolverVariantSequence<float>)));
            Assert.IsFalse(typeof(IResolverVariant<float, string>).IsAssignableFrom(typeof(ResolverVariantSequence<string>)));

            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverVariant<float, string>), null)
                .ToArray();

            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverVariantSequence<>)));
        }

        [Test]
        public void GetAssignableGenericDefinitions_VariantPositionsLeftFree_KeepTheCandidate()
        {
            // The same candidate where nothing pins T: a T of object satisfies both positions, so the row stays and
            // the argument page gets to collect it.
            Assert.IsTrue(typeof(IResolverVariant<string, object>).IsAssignableFrom(typeof(ResolverVariantSequence<object>)),
                "Sanity: object answers for both positions, so the candidate is usable.");

            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverVariant<string, object>), null)
                .ToArray();

            Assert.IsTrue(OffersAnyFormOf(offered, typeof(ResolverVariantSequence<>)));
        }

        [Test]
        public void GetAssignableGenericDefinitions_DefinitionImplementedTwice_JudgesEveryView()
        {
            // ResolverMulti<T> : IResolverThingOf<List<T>>, IResolverThingOf<int>. One view closes the first field,
            // the other the second, and neither closes the third — a verdict read off whichever view
            // Type.GetInterfaces() happens to return first would be wrong for two of these three.
            var forList = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverThingOf<System.Collections.Generic.List<string>>), null)
                .ToArray();

            var forInt = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverThingOf<int>), null)
                .ToArray();

            var forString = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverThingOf<string>), null)
                .ToArray();

            CollectionAssert.Contains(forList, typeof(ResolverMulti<string>),
                "The IResolverThingOf<List<T>> view determines T.");
            CollectionAssert.Contains(forInt, typeof(ResolverMulti<>),
                "The IResolverThingOf<int> view leaves T free, so the argument page still has to collect it.");
            Assert.IsFalse(OffersAnyFormOf(forString, typeof(ResolverMulti<>)),
                "Neither view can produce an IResolverThingOf<string>.");
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

        // A resolver that closes a candidate returns it under a different Type than the definition asserted on, so
        // a candidate that must be gone has to be checked in both forms.
        private static bool OffersAnyFormOf(System.Collections.Generic.IEnumerable<System.Type> offered,
            System.Type definition) =>
            offered.Any(type => type == definition ||
                                (type.IsGenericType && type.GetGenericTypeDefinition() == definition));
    }
}
