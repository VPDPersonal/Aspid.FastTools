using System;
using System.Linq;
using UnityEditor;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A variant, a nested prefab or a scene instance stores an overridden [SerializeReference] type in its
    // PrefabInstance document ("managedReferences[rid]" -> "<asm> <ns>.<class>"), not in a RefIds block. The scans
    // report those types as override entries. The fixtures are Unity 6000.0 output, trimmed to the relevant lines.
    [TestFixture]
    internal sealed class SerializeReferenceYamlPrefabOverrideTests
    {
        internal const long VariantInstanceFileId = 7223051798917698576L;
        internal const long HolderFileId = 6049313956132244469L;
        internal const string BaseGuid = "1d4d79cb574804cd3967d8baa572e9fd";

        internal const long BowRid = 7288618111259901954L;
        internal const long BoxRid = 7288618111259901955L;
        internal const long InnerRid = 7288618111259901956L;

        private const long SceneInstanceFileId = 1327606217L;
        private const long SceneBowRid = 7288618111259901958L;

        private const long NestedHostFileId = 4502687714025278870L;
        private const long NestedInstanceFileId = 5667020914054449524L;
        private const long NestedGhostRid = 1000L;
        private const long NestedBowRid = 7288618111259901957L;

        internal const string VariantPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1001 &7223051798917698576
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 0}
    m_Modifications:
    - target: {fileID: 2537297486090590358, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: m_LocalPosition.x
      value: 0
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: weapon
      value: 7288618111259901954
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: list.Array.size
      value: 2
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'list.Array.data[0]'
      value: 7288618111259901955
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'list.Array.data[1]'
      value: 7288618111259901956
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[7288618111259901954]'
      value: Assembly-CSharp P05.Game.Bow
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[7288618111259901955]'
      value: 'Assembly-CSharp P05.Game.Box`1[[System.Int32, mscorlib]]'
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[7288618111259901956]'
      value: Assembly-CSharp P05.Game.Outer/Inner
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: managedReferences[7288618111259901956].x
      value: 3
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: managedReferences[7288618111259901954].range
      value: 7
      objectReference: {fileID: 0}
    - target: {fileID: 7520176527715267199, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: m_Name
      value: Variant
      objectReference: {fileID: 0}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
";

        // A scene instance whose weapon was cleared (the "managedReferences[-2]" sentinel with an empty value) and
        // whose list element was given a new type.
        private const string SceneWithInstance =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!29 &1
OcclusionCullingSettings:
  m_ObjectHideFlags: 0
  serializedVersion: 2
--- !u!1001 &1327606217
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 0}
    m_Modifications:
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: weapon
      value: -2
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'list.Array.data[0]'
      value: 7288618111259901958
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[-2]'
      value:
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[7288618111259901958]'
      value: Assembly-CSharp P05.Game.Bow
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: managedReferences[7288618111259901958].range
      value: 5
      objectReference: {fileID: 0}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
--- !u!1660057539 &9223372036854775807
SceneRoots:
  m_ObjectHideFlags: 0
  m_Roots:
  - {fileID: 1327606217}
";

        // A prefab with its own component (a RefIds entry) and a nested instance of another prefab (an override).
        private const string NestedPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &4502687714025278870
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_GameObject: {fileID: 4390078645376664616}
  m_Enabled: 1
  m_Script: {fileID: 11500000, guid: 0123456789abcdef0123456789abcdef, type: 3}
  _payload:
    rid: 1000
  references:
    version: 2
    RefIds:
    - rid: 1000
      type: {class: GhostNode, ns: Game, asm: Assembly-CSharp}
      data:
        _value: 1
--- !u!1001 &5667020914054449524
PrefabInstance:
  m_ObjectHideFlags: 0
  serializedVersion: 2
  m_Modification:
    serializedVersion: 3
    m_TransformParent: {fileID: 4502687714025278869}
    m_Modifications:
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: weapon
      value: 7288618111259901957
      objectReference: {fileID: 0}
    - target: {fileID: 6049313956132244469, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[7288618111259901957]'
      value: Assembly-CSharp P05.Game.Bow
      objectReference: {fileID: 0}
    m_RemovedComponents: []
    m_RemovedGameObjects: []
    m_AddedGameObjects: []
    m_AddedComponents: []
  m_SourcePrefab: {fileID: 100100000, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
--- !u!4 &7895663238267212258 stripped
Transform:
  m_CorrespondingSourceObject: {fileID: 2537297486090590358, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
  m_PrefabInstance: {fileID: 5667020914054449524}
  m_PrefabAsset: {fileID: 0}
";

        // A long quoted value wrapped onto a deeper-indented continuation line, and an empty modification list.
        private const string WrappedValuePrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1001 &100
PrefabInstance:
  m_Modification:
    m_Modifications:
    - target: {fileID: 200, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[42]'
      value: 'Assembly-CSharp Game.Pair`2[[Game.Left, Assembly-CSharp],[Game.Right,
        Assembly-CSharp]]'
      objectReference: {fileID: 0}
    m_RemovedComponents: []
--- !u!1001 &101
PrefabInstance:
  m_Modification:
    m_Modifications: []
    m_RemovedComponents: []
";

        // An item whose target has no fileID is dropped, so its fields cannot overwrite the modification before it.
        private const string UnreadableTargetPrefab =
@"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1001 &100
PrefabInstance:
  m_Modification:
    m_Modifications:
    - target: {fileID: 200, guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[1]'
      value: Assembly-CSharp P05.Game.Bow
      objectReference: {fileID: 0}
    - target: {guid: 1d4d79cb574804cd3967d8baa572e9fd, type: 3}
      propertyPath: 'managedReferences[2]'
      value: Assembly-CSharp P05.Game.Other
      objectReference: {fileID: 0}
    m_RemovedComponents: []
";

        // Editors before Unity 6 wrap a long target onto the next line: "guid: <guid>,\n        type: 3}". All targets
        // wrapped, or only the holder's between short ones, which used to fold the wrapped items into the short one.
        private static string WrapTargets(bool all) => Regex.Replace(VariantPrefab,
            all
                ? @"(?m)^(    - target: \{fileID: -?\d+, guid: [0-9a-f]+,) type: 3\}"
                : $@"(?m)^(    - target: \{{fileID: {HolderFileId}, guid: [0-9a-f]+,) type: 3\}}",
            "$1\n        type: 3}");

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

        private string _path;

        [TearDown]
        public void TearDown() =>
            YamlFixtures.Delete(_path);

        [Test]
        public void FindPrefabOverrideReferences_Variant_ReadsEveryTypeOverride()
        {
            _path = YamlFixtures.WriteTemp(VariantPrefab);

            var overrides = SerializeReferenceYamlEditor.FindPrefabOverrideReferences(_path);

            CollectionAssert.AreEqual(new[] { BowRid, BoxRid, InnerRid }, overrides.Select(entry => entry.Rid).ToArray());
            Assert.IsTrue(overrides.All(entry => entry.FileId == VariantInstanceFileId));
            Assert.IsTrue(overrides.All(entry => entry.TargetFileId == HolderFileId && entry.TargetGuid == BaseGuid));

            AssertType(overrides[0].StoredType, "Assembly-CSharp", "P05.Game", "Bow");
            AssertType(overrides[1].StoredType, "Assembly-CSharp", "P05.Game", "Box`1[[System.Int32, mscorlib]]");
            AssertType(overrides[2].StoredType, "Assembly-CSharp", "P05.Game", "Outer/Inner");

            CollectionAssert.AreEqual(new[] { "weapon", "list.Array.data[0]", "list.Array.data[1]" },
                overrides.Select(entry => entry.FieldPath).ToArray());
        }

        [Test]
        public void FindMissingReferences_VariantOverride_ReportsOverrideEntry()
        {
            _path = YamlFixtures.WriteTemp(VariantPrefab);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, type => type.Class != "Bow");

            Assert.AreEqual(1, missing.Count, "A type set only by an override must not read as clean.");
            Assert.IsTrue(missing[0].IsOverride);
            Assert.AreEqual(VariantInstanceFileId, missing[0].FileId);
            Assert.AreEqual(BowRid, missing[0].Rid);
            Assert.AreEqual("weapon", missing[0].FieldPath);
        }

        [Test]
        public void FindMissingReferences_SceneInstance_SkipsClearedSentinel()
        {
            _path = YamlFixtures.WriteTemp(SceneWithInstance);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, _ => false);

            Assert.AreEqual(1, missing.Count);
            Assert.AreEqual(SceneInstanceFileId, missing[0].FileId);
            Assert.AreEqual(SceneBowRid, missing[0].Rid);
            Assert.AreEqual("list.Array.data[0]", missing[0].FieldPath);
            Assert.AreEqual("Bow", missing[0].StoredType.Class);
        }

        [Test]
        public void FindMissingReferences_NestedPrefab_ReportsRefIdsAndOverride()
        {
            _path = YamlFixtures.WriteTemp(NestedPrefab);

            var missing = SerializeReferenceYamlEditor.FindMissingReferences(_path, _ => false);

            Assert.AreEqual(2, missing.Count);

            var own = missing.Single(entry => !entry.IsOverride);
            Assert.AreEqual(NestedHostFileId, own.FileId);
            Assert.AreEqual(NestedGhostRid, own.Rid);

            var nested = missing.Single(entry => entry.IsOverride);
            Assert.AreEqual(NestedInstanceFileId, nested.FileId);
            Assert.AreEqual(NestedBowRid, nested.Rid);
        }

        [Test]
        public void FindPrefabOverrideReferences_WrappedQuotedValue_IsFolded()
        {
            _path = YamlFixtures.WriteTemp(WrappedValuePrefab);

            var overrides = SerializeReferenceYamlEditor.FindPrefabOverrideReferences(_path);

            Assert.AreEqual(1, overrides.Count, "The empty m_Modifications list adds nothing.");
            AssertType(overrides[0].StoredType, "Assembly-CSharp", "Game",
                "Pair`2[[Game.Left, Assembly-CSharp],[Game.Right, Assembly-CSharp]]");
            Assert.AreEqual(string.Empty, overrides[0].FieldPath, "No modification points at rid 42.");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FindPrefabOverrideReferences_WrappedTargets_ReadLikeShortOnes(bool wrapAll)
        {
            var yaml = WrapTargets(wrapAll);
            StringAssert.Contains(",\n        type: 3}", yaml, "The fixture must actually wrap.");
            _path = YamlFixtures.WriteTemp(yaml);

            var overrides = SerializeReferenceYamlEditor.FindPrefabOverrideReferences(_path);

            CollectionAssert.AreEqual(new[] { BowRid, BoxRid, InnerRid }, overrides.Select(entry => entry.Rid).ToArray());
            Assert.IsTrue(overrides.All(entry => entry.TargetFileId == HolderFileId && entry.TargetGuid == BaseGuid));
            CollectionAssert.AreEqual(new[] { "weapon", "list.Array.data[0]", "list.Array.data[1]" },
                overrides.Select(entry => entry.FieldPath).ToArray());
        }

        [Test]
        public void FindPrefabOverrideReferences_UnreadableTarget_DoesNotOverwritePrevious()
        {
            _path = YamlFixtures.WriteTemp(UnreadableTargetPrefab);

            var overrides = SerializeReferenceYamlEditor.FindPrefabOverrideReferences(_path);

            Assert.AreEqual(1, overrides.Count);
            Assert.AreEqual(1L, overrides[0].Rid);
            Assert.AreEqual(200L, overrides[0].TargetFileId);
            Assert.AreEqual("Bow", overrides[0].StoredType.Class);
        }

        [TestCase("Assembly-CSharp Bow", "Assembly-CSharp", "", "Bow")]
        [TestCase("Game Ns.Sub.Outer/Inner", "Game", "Ns.Sub", "Outer/Inner")]
        [TestCase("Game Ns.Box`1[[Other.Ns.T, Other]]", "Game", "Ns", "Box`1[[Other.Ns.T, Other]]")]
        public void TryParseOverrideTypeValue_SplitsLikeRefIdsType(string value, string assembly, string ns, string className)
        {
            Assert.IsTrue(SerializeReferenceYamlEditor.TryParseOverrideTypeValue(value, out var type));
            AssertType(type, assembly, ns, className);
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("Assembly-CSharp")]
        public void TryParseOverrideTypeValue_NoTypeName_ReturnsFalse(string value) =>
            Assert.IsFalse(SerializeReferenceYamlEditor.TryParseOverrideTypeValue(value, out _));

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

        private static void AssertType(ManagedTypeName type, string assembly, string ns, string className)
        {
            Assert.AreEqual(assembly, type.Assembly);
            Assert.AreEqual(ns, type.Namespace);
            Assert.AreEqual(className, type.Class);
        }
    }
}
