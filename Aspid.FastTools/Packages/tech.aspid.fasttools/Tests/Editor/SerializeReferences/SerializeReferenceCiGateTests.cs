using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for the headless CI gate: that the committed gate severity decides the exit code, that the CLI flags
    // override it as documented (ASP-21), and that Execute, which is RunCheck without the exit, logs, writes the report
    // and returns the exit code it should. EditorApplication.Exit is never invoked.
    [TestFixture]
    internal sealed class SerializeReferenceCiGateTests
    {
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "AspidCiGate_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown() => Directory.Delete(_directory, recursive: true);

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
            };

            var lines = SerializeReferenceCiGate.BuildReport(new[] { violation }, unscanned)
                .Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .ToArray();

            CollectionAssert.Contains(lines, "# Not scanned (not text YAML): 2");
            CollectionAssert.Contains(lines, "#   Binary\tAssets/Scene/LightingData.asset");
            CollectionAssert.Contains(lines, "#   LfsPointer\tAssets/B.prefab");
            CollectionAssert.Contains(lines, "MissingType\tAssets/A.prefab\t1\t2\tGhost\t\t\tNs\tAsm");
        }

        // The namespace and assembly close the row, so a parser of the earlier columns is unaffected; only a
        // MissingType row has them.
        [Test]
        public void BuildReport_NamespaceAndAssemblyColumns_FillOnlyForMissingType()
        {
            var report = SerializeReferenceCiGate.BuildReport(new[]
            {
                new GateViolation("Assets/A.prefab", 1, 2, new ManagedTypeName("Asm", "Ns", "Ghost"),
                    GateViolationKind.MissingType, string.Empty),
                new GateViolation("Assets/B.asset", 3, -2, default, GateViolationKind.RequiredUnset, "primary"),
            });

            var lines = report.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();

            CollectionAssert.Contains(lines, "MissingType\tAssets/A.prefab\t1\t2\tGhost\t\t\tNs\tAsm");
            CollectionAssert.Contains(lines, "RequiredUnset\tAssets/B.asset\t3\t-2\t\tprimary\t\t\t");
        }

        // Two types with one class name in different namespaces must read differently in the log.
        [Test]
        public void ToString_MissingType_NamesTheNamespace()
        {
            var inNamespace = new GateViolation("Assets/A.prefab", 1, 2, new ManagedTypeName("Asm", "Ns", "Ghost"),
                GateViolationKind.MissingType, "weapon");
            var global = new GateViolation("Assets/A.prefab", 1, 2, new ManagedTypeName("Asm", string.Empty, "Ghost"),
                GateViolationKind.MissingType, "weapon");

            Assert.AreEqual("Assets/A.prefab : weapon -> missing type Ns.Ghost", inNamespace.ToString());
            Assert.AreEqual("Assets/A.prefab : weapon -> missing type Ghost", global.ToString());
        }

        // -----------------------------------------------------------------------------------------------------
        // -srGateStrict: files the run could not check count as violations
        // -----------------------------------------------------------------------------------------------------

        [TestCase(GateSeverity.Warn, 0, 5, false, 0)]
        [TestCase(GateSeverity.Fail, 0, 5, false, 0)]
        [TestCase(GateSeverity.Warn, 0, 5, true, 0)]
        [TestCase(GateSeverity.Fail, 0, 0, true, 0)]
        [TestCase(GateSeverity.Fail, 0, 5, true, 1)]
        [TestCase(GateSeverity.Fail, 2, 0, true, 1)]
        public void ComputeExitCode_Strict_CountsUncheckedFilesAsViolations(
            GateSeverity severity, int violations, int uncheckedFiles, bool strict, int expected) =>
            Assert.AreEqual(expected, SerializeReferenceCiGate.ComputeExitCode(violations, severity, uncheckedFiles, strict));

        [Test]
        public void CountUnchecked_CountsPointersAndBinariesThatCanHoldReferences()
        {
            var unscanned = new[]
            {
                ("Assets/A.prefab", AssetFileFormat.LfsPointer),
                ("Assets/B.prefab", AssetFileFormat.Binary),
                ("Assets/C.unity", AssetFileFormat.Binary),
            };

            Assert.AreEqual(3, SerializeReferenceGateScanner.CountUnchecked(unscanned, SerializationMode.ForceText));
            Assert.AreEqual(0, SerializeReferenceGateScanner.CountUnchecked(null, SerializationMode.ForceText));
        }

        // -----------------------------------------------------------------------------------------------------
        // Execute — what RunCheck does, with the scan and the severity injected
        // -----------------------------------------------------------------------------------------------------

        private static GateViolation Ghost(string assetPath = "Assets/A.prefab") =>
            new(assetPath, 1, 2, new ManagedTypeName("Asm", "Ns", "Ghost"), GateViolationKind.MissingType, "weapon");

        private string ReportPath => Path.Combine(_directory, "report.txt");

        private string[] Args(params string[] flags) =>
            new[] { "Unity", "-batchmode", "-srGateReport", ReportPath }.Concat(flags).ToArray();

        private static int Execute(
            string[] args,
            GateSeverity committed,
            IReadOnlyList<GateViolation> violations,
            params (string AssetPath, AssetFileFormat Format)[] unscanned) =>
            SerializeReferenceCiGate.Execute(args, () => committed, (_, skipped) =>
            {
                foreach (var file in unscanned) skipped.Add(file);
                return violations;
            });

        private static Regex Line(string text) => new(Regex.Escape(text));

        [Test]
        public void Execute_Fail_WithViolations_LogsErrorsWritesReportAndReturnsOne()
        {
            LogAssert.Expect(LogType.Error, Line("[Aspid FastTools] Assets/A.prefab : weapon -> missing type Ns.Ghost"));

            var exitCode = Execute(Args(), GateSeverity.Fail, new[] { Ghost() });

            Assert.AreEqual(1, exitCode);
            StringAssert.Contains("MissingType\tAssets/A.prefab", File.ReadAllText(ReportPath));
        }

        // Under Warn the exit code is 0, so the lines must not be errors: a wrapper that scans the log for errors
        // would fail the run. An unexpected error line fails the test.
        [Test]
        public void Execute_Warn_WithViolations_LogsWarningsAndReturnsZero()
        {
            LogAssert.Expect(LogType.Warning, Line("[Aspid FastTools] Assets/A.prefab : weapon -> missing type Ns.Ghost"));

            var exitCode = Execute(Args(), GateSeverity.Warn, new[] { Ghost() });

            Assert.AreEqual(0, exitCode);
            StringAssert.Contains("MissingType\tAssets/A.prefab", File.ReadAllText(ReportPath));
        }

        [Test]
        public void Execute_Fail_WithoutViolations_ReturnsZeroAndWritesReport()
        {
            var exitCode = Execute(Args(), GateSeverity.Fail, Array.Empty<GateViolation>());

            Assert.AreEqual(0, exitCode);
            StringAssert.Contains("# Violations: 0", File.ReadAllText(ReportPath));
        }

        [Test]
        public void Execute_Off_SkipsTheScanAndTheReport()
        {
            var scanned = false;

            var exitCode = SerializeReferenceCiGate.Execute(Args(), () => GateSeverity.Off, (_, _) =>
            {
                scanned = true;
                return Array.Empty<GateViolation>();
            });

            Assert.AreEqual(0, exitCode);
            Assert.IsFalse(scanned, "Severity Off must not scan the project.");
            Assert.IsFalse(File.Exists(ReportPath), "Severity Off must not write a report.");
        }

        [Test]
        public void Execute_FailFlag_OverridesOff()
        {
            LogAssert.Expect(LogType.Error, Line("missing type Ns.Ghost"));

            Assert.AreEqual(1, Execute(Args("-srGateFail"), GateSeverity.Off, new[] { Ghost() }));
        }

        [Test]
        public void Execute_WarnOnlyFlag_OverridesFail()
        {
            LogAssert.Expect(LogType.Warning, Line("missing type Ns.Ghost"));

            Assert.AreEqual(0, Execute(Args("-srGateWarnOnly"), GateSeverity.Fail, new[] { Ghost() }));
        }

        [Test]
        public void Execute_Flags_AreCaseInsensitive()
        {
            LogAssert.Expect(LogType.Error, Line("missing type Ns.Ghost"));

            Assert.AreEqual(1, Execute(Args("-SRGATEFAIL"), GateSeverity.Off, new[] { Ghost() }));
        }

        [Test]
        public void Execute_RequiredFlag_ScansRequiredFieldsToo()
        {
            var requested = new List<GateOptions>();
            int Run(params string[] flags) => SerializeReferenceCiGate.Execute(Args(flags), () => GateSeverity.Fail, (options, _) =>
            {
                requested.Add(options);
                return Array.Empty<GateViolation>();
            });

            Run();
            Run("-srGateRequired");

            Assert.IsFalse(requested[0].ScanRequiredFields, "Without -srGateRequired only missing types are checked.");
            Assert.IsTrue(requested[0].ScanMissingTypes);
            Assert.IsTrue(requested[1].ScanRequiredFields, "-srGateRequired adds the required fields.");
            Assert.IsTrue(requested[1].ScanMissingTypes);
        }

        // The violations go to the log first, so a report path that cannot be written does not hide them.
        [Test]
        public void Execute_ReportFolderMissing_StillLogsViolationsAndReturnsTwo()
        {
            var args = new[] { "Unity", "-srGateReport", Path.Combine(_directory, "missing", "report.txt") };

            LogAssert.Expect(LogType.Error, Line("missing type Ns.Ghost"));
            LogAssert.Expect(LogType.Error, new Regex(@"Gate check failed: .*DirectoryNotFoundException", RegexOptions.Singleline));

            Assert.AreEqual(2, Execute(args, GateSeverity.Fail, new[] { Ghost() }));
        }

        [Test]
        public void Execute_ScanThrows_ReturnsTwo()
        {
            LogAssert.Expect(LogType.Error, new Regex(@"Gate check failed: .*boom", RegexOptions.Singleline));

            var exitCode = SerializeReferenceCiGate.Execute(Args(), () => GateSeverity.Fail,
                (_, _) => throw new InvalidOperationException("boom"));

            Assert.AreEqual(2, exitCode);
        }

        // Without git lfs pull nothing was checked, which the exit code alone would hide.
        [Test]
        public void Execute_Strict_UncheckedFile_FailsUnderFail()
        {
            LogAssert.Expect(LogType.Warning, new Regex("1 file\\(s\\) were not checked for SerializeReference problems"));
            LogAssert.Expect(LogType.Error, Line("-srGateStrict: 1 file(s) were not checked."));

            var exitCode = Execute(Args("-srGateStrict"), GateSeverity.Fail, Array.Empty<GateViolation>(),
                ("Assets/Arena.unity", AssetFileFormat.LfsPointer));

            Assert.AreEqual(1, exitCode);
        }

        [Test]
        public void Execute_WithoutStrict_UncheckedFile_StillReturnsZero()
        {
            LogAssert.Expect(LogType.Warning, new Regex("1 file\\(s\\) were not checked for SerializeReference problems"));

            var exitCode = Execute(Args(), GateSeverity.Fail, Array.Empty<GateViolation>(),
                ("Assets/Arena.unity", AssetFileFormat.LfsPointer));

            Assert.AreEqual(0, exitCode);
        }

        [Test]
        public void Execute_Strict_UncheckedFile_UnderWarn_ReturnsZero()
        {
            LogAssert.Expect(LogType.Warning, new Regex("1 file\\(s\\) were not checked for SerializeReference problems"));
            LogAssert.Expect(LogType.Warning, Line("-srGateStrict: 1 file(s) were not checked."));

            var exitCode = Execute(Args("-srGateStrict"), GateSeverity.Warn, Array.Empty<GateViolation>(),
                ("Assets/Arena.unity", AssetFileFormat.LfsPointer));

            Assert.AreEqual(0, exitCode);
        }

        // A project with thousands of broken references would otherwise flood the CI log; the report keeps them all.
        [Test]
        public void LogViolations_ListsAtMostTheCapAndCountsTheRest()
        {
            var cap = SerializeReferenceGateScanner.MaxListedViolations;
            var violations = Enumerable.Range(0, cap + 10).Select(i => Ghost($"Assets/A{i}.prefab")).ToArray();

            for (var i = 0; i < cap; i++)
                LogAssert.Expect(LogType.Error, Line($"[Aspid FastTools] Assets/A{i}.prefab : weapon -> missing type Ns.Ghost"));

            LogAssert.Expect(LogType.Error, Line("… and 10 more violation(s); the report lists all of them."));

            SerializeReferenceCiGate.LogViolations(violations, GateSeverity.Fail);
        }

        [Test]
        public void Execute_ManyViolations_ReportKeepsEveryOne()
        {
            var cap = SerializeReferenceGateScanner.MaxListedViolations;
            var violations = Enumerable.Range(0, cap + 10).Select(i => Ghost($"Assets/A{i}.prefab")).ToArray();

            for (var i = 0; i < cap; i++)
                LogAssert.Expect(LogType.Warning, new Regex(@"missing type Ns\.Ghost"));

            LogAssert.Expect(LogType.Warning, Line("… and 10 more violation(s); the report lists all of them."));

            Assert.AreEqual(0, Execute(Args(), GateSeverity.Warn, violations));

            var rows = File.ReadAllLines(ReportPath).Count(line => line.StartsWith("MissingType\t"));
            Assert.AreEqual(cap + 10, rows);
        }
    }
}
