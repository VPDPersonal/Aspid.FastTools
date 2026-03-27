using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        #region Flex
        /// <summary>
        /// Sets the flex basis of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFlexBasis<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetFlexBasis(value);
            return element;
        }

        /// <summary>
        /// Sets the flex grow factor of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFlexGrow<T>(this T element, StyleFloat value)
            where T : VisualElement
        {
            element.style.SetFlexGrow(value);
            return element;
        }

        /// <summary>
        /// Sets the flex shrink factor of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFlexShrink<T>(this T element, StyleFloat value)
            where T : VisualElement
        {
            element.style.SetFlexShrink(value);
            return element;
        }

        /// <summary>
        /// Sets the flex wrap mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFlexWrap<T>(this T element, StyleEnum<Wrap> value)
            where T : VisualElement
        {
            element.style.SetFlexWrap(value);
            return element;
        }

        /// <summary>
        /// Sets the flex direction of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFlexDirection<T>(this T element, FlexDirection value)
            where T : VisualElement
        {
            element.style.SetFlexDirection(value);
            return element;
        }
        #endregion

        #region Size
        /// <summary>
        /// Sets both width and height of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetSize<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetSize(value);
            return element;
        }

        /// <summary>
        /// Sets the width and/or height of the element's style. Only non-null parameters are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetSize<T>(this T element, StyleLength? width = null, StyleLength? height = null)
            where T : VisualElement
        {
            element.style.SetSize(width, height);
            return element;
        }

        /// <summary>
        /// Sets both min-width and min-height of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMinSize<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinSize(value);
            return element;
        }

        /// <summary>
        /// Sets the min-width and/or min-height of the element's style. Only non-null parameters are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMinSize<T>(this T element, StyleLength? width = null, StyleLength? height = null)
            where T : VisualElement
        {
            element.style.SetMinSize(width, height);
            return element;
        }

        /// <summary>
        /// Sets both max-width and max-height of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMaxSize<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetMaxSize(value);
            return element;
        }

        /// <summary>
        /// Sets the max-width and/or max-height of the element's style. Only non-null parameters are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMaxSize<T>(this T element, StyleLength? width = null, StyleLength? height = null)
            where T : VisualElement
        {
            element.style.SetMaxSize(width, height);
            return element;
        }
        #endregion

        #region Font
        /// <summary>
        /// Sets the font of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityFont<T>(this T element, StyleFont value)
            where T : VisualElement
        {
            element.style.SetUnityFont(value);
            return element;
        }

        /// <summary>
        /// Sets the font size of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetFontSize<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetFontSize(value);
            return element;
        }

        /// <summary>
        /// Sets the font definition (font asset) of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityFontDefinition<T>(this T element, StyleFontDefinition value)
            where T : VisualElement
        {
            element.style.SetUnityFontDefinition(value);
            return element;
        }

        /// <summary>
        /// Sets the font style and weight of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityFontStyleAndWeight<T>(this T element, StyleEnum<FontStyle> value)
            where T : VisualElement
        {
            element.style.SetUnityFontStyleAndWeight(value);
            return element;
        }
        #endregion

        #region Text
        /// <summary>
        /// Sets the word spacing of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetWorldSpacing<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetWorldSpacing(value);
            return element;
        }

        /// <summary>
        /// Sets the letter spacing of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetLetterSpacing<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetLetterSpacing(value);
            return element;
        }

        /// <summary>
        /// Sets the text alignment of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityTextAlign<T>(this T element, TextAnchor value)
            where T : VisualElement
        {
            element.style.SetUnityTextAlign(value);
            return element;
        }

        /// <summary>
        /// Sets the text shadow of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTextShadow<T>(this T element, StyleTextShadow value)
            where T : VisualElement
        {
            element.style.SetTextShadow(value);
            return element;
        }

        /// <summary>
        /// Sets the text outline color of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityTextOutlineColor<T>(this T element, StyleColor value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineColor(value);
            return element;
        }

        /// <summary>
        /// Sets the text outline width of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityTextOutlineWidth<T>(this T element, StyleFloat value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the paragraph spacing of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityParagraphSpacing<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetUnityParagraphSpacing(value);
            return element;
        }

#if UNITY_6000_2_OR_NEWER
        /// <summary>
        /// Sets the text auto-size settings of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityTextAutoSize<T>(this T element, StyleTextAutoSize value)
            where T : VisualElement
        {
            element.style.SetUnityTextAutoSize(value);
            return element;
        }
#endif

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the text generator type of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityTextGenerator<T>(this T element, TextGeneratorType value)
            where T : VisualElement
        {
            element.style.SetUnityTextGenerator(value);
            return element;
        }

        /// <summary>
        /// Sets the editor text rendering mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityEditorTextRenderingMode<T>(this T element, EditorTextRenderingMode value)
            where T : VisualElement
        {
            element.style.SetUnityEditorTextRenderingMode(value);
            return element;
        }
