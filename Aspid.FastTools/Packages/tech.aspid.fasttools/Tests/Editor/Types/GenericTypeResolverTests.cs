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

    internal abstract class ResolverAbstractWithDefaultCtor
    {
        public ResolverAbstractWithDefaultCtor() { }
    }

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

    // The same shape twice, differing only in where T lands: one keeps it as serialized data, the other only ever
    // as a managed reference. What the field may close them over differs accordingly.
#pragma warning disable CS0169
    [System.Serializable]
    internal sealed class ResolverValueSequence<T> : IResolverConverter<T, T>
    {
        [UnityEngine.SerializeField] private T _value;
    }

    internal sealed class ResolverReferenceSequence<T> : IResolverConverter<T, T>
    {
        [UnityEngine.SerializeReference] private IResolverConverter<T, T>[] _items;
    }
#pragma warning restore CS0169

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

    // A parameter the field reaches only through an array of it.
    internal interface IResolverArrayHolder<T> { }

    internal sealed class ResolverArrayBox<T> : IResolverArrayHolder<T[]> { }

    // Constraints that name a parameter: the argument page has to close them over the arguments it knows.
    internal sealed class ResolverComparableBox<T>
        where T : System.IComparable<T> { }

    internal sealed class ResolverDerivedPair<TBase, TDerived>
        where TDerived : TBase { }

    // The CLR enforces only the struct part of unmanaged, so MakeGenericType alone accepts the managed struct.
    internal sealed class ResolverUnmanagedBox<T>
        where T : unmanaged { }

#pragma warning disable CS0169
    internal struct ResolverManagedStruct
    {
        private string _text;
    }

    internal struct ResolverUnmanagedStruct
    {
        private int _count;
        private ResolverStruct _nested;
    }
