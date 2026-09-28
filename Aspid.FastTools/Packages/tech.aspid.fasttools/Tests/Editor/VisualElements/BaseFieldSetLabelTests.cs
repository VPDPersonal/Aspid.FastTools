using System;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Aspid.FastTools.UIElements.Tests
{
    [TestFixture]
    internal sealed class BaseFieldSetLabelTests
    {
        private const string Label = "Label";

        // Every built-in runtime field; each call must resolve a SetLabel overload without type arguments.
        private static IEnumerable<TestCaseData> Fields()
        {
            yield return Case(() => new Toggle().SetLabel(Label));
            yield return Case(() => new TextField().SetLabel(Label));
            yield return Case(() => new IntegerField().SetLabel(Label));
            yield return Case(() => new LongField().SetLabel(Label));
            yield return Case(() => new UnsignedIntegerField().SetLabel(Label));
            yield return Case(() => new UnsignedLongField().SetLabel(Label));
            yield return Case(() => new FloatField().SetLabel(Label));
            yield return Case(() => new DoubleField().SetLabel(Label));
            yield return Case(() => new Hash128Field().SetLabel(Label));
            yield return Case(() => new EnumField().SetLabel(Label));
            yield return Case(() => new Slider().SetLabel(Label));
            yield return Case(() => new SliderInt().SetLabel(Label));
            yield return Case(() => new MinMaxSlider().SetLabel(Label));
            yield return Case(() => new RadioButtonGroup().SetLabel(Label));
            yield return Case(() => new DropdownField().SetLabel(Label));
            yield return Case(() => new RectField().SetLabel(Label));
            yield return Case(() => new RectIntField().SetLabel(Label));
            yield return Case(() => new BoundsField().SetLabel(Label));
            yield return Case(() => new BoundsIntField().SetLabel(Label));
            yield return Case(() => new Vector2Field().SetLabel(Label));
            yield return Case(() => new Vector3Field().SetLabel(Label));
            yield return Case(() => new Vector4Field().SetLabel(Label));
            yield return Case(() => new Vector2IntField().SetLabel(Label));
            yield return Case(() => new Vector3IntField().SetLabel(Label));
            yield return Case(() => new ToggleButtonGroup().SetLabel(Label));
        }

        private static TestCaseData Case<TField>(Func<TField> create)
            where TField : VisualElement =>
            new TestCaseData(new Func<VisualElement>(create)).SetName($"SetLabel_{typeof(TField).Name}");

        [TestCaseSource(nameof(Fields))]
        public void SetLabel_ResolvesWithoutTypeArguments(Func<VisualElement> create)
        {
            var field = create();

            Assert.AreEqual(Label, field.Q<Label>(className: BaseField<int>.labelUssClassName)?.text);
        }
    }
}
