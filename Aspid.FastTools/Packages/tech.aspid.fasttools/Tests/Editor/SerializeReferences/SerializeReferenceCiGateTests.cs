using System.Linq;
using NUnit.Framework;

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

        // Files the YAML pass skipped are counted and listed as comment lines, so the report no longer reads as a
        // clean project and the tab-separated violation lines keep their format.
        [Test]
        public void BuildReport_ListsUnscannedFilesAsComments()
        {
            var violation = new GateViolation("Assets/A.prefab", 1, 2, new ManagedTypeName("Asm", "Ns", "Ghost"),
                GateViolationKind.MissingType, string.Empty);

            var unscanned = new[]
            {
                ("Assets/Scene/LightingData.asset", AssetFileFormat.Binary),
                ("Assets/B.prefab", AssetFileFormat.LfsPointer),
                ("Assets/Old.asset", AssetFileFormat.UnsupportedReferencesVersion),
            };

            var lines = SerializeReferenceCiGate.BuildReport(new[] { violation }, unscanned)
                .Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .ToArray();

            CollectionAssert.Contains(lines, "# Not scanned: 3");
            CollectionAssert.Contains(lines, "#   Binary\tAssets/Scene/LightingData.asset");
            CollectionAssert.Contains(lines, "#   LfsPointer\tAssets/B.prefab");
            CollectionAssert.Contains(lines, "#   UnsupportedReferencesVersion\tAssets/Old.asset");
            CollectionAssert.Contains(lines, "MissingType\tAssets/A.prefab\t1\t2\tGhost\t\t");
        }
    }
}
