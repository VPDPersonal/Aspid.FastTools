using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceDeleteGuard"/>: which types a deleted script takes with it, and how
    /// their usages are counted without and with a warm <see cref="SerializeReferenceTypeUsageIndex"/>.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceDeleteGuardTests
    {
        // GUIDs from the fixture scripts' .meta files, so the lookup does not depend on where the package is installed.
        private const string FixturesScriptGuid = "e6234341c7874a99b575757146d497c8";
        private const string SettingsScriptGuid = "16332bb6fc28429f8b7968f66638aadd";
        private const string GenericScriptGuid = "0d4aeac1171c40c59cd879d9b181d89a";
        private const string NamesakeScriptGuid = "5b0e3c7a9d2f4e18a6c1b7d3e9f02a41";
        private const string PartialScriptGuid = "8c2f4a61e7b94d0fa3e5c9b1d6a7f203";
        private const string PartialPartScriptGuid = "a4d91e7c3b6f482e9c0a5f1b7e2d8c64";
        private const string ProbeAssetPath = "Assets/__AspidDeleteGuardProbe__.asset";

        // The asset name is serialized as m_Name, so the text names DeleteGuardPistol although it holds no reference.
        private const string PlainAssetPath = "Assets/__AspidDeleteGuardPlain_DeleteGuardPistol__.asset";

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);
            AssetDatabase.DeleteAsset(PlainAssetPath);
            SerializeReferenceTypeUsageIndex.Reset();
            SerializeReferenceDeleteGuard.ResetSweep();
        }

        [Test]
        public void ResolveCandidateTypes_FileWithoutClassNamedAfterIt_ReturnsEveryDeclaredReferenceType()
        {
            var types = Resolve(FixturesScriptGuid);

            // DeleteGuardPistol<T> of another script shares the name, not the arity, and the global DeleteGuardPistol
            // the name, not the namespace, so both stay out.
            CollectionAssert.AreEquivalent(
                new[] { typeof(DeleteGuardPistol), typeof(DeleteGuardRifle), typeof(DeleteGuardArmory.Crate) },
                types);
        }

        [Test]
        public void ResolveCandidateTypes_GenericTypeInNestedNamespaceBlocks_ReturnsOnlyThatType()
        {
            var types = Resolve(GenericScriptGuid);

            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardPistol<>) }, types);
        }

        [Test]
        public void ResolveCandidateTypes_ScriptWithOnlyUnityObjectTypes_ReturnsNothing()
        {
            // The script names other fixture types in a comment and a string, which declare nothing.
            var types = Resolve(SettingsScriptGuid);

            CollectionAssert.IsEmpty(types);
        }

        [Test]
        public void ResolveCandidateTypes_GlobalAndUnicodeNamedTypes_ReturnsThemWithoutNamespacedNamesake()
        {
            var types = Resolve(NamesakeScriptGuid);

            CollectionAssert.AreEquivalent(new[] { typeof(global::DeleteGuardPistol), typeof(ОружиеDeleteGuard) }, types);
        }

        [Test]
        public void ResolveCandidateTypes_PartialTypeWithAnotherPart_IsKeptOnlyWhenEveryPartIsDeleted()
        {
            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardRevolver) }, Resolve(PartialScriptGuid));
            CollectionAssert.IsEmpty(Resolve(PartialPartScriptGuid));
            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardRevolver), typeof(DeleteGuardShotgun) },
                Resolve(PartialScriptGuid, PartialPartScriptGuid));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CountUsages_CountsSiblingAndNestedTypes(bool warmIndex)
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            if (warmIndex) SerializeReferenceTypeUsageIndex.FindUsages("warm-up");

            Assert.AreEqual(warmIndex, SerializeReferenceTypeUsageIndex.IsWarm);

            var types = Resolve(FixturesScriptGuid);
            var samples = new List<string>();
            var counts = SerializeReferenceDeleteGuard.CountUsages(types, samples);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
            Assert.AreEqual(1, counts[typeof(DeleteGuardArmory.Crate)]);
            Assert.AreEqual(0, counts[typeof(DeleteGuardRifle)]);
            CollectionAssert.AreEqual(new[] { ProbeAssetPath }, samples);
        }

        [Test]
        public void CountUsages_AssetNamingClassWithoutManagedReferences_IsNotCounted()
        {
            CreatePlainAsset();

            var counts = SerializeReferenceDeleteGuard.CountUsages(new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null);

            Assert.AreEqual(0, counts[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void CountUsages_ReusedSweep_SecondCallReadsOnlyAssetsThatHoldManagedReferences()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            CreatePlainAsset();

            var first = SerializeReferenceDeleteGuard.CountUsages(
                new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null, reuseSweep: true);

            Assert.AreEqual(1, first[typeof(DeleteGuardPistol)]);

            var swept = SerializeReferenceDeleteGuard.SweptReferencePaths;
            CollectionAssert.Contains(swept, ProbeAssetPath);
            CollectionAssert.DoesNotContain(swept, PlainAssetPath);

            var samples = new List<string>();
            var second = SerializeReferenceDeleteGuard.CountUsages(
                new List<Type> { typeof(DeleteGuardArmory.Crate), typeof(DeleteGuardRifle) }, samples, reuseSweep: true);

            Assert.AreEqual(1, second[typeof(DeleteGuardArmory.Crate)]);
            Assert.AreEqual(0, second[typeof(DeleteGuardRifle)]);
            CollectionAssert.AreEqual(new[] { ProbeAssetPath }, samples);
            Assert.AreSame(swept, SerializeReferenceDeleteGuard.SweptReferencePaths, "The second call must not sweep the project again.");

            SerializeReferenceDeleteGuard.ResetSweep();
            Assert.IsNull(SerializeReferenceDeleteGuard.SweptReferencePaths);
        }

        [Test]
        public void CountUsages_WithoutReusedSweep_KeepsNothing()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());

            var counts = SerializeReferenceDeleteGuard.CountUsages(new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null);

            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
            Assert.IsNull(SerializeReferenceDeleteGuard.SweptReferencePaths);
        }

        [Test]
        public void CountUsages_ReusedSweep_AssetSavedAfterSweep_IsCounted()
        {
            var types = new List<Type> { typeof(DeleteGuardPistol) };

            var first = SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null, reuseSweep: true);

            Assert.AreEqual(0, first[typeof(DeleteGuardPistol)]);
            Assert.IsNotNull(SerializeReferenceDeleteGuard.SweptReferencePaths);

            // The import runs the postprocessor, which drops the kept list, so the next call sweeps again.
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());

            var second = SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null, reuseSweep: true);

            Assert.AreEqual(1, second[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void CountUsages_CancelledSweep_CancelsLaterCallsThatReuseIt()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            SerializeReferenceDeleteGuard.KeepSweepUntilTickEnds(cancelled: true);

            var types = new List<Type> { typeof(DeleteGuardPistol) };

            Assert.IsNull(SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null, reuseSweep: true));

            // A call without reuseSweep is stateless and ignores the flag.
            var counts = SerializeReferenceDeleteGuard.CountUsages(types, samplePaths: null);

            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
        }

        [Test]
        public void CountUsages_CancelledSweepAndWarmIndex_StillCounts()
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            SerializeReferenceTypeUsageIndex.FindUsages("warm-up");
            SerializeReferenceDeleteGuard.KeepSweepUntilTickEnds(cancelled: true);

            var counts = SerializeReferenceDeleteGuard.CountUsages(
                new List<Type> { typeof(DeleteGuardPistol) }, samplePaths: null, reuseSweep: true);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
        }

        private static List<Type> Resolve(params string[] scriptGuids) =>
            SerializeReferenceDeleteGuard.ResolveCandidateTypes(scriptGuids.Select(AssetDatabase.GUIDToAssetPath).ToList());

        private static void CreatePlainAsset()
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DeleteGuardSettingsFixture>(), PlainAssetPath);
            AssetDatabase.SaveAssets();

            SerializeReferenceTypeUsageIndex.Reset();
        }

        private static void CreateProbe(ITestWeapon a, ITestWeapon b)
        {
            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = a;
            probe.b = b;

            AssetDatabase.CreateAsset(probe, ProbeAssetPath);
            AssetDatabase.SaveAssets();

            // Creating the asset patches a warm index in place; each case starts from a cold one.
            SerializeReferenceTypeUsageIndex.Reset();
        }
    }
}
