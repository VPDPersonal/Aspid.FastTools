using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    internal sealed class TextInputBaseFieldTextSelectionExtensionsTests
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
        public void TextField_ChainKeepsFieldTypeAndSetsSelectionProperties()
        {
            var field = new TextField()
                .SetSelectable(false)
                .SetSelectAllOnFocus(false)
                .SetSelectAllOnMouseUp(false)
                .SetDoubleClickSelectsWord(false)
                .SetTripleClickSelectsLine(false);

            Assert.IsFalse(field.textSelection.isSelectable);
            Assert.IsFalse(field.selectAllOnFocus);
            Assert.IsFalse(field.selectAllOnMouseUp);
            Assert.IsFalse(field.doubleClickSelectsWord);
            Assert.IsFalse(field.tripleClickSelectsLine);

            field
                .SetSelectable(true)
                .SetSelectAllOnFocus(true)
                .SetSelectAllOnMouseUp(true)
                .SetDoubleClickSelectsWord(true)
                .SetTripleClickSelectsLine(true);

            Assert.IsTrue(field.textSelection.isSelectable);
            Assert.IsTrue(field.selectAllOnFocus);
            Assert.IsTrue(field.selectAllOnMouseUp);
            Assert.IsTrue(field.doubleClickSelectsWord);
            Assert.IsTrue(field.tripleClickSelectsLine);
        }

        [Test]
        public void TextField_SetCursorAndSelectIndexKeepFieldType()
        {
            TextField field = new TextField()
                .SetCursorIndex(0)
                .SetSelectIndex(0);

            Assert.AreEqual(0, field.cursorIndex);
            Assert.AreEqual(0, field.selectIndex);
        }

        [Test]
        public void NumericFields_ResolveTheirValueTypeOverload()
        {
            Assert.IsFalse(new IntegerField().SetSelectAllOnFocus(false).selectAllOnFocus);
            Assert.IsFalse(new LongField().SetSelectAllOnFocus(false).selectAllOnFocus);
            Assert.IsFalse(new FloatField().SetSelectAllOnFocus(false).selectAllOnFocus);
            Assert.IsFalse(new DoubleField().SetSelectAllOnFocus(false).selectAllOnFocus);
            Assert.IsFalse(new UnsignedIntegerField().SetSelectAllOnFocus(false).selectAllOnFocus);
            Assert.IsFalse(new UnsignedLongField().SetSelectAllOnFocus(false).selectAllOnFocus);
            Assert.IsFalse(new Hash128Field().SetSelectAllOnFocus(false).selectAllOnFocus);
        }

        [Test]
        public void CustomValueType_UsesExplicitTypeArguments()
        {
            var field = new ShortField().SetDoubleClickSelectsWord<ShortField, short>(false);

            Assert.IsFalse(field.doubleClickSelectsWord);
        }

        [Test]
        public void TextElement_StillResolvesITextSelectionOverload()
        {
            var label = new Label().SetSelectable(true);

            Assert.IsTrue(((ITextSelection)label).isSelectable);
        }

#if UNITY_6000_3_OR_NEWER
        [Test]
        public void TextField_AddAndRemoveOnCursorIndexChange()
        {
            var calls = 0;
            void OnChange() => calls++;

            var field = new TextField().AddOnCursorIndexChange(OnChange);
            field.textSelection.cursorIndex = 3;

            field.RemoveOnCursorIndexChange(OnChange);
            field.textSelection.cursorIndex = 1;

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void TextField_AddAndRemoveOnSelectIndexChange()
        {
            var calls = 0;
            void OnChange() => calls++;

            var field = new TextField().AddOnSelectIndexChange(OnChange);
            field.textSelection.selectIndex = 3;

            field.RemoveOnSelectIndexChange(OnChange);
            field.textSelection.selectIndex = 1;

            Assert.AreEqual(1, calls);
        }
#endif
    }
}
