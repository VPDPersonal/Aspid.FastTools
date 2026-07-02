using NUnit.Framework;
using Aspid.FastTools.SerializeReferences.Editors;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Pure decision-logic coverage for the headless CI gate: that the committed gate severity decides the exit code
    /// and that the CLI flags override it as documented (ASP-21). No <see cref="UnityEditor.EditorApplication.Exit"/>
    /// is invoked — only the extracted <see cref="SerializeReferenceCiGate.ResolveSeverity"/> and
    /// <see cref="SerializeReferenceCiGate.ComputeExitCode"/> helpers are exercised.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceCiGateTests
    {
        // No violations: never fail, whatever the severity.
        [TestCase(GateSeverity.Off)]
        [TestCase(GateSeverity.Warn)]
        [TestCase(GateSeverity.Fail)]
        public void ComputeExitCode_NoViolations_IsZero(GateSeverity severity) =>
            Assert.AreEqual(0, SerializeReferenceCiGate.ComputeExitCode(0, severity));

        // Violations present: only Fail turns into a non-zero exit code.
        [TestCase(GateSeverity.Off, 0)]
        [TestCase(GateSeverity.Warn, 0)]
        [TestCase(GateSeverity.Fail, 1)]
        public void ComputeExitCode_WithViolations_MatchesSeverity(GateSeverity severity, int expected) =>
            Assert.AreEqual(expected, SerializeReferenceCiGate.ComputeExitCode(3, severity));

        // No flags: the committed Project Settings value passes straight through.
        [TestCase(GateSeverity.Off)]
        [TestCase(GateSeverity.Warn)]
        [TestCase(GateSeverity.Fail)]
        public void ResolveSeverity_NoFlags_UsesCommittedValue(GateSeverity committed) =>
            Assert.AreEqual(committed, SerializeReferenceCiGate.ResolveSeverity(committed, warnOnly: false, failOverride: false));

        // -srGateWarnOnly forces Warn regardless of the committed value.
        [TestCase(GateSeverity.Off)]
        [TestCase(GateSeverity.Warn)]
        [TestCase(GateSeverity.Fail)]
        public void ResolveSeverity_WarnOnly_ForcesWarn(GateSeverity committed) =>
            Assert.AreEqual(GateSeverity.Warn, SerializeReferenceCiGate.ResolveSeverity(committed, warnOnly: true, failOverride: false));

        // -srGateFail forces Fail regardless of the committed value.
        [TestCase(GateSeverity.Off)]
        [TestCase(GateSeverity.Warn)]
        [TestCase(GateSeverity.Fail)]
        public void ResolveSeverity_FailOverride_ForcesFail(GateSeverity committed) =>
            Assert.AreEqual(GateSeverity.Fail, SerializeReferenceCiGate.ResolveSeverity(committed, warnOnly: false, failOverride: true));

        // Both flags together: warn-only wins (the safe choice — never fail unexpectedly).
        [Test]
        public void ResolveSeverity_BothFlags_WarnOnlyWins() =>
            Assert.AreEqual(GateSeverity.Warn, SerializeReferenceCiGate.ResolveSeverity(GateSeverity.Fail, warnOnly: true, failOverride: true));

        // A [MovedFrom]-claimed stale name is a pending migration, not a violation: Unity migrates the reference in
        // memory at load, so the gate must accept it — a properly declared rename can never warn or fail a build /
        // CI run. A scene path exercises the trust-the-claim branch (constraints are unrecoverable for scenes).
        // (RenamedRanged is the [MovedFrom(..., "OldRenamedRanged")] fixture of the ranking tests.)
        [Test]
        public void IsPendingMigration_MovedFromClaimedName_IsNotAViolation()
        {
            var stored = new ManagedTypeName(
                typeof(RenamedRanged).Assembly.GetName().Name, typeof(RenamedRanged).Namespace, "OldRenamedRanged");
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
