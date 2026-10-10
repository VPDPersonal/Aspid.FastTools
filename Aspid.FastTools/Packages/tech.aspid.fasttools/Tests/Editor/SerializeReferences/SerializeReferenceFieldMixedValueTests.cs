using System;
using UnityEditor;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Under a multi-object selection with different types the caption shows "—", which Unity's theme tints through
    // the class its own EnumField puts on the caption; a class Unity does not declare leaves the dash untinted.
    [TestFixture]
    internal sealed class SerializeReferenceFieldMixedValueTests
    {
        private static readonly string MixedValueClass = BaseField<object>.mixedValueLabelUssClassName;

        [Serializable]
        internal sealed class MixedValueShield : ITestWeapon { }

        [Test]
        public void UnityEnumField_PutsTheMixedValueClassOnItsCaption()
        {
            var enumField = new EnumField(FontStyle.Normal) { showMixedValue = true };

            var caption = enumField.Q<TextElement>(className: EnumField.textUssClassName);

            Assert.IsTrue(caption.ClassListContains(MixedValueClass),
                "The field mirrors Unity's EnumField; update it if Unity moved the mixed-value class.");
        }

        [Test]
        public void DifferentTypes_CaptionCarriesTheMixedValueClass()
        {
            var first = ScriptableObject.CreateInstance<LinkerTestObject>();
            var second = ScriptableObject.CreateInstance<LinkerTestObject>();

            try
            {
                first.a = new TestSword();
                second.a = new MixedValueShield();

                var field = CreateField(first, second);
                var caption = field.Q<TextElement>(className: EnumField.textUssClassName);

                Assert.AreEqual("—", caption.text);
                Assert.IsTrue(caption.ClassListContains(MixedValueClass),
                    "The caption must carry Unity's mixed-value class so the theme tints the dash.");
                Assert.IsTrue(caption.parent.ClassListContains(BaseField<object>.inputUssClassName),
                    "Unity's mixed-value rule matches only a direct child of the input, so the caption's parent must carry its class.");
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void SingleObject_CaptionDoesNotCarryTheMixedValueClass()
        {
            var target = ScriptableObject.CreateInstance<LinkerTestObject>();

            try
            {
                target.a = new TestSword();

                var field = CreateField(target);
                var caption = field.Q<TextElement>(className: EnumField.textUssClassName);

                Assert.AreNotEqual("—", caption.text);
                Assert.IsFalse(caption.ClassListContains(MixedValueClass));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        private static SerializeReferenceField CreateField(params Object[] targets)
        {
            var serialized = new SerializedObject(targets);
            return new SerializeReferenceField("A", serialized.FindProperty("a"));
        }
    }
}
