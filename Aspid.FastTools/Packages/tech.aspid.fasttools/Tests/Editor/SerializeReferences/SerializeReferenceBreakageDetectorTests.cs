using UnityEditor;
using UnityEngine;
using NUnit.Framework;

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

        [SetUp]
        public void SetUp()
        {
            // Snapshot the session's real baseline so the assertions below can rebuild it freely.
            _breakageDetection = SerializeReferenceSettings.BreakageDetectionEnabled;
            _established = SessionState.GetBool(SerializeReferenceBreakageDetector.EstablishedKey, false);
            _baseline = SessionState.GetString(SerializeReferenceBreakageDetector.BaselineKey, string.Empty);

            SerializeReferenceSettings.BreakageDetectionEnabled = true;
            SessionState.EraseBool(SerializeReferenceBreakageDetector.EstablishedKey);
            SessionState.EraseString(SerializeReferenceBreakageDetector.BaselineKey);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(ProbeAssetPath);
            SerializeReferenceTypeUsageIndex.Reset();

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
