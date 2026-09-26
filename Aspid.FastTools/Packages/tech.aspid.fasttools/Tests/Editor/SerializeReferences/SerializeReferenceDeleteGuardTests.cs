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
            var types = SerializeReferenceDeleteGuard.ResolveCandidateTypes(AssetDatabase.GUIDToAssetPath(FixturesScriptGuid));

            // DeleteGuardPistol<T> of another script shares the name, not the arity, so it stays out.
            CollectionAssert.AreEquivalent(
                new[] { typeof(DeleteGuardPistol), typeof(DeleteGuardRifle), typeof(DeleteGuardArmory.Crate) },
                types);
        }

        [Test]
        public void ResolveCandidateTypes_GenericTypeInNestedNamespaceBlocks_ReturnsOnlyThatType()
        {
            var types = SerializeReferenceDeleteGuard.ResolveCandidateTypes(AssetDatabase.GUIDToAssetPath(GenericScriptGuid));

            CollectionAssert.AreEquivalent(new[] { typeof(DeleteGuardPistol<>) }, types);
        }

        [Test]
        public void ResolveCandidateTypes_ScriptWithOnlyUnityObjectTypes_ReturnsNothing()
        {
            // The script names other fixture types in a comment and a string, which declare nothing.
            var types = SerializeReferenceDeleteGuard.ResolveCandidateTypes(AssetDatabase.GUIDToAssetPath(SettingsScriptGuid));

            CollectionAssert.IsEmpty(types);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CountUsages_CountsSiblingAndNestedTypes(bool warmIndex)
        {
            CreateProbe(new DeleteGuardPistol(), new DeleteGuardArmory.Crate());
            if (warmIndex) SerializeReferenceTypeUsageIndex.FindUsages("warm-up");

            Assert.AreEqual(warmIndex, SerializeReferenceTypeUsageIndex.IsWarm);

            var types = SerializeReferenceDeleteGuard.ResolveCandidateTypes(AssetDatabase.GUIDToAssetPath(FixturesScriptGuid));
            var samples = new List<string>();
            var counts = SerializeReferenceDeleteGuard.CountUsages(types, samples);

            Assert.IsNotNull(counts);
            Assert.AreEqual(1, counts[typeof(DeleteGuardPistol)]);
            Assert.AreEqual(1, counts[typeof(DeleteGuardArmory.Crate)]);
            Assert.AreEqual(0, counts[typeof(DeleteGuardRifle)]);
            CollectionAssert.AreEqual(new[] { ProbeAssetPath }, samples);
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
