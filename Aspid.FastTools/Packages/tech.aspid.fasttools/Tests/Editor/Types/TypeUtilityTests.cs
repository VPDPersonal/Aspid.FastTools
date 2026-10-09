using System;
using NUnit.Framework;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="TypeUtility"/> — the name formatting every type-selector surface
    /// (rows, captions, tooltips, error messages) builds on.
    /// </summary>
    [TestFixture]
    internal sealed class TypeUtilityTests
    {
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
