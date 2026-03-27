using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static class IStyleExtensions
    {
        #region Flex
        /// <summary>
        /// Sets the flex basis of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetFlexBasis<T>(this T style, StyleLength value)
            where T : IStyle
        {
            style.flexBasis = value;
            return style;
        }

        /// <summary>
        /// Sets the flex grow factor of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetFlexGrow<T>(this T style, StyleFloat value)
            where T : IStyle
        {
            style.flexGrow = value;
            return style;
        }

        /// <summary>
        /// Sets the flex shrink factor of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetFlexShrink<T>(this T style, StyleFloat value)
            where T : IStyle
        {
            style.flexShrink = value;
            return style;
        }

        /// <summary>
        /// Sets the flex wrap mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetFlexWrap<T>(this T style, StyleEnum<Wrap> value)
            where T : IStyle
        {
            style.flexWrap = value;
            return style;
        }

        /// <summary>
        /// Sets the flex direction of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetFlexDirection<T>(this T style, FlexDirection value)
            where T : IStyle
        {
            style.flexDirection = value;
            return style;
        }
        #endregion

        #region Size
        /// <summary>
        /// Sets both width and height of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetSize<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetSize(width: value, height: value);
        }

        /// <summary>
        /// Sets the width and/or height of the style. Only non-null parameters are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetSize<T>(this T style, StyleLength? width = null, StyleLength? height = null)
            where T : IStyle
        {
            if (width.HasValue) style.width = width.Value;
            if (height.HasValue) style.height = height.Value;

            return style;
        }

        /// <summary>
        /// Sets both min-width and min-height of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetMinSize<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetMinSize(width: value, height: value);
        }

        /// <summary>
        /// Sets the min-width and/or min-height of the style. Only non-null parameters are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetMinSize<T>(this T style, StyleLength? width = null, StyleLength? height = null)
            where T : IStyle
        {
            if (width.HasValue) style.minWidth = width.Value;
            if (height.HasValue) style.minHeight = height.Value;

            return style;
        }

        /// <summary>
        /// Sets both max-width and max-height of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetMaxSize<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetMaxSize(width: value, height: value);
        }

        /// <summary>
        /// Sets the max-width and/or max-height of the style. Only non-null parameters are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetMaxSize<T>(this T style, StyleLength? width = null, StyleLength? height = null)
            where T : IStyle
        {
            if (width.HasValue) style.maxWidth = width.Value;
            if (height.HasValue) style.maxHeight = height.Value;

            return style;
        }
        #endregion

        #region Font
        /// <summary>
        /// Sets the font of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityFont<T>(this T style, StyleFont value)
            where T : IStyle
        {
            style.unityFont = value;
            return style;
        }

        /// <summary>
        /// Sets the font size of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetFontSize<T>(this T style, StyleLength value)
            where T : IStyle
        {
            style.fontSize = value;
            return style;
        }

        /// <summary>
        /// Sets the font definition (font asset) of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityFontDefinition<T>(this T style, StyleFontDefinition value)
            where T : IStyle
        {
            style.unityFontDefinition = value;
            return style;
        }

        /// <summary>
        /// Sets the font style and weight of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityFontStyleAndWeight<T>(this T style, StyleEnum<FontStyle> value)
            where T : IStyle
        {
            style.unityFontStyleAndWeight = value;
            return style;
        }
        #endregion

        #region Text
        /// <summary>
        /// Sets the word spacing of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetWorldSpacing<T>(this T style, StyleLength value)
            where T : IStyle
        {
            style.wordSpacing = value;
            return style;
        }

        /// <summary>
        /// Sets the letter spacing of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetLetterSpacing<T>(this T style, StyleLength value)
            where T : IStyle
        {
            style.letterSpacing = value;
            return style;
        }

        /// <summary>
        /// Sets the text alignment of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityTextAlign<T>(this T style, TextAnchor value)
            where T : IStyle
        {
            style.unityTextAlign = value;
            return style;
        }

        /// <summary>
        /// Sets the text shadow of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTextShadow<T>(this T style, StyleTextShadow value)
            where T : IStyle
        {
            style.textShadow = value;
            return style;
        }

        /// <summary>
        /// Sets the text outline color of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityTextOutlineColor<T>(this T style, StyleColor value)
            where T : IStyle
        {
            style.unityTextOutlineColor = value;
            return style;
        }

        /// <summary>
        /// Sets the text outline width of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityTextOutlineWidth<T>(this T style, StyleFloat value)
            where T : IStyle
        {
            style.unityTextOutlineWidth = value;
            return style;
        }

        /// <summary>
        /// Sets the paragraph spacing of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityParagraphSpacing<T>(this T style, StyleLength value)
            where T : IStyle
        {
            style.unityParagraphSpacing = value;
            return style;
        }

