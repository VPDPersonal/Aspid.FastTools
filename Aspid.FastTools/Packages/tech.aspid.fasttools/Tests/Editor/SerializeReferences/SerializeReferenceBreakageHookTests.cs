using System;
using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for SerializeReferenceBreakageHook: which imports schedule a breakage scan, and that a
    // scan waiting for the scripts to compile comes back after the domain reload that ends the compile.
    [TestFixture]
    internal sealed class SerializeReferenceBreakageHookTests
    {
        private const string ProbeAssetPath = "Assets/__AspidBreakageHookProbe__/Probe.asset";

        private static readonly string[] _none = Array.Empty<string>();

        [SetUp]
        public void SetUp() => SerializeReferenceBreakageHook.ResetForTests();

        [TearDown]
        public void TearDown() => SerializeReferenceBreakageHook.ResetForTests();

        [TestCase("Assets/Scripts/Spear.cs")]
        [TestCase("Assets/Scripts/Game.asmdef")]
        [TestCase("Assets/Scripts/Game.asmref")]
        [TestCase("Assets/Plugins/Game.dll")]
        [TestCase("Assets/Plugins/GAME.DLL")]
        [TestCase("Packages/com.example.pkg/Runtime/Example.asmdef")]
        public void Collect_AssemblyOrScriptImported_SchedulesScan(string path)
        {
            Assert.IsTrue(SerializeReferenceBreakageHook.Collect(new[] { path }, _none, _none, _none));
            Assert.IsTrue(SerializeReferenceBreakageHook.IsScanScheduled);
        }

        [Test]
        public void Collect_AssemblyDeletedOrMoved_SchedulesScan()
        {
            Assert.IsTrue(SerializeReferenceBreakageHook.Collect(_none, new[] { "Assets/Plugins/Game.dll" }, _none, _none));

            SerializeReferenceBreakageHook.ResetForTests();

            Assert.IsTrue(SerializeReferenceBreakageHook.Collect(_none, _none,
                new[] { "Assets/Game/Game.asmdef" }, new[] { "Assets/Scripts/Game.asmdef" }));
        }

        [TestCase("Assets/Docs/Readme.txt")]
        [TestCase("Assets/Textures/Icon.png")]
        [TestCase("Assets/Scripts/Game.asmdef.meta")]
        public void Collect_UnrelatedAsset_DoesNotSchedule(string path)
        {
            Assert.IsFalse(SerializeReferenceBreakageHook.Collect(new[] { path }, _none, _none, _none));
            Assert.IsFalse(SerializeReferenceBreakageHook.IsScanScheduled);
        }

        [Test]
        public void Collect_ScanCandidates_AreKeptForTheDetectors()
        {
            SerializeReferenceBreakageHook.Collect(
                new[] { ProbeAssetPath, "Assets/Scripts/Game.asmdef" }, _none, _none, _none);

            CollectionAssert.AreEquivalent(new[] { ProbeAssetPath }, SerializeReferenceBreakageHook.PendingAssets);
        }

        [Test]
        public void DomainReload_ScheduledScan_ComesBackWithItsAssets()
        {
            SerializeReferenceBreakageHook.Collect(new[] { ProbeAssetPath, "Assets/Scripts/Game.asmdef" }, _none, _none, _none);

            SerializeReferenceBreakageHook.SimulateDomainReloadForTests();

            Assert.IsTrue(SerializeReferenceBreakageHook.IsScanScheduled);
            CollectionAssert.AreEquivalent(new[] { ProbeAssetPath }, SerializeReferenceBreakageHook.PendingAssets);
        }

        [Test]
        public void DomainReload_ScheduledScanWithoutAssets_StillComesBack()
        {
            SerializeReferenceBreakageHook.Collect(new[] { "Assets/Scripts/Spear.cs" }, _none, _none, _none);

            SerializeReferenceBreakageHook.SimulateDomainReloadForTests();

            Assert.IsTrue(SerializeReferenceBreakageHook.IsScanScheduled);
            CollectionAssert.IsEmpty(SerializeReferenceBreakageHook.PendingAssets);
        }

        [Test]
        public void DomainReload_NoScanWaiting_SchedulesNothing()
        {
            SerializeReferenceBreakageHook.SimulateDomainReloadForTests();

            Assert.IsFalse(SerializeReferenceBreakageHook.IsScanScheduled);
        }
    }
}
