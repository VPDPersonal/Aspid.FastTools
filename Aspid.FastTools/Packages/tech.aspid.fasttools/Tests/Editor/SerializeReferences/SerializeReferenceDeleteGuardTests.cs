using System;
using System.IO;
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

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);
            SerializeReferenceTypeUsageIndex.Reset();
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

        // Unity writes the Cyrillic class name as escapes, so the file never holds the name as written.
        [TestCase(false)]
        [TestCase(true)]
        public void CountUsages_CountsTypeWithNonAsciiName(bool warmIndex)
        {
            CreateProbe(new ОружиеDeleteGuard(), b: null);
            if (warmIndex) SerializeReferenceTypeUsageIndex.FindUsages("warm-up");

            StringAssert.DoesNotContain("ОружиеDeleteGuard", File.ReadAllText(ProbeAssetPath));

            var counts = SerializeReferenceDeleteGuard.CountUsages(Resolve(NamesakeScriptGuid), samplePaths: null);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(ОружиеDeleteGuard)]);
        }

        private static List<Type> Resolve(params string[] scriptGuids) =>
            SerializeReferenceDeleteGuard.ResolveCandidateTypes(scriptGuids.Select(AssetDatabase.GUIDToAssetPath).ToList());

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
