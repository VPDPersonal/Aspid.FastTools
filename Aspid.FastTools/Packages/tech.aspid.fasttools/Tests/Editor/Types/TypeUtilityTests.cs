using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="TypeUtility"/> — the name formatting every type-selector surface
    /// (rows, captions, tooltips, error messages) builds on.
    /// </summary>
    [TestFixture]
    internal sealed class TypeUtilityTests
    {
        private sealed class Outer<T>
        {
            internal sealed class Inner { }

            internal sealed class Pair<TSecond> { }

            internal sealed class Mid<TMid>
            {
                internal sealed class Leaf<TLeaf> { }
            }
        }

        // A source generator may mark an ordinary class like this; the types written inside it stay selectable.
        [CompilerGenerated]
        internal sealed class MarkedOuter
        {
            internal sealed class Written { }
        }

        // Three or more constants make the compiler keep the data in <PrivateImplementationDetails>, whose nested
        // __StaticArrayInitTypeSize=N struct has no angle brackets in its own name.
        internal static readonly int[] StaticData = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

        [TestCase("List`1", "List")]
        [TestCase("Dictionary`2", "Dictionary")]
        [TestCase("PlainName", "PlainName")]
        [TestCase("", "")]
        public void StripArity_RemovesTheBacktickSuffix(string rawName, string expected) =>
            Assert.AreEqual(expected, TypeUtility.StripArity(rawName));

        [Test]
        public void FormatGenericName_NonGeneric_ReturnsTheShortName() =>
            Assert.AreEqual("Int32", TypeUtility.FormatGenericName(typeof(int)));

        [Test]
        public void FormatGenericName_ClosedGeneric_SpellsTheArguments() =>
            Assert.AreEqual("List<Int32>", TypeUtility.FormatGenericName(typeof(List<int>)));

        [Test]
        public void FormatGenericName_MultipleArguments_AreCommaSeparated() =>
            Assert.AreEqual("Dictionary<Int32, String>", TypeUtility.FormatGenericName(typeof(Dictionary<int, string>)));

        [Test]
        public void FormatGenericName_NestedGeneric_RecursesIntoTheArguments() =>
            Assert.AreEqual("List<List<Int32>>", TypeUtility.FormatGenericName(typeof(List<List<int>>)));

        [Test]
        public void FormatGenericName_OpenDefinition_KeepsTheParameterName() =>
            Assert.AreEqual("List<T>", TypeUtility.FormatGenericName(typeof(List<>)));

        [Test]
        public void FormatGenericName_NestedInGeneric_ListsOnlyItsOwnArguments()
        {
            Assert.AreEqual("Inner", TypeUtility.FormatGenericName(typeof(Outer<>.Inner)));
            Assert.AreEqual("Inner", TypeUtility.FormatGenericName(typeof(Outer<int>.Inner)));
            Assert.AreEqual("Pair<TSecond>", TypeUtility.FormatGenericName(typeof(Outer<>.Pair<>)));
            Assert.AreEqual("Pair<String>", TypeUtility.FormatGenericName(typeof(Outer<int>.Pair<string>)));
            Assert.AreEqual("Leaf<Single>", TypeUtility.FormatGenericName(typeof(Outer<int>.Mid<string>.Leaf<float>)));
        }

        [Test]
        public void FormatDeclaringName_IsNullForATopLevelType_AndSpellsTheOuterTypes()
        {
            Assert.IsNull(TypeUtility.FormatDeclaringName(typeof(TypeUtilityTests)));
            Assert.AreEqual(nameof(TypeUtilityTests), TypeUtility.FormatDeclaringName(typeof(Outer<>)));
            Assert.AreEqual($"{nameof(TypeUtilityTests)}.Outer<T>", TypeUtility.FormatDeclaringName(typeof(Outer<>.Inner)));
            Assert.AreEqual($"{nameof(TypeUtilityTests)}.Outer<Int32>", TypeUtility.FormatDeclaringName(typeof(Outer<int>.Inner)));
            Assert.AreEqual($"{nameof(TypeUtilityTests)}.Outer<Int32>.Mid<String>",
                TypeUtility.FormatDeclaringName(typeof(Outer<int>.Mid<string>.Leaf<float>)));
            Assert.AreEqual($"{nameof(TypeUtilityTests)}.Outer<T>.Mid<TMid>",
                TypeUtility.FormatDeclaringName(typeof(Outer<>.Mid<>.Leaf<>)));
        }

        [Test]
        public void FormatQualifiedName_SpellsNamespaceOuterTypesAndName()
        {
            var prefix = $"{typeof(TypeUtilityTests).Namespace}.{nameof(TypeUtilityTests)}";

            Assert.AreEqual(prefix, TypeUtility.FormatQualifiedName(typeof(TypeUtilityTests)));
            Assert.AreEqual($"{prefix}.Outer<Int32>.Pair<String>", TypeUtility.FormatQualifiedName(typeof(Outer<int>.Pair<string>)));
            Assert.AreEqual("System.Collections.Generic.List<Int32>", TypeUtility.FormatQualifiedName(typeof(List<int>)));
        }

        [Test]
        public void IsCompilerGenerated_TypeNestedInAGeneratedOne_IsTrue()
        {
            var nested = GetGeneratedDataTypes().FirstOrDefault();

            Assert.IsNotNull(nested, "StaticData must make the compiler emit a nested data type.");
            Assert.IsFalse(nested.Name.Contains('<') || nested.Name.Contains('>'),
                "The data type's own name has no angle brackets, so only its outer type gives it away.");
            Assert.IsTrue(TypeUtility.IsCompilerGenerated(nested.DeclaringType));
            Assert.IsTrue(TypeUtility.IsCompilerGenerated(nested));
            Assert.IsFalse(TypeUtility.IsCompilerGenerated(typeof(TypeUtilityTests)));
            Assert.IsFalse(TypeUtility.IsCompilerGenerated(typeof(Outer<>.Inner)));
        }

        [Test]
        public void IsCompilerGenerated_TypeNestedInAMarkedClass_IsFalse()
        {
            Assert.IsTrue(TypeUtility.IsCompilerGenerated(typeof(MarkedOuter)));
            Assert.IsFalse(TypeUtility.IsCompilerGenerated(typeof(MarkedOuter.Written)));
        }

        [Test]
        public void GetAllTypeInfos_LeavesOutTypesNestedInAGeneratedOne()
        {
            var nested = GetGeneratedDataTypes().FirstOrDefault();
            Assert.IsNotNull(nested);

            var offered = TypeInfo.GetAllTypeInfos(new[] { typeof(object) }, TypeAllow.All);

            Assert.IsFalse(offered.Any(info => info.AssemblyQualifiedName == nested.AssemblyQualifiedName),
                "A compiler's data struct must not appear in the <Global> group.");
        }

        [Test]
        public void EnumerateDomainTypes_YieldsTypesFromTheLoadedAssemblies()
        {
            var found = false;

            foreach (var type in TypeUtility.EnumerateDomainTypes())
            {
                Assert.IsNotNull(type, "Unloadable entries must be filtered out, never yielded as null.");

                if (type != typeof(TypeUtilityTests)) continue;
                found = true;
                break;
            }

            Assert.IsTrue(found, "A type of this very test assembly must be part of the domain sweep.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("No.Such.Type, NoSuchAssembly")]
        [TestCase("Malformed[[Name")]
        public void GetTypeOrNull_UnresolvableOrMalformedName_IsNull(string assemblyQualifiedName) =>
            Assert.IsNull(TypeUtility.GetTypeOrNull(assemblyQualifiedName));

        [Test]
        public void GetTypeOrNull_ResolvesALoadedType() =>
            Assert.AreEqual(typeof(TypeUtilityTests), TypeUtility.GetTypeOrNull(typeof(TypeUtilityTests).AssemblyQualifiedName));

        private static IEnumerable<Type> GetGeneratedDataTypes() =>
            typeof(TypeUtilityTests).Assembly.GetTypes().Where(type =>
                type.DeclaringType is { } declaring && declaring.Name.StartsWith("<PrivateImplementationDetails>"));

        [Test]
        public void DomainTypes_IsCachedAndContainsTheTestAssembly()
        {
            var first = TypeUtility.DomainTypes;

            Assert.AreSame(first, TypeUtility.DomainTypes, "The sweep must be cached between calls.");
            CollectionAssert.Contains(first, typeof(TypeUtilityTests));
        }
    }
}
