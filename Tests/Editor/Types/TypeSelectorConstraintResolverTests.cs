using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Guards <see cref="TypeSelectorConstraintResolver"/>: the member-first resolution of a
    /// <see cref="TypeSelectorAttribute"/> string argument (a field/property named by the string supplies the
    /// constraint dynamically) with an assembly-qualified-name fallback, and the warnings surfaced when an
    /// argument resolves to nothing — the runtime mirror of analyzer rules AFT0006–AFT0008.
    /// </summary>
    [TestFixture]
    internal sealed class TypeSelectorConstraintResolverTests
    {
        private sealed class FakeSerializableType : ISerializableType
        {
            public Type BaseType => typeof(object);
            public Type Type { get; set; }
        }

        // The host's fields are read only through reflection (by the resolver under test), so the compiler cannot
        // see them being used — silence the resulting "assigned but never used" / "never used" warnings.
#pragma warning disable CS0169, CS0414, CS0649
        private class BaseHost
        {
            protected Type _inheritedType = typeof(short);
        }

        private sealed class Host : BaseHost
        {
            private Type _weaponType = typeof(int);
            private Type[] _weaponTypes = { typeof(int), typeof(long) };
            private string _typeName = typeof(int).AssemblyQualifiedName;
            private string[] _typeNames = { typeof(int).AssemblyQualifiedName, typeof(long).AssemblyQualifiedName };
            private ISerializableType _wrapper = new FakeSerializableType { Type = typeof(int) };
            private ISerializableType[] _wrappers =
            {
                new FakeSerializableType { Type = typeof(int) },
                new FakeSerializableType { Type = typeof(long) }
            };
            private ISerializableType _unsetWrapper = new FakeSerializableType { Type = null };
            private Type _nullType = null;
            private int _count = 3;
            private static Type _staticType = typeof(int);
        }
#pragma warning restore CS0169, CS0414, CS0649

        private static Type[] Resolve(string name, object target, List<string> warnings = null) =>
            TypeSelectorConstraintResolver.Resolve(new[] { name }, target, warnings);

        [Test]
        public void TypeFieldMember_ReturnsItsValue()
        {
            var types = Resolve("_weaponType", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int) }, types);
        }

        [Test]
        public void TypeArrayMember_ReturnsEveryElement()
        {
            var types = Resolve("_weaponTypes", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(long) }, types);
        }

        [Test]
        public void StringMember_ResolvesTheAssemblyQualifiedName()
        {
            var types = Resolve("_typeName", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int) }, types);
        }

        [Test]
        public void StringArrayMember_ResolvesEveryName()
        {
            var types = Resolve("_typeNames", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(long) }, types);
        }

        [Test]
        public void SerializableTypeMember_ReturnsTheWrappedType()
        {
            var types = Resolve("_wrapper", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int) }, types);
        }

        [Test]
        public void SerializableTypeArrayMember_ReturnsEveryWrappedType()
        {
            var types = Resolve("_wrappers", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(long) }, types);
        }

        [Test]
        public void InheritedMember_IsResolvedThroughTheBaseType()
        {
            var types = Resolve("_inheritedType", new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(short) }, types);
        }

        [Test]
        public void AssemblyQualifiedName_NotAMember_ResolvesAsAType()
        {
            var types = Resolve(typeof(int).AssemblyQualifiedName, new Host());
            CollectionAssert.AreEquivalent(new[] { typeof(int) }, types);
        }

        [Test]
        public void UnknownIdentifier_AddsAWarningAndNoType()
        {
            var warnings = new List<string>();
            var types = Resolve("_missing", new Host(), warnings);

            CollectionAssert.IsEmpty(types);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void UnsuitableMember_AddsAWarningAndNoType()
        {
            var warnings = new List<string>();
            var types = Resolve("_count", new Host(), warnings);

            CollectionAssert.IsEmpty(types);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void StaticMember_IsInvisibleToTheInstanceLookup_AndWarns()
        {
            var warnings = new List<string>();
            var types = Resolve("_staticType", new Host(), warnings);

            CollectionAssert.IsEmpty(types);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void SuitableMemberWithNullValue_AddsNoWarning()
        {
            var warnings = new List<string>();
            var types = Resolve("_nullType", new Host(), warnings);

            CollectionAssert.IsEmpty(types);
            CollectionAssert.IsEmpty(warnings);
        }

        [Test]
        public void SuitableWrapperWithNullType_AddsNoWarning()
        {
            var warnings = new List<string>();
            var types = Resolve("_unsetWrapper", new Host(), warnings);

            CollectionAssert.IsEmpty(types);
            CollectionAssert.IsEmpty(warnings);
        }

        [Test]
        public void BlankName_IsSkippedWithoutWarning()
        {
            var warnings = new List<string>();
            var types = TypeSelectorConstraintResolver.Resolve(new[] { "", "   " }, new Host(), warnings);

            CollectionAssert.IsEmpty(types);
            CollectionAssert.IsEmpty(warnings);
        }

        [Test]
        public void MultipleArguments_AppendEveryResolvedConstraint()
        {
            var warnings = new List<string>();
            var types = TypeSelectorConstraintResolver.Resolve(
                new[] { "_weaponType", "_inheritedType" }, new Host(), warnings);

            CollectionAssert.AreEquivalent(new[] { typeof(int), typeof(short) }, types);
            CollectionAssert.IsEmpty(warnings);
        }
    }
}
