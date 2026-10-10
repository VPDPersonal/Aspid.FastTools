using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using UnityEditor.Build;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Coverage for the player-build gate: that SerializeReferenceBuildGate.Check skips, warns or stops the build as the
    // severity says. The scan is injected, so no project asset is read.
    [TestFixture]
    internal sealed class SerializeReferenceBuildGateTests
    {
        private static GateViolation Ghost(string assetPath = "Assets/A.prefab") =>
            new(assetPath, 1, 2, new ManagedTypeName("Asm", "Ns", "Ghost"), GateViolationKind.MissingType, "weapon");

        private static void Check(
            GateSeverity severity,
            IReadOnlyList<GateViolation> violations,
            params (string AssetPath, AssetFileFormat Format)[] unscanned) =>
            SerializeReferenceBuildGate.Check(severity, skipped =>
            {
                foreach (var file in unscanned) skipped.Add(file);
                return violations;
            });

        [Test]
        public void Check_Off_DoesNotScan()
        {
            var scanned = false;

            SerializeReferenceBuildGate.Check(GateSeverity.Off, _ =>
            {
                scanned = true;
                return new[] { Ghost() };
            });

            Assert.IsFalse(scanned, "Severity Off must not scan the project.");
        }

        [Test]
        public void Check_Warn_WithViolations_LogsOneWarningAndKeepsBuilding()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"1 missing managed reference\(s\) across 1 file\(s\)(.|\n)*Assets/A\.prefab : weapon -> missing type Ns\.Ghost"));

            Assert.DoesNotThrow(() => Check(GateSeverity.Warn, new[] { Ghost() }));
        }

        [Test]
        public void Check_Fail_WithViolations_StopsTheBuildAndNamesThem()
        {
            var exception = Assert.Throws<BuildFailedException>(() => Check(GateSeverity.Fail, new[] { Ghost() }));

            StringAssert.Contains("1 missing managed reference(s) across 1 file(s)", exception.Message);
            StringAssert.Contains("Assets/A.prefab : weapon -> missing type Ns.Ghost", exception.Message);
        }

        [TestCase(GateSeverity.Warn)]
        [TestCase(GateSeverity.Fail)]
        public void Check_WithoutViolations_DoesNotStopTheBuild(GateSeverity severity) =>
            Assert.DoesNotThrow(() => Check(severity, Array.Empty<GateViolation>()));

        // A pointer that was never pulled means nothing was checked: the warning says so, and the build goes on.
        [Test]
        public void Check_Fail_UncheckedFile_WarnsButDoesNotStopTheBuild()
        {
            LogAssert.Expect(LogType.Warning, new Regex(@"1 file\(s\) were not checked for SerializeReference problems"));

            Assert.DoesNotThrow(() => Check(GateSeverity.Fail, Array.Empty<GateViolation>(),
                ("Assets/Arena.unity", AssetFileFormat.LfsPointer)));
        }

        [Test]
        public void BuildSummary_ListsAtMostTheCapAndCountsTheRest()
        {
            var cap = SerializeReferenceGateScanner.MaxListedViolations;
            var violations = Enumerable.Range(0, cap + 10).Select(i => Ghost($"Assets/A{i}.prefab")).ToArray();

            var lines = SerializeReferenceBuildGate.BuildSummary(violations)
                .Split('\n')
                .Select(line => line.TrimEnd('\r'))
                .Where(line => line.Length > 0)
                .ToArray();

            Assert.AreEqual(cap + 2, lines.Length, "The header, the capped list and the line with the rest.");
            StringAssert.Contains($"{cap + 10} missing managed reference(s)", lines[0]);
            StringAssert.Contains($"Assets/A{cap - 1}.prefab", lines[cap]);
            StringAssert.Contains("… and 10 more", lines[cap + 1]);
        }

        [Test]
        public void BuildSummary_WithinTheCap_ListsEveryViolation()
        {
            var cap = SerializeReferenceGateScanner.MaxListedViolations;
            var violations = Enumerable.Range(0, cap).Select(i => Ghost($"Assets/A{i}.prefab")).ToArray();

            var summary = SerializeReferenceBuildGate.BuildSummary(violations);

            StringAssert.DoesNotContain("more", summary);
            StringAssert.Contains($"Assets/A{cap - 1}.prefab", summary);
        }
    }
}