#if UNITY_6000_2_OR_NEWER
        /// <summary>
        /// Sets the text auto-size settings of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityTextAutoSize<T>(this T style, StyleTextAutoSize value)
            where T : IStyle
        {
            style.unityTextAutoSize = value;
            return style;
        }
#endif

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the text generator type of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityTextGenerator<T>(this T style, TextGeneratorType value)
            where T : IStyle
        {
            style.unityTextGenerator = value;
            return style;
        }

        /// <summary>
        /// Sets the editor text rendering mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityEditorTextRenderingMode<T>(this T style, EditorTextRenderingMode value)
            where T : IStyle
        {
            style.unityEditorTextRenderingMode = value;
            return style;
        }
#endif

        /// <summary>
        /// Sets the text overflow mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTextOverflow<T>(this T style, StyleEnum<TextOverflow> value)
            where T : IStyle
        {
            style.textOverflow = value;
            return style;
        }

        /// <summary>
        /// Sets the text overflow position of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityTextOverflowPosition<T>(this T style, TextOverflowPosition value)
            where T : IStyle
        {
            style.unityTextOverflowPosition = value;
            return style;
        }
        #endregion

        #region Color
        /// <summary>
        /// Sets the text color of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetColor<T>(this T style, StyleColor value)
            where T : IStyle
        {
            style.color = value;
            return style;
        }

        /// <summary>
        /// Sets the opacity of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetOpacity<T>(this T style, StyleFloat value)
            where T : IStyle
        {
            style.opacity = value;
            return style;
        }
        #endregion

        #region Align
        /// <summary>
        /// Sets the align-self of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetAlignSelf<T>(this T style, StyleEnum<Align> value)
            where T : IStyle
        {
            style.alignSelf = value;
            return style;
        }

        /// <summary>
        /// Sets the align-items of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetAlignItems<T>(this T style, StyleEnum<Align> value)
            where T : IStyle
        {
            style.alignItems = value;
            return style;
        }

        /// <summary>
        /// Sets the align-content of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetAlignContent<T>(this T style, StyleEnum<Align> value)
            where T : IStyle
        {
            style.alignContent = value;
            return style;
        }
        #endregion

        #region Border
        /// <summary>
        /// Sets the border color on all sides of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBorderColor<T>(this T style, StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(top: value, bottom: value, left: value, right: value);
        }

        /// <summary>
        /// Sets the border color of the style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBorderColor<T>(this T style,
            StyleColor? top = null,
            StyleColor? bottom = null,
            StyleColor? left = null,
            StyleColor? right = null)
            where T : IStyle
        {
            if (top.HasValue) style.borderTopColor = top.Value;
            if (bottom.HasValue) style.borderBottomColor = bottom.Value;

            if (left.HasValue) style.borderLeftColor = left.Value;
            if (right.HasValue) style.borderRightColor = right.Value;

            return style;
        }

        /// <summary>
        /// Sets the border radius on all corners of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBorderRadius<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(topLeft: value, topRight: value, bottomLeft: value, bottomRight: value);
        }

        /// <summary>
        /// Sets the border radius of the style. Only non-null corners are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBorderRadius<T>(this T style,
            StyleLength? topLeft = null,
            StyleLength? topRight = null,
            StyleLength? bottomLeft = null,
            StyleLength? bottomRight = null)
            where T : IStyle
        {
            if (topLeft.HasValue) style.borderTopLeftRadius = topLeft.Value;
            if (topRight.HasValue) style.borderTopRightRadius = topRight.Value;

            if (bottomLeft.HasValue) style.borderBottomLeftRadius = bottomLeft.Value;
            if (bottomRight.HasValue) style.borderBottomRightRadius = bottomRight.Value;

            return style;
        }

        /// <summary>
        /// Sets the border width on all sides of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBorderWidth<T>(this T style, StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(top: value, bottom: value, left: value, right: value);
        }

        /// <summary>
        /// Sets the border width of the style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBorderWidth<T>(this T style,
            StyleFloat? top = null,
            StyleFloat? bottom = null,
            StyleFloat? left = null,
            StyleFloat? right = null)
            where T : IStyle
        {
            if (top.HasValue) style.borderTopWidth = top.Value;
            if (bottom.HasValue) style.borderBottomWidth = bottom.Value;

            if (left.HasValue) style.borderLeftWidth = left.Value;
            if (right.HasValue) style.borderRightWidth = right.Value;

            return style;
        }
        #endregion

        #region Cursor
        /// <summary>
        /// Sets the cursor of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetCursor<T>(this T style, StyleCursor value)
            where T : IStyle
        {
            style.cursor = value;
            return style;
        }
        #endregion

        #region Margin
        /// <summary>
        /// Sets the margin on all sides of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetMargin<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(top: value, bottom: value, left: value, right: value);
        }

        /// <summary>
        /// Sets the margin of the style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetMargin<T>(this T style,
            StyleLength? top = null,
            StyleLength? bottom = null,
            StyleLength? left = null,
            StyleLength? right = null)
            where T : IStyle
        {
            if (top.HasValue) style.marginTop = top.Value;
            if (bottom.HasValue) style.marginBottom = bottom.Value;

            if (left.HasValue) style.marginLeft = left.Value;
            if (right.HasValue) style.marginRight = right.Value;

            return style;
        }
        #endregion

        #region Padding
        /// <summary>
        /// Sets the padding on all sides of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetPadding<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(top: value, bottom: value, left: value, right: value);
        }

        /// <summary>
        /// Sets the padding of the style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetPadding<T>(this T style,
            StyleLength? top = null,
            StyleLength? bottom = null,
            StyleLength? left = null,
            StyleLength? right = null)
            where T : IStyle
        {
            if (top.HasValue) style.paddingTop = top.Value;
            if (bottom.HasValue) style.paddingBottom = bottom.Value;

            if (left.HasValue) style.paddingLeft = left.Value;
            if (right.HasValue) style.paddingRight = right.Value;

            return style;
        }
        #endregion

        #region Display
        /// <summary>
        /// Sets the display style of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetDisplay<T>(this T style, DisplayStyle value)
            where T : IStyle
        {
            style.display = value;
            return style;
        }
        #endregion

        #region Overflow
        /// <summary>
        /// Sets the overflow mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetOverflow<T>(this T style, StyleEnum<Overflow> value)
            where T : IStyle
        {
            style.overflow = value;
            return style;
        }

        /// <summary>
        /// Sets the overflow clip box of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityOverflowClipBox<T>(this T style, StyleEnum<OverflowClipBox> value)
            where T : IStyle
        {
            style.unityOverflowClipBox = value;
            return style;
        }
        #endregion

        #region Distance
        /// <summary>
        /// Sets the top, bottom, left, and right distances of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetDistance<T>(this T style, StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(top: value, bottom: value, left: value, right: value);
        }

        /// <summary>
        /// Sets the positional distances of the style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetDistance<T>(this T style,
            StyleLength? top = null,
            StyleLength? bottom = null,
            StyleLength? left = null,
            StyleLength? right = null)
            where T : IStyle
        {
            if (top.HasValue) style.top = top.Value;
            if (bottom.HasValue) style.bottom = bottom.Value;

            if (left.HasValue) style.left = left.Value;
            if (right.HasValue) style.right = right.Value;

            return style;
        }
        #endregion

        #region Transform
        /// <summary>
        /// Sets the scale transform of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetScale<T>(this T style, StyleScale value)
            where T : IStyle
        {
            style.scale = value;
            return style;
        }

        /// <summary>
        /// Sets the rotation transform of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetRotate<T>(this T style, StyleRotate value)
            where T : IStyle
        {
            style.rotate = value;
            return style;
        }

        /// <summary>
        /// Sets the translation transform of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTranslate<T>(this T style, StyleTranslate value)
            where T : IStyle
        {
            style.translate = value;
            return style;
        }

        /// <summary>
        /// Sets the positioning mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetPosition<T>(this T style, StyleEnum<Position> value)
            where T : IStyle
        {
            style.position = value;
            return style;
        }

        /// <summary>
        /// Sets the transform origin of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTransformOrigin<T>(this T style, StyleTransformOrigin value)
            where T : IStyle
        {
            style.transformOrigin = value;
            return style;
        }
        #endregion

        #region Background
        /// <summary>
        /// Sets the background color of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBackgroundColor<T>(this T style, StyleColor value)
            where T : IStyle
        {
            style.backgroundColor = value;
            return style;
        }

        /// <summary>
        /// Sets the background image of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBackgroundImage<T>(this T style, StyleBackground value)
            where T : IStyle
        {
            style.backgroundImage = value;
            return style;
        }

        /// <summary>
        /// Sets the background size of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBackgroundSize<T>(this T style, StyleBackgroundSize value)
            where T : IStyle
        {
            style.backgroundSize = value;
            return style;
        }

        /// <summary>
        /// Sets the background repeat mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBackgroundRepeat<T>(this T style, StyleBackgroundRepeat value)
            where T : IStyle
        {
            style.backgroundRepeat = value;
            return style;
        }

        /// <summary>
        /// Sets the background image tint color of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnityBackgroundImageTintColor<T>(this T style, StyleColor value)
            where T : IStyle
        {
            style.unityBackgroundImageTintColor = value;
            return style;
        }

        /// <summary>
        /// Sets both X and Y background positions of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBackgroundPosition<T>(this T style, StyleBackgroundPosition value)
            where T : IStyle
        {
            return style.SetBackgroundPosition(x: value, y: value);
        }

        /// <summary>
        /// Sets the background position of the style. Only non-null axes are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetBackgroundPosition<T>(this T style,
            StyleBackgroundPosition? x = null,
            StyleBackgroundPosition? y = null)
            where T : IStyle
        {
            if (x.HasValue) style.backgroundPositionX = x.Value;
            if (y.HasValue) style.backgroundPositionY = y.Value;

            return style;
        }
        #endregion

        #region Transition
        /// <summary>
        /// Sets the transition delay of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTransitionDelay<T>(this T style, StyleList<TimeValue> value)
            where T : IStyle
        {
            style.transitionDelay = value;
            return style;
        }

        /// <summary>
        /// Sets the transition duration of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTransitionDuration<T>(this T style, StyleList<TimeValue> value)
            where T : IStyle
        {
            style.transitionDuration = value;
            return style;
        }

        /// <summary>
        /// Sets the transition property list of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTransitionProperty<T>(this T style, StyleList<StylePropertyName> value)
            where T : IStyle
        {
            style.transitionProperty = value;
            return style;
        }

        /// <summary>
        /// Sets the transition timing function of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetTransitionTimingFunction<T>(this T style, StyleList<EasingFunction> value)
            where T : IStyle
        {
            style.transitionTimingFunction = value;
            return style;
        }
        #endregion

        #region UnitySlice
        /// <summary>
        /// Sets the 9-slice insets on all sides of the style to <paramref name="value"/>.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnitySlice<T>(this T style, StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(top: value, bottom: value, left: value, right: value);
        }

        /// <summary>
        /// Sets the 9-slice insets of the style. Only non-null sides are applied.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnitySlice<T>(this T style,
            StyleInt? top = null,
            StyleInt? bottom = null,
            StyleInt? left = null,
            StyleInt? right = null)
            where T : IStyle
        {
            if (top.HasValue) style.unitySliceTop = top.Value;
            if (bottom.HasValue) style.unitySliceBottom = bottom.Value;

            if (left.HasValue) style.unitySliceLeft = left.Value;
            if (right.HasValue) style.unitySliceRight = right.Value;

            return style;
        }

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the 9-slice type of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetUnitySliceType<T>(this T style, StyleEnum<SliceType> value)
            where T : IStyle
        {
            style.unitySliceType = value;
            return style;
        }
#endif
        #endregion

        #region Visibility
        /// <summary>
        /// Sets the CSS visibility of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetVisibility<T>(this T style, StyleEnum<Visibility> value)
            where T : IStyle
        {
            style.visibility = value;
            return style;
        }
        #endregion

        #region WhiteSpace
        /// <summary>
        /// Sets the white-space mode of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetWhiteSpace<T>(this T style, StyleEnum<WhiteSpace> value)
            where T : IStyle
        {
            style.whiteSpace = value;
            return style;
        }
        #endregion

        #region JustifyContent
        /// <summary>
        /// Sets the justify-content of the style.
        /// </summary>
        /// <returns>The style for method chaining.</returns>
        public static T SetJustifyContent<T>(this T style, StyleEnum<Justify> value)
            where T : IStyle
        {
            style.justifyContent = value;
            return style;
        }
        #endregion
    }
}