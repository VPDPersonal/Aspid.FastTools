using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    internal sealed class TextInputBaseFieldExtensionsTests
    {
        private sealed class ShortField : TextValueField<short>
        {
            public ShortField()
                : base(-1, new ShortInput()) { }

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, short startValue) { }

            protected override string ValueToString(short value) => value.ToString();

            protected override short StringToValue(string str) => short.TryParse(str, out var value) ? value : default;

            private sealed class ShortInput : TextValueInput
            {
                protected override string allowedCharacters => "-0123456789";

                public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, short startValue) { }

                protected override string ValueToString(short value) => value.ToString();
            }
        }

        [Test]
        public void TextField_ChainKeepsFieldTypeAndSetsFieldProperties()
        {
            var field = new TextField()
                .SetMaxLength(64)
                .SetMaskChar('#')
                .SetDelayed(true)
                .SetReadOnly(true)
                .SetPassword(true)
                .SetPlaceholder("Search")
                .SetHidePlaceholderOnFocus(true)
                .SetAutoCorrection(true)
                .SetHideMobileInput(true)
                .SetKeyboardType(TouchScreenKeyboardType.EmailAddress);

            Assert.AreEqual(64, field.maxLength);
            Assert.AreEqual('#', field.maskChar);
            Assert.IsTrue(field.isDelayed);
            Assert.IsTrue(field.isReadOnly);
            Assert.IsTrue(field.isPasswordField);
            Assert.AreEqual("Search", field.textEdition.placeholder);
            Assert.IsTrue(field.textEdition.hidePlaceholderOnFocus);
            Assert.IsTrue(field.autoCorrection);
            Assert.IsTrue(field.hideMobileInput);
            Assert.AreEqual(TouchScreenKeyboardType.EmailAddress, field.keyboardType);
        }

        [Test]
        public void NumericFields_ResolveTheirValueTypeOverload()
        {
            Assert.IsTrue(new IntegerField().SetDelayed(true).isDelayed);
            Assert.IsTrue(new LongField().SetDelayed(true).isDelayed);
            Assert.IsTrue(new FloatField().SetDelayed(true).isDelayed);
            Assert.IsTrue(new DoubleField().SetDelayed(true).isDelayed);
            Assert.IsTrue(new UnsignedIntegerField().SetDelayed(true).isDelayed);
            Assert.IsTrue(new UnsignedLongField().SetDelayed(true).isDelayed);
            Assert.IsTrue(new Hash128Field().SetDelayed(true).isDelayed);
        }

        [Test]
        public void CustomValueType_UsesExplicitTypeArguments()
        {
            var field = new ShortField().SetReadOnly<ShortField, short>(true);

            Assert.IsTrue(field.isReadOnly);
        }

#if UNITY_6000_4_OR_NEWER
        [Test]
        public void GuidField_ChainKeepsFieldTypeAndSetsFieldProperties()
        {
            GUIDField field = new GUIDField()
                .SetLabel("Id")
                .SetMaxLength(32)
                .SetDelayed(true)
                .SetReadOnly(true)
                .SetPlaceholder("Guid");

            Assert.AreEqual("Id", field.label);
            Assert.AreEqual(32, field.maxLength);
            Assert.IsTrue(field.isDelayed);
            Assert.IsTrue(field.isReadOnly);
            Assert.AreEqual("Guid", field.textEdition.placeholder);
        }
#endif

        [Test]
        public void TextElement_StillResolvesITextEditionOverload()
        {
            var label = new Label().SetMaxLength(8);

            Assert.AreEqual(8, ((ITextEdition)label).maxLength);
        }

        [Test]
        public void SetMaxLength_TruncatesCurrentText()
        {
            var field = new TextField { value = "abcdef" }.SetMaxLength(3);

            Assert.AreEqual("abc", field.text);
        }

        [Test]
        public void TextField_MultilineSettersKeepFieldType()
        {
            // Unity ignores the scroller visibility until the field is multiline.
            TextField field = new TextField()
                .SetMultiline(true)
                .SetVerticalScrollerVisibilitySelf(ScrollerVisibility.AlwaysVisible);

            Assert.IsTrue(field.multiline);
            Assert.AreEqual(ScrollerVisibility.AlwaysVisible, field.verticalScrollerVisibility);
        }

        [Test]
        public void NumericFields_SetFormatStringResolvesTheirValueTypeOverload()
        {
            Assert.AreEqual("0000", new IntegerField().SetFormatString("0000").formatString);
            Assert.AreEqual("0000", new LongField().SetFormatString("0000").formatString);
            Assert.AreEqual("0000", new FloatField().SetFormatString("0000").formatString);
            Assert.AreEqual("0000", new DoubleField().SetFormatString("0000").formatString);
            Assert.AreEqual("0000", new UnsignedIntegerField().SetFormatString("0000").formatString);
            Assert.AreEqual("0000", new UnsignedLongField().SetFormatString("0000").formatString);
        }

        [Test]
        public void SetFormatString_FormatsDisplayedValue()
        {
            var field = new IntegerField { value = 7 }.SetFormatString("0000");

            Assert.AreEqual("0007", field.text);
        }

        [Test]
        public void CustomValueType_SetFormatStringUsesExplicitTypeArguments()
        {
            var field = new ShortField().SetFormatString<ShortField, short>("0000");

            Assert.AreEqual("0000", field.formatString);
        }
    }
}
