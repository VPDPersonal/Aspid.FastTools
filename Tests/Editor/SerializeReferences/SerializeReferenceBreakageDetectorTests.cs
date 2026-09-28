using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for the <see cref="SerializeReferenceBreakageDetector"/> baseline: it is established from the asset
    /// text while <see cref="SerializeReferenceTypeUsageIndex"/> stays cold, and follows assets changed afterwards.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceBreakageDetectorTests
    {
        private const string ProbeAssetPath = "Assets/__AspidBreakageDetectorProbe__.asset";

        private bool _breakageDetection;
        private bool _established;
        private string _baseline;
        private Delegate _subscribers;
        private readonly List<BreakageReport> _reports = new();

        // The event's backing field: the tests listen alone, so a report never reaches the editor notification.
        private static readonly FieldInfo _breakageDetected = typeof(SerializeReferenceBreakageDetector)
            .GetField(nameof(SerializeReferenceBreakageDetector.BreakageDetected), BindingFlags.NonPublic | BindingFlags.Static);

        [SetUp]
        public void SetUp()
        {
            // Snapshot the session's real baseline so the assertions below can rebuild it freely.
            _breakageDetection = SerializeReferenceSettings.BreakageDetectionEnabled;
            _established = SessionState.GetBool(SerializeReferenceBreakageDetector.EstablishedKey, false);
            _baseline = SessionState.GetString(SerializeReferenceBreakageDetector.BaselineKey, string.Empty);

            // A sweep the session started before the probe existed must not finish in its place.
            SerializeReferenceBreakageDetector.ResetForTests();

            _subscribers = (Delegate)_breakageDetected.GetValue(null);
            _breakageDetected.SetValue(null, null);
            _reports.Clear();
            SerializeReferenceBreakageDetector.BreakageDetected += _reports.Add;

            SerializeReferenceSettings.BreakageDetectionEnabled = true;
            SessionState.EraseBool(SerializeReferenceBreakageDetector.EstablishedKey);
            SessionState.EraseString(SerializeReferenceBreakageDetector.BaselineKey);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);
            SerializeReferenceTypeUsageIndex.Reset();
            SerializeReferenceBreakageDetector.ResetForTests();
            _breakageDetected.SetValue(null, _subscribers);

            SerializeReferenceSettings.BreakageDetectionEnabled = _breakageDetection;
            SessionState.SetBool(SerializeReferenceBreakageDetector.EstablishedKey, _established);
            SessionState.SetString(SerializeReferenceBreakageDetector.BaselineKey, _baseline);
        }

        [Test]
        public void Scan_ColdIndex_EstablishesBaselineFromAssetText()
        {
            SaveProbe(new TestSword());

            SerializeReferenceBreakageDetector.Scan();
            SerializeReferenceBreakageDetector.CompleteSweep();

            Assert.IsFalse(SerializeReferenceTypeUsageIndex.IsWarm, "Establishing the baseline must not warm the index.");
            Assert.IsTrue(SerializeReferenceBreakageDetector.IsEstablished);
            CollectionAssert.AreEquivalent(new[] { KeyOf<TestSword>() },
                SerializeReferenceBreakageDetector.GetBaselineKeys(ProbeAssetPath));
        }

        [Test]
        public void Scan_ChangedAsset_ReplacesItsBaselineEntry()
        {
            SaveProbe(new TestSword());
            SerializeReferenceBreakageDetector.Scan();
            SerializeReferenceBreakageDetector.CompleteSweep();

            // TestSword's last usage is gone, so renaming it later must not be reported as a breakage.
            SaveProbe(new DeleteGuardPistol());
            SerializeReferenceBreakageDetector.Scan(new[] { ProbeAssetPath });
            SerializeReferenceBreakageDetector.CompleteSweep();

            CollectionAssert.AreEquivalent(new[] { KeyOf<DeleteGuardPistol>() },
                SerializeReferenceBreakageDetector.GetBaselineKeys(ProbeAssetPath));
        }

        [Test]
        public void Scan_ChangedAssetNoLongerHoldsBrokenType_DoesNotReport()
        {
            // A pulled rename: the baseline still lists the old name, the re-saved asset holds only the new one.
            SaveProbe(new TestSword());
            SeedBaseline(BrokenKey);

            SerializeReferenceBreakageDetector.Scan(new[] { ProbeAssetPath });
            SerializeReferenceBreakageDetector.CompleteSweep();

            CollectionAssert.IsEmpty(_reports);
            CollectionAssert.AreEquivalent(new[] { KeyOf<TestSword>() },
                SerializeReferenceBreakageDetector.GetBaselineKeys(ProbeAssetPath));
        }

        [Test]
        public void Scan_UnchangedAssetHoldsBrokenType_ReportsItOnce()
        {
            SaveProbe(new TestSword());
            SeedBaseline(BrokenKey);

            SerializeReferenceBreakageDetector.Scan();
            SerializeReferenceBreakageDetector.Scan();

            Assert.AreEqual(1, _reports.Count);
            Assert.AreEqual(1, _reports[0].Entries.Count);
            CollectionAssert.IsEmpty(SerializeReferenceBreakageDetector.GetBaselineKeys(ProbeAssetPath));
        }

        // A stored type no assembly declares, as after a rename.
        private const string BrokenKey = "Assembly-CSharp|Aspid.BreakageDetectorProbe|RenamedAway";

        private static void SeedBaseline(string key)
        {
            SessionState.SetString(SerializeReferenceBreakageDetector.BaselineKey, $"{ProbeAssetPath}\t{key}");
            SessionState.SetBool(SerializeReferenceBreakageDetector.EstablishedKey, true);
        }

        private static void SaveProbe(ITestWeapon weapon)
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);

            var probe = ScriptableObject.CreateInstance<LinkerTestObject>();
            probe.a = weapon;

            AssetDatabase.CreateAsset(probe, ProbeAssetPath);
            AssetDatabase.SaveAssets();

            SerializeReferenceTypeUsageIndex.Reset();
        }

        private static string KeyOf<T>() => SerializeReferenceHelpers.StoredTypeKey(ManagedTypeName.FromType(typeof(T)));
    }
}
