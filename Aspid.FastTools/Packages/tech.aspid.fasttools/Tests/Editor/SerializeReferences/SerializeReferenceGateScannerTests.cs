using NUnit.Framework;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for <see cref="SerializeReferenceGateScanner.IsPendingMigration"/> — the gate's pre-filter that keeps
    /// properly declared renames from ever warning or failing a build / CI run.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceGateScannerTests
    {
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
