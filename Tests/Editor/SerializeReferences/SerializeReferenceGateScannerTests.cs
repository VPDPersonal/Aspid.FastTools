using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceGateScanner.IsPendingMigration"/> — the gate's pre-filter that keeps
    /// properly declared renames from ever warning or failing a build / CI run — and for <see cref="SerializeReferenceGateScanner.Scan"/>
    /// itself surfacing unset required fields (the data source for the Project References "Required violations" group).
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceGateScannerTests
    {
        private const string ProbeAssetPath = "Assets/__AspidGateScannerRequiredProbe__.asset";

        // Scan(RequiredOnly) is the exact call the Project References "Required violations" group makes; this proves
        // it surfaces both an unset managed reference and an unset [TypeSelector(Required = true)] string field on a
        // saved asset (RequiredTestObject, shared fixture — see SerializeReferenceTestFixtures.cs).
        [Test]
        public void Scan_RequiredOnly_SurfacesUnsetRequiredFieldsOnSavedAsset()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly);
                var forProbe = violations.Where(v => v.AssetPath == ProbeAssetPath && v.FileId == fileId).ToList();

                Assert.IsTrue(forProbe.All(v => v.Kind == GateViolationKind.RequiredUnset));
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "Unset [SerializeReference, TypeSelector(Required = true)] field must be reported.");
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredString)),
                    "Unset [TypeSelector(Required = true)] string field must be reported.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // ScanAssetRequiredFields is the scoped, single-asset entry point the Inspect Asset graph calls on every
        // Rescan; it must agree with Scan(RequiredOnly) for that one asset without sweeping the whole project.
        [Test]
        public void ScanAssetRequiredFields_SavedAsset_MatchesProjectScan()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.ScanAssetRequiredFields(ProbeAssetPath);
                var forProbe = violations.Where(v => v.FileId == fileId).ToList();

                Assert.IsTrue(forProbe.All(v => v.Kind == GateViolationKind.RequiredUnset));
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "Unset [SerializeReference, TypeSelector(Required = true)] field must be reported.");
                Assert.IsTrue(forProbe.Any(v => v.FieldPath == nameof(RequiredTestObject.requiredString)),
                    "Unset [TypeSelector(Required = true)] string field must be reported.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // A project sweep loads every prefab and asset it audits; it must release them, or a large project ends up
        // holding all of its content in memory at once.
        [Test]
        public void Scan_RequiredOnly_UnloadsAssetsItLoaded()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                Resources.UnloadAsset(probe);
                Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(ProbeAssetPath), Is.False);

                SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly);

                Assert.IsFalse(AssetDatabase.IsMainAssetAtPathLoaded(ProbeAssetPath),
                    "An asset the sweep loaded for its audit must not stay loaded after the scan.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // The sweep must also unload during the loop, not only at its end, or its peak memory still grows with the
        // project. With a batch of one file, the first probe is released before the sweep reaches the second.
        [Test]
        public void Scan_RequiredOnly_UnloadsLoadedAssetsDuringTheSweep()
        {
            const string secondProbePath = "Assets/__AspidGateScannerRequiredProbe2__.asset";

            var batchSize = SerializeReferenceGateScanner.UnloadEveryLoadedFiles;
            var probes = new[] { ProbeAssetPath, secondProbePath };
            try
            {
                foreach (var path in probes)
                {
                    var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
                    AssetDatabase.CreateAsset(probe, path);
                    Resources.UnloadAsset(probe);
                    Assume.That(AssetDatabase.IsMainAssetAtPathLoaded(path), Is.False);
                }

                SerializeReferenceGateScanner.UnloadEveryLoadedFiles = 1;

                string firstVisited = null;
                bool? firstLoadedAtSecond = null;

                SerializeReferenceGateScanner.Scan(GateOptions.RequiredOnly, (_, path) =>
                {
                    if (!probes.Contains(path)) return;

                    if (firstVisited is null) firstVisited = path;
                    else firstLoadedAtSecond = AssetDatabase.IsMainAssetAtPathLoaded(firstVisited);
                });

                Assert.AreEqual(false, firstLoadedAtSecond,
                    "An asset loaded earlier in the sweep must be unloaded before the sweep ends.");
            }
            finally
            {
                SerializeReferenceGateScanner.UnloadEveryLoadedFiles = batchSize;
                foreach (var path in probes) AssetDatabase.DeleteAsset(path);
            }
        }

        // A prefab variant or a prefab nesting an edited prefab inherits the edit, so a rescan after a bulk edit of a
        // prefab must re-audit them too; an unrelated prefab keeps its cached entries.
        [Test]
        public void RescanRequiredFields_EditedPrefab_AlsoRescansItsVariantsAndNestingPrefabs()
        {
            const string basePath = "Assets/__AspidGateScannerBase__.prefab";
            const string variantPath = "Assets/__AspidGateScannerVariant__.prefab";
            const string nestingPath = "Assets/__AspidGateScannerNesting__.prefab";
            const string unrelatedPath = "Assets/__AspidGateScannerUnrelated__.prefab";

            var temporary = new List<GameObject>();
            try
            {
                var baseRoot = new GameObject("Base");
                temporary.Add(baseRoot);
                var basePrefab = PrefabUtility.SaveAsPrefabAsset(baseRoot, basePath);

                var variantRoot = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                temporary.Add(variantRoot);
                PrefabUtility.SaveAsPrefabAsset(variantRoot, variantPath);

                var nestingRoot = new GameObject("Nesting");
                temporary.Add(nestingRoot);
                ((GameObject)PrefabUtility.InstantiatePrefab(basePrefab)).transform.SetParent(nestingRoot.transform);
                PrefabUtility.SaveAsPrefabAsset(nestingRoot, nestingPath);

                var unrelatedRoot = new GameObject("Unrelated");
                temporary.Add(unrelatedRoot);
                PrefabUtility.SaveAsPrefabAsset(unrelatedRoot, unrelatedPath);

                var cached = new[]
                {
                    new GateViolation(variantPath, 1, 0, default, GateViolationKind.RequiredUnset, "stale"),
                    new GateViolation(nestingPath, 1, 0, default, GateViolationKind.RequiredUnset, "stale"),
                    new GateViolation(unrelatedPath, 1, 0, default, GateViolationKind.RequiredUnset, "kept"),
                };

                var refreshed = SerializeReferenceGateScanner.RescanRequiredFields(cached, new[] { basePath });

                Assert.IsFalse(refreshed.Any(v => v.FieldPath == "stale"),
                    "A variant or a nesting prefab of the edited prefab must be re-audited.");
                Assert.IsTrue(refreshed.Any(v => v.AssetPath == unrelatedPath && v.FieldPath == "kept"),
                    "An unrelated prefab keeps its cached violations.");
            }
            finally
            {
                foreach (var gameObject in temporary) Object.DestroyImmediate(gameObject);
                foreach (var path in new[] { nestingPath, variantPath, unrelatedPath, basePath }) AssetDatabase.DeleteAsset(path);
            }
        }

        // After a bulk edit the Project References window re-audits only the rewritten files; the cached entries of
        // those files are replaced and every other file's are kept.
        [Test]
        public void RescanRequiredFields_ReplacesOnlyTheRescannedFiles()
        {
            const string otherPath = "Assets/__AspidGateScannerUntouched__.asset";

            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);

                var cached = new[]
                {
                    new GateViolation(ProbeAssetPath, 1, 0, default, GateViolationKind.RequiredUnset, "stale"),
                    new GateViolation(otherPath, 1, 0, default, GateViolationKind.RequiredUnset, "kept"),
                };

                var refreshed = SerializeReferenceGateScanner.RescanRequiredFields(cached, new[] { ProbeAssetPath });

                Assert.IsTrue(refreshed.Any(v => v.AssetPath == otherPath && v.FieldPath == "kept"),
                    "A file outside the rescan keeps its cached violations.");
                Assert.IsFalse(refreshed.Any(v => v.FieldPath == "stale"), "A rescanned file drops its cached entries.");
                Assert.IsTrue(refreshed.Any(v => v.AssetPath == ProbeAssetPath && v.FieldPath == nameof(RequiredTestObject.requiredRef)),
                    "A rescanned file reports its current violations.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        [Test]
        public void ScanAssetRequiredFields_NonCandidatePath_ReturnsEmpty()
        {
            Assert.AreEqual(0, SerializeReferenceGateScanner.ScanAssetRequiredFields("Assets/Fake.txt").Count);
        }

        // The Inspect Asset graph (SerializeReferenceGraphView) badges an empty [SerializeReference] slot as REQUIRED
        // by matching its graph field path — built independently by SerializeReferenceGraphScanner straight from
        // YAML — against this scan's GateViolation.FieldPath (a live SerializedProperty.propertyPath), after the same
        // "[i]" -> ".Array.data[i]" normalization the view applies. This proves the two paths actually agree for a
        // real saved asset, not just that each individually finds the field.
        [Test]
        public void ScanAssetRequiredFields_UnsetManagedReference_MatchesGraphScannerEmptyRootPath()
        {
            var probe = ScriptableObject.CreateInstance<RequiredTestObject>();
            try
            {
                AssetDatabase.CreateAsset(probe, ProbeAssetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(probe, out _, out long fileId);

                var violations = SerializeReferenceGateScanner.ScanAssetRequiredFields(ProbeAssetPath);
                var document = SerializeReferenceGraphScanner.Build(ProbeAssetPath).Single(doc => doc.FileId == fileId);
                var emptyRoot = document.Roots.Single(root => root.IsEmpty);

                var normalizedGraphPath = Regex.Replace(emptyRoot.Label, @"\[(\d+)\]", ".Array.data[$1]");

                Assert.IsTrue(
                    violations.Any(v => v.FileId == fileId && v.FieldPath == normalizedGraphPath),
                    $"Normalized graph path '{normalizedGraphPath}' must match a ScanAssetRequiredFields violation's FieldPath.");
            }
            finally
            {
                AssetDatabase.DeleteAsset(ProbeAssetPath);
            }
        }

        // A [MovedFrom]-claimed stale name is a pending migration, not a violation: Unity migrates the reference in
        // memory at load, so the gate must accept it — a properly declared rename can never warn or fail a build /
        // CI run. A scene path exercises the trust-the-claim branch (constraints are unrecoverable for scenes).
        // (RenamedRanged is the shared [MovedFrom(..., "OldRenamedRanged")] fixture.)
        [Test]
        public void IsPendingMigration_MovedFromClaimedName_IsNotAViolation()
        {
            var stored = new ManagedTypeName(typeof(RenamedRanged).Assembly.GetName().Name, typeof(RenamedRanged).Namespace, "OldRenamedRanged");
            var entry = new MissingReferenceEntry(fileId: 1, rid: 100, stored);

            Assert.IsTrue(SerializeReferenceGateScanner.IsPendingMigration("Assets/Fake.unity", entry));
        }

        [Test]
        public void IsPendingMigration_UnknownName_StaysAViolation()
        {
            var stored = new ManagedTypeName(
                typeof(RenamedRanged).Assembly.GetName().Name, typeof(RenamedRanged).Namespace, "GhostNeverExisted");
            var entry = new MissingReferenceEntry(fileId: 1, rid: 100, stored);

            Assert.IsFalse(SerializeReferenceGateScanner.IsPendingMigration("Assets/Fake.unity", entry));
        }
    }
}
