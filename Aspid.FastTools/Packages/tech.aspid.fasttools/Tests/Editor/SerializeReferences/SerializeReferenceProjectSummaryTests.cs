using NUnit.Framework;
using System.Collections.Generic;
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
        public void MissingReferencesClean_CheckedWithViolations_WarnsWithoutClaimingClean()
        {
            Assert.AreEqual(StatusStyle.Type.Warning,
                SerializeReferenceProjectSummary.GetMissingReferencesCleanStatus(RequiredAuditState.Checked, true));

            // The Required heading and the header tooltip say how to act on the rows, so the line under the header stays empty.
            Assert.IsEmpty(SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, true));
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
            StringAssert.Contains(reason, SerializeReferenceProjectSummary.BuildResultsHintText(state));

        [Test]
        public void BuildResultsGuideText_WithViolations_PointsToRows() =>
            StringAssert.Contains("required-violation row",
                SerializeReferenceProjectSummary.BuildResultsGuideText(hasRequiredViolations: true));

        // How to act on the results is the header's tooltip; the line under it keeps only what needs attention.
        [Test]
        public void BuildResultsHintText_CheckedWithNothingStale_IsEmpty() =>
            Assert.IsEmpty(SerializeReferenceProjectSummary.BuildResultsHintText(RequiredAuditState.Checked));

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
            StringAssert.Contains("2 files were not checked (binary, Git LFS pointers or not readable)", message);
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
            StringAssert.StartsWith("1 file was not checked (binary, a Git LFS pointer or not readable)", SerializeReferenceProjectSummary.BuildUnreadFilesText(1));
            StringAssert.StartsWith("3 files were not checked (binary, Git LFS pointers or not readable)", SerializeReferenceProjectSummary.BuildUnreadFilesText(3));
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
            StringAssert.Contains("4 files were not checked (binary, Git LFS pointers or not readable)", hint);
        }

        // Rows read by the last scan say so in their own tooltips, not in the line under the header.
        [TestCase(true)]
        [TestCase(false)]
        public void BuildMissingReferencesCleanHintText_DoesNotMentionTheLastScan(bool hasRequiredViolations) =>
            StringAssert.DoesNotContain("last scan",
                SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, hasRequiredViolations));

        [Test]
        public void BuildMissingReferencesCleanHintText_RequiredViolationsAndUnreadFiles_KeepsTheUnreadCount() =>
            StringAssert.StartsWith("2 files were not checked",
                SerializeReferenceProjectSummary.BuildMissingReferencesCleanHintText(RequiredAuditState.Checked, true, unreadFileCount: 2));

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
        public void BuildResultsGuideText_NoBrokenGroups_DoesNotMentionFixAll()
        {
            var guide = SerializeReferenceProjectSummary.BuildResultsGuideText(hasRequiredViolations: true, hasBrokenGroups: false);

            StringAssert.DoesNotContain("Fix all", guide);
            StringAssert.Contains("required-violation row", guide);
        }

        [Test]
        public void BuildResultsGuideText_BrokenGroups_ExplainsFixAll() =>
            StringAssert.StartsWith("Each group is a broken stored type", SerializeReferenceProjectSummary.BuildResultsGuideText(
                hasRequiredViolations: false, hasBrokenGroups: true));

        [Test]
        public void BuildResultsGuideText_OnlyOverrides_ExplainsOverridesWithoutFixAll()
        {
            var hint = SerializeReferenceProjectSummary.BuildResultsGuideText(
                hasRequiredViolations: false, hasOverrides: true, hasBrokenGroups: false);

            StringAssert.StartsWith("Prefab instance overrides are fixed on the instance", hint);
            StringAssert.DoesNotContain("Fix all", hint);
        }

        // Type names and required violations are those of the last scan; their tooltips say so, the hint does not.
        [Test]
        public void BuildResultsHintText_Checked_DoesNotMentionTheLastScan() =>
            StringAssert.DoesNotContain("last scan",
                SerializeReferenceProjectSummary.BuildResultsHintText(RequiredAuditState.Checked));

        [Test]
        public void BuildResultsHintText_TypeNamesNotScanned_SaysToRescan()
        {
            var hint = SerializeReferenceProjectSummary.BuildResultsHintText(RequiredAuditState.NotScanned, typeNamesScanned: false);

            StringAssert.Contains("Type names and required fields have not been checked", hint);
            StringAssert.DoesNotContain("last scan", hint);
        }

        [Test]
        public void BuildResultsHintText_UnreadFiles_AppendsTheCount() =>
            StringAssert.EndsWith("2 files were not checked (binary, Git LFS pointers or not readable) — see the Console.",
                SerializeReferenceProjectSummary.BuildResultsHintText(RequiredAuditState.Checked, unreadFileCount: 2));

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

        // A card never builds fewer rows than its first page and never more than its entries.
        [TestCase(0, 450, 200)]
        [TestCase(200, 450, 200)]
        [TestCase(400, 450, 400)]
        [TestCase(600, 450, 450)]
        [TestCase(0, 50, 50)]
        [TestCase(0, 0, 0)]
        public void GetShownRows_StaysBetweenTheFirstPageAndTheEntryCount(int requested, int total, int expected) =>
            Assert.AreEqual(expected, SerializeReferenceProjectSummary.GetShownRows(requested, total));

        [Test]
        public void ShowMore_PagesThroughEveryEntry()
        {
            const int total = 450;

            var shown = SerializeReferenceProjectSummary.GetShownRows(requested: 0, total);
            var pages = new List<int> { shown };

            while (shown < total)
            {
                shown = SerializeReferenceProjectSummary.GetShownRows(
                    SerializeReferenceProjectSummary.GetNextShownRows(shown), total);
                pages.Add(shown);
            }

            CollectionAssert.AreEqual(new[] { 200, 400, 450 }, pages);
        }

        [TestCase(200, 450, "Show more (200 of 450 shown)", "Show the next 200 rows.")]
        [TestCase(400, 450, "Show more (400 of 450 shown)", "Show the next 50 rows.")]
        [TestCase(200, 201, "Show more (200 of 201 shown)", "Show the next 1 row.")]
        public void BuildShowMore_NamesTheRowsTheNextPageAdds(int shown, int total, string text, string tooltip)
        {
            Assert.AreEqual(text, SerializeReferenceProjectSummary.BuildShowMoreText(shown, total));
            Assert.AreEqual(tooltip, SerializeReferenceProjectSummary.BuildShowMoreTooltip(shown, total));
        }

        [Test]
        public void GroupByFile_KeepsTheFirstOrderOfFilesAndCollectsTheirRids()
        {
            var stored = new ManagedTypeName("Game", "Game.Weapons", "Ghost");
            var entries = new[]
            {
                new MissingReferenceLocation("Assets/B.asset", new MissingReferenceEntry(fileId: 1, rid: 10, stored)),
                new MissingReferenceLocation("Assets/A.asset", new MissingReferenceEntry(fileId: 1, rid: 20, stored)),
                new MissingReferenceLocation("Assets/B.asset", new MissingReferenceEntry(fileId: 1, rid: 11, stored)),
            };

            var files = SerializeReferenceProjectSummary.GroupByFile(entries);

            Assert.AreEqual(2, files.Count, "Each file must get one row.");
            Assert.AreEqual("Assets/B.asset", files[0].AssetPath);
            CollectionAssert.AreEqual(new long[] { 10, 11 }, files[0].Rids);
            Assert.AreEqual("Assets/A.asset", files[1].AssetPath);
            CollectionAssert.AreEqual(new long[] { 20 }, files[1].Rids);
        }

        [TestCase(new long[] { 8000 }, "rid 8000")]
        [TestCase(new long[] { 8000, 8001, 8003 }, "3 rids")]
        [TestCase(new long[] { 7288617647077589203, 7288617647077589204 }, "2 rids")]
        public void DescribeRids_NamesOneRidAndCountsSeveral(long[] rids, string expected) =>
            Assert.AreEqual(expected, SerializeReferenceProjectSummary.DescribeRids(rids));
    }
}
