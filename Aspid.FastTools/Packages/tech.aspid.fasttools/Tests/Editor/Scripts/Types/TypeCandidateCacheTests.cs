using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Aspid.FastTools.Types;
using System.Reflection.Emit;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;
using TypeInfo = Aspid.FastTools.Types.Editors.TypeInfo;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.Types
{
    // The candidate list a picker reuses between openings must hold what a scan of every loaded type finds, must not
    // fix the predicate of the first opening, and must follow the loaded types when an assembly loads.
    [TestFixture]
    internal sealed class TypeCandidateCacheTests
    {
        private interface IScanProbe { }

        private abstract class ScanProbeBase<T> : IScanProbe { }

        private sealed class IntScanProbe : ScanProbeBase<int> { }

        private sealed class StringScanProbe : ScanProbeBase<string> { }

        private abstract class AbstractScanProbe : IScanProbe { }

        [TypeSelectorDisplay(Hidden = true)]
        private sealed class HiddenScanProbe : IScanProbe { }

        // What the checks of the scan read from one type, read once instead of once per combination.
        private sealed class ScannedType
        {
            public readonly Type Type;
            public readonly bool IsStatic;
            public readonly bool IsHidden;
            public readonly bool IsAbstract;
            public readonly bool IsInterface;
            public readonly bool IsEditorOnly;
            public readonly bool IsCompilerGenerated;

            public ScannedType(Type type)
            {
                Type = type;
                IsStatic = type.IsAbstract && type.IsSealed;
                IsAbstract = type.IsAbstract;
                IsInterface = type.IsInterface;
                IsEditorOnly = TypeUtility.IsEditorOnlyAssembly(type.Assembly);
                IsCompilerGenerated = TypeUtility.IsCompilerGenerated(type);

                // The scan reads the attributes of a type only after the checks above have kept it.
                IsHidden = !IsStatic && !IsCompilerGenerated && TypeSelectorHelpers.IsHiddenFromPicker(type);
            }
        }

        private static readonly Func<Type, bool>[] _filters =
        {
            null,
            type => type.IsPublic,
            type => type.IsClass && !type.IsGenericTypeDefinition,
        };

        [Test]
        public void GetAllTypeInfos_Object_MatchesAScanOfEveryType() =>
            AssertMatchesAScan(typeof(object));

        [Test]
        public void GetAllTypeInfos_Interface_MatchesAScanOfEveryType()
        {
            AssertMatchesAScan(typeof(IScanProbe));
            AssertMatchesAScan(typeof(System.Collections.IEnumerable));
        }

        [Test]
        public void GetAllTypeInfos_GenericBase_MatchesAScanOfEveryType()
        {
            AssertMatchesAScan(typeof(ScanProbeBase<int>));
            AssertMatchesAScan(typeof(IEquatable<string>));
            AssertMatchesAScan(typeof(ScanProbeBase<>));
        }

        [Test]
        public void GetAllTypeInfos_SeveralBaseTypes_MatchesAScanOfEveryType()
        {
            AssertMatchesAScan(typeof(IScanProbe), typeof(ScanProbeBase<int>));
            AssertMatchesAScan(typeof(ScanProbeBase<int>), typeof(IScanProbe));
        }

        [Test]
        public void GetCandidates_Object_IncludesTheTypesOfTheSystemLibrary()
        {
            var candidates = GetCandidates(typeof(object));

            CollectionAssert.Contains(candidates, typeof(string), "The scan walks every loaded assembly, not only the ones Unity compiles.");
            CollectionAssert.Contains(candidates, typeof(List<>), "Generic types are listed by their definition.");
            CollectionAssert.Contains(candidates, typeof(IntScanProbe));
        }

        [Test]
        public void GetCandidates_SameKey_ReturnsTheSameList()
        {
            var first = GetCandidates(typeof(IScanProbe));

            Assert.AreSame(first, GetCandidates(typeof(IScanProbe)),
                "A picker must not scan the loaded types again on every opening.");
            CollectionAssert.AreEquivalent(
                new[]
                {
                    typeof(ScanProbeBase<>), typeof(IntScanProbe), typeof(StringScanProbe),
                    typeof(AbstractScanProbe), typeof(HiddenScanProbe),
                },
                GetCandidates(typeof(IScanProbe), TypeAllow.Abstract, includeHidden: true)
                    .Where(type => type.DeclaringType == typeof(TypeCandidateCacheTests)));
        }

        [Test]
        public void GetCandidates_BaseTypesChangedAfterwards_KeepsTheListOfTheKey()
        {
            // No other test asks for this key, so the first call stores it.
            var baseTypes = new[] { typeof(IScanProbe), typeof(object) };
            var first = TypeInfo.GetCandidates(baseTypes, TypeAllow.None, includeHidden: false, excludeEditorOnly: false);

            baseTypes[1] = typeof(IDisposable);
            TypeInfo.GetCandidates(baseTypes, TypeAllow.None, includeHidden: false, excludeEditorOnly: false);

            Assert.AreSame(first,
                TypeInfo.GetCandidates(new[] { typeof(IScanProbe), typeof(object) }, TypeAllow.None, includeHidden: false, excludeEditorOnly: false),
                "The array a caller reuses for another picker must not rewrite the key of a stored list.");
        }

        [Test]
        public void GetCandidates_AnotherKey_ReturnsAnotherList()
        {
            var plain = GetCandidates(typeof(IScanProbe));

            Assert.AreNotSame(plain, GetCandidates(typeof(IScanProbe), TypeAllow.Abstract));
            Assert.AreNotSame(plain, GetCandidates(typeof(IScanProbe), includeHidden: true));
            Assert.AreNotSame(plain, GetCandidates(typeof(IScanProbe), excludeEditorOnly: true));
            Assert.AreNotSame(plain, GetCandidates(typeof(object)));
        }

        [Test]
        public void GetAllTypeInfos_PredicateOfAnotherOpening_IsNotFixedByTheFirst()
        {
            var baseTypes = new[] { typeof(IScanProbe) };

            var ints = TypeInfo.GetAllTypeInfos(baseTypes, TypeAllow.None, type => type == typeof(IntScanProbe));
            var strings = TypeInfo.GetAllTypeInfos(baseTypes, TypeAllow.None, type => type == typeof(StringScanProbe));

            CollectionAssert.AreEqual(new[] { typeof(IntScanProbe).AssemblyQualifiedName }, ints.Select(info => info.AssemblyQualifiedName));
            CollectionAssert.AreEqual(new[] { typeof(StringScanProbe).AssemblyQualifiedName }, strings.Select(info => info.AssemblyQualifiedName));
        }

        [Test]
        public void GetAllTypeInfos_Predicate_RunsOnlyOnTheCandidates_AtEveryOpening()
        {
            var baseTypes = new[] { typeof(IScanProbe) };
            var candidates = GetCandidates(typeof(IScanProbe)).Length;
            var seen = new List<Type>();

            Func<Type, bool> record = type =>
            {
                seen.Add(type);
                return true;
            };

            TypeInfo.GetAllTypeInfos(baseTypes, TypeAllow.None, record);
            TypeInfo.GetAllTypeInfos(baseTypes, TypeAllow.None, record);

            Assert.AreEqual(candidates * 2, seen.Count,
                "Each opening asks the predicate once per candidate, and never about a type the other checks reject.");
            CollectionAssert.DoesNotContain(seen, typeof(AbstractScanProbe));
            CollectionAssert.DoesNotContain(seen, typeof(HiddenScanProbe));
            CollectionAssert.DoesNotContain(seen, typeof(object));
        }

        [Test]
        public void GetCandidates_AfterAnAssemblyLoads_IncludesItsTypes()
        {
            var before = GetCandidates(typeof(object), TypeAllow.All, includeHidden: true);
            var probe = LoadHiddenCandidateAssembly();

            var after = GetCandidates(typeof(object), TypeAllow.All, includeHidden: true);

            CollectionAssert.DoesNotContain(before, probe);
            Assert.AreNotSame(before, after, "A loaded assembly must drop the cached lists.");
            CollectionAssert.Contains(after, probe, "A type of a late assembly must reach the next opening.");
            Assert.AreSame(after, GetCandidates(typeof(object), TypeAllow.All, includeHidden: true));
        }

        private static Type[] GetCandidates(
            Type baseType,
            TypeAllow allow = TypeAllow.None,
            bool includeHidden = false,
            bool excludeEditorOnly = false) =>
            TypeInfo.GetCandidates(new[] { baseType }, allow, includeHidden, excludeEditorOnly);

        // Every combination of the flags that make up a key, each asked twice: the first call may build the list and the
        // second is served from it. A predicate takes part in every combination, so all three of them get a turn.
        private static void AssertMatchesAScan(params Type[] baseTypes)
        {
            var turn = 0;
            var assignable = AssignableTypes(baseTypes);

            foreach (var allow in new[] { TypeAllow.None, TypeAllow.Abstract, TypeAllow.Interface, TypeAllow.All })
            foreach (var includeHidden in new[] { false, true })
            foreach (var excludeEditorOnly in new[] { false, true })
            {
                var filter = _filters[turn++ % _filters.Length];
                var expected = ScanEveryType(assignable, allow, filter, includeHidden, excludeEditorOnly);
                var message = $"{string.Join(", ", baseTypes.Select(type => type.Name))}: allow {allow}, " +
                              $"includeHidden {includeHidden}, excludeEditorOnly {excludeEditorOnly}, filter #{turn - 1}";

                for (var call = 1; call <= 2; call++)
                {
                    var offered = TypeInfo
                        .GetAllTypeInfos(baseTypes, allow, filter, includeHidden: includeHidden, excludeEditorOnly: excludeEditorOnly)
                        .Select(info => info.AssemblyQualifiedName);

                    CollectionAssert.AreEqual(expected, offered, $"Call {call} of {message}.");
                }
            }
        }

        // The scan GetAllTypeInfos made before it kept the candidates: every check on every loaded type, at every call.
        private static string[] ScanEveryType(
            ScannedType[] assignable,
            TypeAllow allow,
            Func<Type, bool> filter,
            bool includeHidden,
            bool excludeEditorOnly) =>
            assignable
                .Where(scanned => !scanned.IsStatic &&
                    (allow.HasFlag(TypeAllow.Abstract) || scanned.IsInterface || !scanned.IsAbstract) &&
                    (allow.HasFlag(TypeAllow.Interface) || !scanned.IsInterface) &&
                    (!excludeEditorOnly || !scanned.IsEditorOnly) &&
                    !scanned.IsCompilerGenerated &&
                    (includeHidden || !scanned.IsHidden) &&
                    (filter is null || filter(scanned.Type)))
                .Select(scanned => scanned.Type.AssemblyQualifiedName)
                .ToArray();

        // Every loaded type that derives from all the base types, in the order of TypeUtility.DomainTypes, with what the
        // checks of the scan read from it. The Editor loads tens of thousands of types and the combinations differ only
        // in how they weigh these facts, so each type is reflected on once per base types, not once per combination.
        private static ScannedType[] AssignableTypes(Type[] baseTypes) =>
            TypeUtility.DomainTypes
                .Where(type => baseTypes.All(baseType => baseType.IsAssignableFrom(type)))
                .Select(type => new ScannedType(type))
                .ToArray();

        // Defining a dynamic assembly raises AppDomain.AssemblyLoad, as loading a file does. The assembly stays loaded
        // for the Editor session, so its type opts out of the picker: only a repair picker, which asks for hidden types,
        // ever sees it.
        private static Type LoadHiddenCandidateAssembly()
        {
            var name = new AssemblyName($"Aspid.FastTools.CandidateProbe.{Guid.NewGuid():N}");
            var module = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run)
                .DefineDynamicModule(name.Name);

            var type = module.DefineType("HiddenCandidateProbe", TypeAttributes.Public);
            var hidden = typeof(TypeSelectorDisplayAttribute);

            type.SetCustomAttribute(new CustomAttributeBuilder(
                hidden.GetConstructor(Type.EmptyTypes)!,
                Array.Empty<object>(),
                new[] { hidden.GetProperty(nameof(TypeSelectorDisplayAttribute.Hidden))! },
                new object[] { true }));

            return type.CreateType();
        }
    }
}
