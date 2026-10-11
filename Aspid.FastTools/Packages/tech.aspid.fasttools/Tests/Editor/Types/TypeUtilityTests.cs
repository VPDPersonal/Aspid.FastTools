using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;
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
        public void SweepDomainTypes_YieldsTypesFromTheLoadedAssemblies()
        {
            var types = TypeUtility.SweepDomainTypes(AppDomain.CurrentDomain.GetAssemblies, out _);

            CollectionAssert.AllItemsAreNotNull(types, "Unloadable entries must be filtered out, never yielded as null.");
            CollectionAssert.Contains(types, typeof(TypeUtilityTests), "A type of this very test assembly must be part of the domain sweep.");
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
        public void GetTypeOrNull_UnresolvedName_IsLookedUpOnce()
        {
            var assemblyName = $"Aspid.FastTools.Missing.{Guid.NewGuid():N}";
            using var lookups = new AssemblyLookupCounter(assemblyName);

            Assert.IsNull(TypeUtility.GetTypeOrNull($"Missing.Type, {assemblyName}"));
            var afterFirst = lookups.Count;

            Assert.IsNull(TypeUtility.GetTypeOrNull($"Missing.Type, {assemblyName}"));

            Assert.Greater(afterFirst, 0, "The probe must see the first lookup.");
            Assert.AreEqual(afterFirst, lookups.Count, "A name that does not resolve must not be looked up again.");
        }

        [Test]
        public void GetTypeOrNull_UnresolvedName_IsLookedUpAgainAfterAnAssemblyLoads()
        {
            var assemblyName = $"Aspid.FastTools.Missing.{Guid.NewGuid():N}";
            using var lookups = new AssemblyLookupCounter(assemblyName);

            TypeUtility.GetTypeOrNull($"Missing.Type, {assemblyName}");
            var afterFirst = lookups.Count;

            LoadProbeAssembly();
            TypeUtility.GetTypeOrNull($"Missing.Type, {assemblyName}");

            Assert.Greater(lookups.Count, afterFirst, "A loaded assembly may hold the type, so the name is tried again.");
        }

        [Test]
        public void DomainTypes_IsCachedAndContainsTheTestAssembly()
        {
            var first = TypeUtility.DomainTypes;

            Assert.AreSame(first, TypeUtility.DomainTypes, "The sweep must be cached between calls.");
            CollectionAssert.Contains(first, typeof(TypeUtilityTests));
        }

        [Test]
        public void DomainTypes_AfterAnAssemblyLoads_IncludesItsTypes()
        {
            var before = TypeUtility.DomainTypes;
            var probe = LoadProbeAssembly();

            var after = TypeUtility.DomainTypes;

            Assert.AreNotSame(before, after, "A loaded assembly must drop the cached sweep.");
            CollectionAssert.Contains(after, probe);
        }

        [Test]
        public void SweepDomainTypes_NoAssemblyLoads_RunsOnePass()
        {
            var passes = 0;

            TypeUtility.SweepDomainTypes(() =>
            {
                passes++;
                return Array.Empty<Assembly>();
            }, out _);

            Assert.AreEqual(1, passes);
        }

        [Test]
        public void SweepDomainTypes_AssemblyLoadedDuringAPass_ReadsOnlyTheNewAssemblyInTheNextPass()
        {
            var passes = 0;
            Type probe = null;
            var own = typeof(TypeUtilityTests).Assembly;

            var types = TypeUtility.SweepDomainTypes(() =>
            {
                if (passes++ > 0) return new[] { own, probe.Assembly };

                probe = LoadProbeAssembly();
                return new[] { own };
            }, out _);

            Assert.AreEqual(2, passes, "The pass that loaded an assembly missed its types, so it must be repeated once.");
            CollectionAssert.Contains(types, probe, "The assembly found by the repeat pass must be read.");
            CollectionAssert.AllItemsAreUnique(types, "A repeat pass must not read an assembly twice.");
        }

        [Test]
        public void SweepDomainTypes_EveryPassLoadsAnAssembly_StopsAtTheCap()
        {
            var passes = 0;

            TypeUtility.SweepDomainTypes(() =>
            {
                passes++;
                LoadProbeAssembly();
                return Array.Empty<Assembly>();
            }, out _);

            Assert.AreEqual(TypeUtility.MaxDomainSweeps, passes);
        }

        // Defining a dynamic assembly raises AppDomain.AssemblyLoad, as loading a file does. The assembly stays loaded
        // for the Editor session, so the type gets a compiler-generated name that the type picker hides.
        private static Type LoadProbeAssembly()
        {
            var name = new AssemblyName($"Aspid.FastTools.LoadProbe.{Guid.NewGuid():N}");
            var module = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run)
                .DefineDynamicModule(name.Name);

            return module.DefineType("<LoadProbe>", TypeAttributes.Public).CreateType();
        }

        // Type.GetType asks the domain for an assembly it cannot find, which tells how often a name is resolved.
        private sealed class AssemblyLookupCounter : IDisposable
        {
            private readonly string _assemblyName;

            public int Count { get; private set; }

            public AssemblyLookupCounter(string assemblyName)
            {
                _assemblyName = assemblyName;
                AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            }

            public void Dispose() =>
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;

            private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
            {
                if (args.Name.StartsWith(_assemblyName, StringComparison.Ordinal)) Count++;
                return null;
            }
        }
    }
}
