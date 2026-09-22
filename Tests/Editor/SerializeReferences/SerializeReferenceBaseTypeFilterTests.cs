using System;
using System.Linq;
using NUnit.Framework;
using Aspid.FastTools.Types.Editors;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    internal interface IFilterWeapon { }

    internal interface IFilterMelee { }

    internal interface IFilterRanged { }

    internal sealed class FilterSword : IFilterWeapon, IFilterMelee { }

    internal sealed class FilterBow : IFilterWeapon, IFilterRanged { }

    internal sealed class FilterGlaive : IFilterWeapon, IFilterMelee, IFilterRanged { }

    [Serializable]
    internal sealed class FilterGenericBow<T> : IFilterWeapon, IFilterRanged { }

    [Serializable]
    internal sealed class FilterGenericGlaive<T> : IFilterWeapon, IFilterMelee, IFilterRanged { }

    /// <summary>
    /// Locks the <c>[TypeSelector]</c> base-type rule on a <c>[SerializeReference]</c> field: a candidate must be
    /// assignable to <b>every</b> attribute type, the same intersection a string or <c>SerializableType</c> field
    /// applies — and concrete classes and open generic definitions must agree on it.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceBaseTypeFilterTests
    {
        private static readonly Type[] MeleeAndRanged = { typeof(IFilterMelee), typeof(IFilterRanged) };

        [Test]
        public void SeveralBaseTypes_AcceptOnlyTypesAssignableToAll()
        {
            var filter = SerializeReferenceHelpers.BuildAssignableFilter(MeleeAndRanged);

            Assert.IsTrue(filter(typeof(FilterGlaive)));
            Assert.IsFalse(filter(typeof(FilterSword)), "Matching one of the attribute types is not enough.");
            Assert.IsFalse(filter(typeof(FilterBow)), "Matching one of the attribute types is not enough.");
        }

        [Test]
        public void SingleBaseType_AcceptsItsImplementations()
        {
            var filter = SerializeReferenceHelpers.BuildAssignableFilter(new[] { typeof(IFilterMelee) });

            Assert.IsTrue(filter(typeof(FilterSword)));
            Assert.IsTrue(filter(typeof(FilterGlaive)));
            Assert.IsFalse(filter(typeof(FilterBow)));
        }

        [Test]
        public void NoNarrowingType_KeepsEveryCandidate()
        {
            Assert.IsTrue(SerializeReferenceHelpers.BuildAssignableFilter(null)(typeof(FilterBow)));
            Assert.IsTrue(SerializeReferenceHelpers.BuildAssignableFilter(Array.Empty<Type>())(typeof(FilterBow)));
            Assert.IsTrue(SerializeReferenceHelpers.BuildAssignableFilter(new[] { typeof(object) })(typeof(FilterBow)),
                "object is the unconstrained default, not a narrowing type.");
        }

        [Test]
        public void SeveralBaseTypes_NarrowGenericDefinitionsTheSameWay()
        {
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IFilterWeapon), MeleeAndRanged)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(FilterGenericGlaive<>));
            CollectionAssert.DoesNotContain(offered, typeof(FilterGenericBow<>),
                "A generic candidate must meet the same intersection as a concrete one.");
        }
    }
}
