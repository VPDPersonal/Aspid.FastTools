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
    }
}
