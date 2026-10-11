using NUnit.Framework;
using System.Collections.Generic;
using Aspid.FastTools.UIElements.Editors.Internal;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    /// <summary>
    /// Coverage for the Project References texts that depend on whether the required-field audit ran: only a sweep
    /// that checked required fields may call the project clean.
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
    }
}
