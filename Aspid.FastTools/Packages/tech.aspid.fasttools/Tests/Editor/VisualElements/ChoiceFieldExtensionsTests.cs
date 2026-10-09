using NUnit.Framework;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class ChoiceFieldExtensionsTests
    {
        [Test]
        public void DropdownField_SettersKeepFieldType()
        {
            var choices = new List<string> { "A", "B", "C" };

            DropdownField field = new DropdownField()
                .SetChoices(choices)
                .SetIndex(1);

            Assert.AreSame(choices, field.choices);
            Assert.AreEqual(1, field.index);
            Assert.AreEqual("B", field.value);
        }

        [Test]
        public void DropdownField_SetFormatCallbacksStoreThem()
        {
            static string ToLower(string choice) => choice.ToLowerInvariant();
            static string ToUpper(string choice) => choice.ToUpperInvariant();

            // Unity calls the selected-value callback at once, so a choice must be selected first.
            var field = new DropdownField()
                .SetChoices(new List<string> { "A" })
                .SetIndex(0)
                .SetFormatListItemCallback(ToLower)
                .SetFormatSelectedValueCallback(ToUpper);

            Assert.AreEqual("a", field.formatListItemCallback("A"));
            Assert.AreEqual("A", field.formatSelectedValueCallback("a"));
        }

        [Test]
        public void DropdownField_SetFormatSelectedValueCallbackFormatsDisplayedText()
        {
            var field = new DropdownField()
                .SetChoices(new List<string> { "A", "B" })
                .SetIndex(1)
                .SetFormatSelectedValueCallback(choice => choice.ToLowerInvariant());

            Assert.AreEqual("b", field.text);
        }

        [Test]
        public void PopupField_UsesExplicitTypeArgumentsExceptSetChoices()
        {
            var field = new PopupField<int>()
                .SetChoices(new List<int> { 10, 20, 30 })
                .SetIndex<PopupField<int>, int>(2)
                .SetFormatListItemCallback<PopupField<int>, int>(choice => $"#{choice}")
                .SetFormatSelectedValueCallback<PopupField<int>, int>(choice => $"[{choice}]");

            Assert.AreEqual(30, field.value);
            Assert.AreEqual("#10", field.formatListItemCallback(10));
            Assert.AreEqual("[30]", field.text);
        }

        [Test]
        public void RadioButtonGroup_SetChoicesCreatesButtons()
        {
            var choices = new[] { "A", "B", "C" };

            RadioButtonGroup group = new RadioButtonGroup().SetChoices(choices);

            CollectionAssert.AreEqual(choices, group.choices);
            Assert.AreEqual(3, group.Query<RadioButton>().ToList().Count);
        }

        [Test]
        public void RadioButtonGroup_SetChoicesAcceptsList()
        {
            var group = new RadioButtonGroup().SetChoices(new List<string> { "A", "B" });

            Assert.AreEqual(2, group.Query<RadioButton>().ToList().Count);
        }
    }
}