#pragma warning restore CS0169

    // A key the field fixes to a value type, which the class constraint then refuses.
    internal sealed class ResolverClassKeyed<TKey> : IResolverKeyed<TKey>
        where TKey : class { }

    // One variant definition per direction, and candidates whose argument there is partially open.
    internal interface IResolverSource<out T> { }

    internal interface IResolverSink<in T> { }

    internal sealed class ResolverSourceOf<T> : IResolverSource<T> { }

    internal sealed class ResolverSinkOf<T> : IResolverSink<T> { }

    internal sealed class ResolverPairSource<TKey, TValue>
        : IResolverSource<System.Collections.Generic.KeyValuePair<TKey, TValue>> { }

    internal sealed class ResolverListSource<T> : IResolverSource<System.Collections.Generic.List<T>> { }

    internal sealed class ResolverStructSource<T> : IResolverSource<T>
        where T : struct { }

    // Through the second view any T is an IResolverSink<List<int>>, so the first view's T = int decides nothing.
    internal sealed class ResolverSinkTwice<T> : IResolverSink<System.Collections.Generic.List<T>>, IResolverSink<object> { }

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
        public void SatisfiesSpecialConstraints_StructConstraint_RejectsNullableValueType()
        {
            var parameter = typeof(StructBox<>).GetGenericArguments()[0];

            Assert.IsFalse(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(int?)));
        }

        [Test]
        public void SatisfiesSpecialConstraints_NewConstraint_RejectsAbstractClassWithPublicConstructor()
        {
            var parameter = typeof(CtorBox<>).GetGenericArguments()[0];

            Assert.IsFalse(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverAbstractWithDefaultCtor)));
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
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<string, string>), null, (_, _, _) => false)
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
            // End-to-end with the real argument filter, on the shape the picker reported. ResolverValueSequence
            // stores T, so Vector2 has to clear the serializability bar — and before the engine's own types were
            // recognised it did not, leaving the row open with an argument page that refused Vector2 as well.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<UnityEngine.Vector2, UnityEngine.Vector2>),
                    null, SerializeReferences.Editors.SerializeReferenceHelpers.IsAcceptableGenericArgument)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverValueSequence<UnityEngine.Vector2>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverValueSequence<>),
                "…and its open definition must not be offered beside it.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_ParameterOnlyBehindAReference_ClosesOverAnUnserializableArgument()
        {
            // Two candidates of identical shape against one field. Ray is a type Unity does not serialize, which
            // matters only for the candidate that would store it: the one holding nothing but a
            // [SerializeReference] array closes over Ray happily, and refusing it would hide a usable row.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<UnityEngine.Ray, UnityEngine.Ray>),
                    null, SerializeReferences.Editors.SerializeReferenceHelpers.IsAcceptableGenericArgument)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverReferenceSequence<UnityEngine.Ray>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverValueSequence<UnityEngine.Ray>),
                "The candidate that stores T must not be closed over an argument Unity would drop.");
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
                argumentFilter: (_, _, argument) => argument != typeof(string)));

            Assert.IsNull(closed);
        }

        [Test]
        public void TryInferFromFieldType_ArgumentAcceptedByFilter_InfersArguments()
        {
            Assert.IsTrue(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverConverter<string, string>), typeof(ResolverSequence<>), out var closed,
                argumentFilter: (_, _, argument) => argument == typeof(string)));

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

        [Test]
        public void TryInferFromFieldType_ArrayArgument_InfersItsElementType()
        {
            Assert.IsTrue(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverArrayHolder<int[]>), typeof(ResolverArrayBox<>), out var closed));

            Assert.AreEqual(typeof(ResolverArrayBox<int>), closed);
        }

        [Test]
        public void TryInferFromFieldType_ArrayOfAnotherRank_Fails()
        {
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverArrayHolder<int[,]>), typeof(ResolverArrayBox<>), out var closed));

            Assert.IsNull(closed);
        }

        [Test]
        public void GetAssignableGenericDefinitions_ArrayArgument_IsOfferedClosed()
        {
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverArrayHolder<string[]>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverArrayBox<string>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverArrayBox<>));
        }

        [Test]
        public void CanCloseWithArguments_ConstraintNamingItsOwnParameter_ChecksTheCandidate()
        {
            Assert.IsTrue(GenericTypeResolver.CanCloseWithArguments(
                typeof(ResolverComparableBox<>), new[] { typeof(int) }, fieldTypes: null));

            Assert.IsFalse(GenericTypeResolver.CanCloseWithArguments(
                typeof(ResolverComparableBox<>), new[] { typeof(ResolverClass) }, fieldTypes: null),
                "ResolverClass is not an IComparable<ResolverClass>, so the page must not offer it.");
        }

        [Test]
        public void CanCloseWithArguments_ConstraintNamingAnEarlierParameter_UsesItsArgument()
        {
            Assert.IsTrue(GenericTypeResolver.CanCloseWithArguments(
                typeof(ResolverDerivedPair<,>), new[] { typeof(IResolverThing), typeof(ResolverClass) }, fieldTypes: null));

            Assert.IsFalse(GenericTypeResolver.CanCloseWithArguments(
                typeof(ResolverDerivedPair<,>), new[] { typeof(IResolverThing), typeof(OpenBox<int>) }, fieldTypes: null));
        }

        [Test]
        public void CanCloseWithArguments_ConstraintNamingALaterParameter_IsLeftForLater()
        {
            // TDerived : TBase cannot be judged while TBase is the one being chosen.
            Assert.IsTrue(GenericTypeResolver.CanCloseWithArguments(
                typeof(ResolverDerivedPair<,>), new[] { typeof(ResolverClass) }, fieldTypes: null));
        }

        [Test]
        public void SatisfiesSpecialConstraints_UnmanagedConstraint_RejectsStructWithReferences()
        {
            var parameter = typeof(ResolverUnmanagedBox<>).GetGenericArguments()[0];

            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(int)));
            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverUnmanagedStruct)));
            Assert.IsFalse(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverManagedStruct)),
                "A struct holding a string is not unmanaged.");
        }

        [Test]
        public void SatisfiesSpecialConstraints_StructConstraint_AcceptsStructWithReferences()
        {
            var parameter = typeof(StructBox<>).GetGenericArguments()[0];

            Assert.IsTrue(GenericTypeResolver.SatisfiesSpecialConstraints(parameter, typeof(ResolverManagedStruct)));
        }

        [Test]
        public void TryConstruct_UnmanagedConstraintViolated_FailsWithError()
        {
            Assert.IsFalse(GenericTypeResolver.TryConstruct(
                typeof(ResolverUnmanagedBox<>), new[] { typeof(ResolverManagedStruct) }, fieldTypes: null,
                out var closed, out var error));

            Assert.IsNull(closed);
            Assert.IsNotNull(error);
        }

        [Test]
        public void GetAssignableGenericDefinitions_FieldFixesAnArgumentItsConstraintRejects_DropsTheCandidate()
        {
            // IResolverKeyed<int> forces TKey = int, which `where TKey : class` refuses: every choice on the
            // argument page would end in an error.
            var forInt = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverKeyed<int>), null)
                .ToArray();

            var forString = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverKeyed<string>), null)
                .ToArray();

            Assert.IsFalse(OffersAnyFormOf(forInt, typeof(ResolverClassKeyed<>)));
            CollectionAssert.Contains(forString, typeof(ResolverClassKeyed<string>));
        }

        [Test]
        public void GetAssignableGenericDefinitions_PartiallyOpenArgumentThatCannotVary_DropsTheCandidate()
        {
            // At a covariant position the argument must convert to string: a KeyValuePair never converts, a
            // List<T> is no string, and a struct-constrained T can be neither.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverSource<string>), null)
                .ToArray();

            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverPairSource<,>)));
            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverListSource<>)));
            Assert.IsFalse(OffersAnyFormOf(offered, typeof(ResolverStructSource<>)));
        }

        [Test]
        public void GetAssignableGenericDefinitions_PartiallyOpenArgumentThatCanVary_KeepsTheCandidate()
        {
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(
                    typeof(IResolverSource<System.Collections.Generic.IEnumerable<int>>), null)
                .ToArray();

            Assert.IsTrue(OffersAnyFormOf(offered, typeof(ResolverListSource<>)),
                "List<int> is an IEnumerable<int>, so the candidate closes.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_VariantPositionWithOtherFits_OffersClosedAndOpen()
        {
            // ResolverSinkOf<object> is an IResolverSink<ResolverClass> as well: the closed row stays a shortcut,
            // and the open row leads to the other arguments.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverSink<ResolverClass>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverSinkOf<ResolverClass>));
            CollectionAssert.Contains(offered, typeof(ResolverSinkOf<>));
            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverSink<ResolverClass>), typeof(ResolverSinkOf<>), out _),
                "Picking the open row must open the argument page.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_VariantPositionWithOneFit_OffersOnlyClosed()
        {
            // A covariant position takes subtypes, and string has none.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverSource<string>), null)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverSourceOf<string>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverSourceOf<>));
        }

        [Test]
        public void TryInferFromFieldType_ViewThatLeavesTheParameterFree_Fails()
        {
            Assert.IsTrue(typeof(IResolverSink<System.Collections.Generic.List<int>>)
                    .IsAssignableFrom(typeof(ResolverSinkTwice<string>)),
                "Sanity: the IResolverSink<object> view accepts any T.");

            Assert.IsFalse(GenericTypeResolver.TryInferFromFieldType(
                typeof(IResolverSink<System.Collections.Generic.List<int>>), typeof(ResolverSinkTwice<>), out _));
        }

        [Test]
        public void CanCloseWithArguments_VariantField_KeepsOnlyArgumentsThatConvert()
        {
            var sink = new[] { typeof(IResolverSink<ResolverClass>) };
            var source = new[] { typeof(IResolverSource<IResolverThing>) };

            Assert.IsTrue(GenericTypeResolver.CanCloseWithArguments(typeof(ResolverSinkOf<>), new[] { typeof(object) }, sink));
            Assert.IsFalse(GenericTypeResolver.CanCloseWithArguments(typeof(ResolverSinkOf<>), new[] { typeof(OpenBox<int>) }, sink));

            Assert.IsTrue(GenericTypeResolver.CanCloseWithArguments(typeof(ResolverSourceOf<>), new[] { typeof(ResolverClass) }, source));
            Assert.IsFalse(GenericTypeResolver.CanCloseWithArguments(typeof(ResolverSourceOf<>), new[] { typeof(ResolverStruct) }, source),
                "Variance does not box, so a struct implementing the interface still does not fit.");
        }

        [Test]
        public void CanCloseWithArguments_InvariantField_KeepsOnlyTheFieldArgument()
        {
            var fieldTypes = new[] { typeof(IResolverKeyed<string>) };

            Assert.IsTrue(GenericTypeResolver.CanCloseWithArguments(typeof(ResolverPair<,>), new[] { typeof(string) }, fieldTypes));
            Assert.IsFalse(GenericTypeResolver.CanCloseWithArguments(typeof(ResolverPair<,>), new[] { typeof(int) }, fieldTypes));
        }

        // A resolver that closes a candidate returns it under a different Type than the definition asserted on, so
        // a candidate that must be gone has to be checked in both forms.
        private static bool OffersAnyFormOf(System.Collections.Generic.IEnumerable<System.Type> offered,
            System.Type definition) =>
            offered.Any(type => type == definition ||
                                (type.IsGenericType && type.GetGenericTypeDefinition() == definition));
    }
}
