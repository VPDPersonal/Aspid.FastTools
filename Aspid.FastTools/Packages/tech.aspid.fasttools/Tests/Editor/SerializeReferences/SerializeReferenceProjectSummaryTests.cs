using NUnit.Framework;
using Aspid.FastTools.UIElements.Editors.Internal;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for the Project References texts that depend on what the sweep covered: only a sweep that checked
    /// type names and required fields, and read every file, may call the project clean.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceProjectSummaryTests
    {
        [TestCase(false, false, false, RequiredAuditState.NotScanned)]
        [TestCase(false, true, true, RequiredAuditState.NotScanned)]
        [TestCase(true, true, true, RequiredAuditState.Disabled)]
        [TestCase(true, true, false, RequiredAuditState.NotScanned)]
        [TestCase(true, false, false, RequiredAuditState.Checked)]
        [TestCase(true, false, true, RequiredAuditState.Checked)]
        public void GetRequiredAuditState_MapsScanAndSeverity(
            bool scanned, bool checkDisabled, bool severityOff, RequiredAuditState expected) =>
            Assert.AreEqual(expected, SerializeReferenceProjectSummary.GetRequiredAuditState(scanned, checkDisabled, severityOff));

        [Test]
        public void BuildNothingFoundState_Checked_ReportsProjectClean()
        {
            var (success, title, _) = SerializeReferenceProjectSummary.BuildNothingFoundState(RequiredAuditState.Checked);

            Assert.IsTrue(success);
            Assert.AreEqual("Project clean", title);
        }

        [TestCase(RequiredAuditState.NotScanned, "Rescan")]
        [TestCase(RequiredAuditState.Disabled, "severity is Off")]
        public void BuildNothingFoundState_RequiredNotChecked_DoesNotClaimClean(RequiredAuditState state, string reason)
        {
            var (success, title, message) = SerializeReferenceProjectSummary.BuildNothingFoundState(state);

            Assert.IsFalse(success, "An audit that did not check required fields must not report success.");
            Assert.AreNotEqual("Project clean", title);
            StringAssert.DoesNotContain("unset required fields found", message);
            StringAssert.Contains(reason, message);
        }

        [Test]
        public void BuildResultsHeaderText_CountsTypeNamesApart() =>
            Assert.AreEqual("2 missing references, 1 missing type name, 3 required violations",
                SerializeReferenceProjectSummary.BuildResultsHeaderText(2, 0, 3, typeNameCount: 1));

        [Test]
        public void BuildMissingReferencesCleanHintText_Checked_SaysNothingLeft() =>
            StringAssert.StartsWith("Nothing left to repair",
                SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, false));

        [TestCase(RequiredAuditState.NotScanned)]
        [TestCase(RequiredAuditState.Disabled)]
        public void BuildMissingReferencesCleanHintText_RequiredNotChecked_DoesNotSayNothingLeft(RequiredAuditState state) =>
            StringAssert.DoesNotContain("Nothing left to repair",
                SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(state, false));

        // A clear on disk keeps the audit checked, and nulling a required field adds a violation listed below.
        [Test]
        public void MissingReferencesClean_CheckedWithViolations_WarnsAndPointsToRows()
        {
            Assert.AreEqual(StatusStyle.Type.Warning,
                SerializeReferenceProjectSummary.GetMissingReferencesCleanStatus(RequiredAuditState.Checked, true));

            var hint = SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, true);
            StringAssert.Contains("required-violation row", hint);
            StringAssert.DoesNotContain("Nothing left to repair", hint);
        }

        [TestCase(RequiredAuditState.Checked, StatusStyle.Type.Success)]
        [TestCase(RequiredAuditState.NotScanned, StatusStyle.Type.Info)]
        [TestCase(RequiredAuditState.Disabled, StatusStyle.Type.Info)]
        public void GetMissingReferencesCleanStatus_NoViolations_SucceedsOnlyWhenChecked(
            RequiredAuditState state, StatusStyle.Type expected) =>
            Assert.AreEqual(expected, SerializeReferenceProjectSummary.GetMissingReferencesCleanStatus(state, false));

        [Test]
        public void BuildRequiredNotCheckedText_Disabled_SaysToRescan() =>
            StringAssert.Contains("then Rescan",
                SerializeReferenceProjectSummary.BuildRequiredNotCheckedText(RequiredAuditState.Disabled));

        [TestCase(RequiredAuditState.NotScanned, "Rescan")]
        [TestCase(RequiredAuditState.Disabled, "severity is Off")]
        public void BuildResultsHintText_RequiredNotChecked_SaysWhy(RequiredAuditState state, string reason) =>
            StringAssert.Contains(reason, SerializeReferenceProjectSummary.BuildResultsHintText(false, state));

        [Test]
        public void BuildResultsHintText_CheckedWithViolations_PointsToRows() =>
            StringAssert.Contains("required-violation row",
                SerializeReferenceProjectSummary.BuildResultsHintText(true, RequiredAuditState.Checked));

        // The index can be warm without Scan Project (Unity Search, Asset References), and then nobody read type names.
        [Test]
        public void BuildNothingFoundState_TypeNamesNotScanned_DoesNotClaimTypeNames()
        {
            var (success, title, message) = SerializeReferenceProjectSummary.BuildNothingFoundState(
                RequiredAuditState.NotScanned, typeNamesScanned: false);

            Assert.IsFalse(success);
            Assert.AreNotEqual("Project clean", title);
            StringAssert.StartsWith("No missing managed references found", message);
            StringAssert.DoesNotContain("or type names", message);
            StringAssert.DoesNotContain("anywhere under Assets/", message);
            StringAssert.Contains("in the files that could be read", message);
            StringAssert.Contains("Type names and required fields have not been checked", message);
            StringAssert.Contains("Rescan", message);
        }

        [Test]
        public void BuildNothingFoundState_UnreadFiles_DoesNotClaimClean()
        {
            var (success, title, message) = SerializeReferenceProjectSummary.BuildNothingFoundState(
                RequiredAuditState.Checked, unreadFileCount: 2);

            Assert.IsFalse(success, "A sweep that skipped files must not report success.");
            Assert.AreNotEqual("Project clean", title);
            StringAssert.DoesNotContain("anywhere under Assets/", message);
            StringAssert.Contains("in the files that could be read", message);
            StringAssert.Contains("2 binary or Git LFS files were not checked", message);
            StringAssert.Contains("Console", message);
        }

        [Test]
        public void BuildNothingFoundState_AllChecked_ReportsProjectClean()
        {
            var (success, title, message) = SerializeReferenceProjectSummary.BuildNothingFoundState(
                RequiredAuditState.Checked, typeNamesScanned: true, unreadFileCount: 0);

            Assert.IsTrue(success);
            Assert.AreEqual("Project clean", title);
            StringAssert.Contains("anywhere under Assets/", message);
        }

        [Test]
        public void BuildUnreadFilesText_CountsFiles()
        {
            StringAssert.StartsWith("1 binary or Git LFS file was not checked", SerializeReferenceProjectSummary.BuildUnreadFilesText(1));
            StringAssert.StartsWith("3 binary or Git LFS files were not checked", SerializeReferenceProjectSummary.BuildUnreadFilesText(3));
        }

        [TestCase(RequiredAuditState.Checked, true, 0, StatusStyle.Type.Success)]
        [TestCase(RequiredAuditState.Checked, true, 1, StatusStyle.Type.Info)]
        [TestCase(RequiredAuditState.Checked, false, 0, StatusStyle.Type.Info)]
        [TestCase(RequiredAuditState.NotScanned, true, 0, StatusStyle.Type.Info)]
        public void GetMissingReferencesCleanStatus_SucceedsOnlyWhenEverythingWasChecked(
            RequiredAuditState state, bool typeNamesScanned, int unreadFileCount, StatusStyle.Type expected) =>
            Assert.AreEqual(expected,
                SerializeReferenceProjectSummary.GetMissingReferencesCleanStatus(state, false, typeNamesScanned, unreadFileCount));

        [Test]
        public void BuildMissingReferencesCleanHintText_UnreadFiles_DoesNotPromiseACleanProject()
        {
            var hint = SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(
                RequiredAuditState.Checked, false, unreadFileCount: 4);

            StringAssert.StartsWith("Nothing left to repair in the files that were read.", hint);
            StringAssert.DoesNotContain("confirm it's clean", hint);
            StringAssert.Contains("4 binary or Git LFS files were not checked", hint);
        }

        // The rows that stay listed after a repair are those of the last scan, as in the results hint.
        [Test]
        public void BuildMissingReferencesCleanHintText_RequiredViolations_SaysTheyAreAsOfTheLastScan()
        {
            var hint = SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, true);

            StringAssert.StartsWith("Click a required-violation row", hint);
            StringAssert.Contains("as of the last scan", hint);
        }

        [Test]
        public void BuildMissingReferencesCleanHintText_NoRequiredViolations_DoesNotMentionTheLastScan() =>
            StringAssert.DoesNotContain("last scan",
                SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, false));

        [Test]
        public void BuildMissingReferencesCleanHintText_TypeNamesNotScanned_SaysToRescan()
        {
            var hint = SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(
                RequiredAuditState.NotScanned, false, typeNamesScanned: false);

            StringAssert.DoesNotContain("Nothing left to repair", hint);
            StringAssert.Contains("Type names", hint);
            StringAssert.Contains("Rescan", hint);
        }

        // Required violations alone give the results no group, so there is nothing for Fix all to act on.
        [Test]
        public void BuildResultsHintText_NoBrokenGroups_DoesNotMentionFixAll()
        {
            var hint = SerializeReferenceProjectSummary.BuildResultsHintText(true, RequiredAuditState.Checked, hasBrokenGroups: false);

            StringAssert.DoesNotContain("Fix all", hint);
            StringAssert.Contains("required-violation row", hint);
        }

        [Test]
        public void BuildResultsHintText_BrokenGroups_ExplainsFixAll() =>
            StringAssert.StartsWith("Each group is a broken stored type", SerializeReferenceProjectSummary.BuildResultsHintText(
                false, RequiredAuditState.Checked, hasBrokenGroups: true));

        [Test]
        public void BuildResultsHintText_OnlyOverrides_ExplainsOverridesWithoutFixAll()
        {
            var hint = SerializeReferenceProjectSummary.BuildResultsHintText(
                false, RequiredAuditState.Checked, hasOverrides: true, hasBrokenGroups: false);

            StringAssert.StartsWith("Prefab instance overrides are fixed on the instance", hint);
            StringAssert.DoesNotContain("Fix all", hint);
        }

        // Missing references follow the live index; type names and required violations are those of the last scan.
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void BuildResultsHintText_ScanTimeRows_SayTheyAreAsOfTheLastScan(bool hasRequiredViolations, bool hasTypeNames) =>
            StringAssert.Contains("as of the last scan", SerializeReferenceProjectSummary.BuildResultsHintText(
                hasRequiredViolations, RequiredAuditState.Checked, hasTypeNames: hasTypeNames));

        [Test]
        public void BuildResultsHintText_NoScanTimeRows_DoesNotMentionTheLastScan() =>
            StringAssert.DoesNotContain("last scan",
                SerializeReferenceProjectSummary.BuildResultsHintText(false, RequiredAuditState.Checked));

        [Test]
        public void BuildResultsHintText_TypeNamesNotScanned_SaysToRescan()
        {
            var hint = SerializeReferenceProjectSummary.BuildResultsHintText(
                false, RequiredAuditState.NotScanned, typeNamesScanned: false);

            StringAssert.Contains("Type names and required fields have not been checked", hint);
            StringAssert.DoesNotContain("last scan", hint);
        }

        [Test]
        public void BuildResultsHintText_UnreadFiles_AppendsTheCount() =>
            StringAssert.EndsWith("2 binary or Git LFS files were not checked — see the Console.",
                SerializeReferenceProjectSummary.BuildResultsHintText(false, RequiredAuditState.Checked, unreadFileCount: 2));

        // A read-only file is logged; an entry that changed since the scan is not, so the text names both causes.
        [Test]
        public void BuildNotRewrittenText_NamesTheCountAndBothCauses()
        {
            var rewritten = SerializeReferenceProjectSummary.BuildNotRewrittenText(3);
            var cleared = SerializeReferenceProjectSummary.BuildNotRewrittenText(2, verb: "cleared");

            StringAssert.StartsWith("3 could not be rewritten", rewritten);
            StringAssert.StartsWith("2 could not be cleared", cleared);

            foreach (var text in new[] { rewritten, cleared })
            {
                StringAssert.Contains("read-only or locked (see the Console)", text);
                StringAssert.Contains("changed since the scan", text);
            }
        }

        [Test]
        public void JoinSentences_LeavesOutNullAndEmptyParts()
        {
            Assert.AreEqual("A. B.", SerializeReferenceProjectSummary.JoinSentences("A.", null, string.Empty, "B."));
            Assert.AreEqual("B.", SerializeReferenceProjectSummary.JoinSentences(null, "B."));
            Assert.AreEqual(string.Empty, SerializeReferenceProjectSummary.JoinSentences(null, string.Empty));
        }
    }
}
