using System;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The tests of this fixture that need Unity or the package's Editor assembly. Aspid.FastTools.YamlTests runs
    // the rest without Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class SerializeReferenceYamlPrefabOverrideTests
    {
        // Two components of one instance that override the same (legacy 1000-based) rid; each gets its own type.
        private static string TwoTargetsSameRid(string firstType, string secondType) =>
$@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1001 &100
PrefabInstance:
  m_Modification:
    m_Modifications:
    - target: {{fileID: 200, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}}
      propertyPath: 'managedReferences[1000]'
      value: {firstType}
      objectReference: {{fileID: 0}}
    - target: {{fileID: 201, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}}
      propertyPath: 'managedReferences[1000]'
      value: {secondType}
      objectReference: {{fileID: 0}}
    m_RemovedComponents: []
";

        [Test]
        public void TypeUsageIndex_VariantOverride_IsIndexedAsOverride()
        {
            _path = YamlFixtures.WriteTemp(VariantPrefab);

            WithSeededIndex(_path, "variant", () =>
            {
                var bow = SerializeReferenceTypeUsageIndex.FindUsages(
                    SerializeReferenceHelpers.StoredTypeKey(new ManagedTypeName("Assembly-CSharp", "P05.Game", "Bow")));

                Assert.AreEqual(1, bow.Count, "The delete guard and Project References read the type from here.");
                var usage = bow.Single();
                Assert.IsTrue(usage.IsOverride);
                Assert.IsFalse(usage.Resolves);
                Assert.AreEqual(VariantInstanceFileId, usage.FileId);
                Assert.AreEqual(BowRid, usage.Rid);

                Assert.AreEqual(3, SerializeReferenceTypeUsageIndex.EnumerateUnresolved().Count(entry => entry.IsOverride));
            });
        }

        [Test]
        public void TypeUsageIndex_SameRidOnTwoTargets_CountsBoth()
        {
            _path = YamlFixtures.WriteTemp(TwoTargetsSameRid("Assembly-CSharp P05.Game.Bow", "Assembly-CSharp P05.Game.Bow"));

            WithSeededIndex(_path, "shared", () =>
            {
                var bow = SerializeReferenceTypeUsageIndex.FindUsages(
                    SerializeReferenceHelpers.StoredTypeKey(new ManagedTypeName("Assembly-CSharp", "P05.Game", "Bow")));

                Assert.AreEqual(2, bow.Count, "Each overridden component is its own usage.");
                CollectionAssert.AreEquivalent(new[] { 200L, 201L }, bow.Select(usage => usage.TargetFileId).ToArray());
            });
        }

        // The gate treats a [MovedFrom]-claimed override as a pending migration, and Migrate all does not rewrite it,
        // so Project References lists it with its target. (RenamedRanged is the shared [MovedFrom(..., "OldRenamedRanged")]
        // fixture.)
        [Test]
        public void CollectOverridesFromIndex_MovedFromClaimedName_IsListedAsPendingMigration()
        {
            var renamed = $"{typeof(RenamedRanged).Assembly.GetName().Name} {typeof(RenamedRanged).Namespace}.OldRenamedRanged";
            _path = YamlFixtures.WriteTemp(TwoTargetsSameRid(renamed, "Assembly-CSharp P05.Game.Bow"));

            // CollectOverridesFromIndex maps the guid back to a path, so it must be a real asset's.
            var guid = AssetDatabase.AssetPathToGUID("Packages/tech.aspid.fasttools/package.json");
            Assert.IsNotEmpty(guid);

            WithSeededIndex(_path, guid, () =>
            {
                var overrides = MissingReferenceGroup.CollectOverridesFromIndex();
                Assert.AreEqual(2, overrides.Count);

                var renamedEntry = overrides.Single(entry => entry.Entry.StoredType.Class == "OldRenamedRanged");
                var bowEntry = overrides.Single(entry => entry.Entry.StoredType.Class == "Bow");
                Assert.AreEqual(typeof(RenamedRanged), MissingReferenceGroup.OverrideMigrationTarget(renamedEntry));
                Assert.IsNull(MissingReferenceGroup.OverrideMigrationTarget(bowEntry));
            });
        }

        // Seeds the index with this one file instead of warming it over the whole project.
        private static void WithSeededIndex(string path, string guid, Action body)
        {
            var indexField = typeof(SerializeReferenceTypeUsageIndex).GetField("_index", BindingFlags.NonPublic | BindingFlags.Static);
            var addAsset = typeof(SerializeReferenceTypeUsageIndex).GetMethod("AddAsset", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(indexField);
            Assert.IsNotNull(addAsset);

            var previous = indexField.GetValue(null);
            try
            {
                indexField.SetValue(null, new Dictionary<string, HashSet<SerializeReferenceTypeUsageIndex.Usage>>(StringComparer.Ordinal));
                addAsset.Invoke(null, new object[] { path, guid });
                body();
            }
            finally
            {
                indexField.SetValue(null, previous);
            }
        }

        [Test]
        public void SearchItemId_SameRidOnTwoTargets_Differs()
        {
            var bow = new ManagedTypeName("Assembly-CSharp", "P05.Game", "Bow");
            var first = new SerializeReferenceTypeUsageIndex.Usage("guid", 100, 1000, false, bow, isOverride: true, 200, BaseGuid);
            var second = new SerializeReferenceTypeUsageIndex.Usage("guid", 100, 1000, false, bow, isOverride: true, 201, BaseGuid);

            Assert.AreNotEqual(SerializeReferenceUsageSearchProvider.ItemId(first), SerializeReferenceUsageSearchProvider.ItemId(second),
                "Unity Search dedupes by id, so one of the two usages would vanish.");
            Assert.AreEqual("guid:100:1000",
                SerializeReferenceUsageSearchProvider.ItemId(new SerializeReferenceTypeUsageIndex.Usage("guid", 100, 1000, false, bow)));
        }

        [Test]
        public void CiReport_Override_IsMarkedInOriginColumn()
        {
            var bow = new ManagedTypeName("Assembly-CSharp", "P05.Game", "Bow");
            var report = SerializeReferenceCiGate.BuildReport(new[]
            {
                new GateViolation("Assets/Variant.prefab", VariantInstanceFileId, BowRid, bow, GateViolationKind.MissingType,
                    "weapon", isOverride: true),
                new GateViolation("Assets/Base.prefab", HolderFileId, BowRid, bow, GateViolationKind.MissingType, string.Empty),
            });

            var rows = report.Split('\n').Select(line => line.TrimEnd('\r')).Where(line => line.StartsWith("MissingType")).ToArray();
            Assert.AreEqual(2, rows.Length);
            Assert.AreEqual($"MissingType\tAssets/Variant.prefab\t{VariantInstanceFileId}\t{BowRid}\tBow\tweapon\toverride", rows[0]);
            Assert.AreEqual($"MissingType\tAssets/Base.prefab\t{HolderFileId}\t{BowRid}\tBow\t\t", rows[1]);
        }

        [Test]
        public void GateViolation_Override_NamesFieldAndOverride()
        {
            var violation = new GateViolation("Assets/Variant.prefab", VariantInstanceFileId, BowRid,
                new ManagedTypeName("Assembly-CSharp", "P05.Game", "Bow"), GateViolationKind.MissingType, "weapon",
                isOverride: true);

            Assert.AreEqual("Assets/Variant.prefab : weapon -> missing type Bow (prefab instance override)", violation.ToString());
        }
    }
}
