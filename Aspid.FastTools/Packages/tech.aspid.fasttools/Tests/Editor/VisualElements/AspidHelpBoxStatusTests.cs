using System.Linq;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Editors.Internal.Tests
{
    /// <summary>
    /// A help box must look the same whether its status and message type come from the preset, a UXML attribute or
    /// a fluent extension: the box and both of its labels carry one status.
    /// </summary>
    [TestFixture]
    internal sealed class AspidHelpBoxStatusTests
    {
        [Test]
        public void MessageType_Property_DerivesStatus()
        {
            var box = new AspidHelpBox("Title", "Message", AspidHelpBoxPreset.Default)
                .SetMessageType(HelpBoxMessageType.Warning);

            Assert.AreEqual(StatusStyle.Type.Warning, box.Status);
            Assert.IsTrue(box.ClassListContains(StatusStyle.WarningClass));
            AssertLabelsCarry(box, StatusStyle.Type.Warning);
        }

        [Test]
        public void MessageType_Property_MatchesPreset()
        {
            var fromPreset = new AspidHelpBox("Title", "Message",
                AspidHelpBoxPreset.Default.SetMessageType(HelpBoxMessageType.Error));
            var fromProperty = new AspidHelpBox("Title", "Message", AspidHelpBoxPreset.Default)
                .SetMessageType(HelpBoxMessageType.Error);

            Assert.AreEqual(fromPreset.Status, fromProperty.Status);
            CollectionAssert.AreEqual(LabelStatuses(fromPreset), LabelStatuses(fromProperty));
        }

        [Test]
        public void Status_Property_ReachesTitleAndMessage()
        {
            var box = new AspidHelpBox("Title", "Message", AspidHelpBoxPreset.Default)
                .SetStatus(StatusStyle.Type.Success);

            AssertLabelsCarry(box, StatusStyle.Type.Success);
        }

        [Test]
        public void Title_AddedLater_CarriesStatus()
        {
            var box = new AspidHelpBox("Message", AspidHelpBoxPreset.Default)
                .SetStatus(StatusStyle.Type.Info)
                .SetTitle("Title");

            Assert.AreEqual(2, box.Query<AspidLabel>().ToList().Count);
            AssertLabelsCarry(box, StatusStyle.Type.Info);
        }

        [Test]
        public void DerivedStatus_FollowsMessageType()
        {
            var box = new AspidHelpBox("Title", "Message", AspidHelpBoxPreset.Default)
                .SetMessageType(HelpBoxMessageType.Warning)
                .SetMessageType(HelpBoxMessageType.Error);

            Assert.AreEqual(StatusStyle.Type.Error, box.Status);
            Assert.IsFalse(box.ClassListContains(StatusStyle.WarningClass));
            AssertLabelsCarry(box, StatusStyle.Type.Error);
        }

        [Test]
        public void ExplicitStatus_WinsOverMessageType()
        {
            var box = new AspidHelpBox("Title", "Message", AspidHelpBoxPreset.Default)
                .SetStatus(StatusStyle.Type.Success)
                .SetMessageType(HelpBoxMessageType.Error);

            Assert.AreEqual(StatusStyle.Type.Success, box.Status);
            AssertLabelsCarry(box, StatusStyle.Type.Success);
        }

        private static void AssertLabelsCarry(AspidHelpBox box, StatusStyle.Type status)
        {
            var statuses = LabelStatuses(box);
            Assert.IsNotEmpty(statuses);
            Assert.That(statuses, Is.All.EqualTo(status), "The title and message must carry the box status.");
        }

        private static StatusStyle.Type[] LabelStatuses(AspidHelpBox box) =>
            box.Query<AspidLabel>().ToList().Select(label => label.LabelStatus).ToArray();
    }
}
