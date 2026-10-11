using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using static Aspid.FastTools.SerializeReferences.Editors.Tests.SerializeReferenceYamlPrefabOverrideTests;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // The call sites that feed prefab instance overrides into the gate, the delete guard and the breakage detector.
    // Each test imports the variant fixture under Assets/, where the project sweeps read it, on top of a real base
    // prefab (a variant whose parent is missing fails its import).
    [TestFixture]
    internal sealed class SerializeReferencePrefabOverrideWiringTests
    {
        private const string ProbePath = "Assets/__AspidPrefabOverrideWiringProbe__.prefab";
        private const string BasePath = "Assets/__AspidPrefabOverrideWiringBase__.prefab";

        private static readonly ManagedTypeName _bow = new("Assembly-CSharp", "P05.Game", "Bow");

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbePath);
            AssetDatabase.DeleteAsset(BasePath);
        }

        [Test]
        public void GateScan_VariantOverride_ReportsOverrideAndSkipsMovedFromClaim()
        {
            ImportProbe();

            var violations = SerializeReferenceGateScanner.Scan(GateOptions.MissingOnly)
                .Where(violation => violation.AssetPath == ProbePath)
                .ToList();

            Assert.AreEqual(1, violations.Count, "Bow is missing; OldRenamedRanged is a pending migration; the probe type resolves.");
            Assert.IsTrue(violations[0].IsOverride);
            Assert.AreEqual("weapon", violations[0].FieldPath);
            Assert.AreEqual(VariantInstanceFileId, violations[0].FileId);
            Assert.AreEqual(BowRid, violations[0].Rid);
        }

        [Test]
        public void DeleteGuard_ColdIndex_CountsOverrideUsage()
        {
            ImportProbe();

            WithColdIndex(() =>
            {
                var sample = new List<string>();
                var counts = SerializeReferenceDeleteGuard.CountUsages(new List<Type> { typeof(OverrideWiringProbe) }, sample);
                Assert.AreEqual(1, counts[typeof(OverrideWiringProbe)], "The variant holds no RefIds block, only the override.");
                CollectionAssert.Contains(sample, ProbePath);
            });
        }

        [Test]
        public void BreakageBaseline_ColdSweep_KeepsOverrideType()
        {
            ImportProbe();

            var established = SessionState.GetBool(SerializeReferenceBreakageDetector.EstablishedKey, false);
            var baseline = SerializeReferenceBreakageDetector.ExportBaseline();
            var enabled = SerializeReferenceSettings.BreakageDetectionEnabled;
            try
            {
                SerializeReferenceSettings.BreakageDetectionEnabled = true;
                SerializeReferenceBreakageDetector.ResetForTests();
                SessionState.EraseBool(SerializeReferenceBreakageDetector.EstablishedKey);
                SerializeReferenceBreakageDetector.ImportBaseline(string.Empty);

                WithColdIndex(() =>
                {
                    SerializeReferenceBreakageDetector.Scan();
                    SerializeReferenceBreakageDetector.CompleteSweep();
                });

                var key = SerializeReferenceHelpers.StoredTypeKey(ManagedTypeName.FromType(typeof(OverrideWiringProbe)));
                CollectionAssert.Contains(SerializeReferenceBreakageDetector.GetBaselineKeys(ProbePath), key);
            }
            finally
            {
                SerializeReferenceBreakageDetector.ResetForTests();
                SerializeReferenceSettings.BreakageDetectionEnabled = enabled;
                SessionState.SetBool(SerializeReferenceBreakageDetector.EstablishedKey, established);
                SerializeReferenceBreakageDetector.ImportBaseline(baseline);
            }
        }

        [Test]
        public void BreakageReport_OverrideEntry_IsNotRepairable()
        {
            var guid = ImportProbe();

            var unresolved = SerializeReferenceTypeUsageIndex.CollectUsages(ProbePath, guid)
                .Where(usage => !usage.Resolves)
                .ToList();
            // A RefIds entry of the same broken type in the same asset stays repairable.
            unresolved.Add(new SerializeReferenceTypeUsageIndex.Usage(guid, HolderFileId, 1, resolves: false, _bow));

            var baseline = new HashSet<string>(StringComparer.Ordinal) { SerializeReferenceHelpers.StoredTypeKey(_bow) };
            var report = SerializeReferenceBreakageDetector.BuildReport(unresolved, baseline);

            Assert.AreEqual(2, report.Entries.Count);
            Assert.IsFalse(report.Entries.Single(entry => entry.FileId == VariantInstanceFileId).IsRepairable);
            Assert.IsTrue(report.Entries.Single(entry => entry.FileId == HolderFileId).IsRepairable);
        }

        // The variant with its Box override renamed to a [MovedFrom]-claimed name and its Inner override set to a type
        // no other asset uses. (RenamedRanged is the shared [MovedFrom(..., "OldRenamedRanged")] fixture.)
        private static string ImportProbe()
        {
            var root = new GameObject("Base");
            try
            {
                PrefabUtility.SaveAsPrefabAsset(root, BasePath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }

            var assembly = typeof(OverrideWiringProbe).Assembly.GetName().Name;
            var yaml = VariantPrefab
                .Replace(BaseGuid, AssetDatabase.AssetPathToGUID(BasePath))
                .Replace("'Assembly-CSharp P05.Game.Box`1[[System.Int32, mscorlib]]'",
                    $"{assembly} {typeof(RenamedRanged).Namespace}.OldRenamedRanged")
                .Replace("Assembly-CSharp P05.Game.Outer/Inner", $"{assembly} {typeof(OverrideWiringProbe).FullName}");

            File.WriteAllText(ProbePath, yaml);
            AssetDatabase.ImportAsset(ProbePath, ImportAssetOptions.ForceSynchronousImport);

            var guid = AssetDatabase.AssetPathToGUID(ProbePath);
            Assert.IsNotEmpty(guid);
            return guid;
        }

        // The delete guard answers from a warm index; the cold sweep is the path that reads the files.
        private static void WithColdIndex(Action body)
        {
            var indexField = typeof(SerializeReferenceTypeUsageIndex).GetField("_index", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(indexField);

            var previous = indexField.GetValue(null);
            try
            {
                indexField.SetValue(null, null);
                body();
            }
            finally
            {
                indexField.SetValue(null, previous);
            }
        }
    }

    // Stored only by the wiring probe's override, so a project sweep counts exactly that usage.
    [Serializable]
    internal sealed class OverrideWiringProbe { }
}
