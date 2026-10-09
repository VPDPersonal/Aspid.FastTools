using System;
using System.Linq;
using UnityEngine;
using NUnit.Framework;
using System.Reflection;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Aspid.FastTools.UIElements.Tests
{
    /// <summary>
    /// Anchors for the <see cref="IStyle"/> wrappers: the bold and italic transitions, the shorthand families
    /// (<c>X</c>, <c>Y</c> and the single sides), the string color overloads and the Resources helper.
    /// </summary>
    [TestFixture]
    internal sealed class IStyleExtensionsTests
    {
        private const string Top = "Top";
        private const string Left = "Left";
        private const string Right = "Right";
        private const string Bottom = "Bottom";

        private const string ExistingTexturePath = "Icons/aspid_icon_home";
        private const string MissingTexturePath = "Icons/aspid_icon_missing";

        // An obsolete property such as unityBackgroundScaleMode reports a value that nobody set.
        private static readonly PropertyInfo[] StyleProperties = typeof(IStyle)
            .GetProperties()
            .Where(property => !property.IsDefined(typeof(ObsoleteAttribute), inherit: false))
            .ToArray();

        #region Bold and italic
        [TestCase(FontStyle.Normal, FontStyle.Bold)]
        [TestCase(FontStyle.Italic, FontStyle.BoldAndItalic)]
        [TestCase(FontStyle.Bold, FontStyle.Bold)]
        [TestCase(FontStyle.BoldAndItalic, FontStyle.BoldAndItalic)]
        public void AddBold_Transitions(FontStyle current, FontStyle expected) =>
            AssertTransition(current, expected, style => style.AddBoldUnityFontStyleAndWeight(), element => element.AddBoldUnityFontStyleAndWeight());

        [TestCase(FontStyle.Bold, FontStyle.Normal)]
        [TestCase(FontStyle.BoldAndItalic, FontStyle.Italic)]
        [TestCase(FontStyle.Normal, FontStyle.Normal)]
        [TestCase(FontStyle.Italic, FontStyle.Italic)]
        public void RemoveBold_Transitions(FontStyle current, FontStyle expected) =>
            AssertTransition(current, expected, style => style.RemoveBoldUnityFontStyleAndWeight(), element => element.RemoveBoldUnityFontStyleAndWeight());

        [TestCase(FontStyle.Normal, FontStyle.Italic)]
        [TestCase(FontStyle.Bold, FontStyle.BoldAndItalic)]
        [TestCase(FontStyle.Italic, FontStyle.Italic)]
        [TestCase(FontStyle.BoldAndItalic, FontStyle.BoldAndItalic)]
        public void AddItalic_Transitions(FontStyle current, FontStyle expected) =>
            AssertTransition(current, expected, style => style.AddItalicUnityFontStyleAndWeight(), element => element.AddItalicUnityFontStyleAndWeight());

        [TestCase(FontStyle.Italic, FontStyle.Normal)]
        [TestCase(FontStyle.BoldAndItalic, FontStyle.Bold)]
        [TestCase(FontStyle.Normal, FontStyle.Normal)]
        [TestCase(FontStyle.Bold, FontStyle.Bold)]
        public void RemoveItalic_Transitions(FontStyle current, FontStyle expected) =>
            AssertTransition(current, expected, style => style.RemoveItalicUnityFontStyleAndWeight(), element => element.RemoveItalicUnityFontStyleAndWeight());

        [TestCase(FontStyle.Normal)]
        [TestCase(FontStyle.Bold)]
        [TestCase(FontStyle.Italic)]
        [TestCase(FontStyle.BoldAndItalic)]
        public void SetNormal_ClearsBoldAndItalic(FontStyle current) =>
            AssertTransition(current, FontStyle.Normal, style => style.SetNormalUnityFontStyleAndWeight(), element => element.SetNormalUnityFontStyleAndWeight());

        [Test]
        public void AddBold_WithoutInlineValue_WritesBold()
        {
            var element = new VisualElement();

            element.style.AddBoldUnityFontStyleAndWeight();

            Assert.AreEqual(FontStyle.Bold, element.style.unityFontStyleAndWeight.value);
        }

        [Test]
        public void RemoveBold_WithoutInlineValue_WritesNothing()
        {
            var element = new VisualElement();

            element.style.RemoveBoldUnityFontStyleAndWeight();

            Assert.AreEqual(StyleKeyword.Null, element.style.unityFontStyleAndWeight.keyword);
        }

        private static void AssertTransition(
            FontStyle current,
            FontStyle expected,
            Func<IStyle, IStyle> viaStyle,
            Func<VisualElement, VisualElement> viaElement)
        {
            var styleOwner = new VisualElement();
            styleOwner.style.unityFontStyleAndWeight = current;
            var style = viaStyle(styleOwner.style);

            var element = new VisualElement();
            element.style.unityFontStyleAndWeight = current;
            var result = viaElement(element);

            Assert.AreEqual(expected, style.unityFontStyleAndWeight.value, "IStyle overload");
            Assert.AreSame(element, result, "The VisualElement overload must return the element for chaining.");
            Assert.AreEqual(expected, element.style.unityFontStyleAndWeight.value, "VisualElement overload");
        }
        #endregion

        #region Shorthand families
        [TestCaseSource(nameof(ShorthandCases))]
        public void Shorthand_WritesOnlyItsProperties(Action<IStyle> apply, string[] expected)
        {
            var style = new VisualElement().style;

            apply(style);

            CollectionAssert.AreEquivalent(expected, GetSetPropertyNames(style));
            Assert.AreEqual(1, GetSetValues(style).Distinct().Count(), "Every property of the shorthand must get the same value.");
        }

        private static IEnumerable<TestCaseData> ShorthandCases()
        {
            // Margin, padding and distance.
            yield return Case("SetMargin", style => style.SetMargin(3f), Names("margin", "", Top, Right, Bottom, Left));
            yield return Case("SetMarginX", style => style.SetMarginX(3f), Names("margin", "", Right, Left));
            yield return Case("SetMarginY", style => style.SetMarginY(3f), Names("margin", "", Top, Bottom));
            yield return Case("SetMarginTop", style => style.SetMarginTop(3f), Names("margin", "", Top));
            yield return Case("SetMarginRight", style => style.SetMarginRight(3f), Names("margin", "", Right));
            yield return Case("SetMarginBottom", style => style.SetMarginBottom(3f), Names("margin", "", Bottom));
            yield return Case("SetMarginLeft", style => style.SetMarginLeft(3f), Names("margin", "", Left));

            yield return Case("SetPadding", style => style.SetPadding(3f), Names("padding", "", Top, Right, Bottom, Left));
            yield return Case("SetPaddingX", style => style.SetPaddingX(3f), Names("padding", "", Right, Left));
            yield return Case("SetPaddingY", style => style.SetPaddingY(3f), Names("padding", "", Top, Bottom));
            yield return Case("SetPaddingTop", style => style.SetPaddingTop(3f), Names("padding", "", Top));
            yield return Case("SetPaddingRight", style => style.SetPaddingRight(3f), Names("padding", "", Right));
            yield return Case("SetPaddingBottom", style => style.SetPaddingBottom(3f), Names("padding", "", Bottom));
            yield return Case("SetPaddingLeft", style => style.SetPaddingLeft(3f), Names("padding", "", Left));

            yield return Case("SetDistance", style => style.SetDistance(3f), "top", "right", "bottom", "left");
            yield return Case("SetDistanceX", style => style.SetDistanceX(3f), "right", "left");
            yield return Case("SetDistanceY", style => style.SetDistanceY(3f), "top", "bottom");
            yield return Case("SetTop", style => style.SetTop(3f), "top");
            yield return Case("SetRight", style => style.SetRight(3f), "right");
            yield return Case("SetBottom", style => style.SetBottom(3f), "bottom");
            yield return Case("SetLeft", style => style.SetLeft(3f), "left");

            // Border.
            yield return Case("SetBorderWidth", style => style.SetBorderWidth(3f), Names("border", "Width", Top, Right, Bottom, Left));
            yield return Case("SetBorderWidthX", style => style.SetBorderWidthX(3f), Names("border", "Width", Right, Left));
            yield return Case("SetBorderWidthY", style => style.SetBorderWidthY(3f), Names("border", "Width", Top, Bottom));
            yield return Case("SetBorderWidthTop", style => style.SetBorderWidthTop(3f), Names("border", "Width", Top));
            yield return Case("SetBorderWidthRight", style => style.SetBorderWidthRight(3f), Names("border", "Width", Right));
            yield return Case("SetBorderWidthBottom", style => style.SetBorderWidthBottom(3f), Names("border", "Width", Bottom));
            yield return Case("SetBorderWidthLeft", style => style.SetBorderWidthLeft(3f), Names("border", "Width", Left));

            yield return Case("SetBorderRadius", style => style.SetBorderRadius(3f), Names("border", "Radius", Top + Left, Top + Right, Bottom + Right, Bottom + Left));
            yield return Case("SetBorderRadiusTop", style => style.SetBorderRadiusTop(3f), Names("border", "Radius", Top + Left, Top + Right));
            yield return Case("SetBorderRadiusBottom", style => style.SetBorderRadiusBottom(3f), Names("border", "Radius", Bottom + Left, Bottom + Right));
            yield return Case("SetBorderRadiusLeft", style => style.SetBorderRadiusLeft(3f), Names("border", "Radius", Top + Left, Bottom + Left));
            yield return Case("SetBorderRadiusRight", style => style.SetBorderRadiusRight(3f), Names("border", "Radius", Top + Right, Bottom + Right));
            yield return Case("SetBorderRadiusTopLeft", style => style.SetBorderRadiusTopLeft(3f), Names("border", "Radius", Top + Left));
            yield return Case("SetBorderRadiusTopRight", style => style.SetBorderRadiusTopRight(3f), Names("border", "Radius", Top + Right));
            yield return Case("SetBorderRadiusBottomLeft", style => style.SetBorderRadiusBottomLeft(3f), Names("border", "Radius", Bottom + Left));
            yield return Case("SetBorderRadiusBottomRight", style => style.SetBorderRadiusBottomRight(3f), Names("border", "Radius", Bottom + Right));

            yield return Case("SetBorderColor", style => style.SetBorderColor(Color.red), Names("border", "Color", Top, Right, Bottom, Left));
            yield return Case("SetBorderColorX", style => style.SetBorderColorX(Color.red), Names("border", "Color", Right, Left));
            yield return Case("SetBorderColorY", style => style.SetBorderColorY(Color.red), Names("border", "Color", Top, Bottom));
            yield return Case("SetBorderColorTop", style => style.SetBorderColorTop(Color.red), Names("border", "Color", Top));
            yield return Case("SetBorderColorRight", style => style.SetBorderColorRight(Color.red), Names("border", "Color", Right));
            yield return Case("SetBorderColorBottom", style => style.SetBorderColorBottom(Color.red), Names("border", "Color", Bottom));
            yield return Case("SetBorderColorLeft", style => style.SetBorderColorLeft(Color.red), Names("border", "Color", Left));

            yield return Case("SetBorderColor(string)", style => style.SetBorderColor("#FF0000"), Names("border", "Color", Top, Right, Bottom, Left));
            yield return Case("SetBorderColorX(string)", style => style.SetBorderColorX("#FF0000"), Names("border", "Color", Right, Left));
            yield return Case("SetBorderColorY(string)", style => style.SetBorderColorY("#FF0000"), Names("border", "Color", Top, Bottom));
            yield return Case("SetBorderColorTop(string)", style => style.SetBorderColorTop("#FF0000"), Names("border", "Color", Top));
            yield return Case("SetBorderColorRight(string)", style => style.SetBorderColorRight("#FF0000"), Names("border", "Color", Right));
            yield return Case("SetBorderColorBottom(string)", style => style.SetBorderColorBottom("#FF0000"), Names("border", "Color", Bottom));
            yield return Case("SetBorderColorLeft(string)", style => style.SetBorderColorLeft("#FF0000"), Names("border", "Color", Left));

            // Size.
            yield return Case("SetSize", style => style.SetSize(3f), "width", "height");
            yield return Case("SetWidth", style => style.SetWidth(3f), "width");
            yield return Case("SetHeight", style => style.SetHeight(3f), "height");
            yield return Case("SetMinSize", style => style.SetMinSize(3f), "minWidth", "minHeight");
            yield return Case("SetMaxSize", style => style.SetMaxSize(3f), "maxWidth", "maxHeight");
            yield return Case("SetMinWidth", style => style.SetMinWidth(3f), "minWidth");
            yield return Case("SetMinHeight", style => style.SetMinHeight(3f), "minHeight");
            yield return Case("SetMaxWidth", style => style.SetMaxWidth(3f), "maxWidth");
            yield return Case("SetMaxHeight", style => style.SetMaxHeight(3f), "maxHeight");

            // Slice.
            yield return Case("SetUnitySlice", style => style.SetUnitySlice(3), Names("unitySlice", "", Top, Right, Bottom, Left));
            yield return Case("SetUnitySliceX", style => style.SetUnitySliceX(3), Names("unitySlice", "", Right, Left));
            yield return Case("SetUnitySliceY", style => style.SetUnitySliceY(3), Names("unitySlice", "", Top, Bottom));
            yield return Case("SetUnitySliceTop", style => style.SetUnitySliceTop(3), Names("unitySlice", "", Top));
            yield return Case("SetUnitySliceRight", style => style.SetUnitySliceRight(3), Names("unitySlice", "", Right));
            yield return Case("SetUnitySliceBottom", style => style.SetUnitySliceBottom(3), Names("unitySlice", "", Bottom));
            yield return Case("SetUnitySliceLeft", style => style.SetUnitySliceLeft(3), Names("unitySlice", "", Left));

            // Colors from a string.
            yield return Case("SetColor(string)", style => style.SetColor("#FF0000"), "color");
            yield return Case("SetBackgroundColor(string)", style => style.SetBackgroundColor("#FF0000"), "backgroundColor");
            yield return Case("SetUnityBackgroundImageTintColor(string)", style => style.SetUnityBackgroundImageTintColor("#FF0000"), "unityBackgroundImageTintColor");
            yield return Case("SetUnityTextOutlineColor(string)", style => style.SetUnityTextOutlineColor("#FF0000"), "unityTextOutlineColor");
        }

        [Test]
        public void SetMargin_NamedSides_WriteOnlyTheGivenSides()
        {
            var style = new VisualElement().style;

            style.SetMargin(top: 1f, left: 4f);

            Assert.AreEqual(new Length(1f), style.marginTop.value);
            Assert.AreEqual(new Length(4f), style.marginLeft.value);
            CollectionAssert.AreEquivalent(new[] { "marginTop", "marginLeft" }, GetSetPropertyNames(style));
        }

        [Test]
        public void SetBorderColor_NamedSides_KeepEachColorOnItsSide()
        {
            var style = new VisualElement().style;

            style.SetBorderColor(top: Color.red, right: Color.green, bottom: Color.blue, left: Color.white);

            Assert.AreEqual(Color.red, style.borderTopColor.value);
            Assert.AreEqual(Color.green, style.borderRightColor.value);
            Assert.AreEqual(Color.blue, style.borderBottomColor.value);
            Assert.AreEqual(Color.white, style.borderLeftColor.value);
        }

        [Test]
        public void SetBorderRadius_NamedCorners_KeepEachRadiusOnItsCorner()
        {
            var style = new VisualElement().style;

            style.SetBorderRadius(topLeft: 1f, topRight: 2f, bottomRight: 3f, bottomLeft: 4f);

            Assert.AreEqual(new Length(1f), style.borderTopLeftRadius.value);
            Assert.AreEqual(new Length(2f), style.borderTopRightRadius.value);
            Assert.AreEqual(new Length(3f), style.borderBottomRightRadius.value);
            Assert.AreEqual(new Length(4f), style.borderBottomLeftRadius.value);
        }

        [Test]
        public void SetSize_NamedAxes_KeepEachLengthOnItsAxis()
        {
            var style = new VisualElement().style;

            style.SetSize(width: 10f, height: 20f).SetMinSize(minWidth: 1f, minHeight: 2f).SetMaxSize(maxWidth: 30f, maxHeight: 40f);

            Assert.AreEqual(new Length(10f), style.width.value);
            Assert.AreEqual(new Length(20f), style.height.value);
            Assert.AreEqual(new Length(1f), style.minWidth.value);
            Assert.AreEqual(new Length(2f), style.minHeight.value);
            Assert.AreEqual(new Length(30f), style.maxWidth.value);
            Assert.AreEqual(new Length(40f), style.maxHeight.value);
        }

        [Test]
        public void SetBorderColor_InvalidString_LogsWarningAndKeepsStyle()
        {
            var style = new VisualElement().style;
            LogAssert.Expect(LogType.Warning, "Failed to parse color string: 'not-a-color'");

            style.SetBorderColor("not-a-color").SetBorderColorX("not-a-color");

            Assert.IsEmpty(GetSetPropertyNames(style));
        }

        private static TestCaseData Case(string name, Action<IStyle> apply, params string[] properties) =>
            new TestCaseData(apply, properties).SetName(name);

        private static string[] Names(string prefix, string suffix, params string[] sides) =>
            sides.Select(side => prefix + side + suffix).ToArray();
        #endregion

        #region Resources
        [Test]
        public void SetBackgroundImageFromResources_ExistingPath_SetsTexture()
        {
            var expected = Resources.Load<Texture2D>(ExistingTexturePath);
            Assert.IsNotNull(expected, "The test texture must be loadable from Resources.");
            var element = new VisualElement();

            element.style.SetBackgroundImageFromResources(ExistingTexturePath);

            Assert.AreSame(expected, element.style.backgroundImage.value.texture);
        }

        [Test]
        public void SetBackgroundImageFromResources_MissingPath_LogsWarningAndKeepsStyle()
        {
            var element = new VisualElement();
            LogAssert.Expect(LogType.Warning, $"Failed to load Texture2D from Resources path: '{MissingTexturePath}'");

            element.style.SetBackgroundImageFromResources(MissingTexturePath);

            Assert.AreEqual(StyleKeyword.Null, element.style.backgroundImage.keyword);
        }

        [Test]
        public void ElementSetBackgroundImageFromResources_ExistingPath_ReturnsElementAndSetsTexture()
        {
            var element = new VisualElement();

            var result = element.SetBackgroundImageFromResources(ExistingTexturePath);

            Assert.AreSame(element, result);
            Assert.AreSame(Resources.Load<Texture2D>(ExistingTexturePath), element.style.backgroundImage.value.texture);
        }
        #endregion

        #region VisualElement wrappers
        [Test]
        public void VisualElementWrappers_WriteTheSameStyleAsTheIStyleOverloads()
        {
            var styleMethods = typeof(IStyleExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.IsGenericMethodDefinition)
                .ToArray();

            var mismatches = new List<string>();
            var compared = 0;

            // A wrapper without an IStyle twin (SetName, SetTooltip and so on) is not a style wrapper.
            foreach (var elementMethod in typeof(VisualElementExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (!elementMethod.IsGenericMethodDefinition) continue;

                var twin = styleMethods.FirstOrDefault(style => IsTwin(style, elementMethod));
                if (twin is null) continue;

                var parameters = twin.GetParameters();
                var arguments = parameters
                    .Skip(1)
                    .Select((parameter, index) => CreateArgument(parameter.ParameterType, parameter.Name, index))
                    .ToArray();

                var viaStyle = new VisualElement();
                var viaElement = new VisualElement();

                twin.MakeGenericMethod(typeof(IStyle)).Invoke(null, Prepend(viaStyle.style, arguments));
                elementMethod.MakeGenericMethod(typeof(VisualElement)).Invoke(null, Prepend(viaElement, arguments));
                compared++;

                var expected = Snapshot(viaStyle.style);
                var actual = Snapshot(viaElement.style);
                if (!expected.SequenceEqual(actual))
                {
                    mismatches.Add($"{Describe(twin)}: IStyle wrote [{string.Join(", ", expected)}], " +
                                   $"VisualElement wrote [{string.Join(", ", actual)}]");
                }
            }

            Assert.Greater(compared, 100, "The reflection lookup must find the IStyle twins of the VisualElement wrappers.");
            Assert.IsEmpty(mismatches, string.Join(Environment.NewLine, mismatches));
        }

        private static bool IsTwin(MethodInfo style, MethodInfo element) =>
            style.Name == element.Name
            && style.GetParameters().Skip(1).Select(parameter => parameter.ParameterType)
                .SequenceEqual(element.GetParameters().Skip(1).Select(parameter => parameter.ParameterType));

        private static object[] Prepend(object first, object[] rest) =>
            new[] { first }.Concat(rest).ToArray();

        private static string Describe(MethodInfo method) =>
            $"{method.Name}({string.Join(", ", method.GetParameters().Skip(1).Select(parameter => parameter.ParameterType.Name))})";

        // Each argument is distinct, so a wrapper that swaps two sides writes a different snapshot.
        private static object CreateArgument(Type type, string name, int index)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            var number = index + 1f;

            if (type == typeof(string)) return name == "path" ? ExistingTexturePath : "#336699";
            if (type == typeof(StyleInt)) return new StyleInt(index + 1);
            if (type == typeof(StyleFloat)) return new StyleFloat(number);
            if (type == typeof(StyleLength)) return new StyleLength(number);
            if (type == typeof(StyleColor)) return new StyleColor(new Color(number / 10f, 0f, 0f));
            if (type.IsEnum) return Enum.GetValues(type).Cast<object>().Last();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(StyleEnum<>))
                return Activator.CreateInstance(type, Enum.GetValues(type.GetGenericArguments()[0]).Cast<object>().Last());

            return Activator.CreateInstance(type);
        }
        #endregion

        #region Style snapshot
        private static string[] GetSetPropertyNames(IStyle style) =>
            StyleProperties
                .Where(property => IsSet(property.GetValue(style)))
                .Select(property => property.Name)
                .ToArray();

        private static string[] GetSetValues(IStyle style) =>
            StyleProperties
                .Select(property => property.GetValue(style))
                .Where(IsSet)
                .Select(GetValue)
                .ToArray();

        private static string[] Snapshot(IStyle style) =>
            StyleProperties
                .Where(property => IsSet(property.GetValue(style)))
                .Select(property => $"{property.Name}={GetValue(property.GetValue(style))}")
                .ToArray();

        // A style value that is not set inline reports the Null keyword.
        private static bool IsSet(object value) =>
            (StyleKeyword)value.GetType().GetProperty("keyword").GetValue(value) != StyleKeyword.Null;

        private static string GetValue(object value)
        {
            var type = value.GetType();
            return $"{type.GetProperty("keyword").GetValue(value)}:{type.GetProperty("value").GetValue(value)}";
        }
        #endregion
    }
}