#endif

        /// <summary>
        /// Sets the text overflow mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTextOverflow<T>(this T element, StyleEnum<TextOverflow> value)
            where T : VisualElement
        {
            element.style.SetTextOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets the text overflow position of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityTextOverflowPosition<T>(this T element, TextOverflowPosition value)
            where T : VisualElement
        {
            element.style.SetUnityTextOverflowPosition(value);
            return element;
        }
        #endregion

        #region Color
        /// <summary>
        /// Sets the text color of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetColor<T>(this T element, StyleColor value)
            where T : VisualElement
        {
            element.style.SetColor(value);
            return element;
        }

        /// <summary>
        /// Sets the opacity of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetOpacity<T>(this T element, StyleFloat value)
            where T : VisualElement
        {
            element.style.SetOpacity(value);
            return element;
        }
        #endregion

        #region Align
        /// <summary>
        /// Sets the align-self of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetAlignSelf<T>(this T element, StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignSelf(value);
            return element;
        }

        /// <summary>
        /// Sets the align-items of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetAlignItems<T>(this T element, StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignItems(value);
            return element;
        }

        /// <summary>
        /// Sets the align-content of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetAlignContent<T>(this T element, StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignContent(value);
            return element;
        }
        #endregion

        #region Border
        /// <summary>
        /// Sets the border color on all sides of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBorderColor<T>(this T element, StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColor(value);
            return element;
        }

        /// <summary>
        /// Sets the border color of the element's style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBorderColor<T>(this T element,
            StyleColor? top = null,
            StyleColor? bottom = null,
            StyleColor? left  = null,
            StyleColor? right = null)
            where T : VisualElement
        {
            element.style.SetBorderColor(top, bottom, left, right);
            return element;
        }

        /// <summary>
        /// Sets the border radius on all corners of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBorderRadius<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadius(value);
            return element;
        }

        /// <summary>
        /// Sets the border radius of the element's style. Only non-null corners are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBorderRadius<T>(this T element,
            StyleLength? topLeft = null,
            StyleLength? topRight = null,
            StyleLength? bottomLeft = null,
            StyleLength? bottomRight = null)
            where T : VisualElement
        {
            element.style.SetBorderRadius(topLeft,  topRight, bottomLeft, bottomRight);
            return element;
        }

        /// <summary>
        /// Sets the border width on all sides of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBorderWidth<T>(this T element, StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the border width of the element's style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBorderWidth<T>(this T element,
            StyleFloat? top = null,
            StyleFloat? bottom = null,
            StyleFloat? left = null,
            StyleFloat? right = null)
            where T : VisualElement
        {
            element.style.SetBorderWidth(top, bottom, left, right);
            return element;
        }
        #endregion

        #region Cursor
        /// <summary>
        /// Sets the cursor of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetCursor<T>(this T element, StyleCursor value)
            where T : VisualElement
        {
            element.style.SetCursor(value);
            return element;
        }
        #endregion

        #region Margin
        /// <summary>
        /// Sets the margin on all sides of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMargin<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetMargin(value);
            return element;
        }

        /// <summary>
        /// Sets the margin of the element's style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMargin<T>(this T element,
            StyleLength? top = null,
            StyleLength? bottom = null,
            StyleLength? left = null,
            StyleLength? right = null)
            where T : VisualElement
        {
            element.style.SetMargin(top, bottom, left, right);
            return element;
        }
        #endregion

        #region Padding
        /// <summary>
        /// Sets the padding on all sides of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetPadding<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetPadding(value);
            return element;
        }

        /// <summary>
        /// Sets the padding of the element's style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetPadding<T>(this T element,
            StyleLength? top = null,
            StyleLength? bottom = null,
            StyleLength? left = null,
            StyleLength? right = null)
            where T : VisualElement
        {
            element.style.SetPadding(top, bottom, left, right);
            return element;
        }
        #endregion

        #region Display
        /// <summary>
        /// Sets the display style of the element.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetDisplay<T>(this T element, DisplayStyle value)
            where T : VisualElement
        {
            element.style.SetDisplay(value);
            return element;
        }
        #endregion

        #region Overflow
        /// <summary>
        /// Sets the overflow mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetOverflow<T>(this T element, StyleEnum<Overflow> value)
            where T : VisualElement
        {
            element.style.SetOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets the overflow clip box of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityOverflowClipBox<T>(this T element, StyleEnum<OverflowClipBox> value)
            where T : VisualElement
        {
            element.style.SetUnityOverflowClipBox(value);
            return element;
        }
        #endregion

        #region Distance
        /// <summary>
        /// Sets the top, bottom, left, and right distances of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetDistance<T>(this T element, StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistance(value);
            return element;
        }

        /// <summary>
        /// Sets the positional distances of the element's style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetDistance<T>(this T element,
            StyleLength? top = null,
            StyleLength? bottom = null,
            StyleLength? left = null,
            StyleLength? right = null)
            where T : VisualElement
        {
            element.style.SetDistance(top, bottom, left, right);
            return element;
        }
        #endregion

        #region Transform
        /// <summary>
        /// Sets the scale transform of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetScale<T>(this T element, StyleScale value)
            where T : VisualElement
        {
            element.style.SetScale(value);
            return element;
        }

        /// <summary>
        /// Sets the rotation transform of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetRotate<T>(this T element, StyleRotate value)
            where T : VisualElement
        {
            element.style.SetRotate(value);
            return element;
        }

        /// <summary>
        /// Sets the translation transform of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTranslate<T>(this T element, StyleTranslate value)
            where T : VisualElement
        {
            element.style.SetTranslate(value);
            return element;
        }

        /// <summary>
        /// Sets the positioning mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetPosition<T>(this T element, StyleEnum<Position> value)
            where T : VisualElement
        {
            element.style.SetPosition(value);
            return element;
        }

        /// <summary>
        /// Sets the transform origin of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTransformOrigin<T>(this T element, StyleTransformOrigin value)
            where T : VisualElement
        {
            element.style.SetTransformOrigin(value);
            return element;
        }
        #endregion

        #region Background
        /// <summary>
        /// Sets the background color of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBackgroundColor<T>(this T element, StyleColor value)
            where T : VisualElement
        {
            element.style.SetBackgroundColor(value);
            return element;
        }

        /// <summary>
        /// Sets the background image of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBackgroundImage<T>(this T element, StyleBackground value)
            where T : VisualElement
        {
            element.style.SetBackgroundImage(value);
            return element;
        }

        /// <summary>
        /// Sets the background size of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBackgroundSize<T>(this T element, StyleBackgroundSize value)
            where T : VisualElement
        {
            element.style.SetBackgroundSize(value);
            return element;
        }

        /// <summary>
        /// Sets the background repeat mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBackgroundRepeat<T>(this T element, StyleBackgroundRepeat value)
            where T : VisualElement
        {
            element.style.SetBackgroundRepeat(value);
            return element;
        }

        /// <summary>
        /// Sets the background image tint color of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnityBackgroundImageTintColor<T>(this T element, StyleColor value)
            where T : VisualElement
        {
            element.style.SetUnityBackgroundImageTintColor(value);
            return element;
        }

        /// <summary>
        /// Sets both X and Y background positions of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBackgroundPosition<T>(this T element, StyleBackgroundPosition value)
            where T : VisualElement
        {
            element.style.SetBackgroundPosition(value);
            return element;
        }

        /// <summary>
        /// Sets the background position of the element's style. Only non-null axes are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBackgroundPosition<T>(this T element,
            StyleBackgroundPosition? x = null,
            StyleBackgroundPosition? y = null)
            where T : VisualElement
        {
            element.style.SetBackgroundPosition(x, y);
            return element;
        }
        #endregion

        #region Transition
        /// <summary>
        /// Sets the transition delay of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTransitionDelay<T>(this T element, StyleList<TimeValue> value)
            where T : VisualElement
        {
            element.style.SetTransitionDelay(value);
            return element;
        }

        /// <summary>
        /// Sets the transition duration of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTransitionDuration<T>(this T element, StyleList<TimeValue> value)
            where T : VisualElement
        {
            element.style.SetTransitionDuration(value);
            return element;
        }

        /// <summary>
        /// Sets the transition property list of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTransitionProperty<T>(this T element, StyleList<StylePropertyName> value)
            where T : VisualElement
        {
            element.style.SetTransitionProperty(value);
            return element;
        }

        /// <summary>
        /// Sets the transition timing function of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetTransitionTimingFunction<T>(this T element, StyleList<EasingFunction> value)
            where T : VisualElement
        {
            element.style.SetTransitionTimingFunction(value);
            return element;
        }
        #endregion

        #region UnitySlice
        /// <summary>
        /// Sets the 9-slice insets on all sides of the element's style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnitySlice<T>(this T element, StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySlice(value);
            return element;
        }

        /// <summary>
        /// Sets the 9-slice insets of the element's style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnitySlice<T>(this T element,
            StyleInt? top = null,
            StyleInt? bottom = null,
            StyleInt? left = null,
            StyleInt? right = null)
            where T : VisualElement
        {
            element.style.SetUnitySlice(top, bottom, left, right);
            return element;
        }

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the 9-slice type of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetUnitySliceType<T>(this T element, StyleEnum<SliceType> value)
            where T : VisualElement
        {
            element.style.SetUnitySliceType(value);
            return element;
        }
#endif
        #endregion

        #region Visibility
        /// <summary>
        /// Sets the CSS visibility of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetVisibility<T>(this T element, StyleEnum<Visibility> value)
            where T : VisualElement
        {
            element.style.SetVisibility(value);
            return element;
        }
        #endregion

        #region WhiteSpace
        /// <summary>
        /// Sets the white-space mode of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetWhiteSpace<T>(this T element, StyleEnum<WhiteSpace> value)
            where T : VisualElement
        {
            element.style.SetWhiteSpace(value);
            return element;
        }
        #endregion

        #region JustifyContent
        /// <summary>
        /// Sets the justify-content of the element's style.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetJustifyContent<T>(this T element, StyleEnum<Justify> value)
            where T : VisualElement
        {
            element.style.SetJustifyContent(value);
            return element;
        }
        #endregion
    }
}