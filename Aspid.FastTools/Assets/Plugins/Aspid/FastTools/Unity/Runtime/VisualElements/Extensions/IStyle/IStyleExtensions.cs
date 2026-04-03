using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class IStyleExtensions
    {
        #region Flex
        /// <summary>
        /// Initial main size of a flex item, on the main flex axis. The final layout might be smaller or larger, according to the flex shrinking and growing determined by the other flex properties.
        /// </summary>
        public static T SetFlexBasis<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            style.flexBasis = value;
            return style;
        }

        /// <summary>
        /// Specifies how the item will grow relative to the rest of the flexible items inside the same container.
        /// </summary>
        public static T SetFlexGrow<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            style.flexGrow = value;
            return style;
        }

        /// <summary>
        /// Specifies how the item will shrink relative to the rest of the flexible items inside the same container.
        /// </summary>
        public static T SetFlexShrink<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            style.flexShrink = value;
            return style;
        }

        /// <summary>
        /// Placement of children over multiple lines if not enough space is available in this container.
        /// </summary>
        public static T SetFlexWrap<T>(
            this T style,
            StyleEnum<Wrap> value)
            where T : IStyle
        {
            style.flexWrap = value;
            return style;
        }

        /// <summary>
        /// Direction of the main axis to layout children in a container.
        /// </summary>
        public static T SetFlexDirection<T>(
            this T style,
            FlexDirection value)
            where T : IStyle
        {
            style.flexDirection = value;
            return style;
        }
        #endregion

        #region Size
        /// <summary>
        /// Sets both the <c>width</c> and <c>height</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The size to apply to both width and height.</param>
        public static T SetSize<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetSize(
                width: value,
                height: value);
        }

        /// <summary>
        /// Sets the <c>width</c> and/or <c>height</c> CSS properties.
        /// </summary>
        /// <param name="width">The width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The height to set, or <see langword="null"/> to leave unchanged.</param>
        public static T SetSize<T>(
            this T style,
            StyleLength? width = null,
            StyleLength? height = null)
            where T : IStyle
        {
            if (width.HasValue) style.width = width.Value;
            if (height.HasValue) style.height = height.Value;

            return style;
        }

        /// <summary>
        /// Sets both the <c>min-width</c> and <c>min-height</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The minimum size to apply to both width and height.</param>
        public static T SetMinSize<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMinSize(
                width: value,
                height: value);
        }

        /// <summary>
        /// Sets the <c>min-width</c> and/or <c>min-height</c> CSS properties.
        /// </summary>
        /// <param name="width">The minimum width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The minimum height to set, or <see langword="null"/> to leave unchanged.</param>
        public static T SetMinSize<T>(
            this T style,
            StyleLength? width = null,
            StyleLength? height = null)
            where T : IStyle
        {
            if (width.HasValue) style.minWidth = width.Value;
            if (height.HasValue) style.minHeight = height.Value;

            return style;
        }

        /// <summary>
        /// Sets both the <c>max-width</c> and <c>max-height</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The maximum size to apply to both width and height.</param>
        public static T SetMaxSize<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMaxSize(
                width: value,
                height: value);
        }

        /// <summary>
        /// Sets the <c>max-width</c> and/or <c>max-height</c> CSS properties.
        /// </summary>
        /// <param name="width">The maximum width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The maximum height to set, or <see langword="null"/> to leave unchanged.</param>
        public static T SetMaxSize<T>(
            this T style,
            StyleLength? width = null,
            StyleLength? height = null)
            where T : IStyle
        {
            if (width.HasValue) style.maxWidth = width.Value;
            if (height.HasValue) style.maxHeight = height.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>width</c> CSS property.
        /// </summary>
        /// <param name="value">The width to set.</param>
        public static T SetWidth<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetSize(width: value);
        }

        /// <summary>
        /// Sets the <c>min-width</c> CSS property.
        /// </summary>
        /// <param name="value">The minimum width to set.</param>
        public static T SetMinWidth<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMinSize(width: value);
        }

        /// <summary>
        /// Sets the <c>max-width</c> CSS property.
        /// </summary>
        /// <param name="value">The maximum width to set.</param>
        public static T SetMaxWidth<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMaxSize(width: value);
        }

        /// <summary>
        /// Sets the <c>height</c> CSS property.
        /// </summary>
        /// <param name="value">The height to set.</param>
        public static T SetHeight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetSize(height: value);
        }

        /// <summary>
        /// Sets the <c>min-height</c> CSS property.
        /// </summary>
        /// <param name="value">The minimum height to set.</param>
        public static T SetMinHeight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMinSize(height: value);
        }

        /// <summary>
        /// Sets the <c>max-height</c> CSS property.
        /// </summary>
        /// <param name="value">The maximum height to set.</param>
        public static T SetMaxHeight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMaxSize(height: value);
        }
        #endregion

        #region Font
        /// <summary>
        /// Font to draw the element's text, defined as a Font object.
        /// </summary>
        public static T SetUnityFont<T>(
            this T style,
            StyleFont value)
            where T : IStyle
        {
            style.unityFont = value;
            return style;
        }

        /// <summary>
        /// Font size to draw the element's text, specified in point size.
        /// </summary>
        public static T SetFontSize<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            style.fontSize = value;
            return style;
        }

        /// <summary>
        /// Font to draw the element's text, defined as a FontDefinition structure. It takes precedence over -unity-font.
        /// </summary>
        public static T SetUnityFontDefinition<T>(
            this T style,
            StyleFontDefinition value)
            where T : IStyle
        {
            style.unityFontDefinition = value;
            return style;
        }

        /// <summary>
        /// Font style and weight (normal, bold, italic) to draw the element's text.
        /// </summary>
        public static T SetUnityFontStyleAndWeight<T>(
            this T style,
            StyleEnum<FontStyle> value)
            where T : IStyle
        {
            style.unityFontStyleAndWeight = value;
            return style;
        }
        #endregion

        #region Text
        /// <summary>
        /// Increases or decreases the space between words.
        /// </summary>
        public static T SetWorldSpacing<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            style.wordSpacing = value;
            return style;
        }

        /// <summary>
        /// Increases or decreases the space between characters.
        /// </summary>
        public static T SetLetterSpacing<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            style.letterSpacing = value;
            return style;
        }

        /// <summary>
        /// Horizontal and vertical text alignment in the element's box.
        /// </summary>
        public static T SetUnityTextAlign<T>(
            this T style,
            TextAnchor value)
            where T : IStyle
        {
            style.unityTextAlign = value;
            return style;
        }

        /// <summary>
        /// Drop shadow of the text.
        /// </summary>
        public static T SetTextShadow<T>(
            this T style,
            StyleTextShadow value)
            where T : IStyle
        {
            style.textShadow = value;
            return style;
        }

        /// <summary>
        /// Outline color of the text.
        /// </summary>
        public static T SetUnityTextOutlineColor<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            style.unityTextOutlineColor = value;
            return style;
        }

        /// <summary>
        /// Outline width of the text.
        /// </summary>
        public static T SetUnityTextOutlineWidth<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            style.unityTextOutlineWidth = value;
            return style;
        }

        /// <summary>
        /// Increases or decreases the space between paragraphs.
        /// </summary>
        public static T SetUnityParagraphSpacing<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            style.unityParagraphSpacing = value;
            return style;
        }

#if UNITY_6000_2_OR_NEWER
        /// <summary>
        /// Overrides any explicit font-size to scale text within the defined minimum and maximum bounds, recalculating as needed to fit its container.
        /// </summary>
        public static T SetUnityTextAutoSize<T>(
            this T style,
            StyleTextAutoSize value)
            where T : IStyle
        {
            style.unityTextAutoSize = value;
            return style;
        }
#endif

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Switches between Unity's standard and advanced text generator.
        /// </summary>
        public static T SetUnityTextGenerator<T>(
            this T style,
            TextGeneratorType value)
            where T : IStyle
        {
            style.unityTextGenerator = value;
            return style;
        }

        /// <summary>
        /// TextElement editor rendering mode.
        /// </summary>
        public static T SetUnityEditorTextRenderingMode<T>(
            this T style,
            EditorTextRenderingMode value)
            where T : IStyle
        {
            style.unityEditorTextRenderingMode = value;
            return style;
        }
#endif

        /// <summary>
        /// The element's text overflow mode.
        /// </summary>
        public static T SetTextOverflow<T>(
            this T style,
            StyleEnum<TextOverflow> value)
            where T : IStyle
        {
            style.textOverflow = value;
            return style;
        }

        /// <summary>
        /// The element's text overflow position.
        /// </summary>
        public static T SetUnityTextOverflowPosition<T>(
            this T style,
            TextOverflowPosition value)
            where T : IStyle
        {
            style.unityTextOverflowPosition = value;
            return style;
        }
        #endregion

        #region Color
        /// <summary>
        /// Color to use when drawing the text of an element.
        /// </summary>
        public static T SetColor<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            style.color = value;
            return style;
        }

        /// <summary>
        /// Specifies the transparency of an element and of its children.
        /// </summary>
        public static T SetOpacity<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            style.opacity = value;
            return style;
        }
        #endregion

        #region Align
        /// <summary>
        /// Similar to align-items, but only for this specific element.
        /// </summary>
        public static T SetAlignSelf<T>(
            this T style,
            StyleEnum<Align> value)
            where T : IStyle
        {
            style.alignSelf = value;
            return style;
        }

        /// <summary>
        /// Alignment of children on the cross axis of this container.
        /// </summary>
        public static T SetAlignItems<T>(
            this T style,
            StyleEnum<Align> value)
            where T : IStyle
        {
            style.alignItems = value;
            return style;
        }

        /// <summary>
        /// Alignment of the whole area of children on the cross axis if they span over multiple lines in this container.
        /// </summary>
        public static T SetAlignContent<T>(
            this T style,
            StyleEnum<Align> value)
            where T : IStyle
        {
            style.alignContent = value;
            return style;
        }
        #endregion

        #region Aspect
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Sets a preferred aspect ratio for the box, which will be used in the calculation of auto sizes and some other layout functions.
        /// </summary>
        public static T SetAspectRation<T>(
            this T style,
            StyleRatio value)
            where T : IStyle
        {
            style.aspectRatio = value;
            return style;
        }
#endif
        #endregion

        #region Filter
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Filter effects to apply to the element.
        /// </summary>
        public static T SetFilter<T>(
            this T style,
            StyleList<FilterFunction> value)
            where T : IStyle
        {
            style.filter = value;
            return style;
        }
#endif
        #endregion

        #region Border
        /// <summary>
        /// Sets all four border color CSS properties (<c>border-top-color</c>, <c>border-right-color</c>, <c>border-bottom-color</c>, <c>border-left-color</c>) to the same value.
        /// </summary>
        /// <param name="value">The border color to apply to all sides.</param>
        public static T SetBorderColor<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(
                top: value,
                right: value,
                bottom: value,
                left: value);
        }

        /// <summary>
        /// Sets the border color CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left border color, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBorderColor<T>(
            this T style,
            StyleColor? top = null,
            StyleColor? right = null,
            StyleColor? bottom = null,
            StyleColor? left = null)
            where T : IStyle
        {
            if (top.HasValue) style.borderTopColor = top.Value;
            if (right.HasValue) style.borderRightColor = right.Value;
            if (bottom.HasValue) style.borderBottomColor = bottom.Value;
            if (left.HasValue) style.borderLeftColor = left.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>border-left-color</c> and <c>border-right-color</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border color to apply to the left and right sides.</param>
        public static T SetBorderColorX<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(
                left: value,
                right: value);
        }

        /// <summary>
        /// Sets the <c>border-top-color</c> and <c>border-bottom-color</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border color to apply to the top and bottom sides.</param>
        public static T SetBorderColorY<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(
                top: value,
                bottom: value);
        }

        /// <summary>
        /// Sets the <c>border-top-color</c> CSS property.
        /// </summary>
        /// <param name="value">The top border color to set.</param>
        public static T SetBorderColorTop<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(top: value);
        }

        /// <summary>
        /// Sets the <c>border-right-color</c> CSS property.
        /// </summary>
        /// <param name="value">The right border color to set.</param>
        public static T SetBorderColorRight<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(right: value);
        }

        /// <summary>
        /// Sets the <c>border-bottom-color</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom border color to set.</param>
        public static T SetBorderColorBottom<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(bottom: value);
        }

        /// <summary>
        /// Sets the <c>border-left-color</c> CSS property.
        /// </summary>
        /// <param name="value">The left border color to set.</param>
        public static T SetBorderColorLeft<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            return style.SetBorderColor(left: value);
        }

        /// <summary>
        /// Sets all four border radius CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border radius to apply to all corners.</param>
        public static T SetBorderRadius<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(
                topLeft: value,
                topRight: value,
                bottomRight: value,
                bottomLeft: value);
        }

        /// <summary>
        /// Sets the border radius CSS properties for individual corners.
        /// </summary>
        /// <param name="topLeft">The top-left radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="topRight">The top-right radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottomRight">The bottom-right radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottomLeft">The bottom-left radius, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBorderRadius<T>(
            this T style,
            StyleLength? topLeft = null,
            StyleLength? topRight = null,
            StyleLength? bottomRight = null,
            StyleLength? bottomLeft = null)
            where T : IStyle
        {
            if (topLeft.HasValue) style.borderTopLeftRadius = topLeft.Value;
            if (topRight.HasValue) style.borderTopRightRadius = topRight.Value;
            if (bottomRight.HasValue) style.borderBottomRightRadius = bottomRight.Value;
            if (bottomLeft.HasValue) style.borderBottomLeftRadius = bottomLeft.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>border-top-left-radius</c> and <c>border-top-right-radius</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The radius to apply to both top corners.</param>
        public static T SetBorderRadiusTop<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(
                topLeft: value,
                topRight: value);
        }

        /// <summary>
        /// Sets the <c>border-bottom-right-radius</c> and <c>border-bottom-left-radius</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The radius to apply to both bottom corners.</param>
        public static T SetBorderRadiusBottom<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(
                bottomRight: value,
                bottomLeft: value);
        }

        /// <summary>
        /// Sets the <c>border-top-left-radius</c> CSS property.
        /// </summary>
        /// <param name="value">The top-left corner radius to set.</param>
        public static T SetBorderRadiusTopLeft<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(topLeft: value);
        }

        /// <summary>
        /// Sets the <c>border-top-right-radius</c> CSS property.
        /// </summary>
        /// <param name="value">The top-right corner radius to set.</param>
        public static T SetBorderRadiusTopRight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(topRight: value);
        }

        /// <summary>
        /// Sets the <c>border-bottom-right-radius</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom-right corner radius to set.</param>
        public static T SetBorderRadiusBottomRight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(bottomRight: value);
        }

        /// <summary>
        /// Sets the <c>border-bottom-left-radius</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom-left corner radius to set.</param>
        public static T SetBorderRadiusBottomLeft<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetBorderRadius(bottomLeft: value);
        }

        /// <summary>
        /// Sets all four border width CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border width to apply to all sides.</param>
        public static T SetBorderWidth<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(
                top: value,
                right: value,
                bottom: value,
                left: value);
        }

        /// <summary>
        /// Sets the border width CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left border width, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBorderWidth<T>(
            this T style,
            StyleFloat? top = null,
            StyleFloat? right = null,
            StyleFloat? bottom = null,
            StyleFloat? left = null)
            where T : IStyle
        {
            if (top.HasValue) style.borderTopWidth = top.Value;
            if (right.HasValue) style.borderRightWidth = right.Value;
            if (bottom.HasValue) style.borderBottomWidth = bottom.Value;
            if (left.HasValue) style.borderLeftWidth = left.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>border-left-width</c> and <c>border-right-width</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border width to apply to the left and right sides.</param>
        public static T SetBorderWidthX<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(right: value, left: value);
        }

        /// <summary>
        /// Sets the <c>border-top-width</c> and <c>border-bottom-width</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border width to apply to the top and bottom sides.</param>
        public static T SetBorderWidthY<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(top: value, bottom: value);
        }

        /// <summary>
        /// Sets the <c>border-top-width</c> CSS property.
        /// </summary>
        /// <param name="value">The top border width to set.</param>
        public static T SetBorderWidthTop<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(top: value);
        }

        /// <summary>
        /// Sets the <c>border-right-width</c> CSS property.
        /// </summary>
        /// <param name="value">The right border width to set.</param>
        public static T SetBorderWidthRight<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(right: value);
        }

        /// <summary>
        /// Sets the <c>border-bottom-width</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom border width to set.</param>
        public static T SetBorderWidthBottom<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(bottom: value);
        }

        /// <summary>
        /// Sets the <c>border-left-width</c> CSS property.
        /// </summary>
        /// <param name="value">The left border width to set.</param>
        public static T SetBorderWidthLeft<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            return style.SetBorderWidth(left: value);
        }
        #endregion

        #region Cursor
        /// <summary>
        /// Mouse cursor to display when the mouse pointer is over an element.
        /// </summary>
        public static T SetCursor<T>(
            this T style,
            StyleCursor value)
            where T : IStyle
        {
            style.cursor = value;
            return style;
        }
        #endregion

        #region Margin
        /// <summary>
        /// Sets all four margin CSS properties to the same value.
        /// </summary>
        /// <param name="value">The margin to apply to all sides.</param>
        public static T SetMargin<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(
                top: value,
                right: value,
                bottom: value,
                left: value);
        }

        /// <summary>
        /// Sets the margin CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left margin, or <see langword="null"/> to leave unchanged.</param>
        public static T SetMargin<T>(
            this T style,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null,
            StyleLength? left = null)
            where T : IStyle
        {
            if (top.HasValue) style.marginTop = top.Value;
            if (right.HasValue) style.marginRight = right.Value;
            if (bottom.HasValue) style.marginBottom = bottom.Value;
            if (left.HasValue) style.marginLeft = left.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>margin-left</c> and <c>margin-right</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal margin to set.</param>
        public static T SetMarginX<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(
                right: value,
                left: value);
        }

        /// <summary>
        /// Sets the <c>margin-top</c> and <c>margin-bottom</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The vertical margin to set.</param>
        public static T SetMarginY<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(
                top: value,
                bottom: value);
        }

        /// <summary>
        /// Sets the <c>margin-top</c> CSS property.
        /// </summary>
        /// <param name="value">The top margin to set.</param>
        public static T SetMarginTop<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(top: value);
        }

        /// <summary>
        /// Sets the <c>margin-right</c> CSS property.
        /// </summary>
        /// <param name="value">The right margin to set.</param>
        public static T SetMarginRight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(right: value);
        }

        /// <summary>
        /// Sets the <c>margin-bottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom margin to set.</param>
        public static T SetMarginBottom<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(bottom: value);
        }

        /// <summary>
        /// Sets the <c>margin-left</c> CSS property.
        /// </summary>
        /// <param name="value">The left margin to set.</param>
        public static T SetMarginLeft<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetMargin(left: value);
        }
        #endregion

        #region Padding
        /// <summary>
        /// Sets all four padding CSS properties to the same value.
        /// </summary>
        /// <param name="value">The padding to apply to all sides.</param>
        public static T SetPadding<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(
                top: value,
                right: value,
                bottom: value,
                left: value);
        }

        /// <summary>
        /// Sets the padding CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left padding, or <see langword="null"/> to leave unchanged.</param>
        public static T SetPadding<T>(
            this T style,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null,
            StyleLength? left = null)
            where T : IStyle
        {
            if (top.HasValue) style.paddingTop = top.Value;
            if (right.HasValue) style.paddingRight = right.Value;
            if (bottom.HasValue) style.paddingBottom = bottom.Value;
            if (left.HasValue) style.paddingLeft = left.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>padding-left</c> and <c>padding-right</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal padding to set.</param>
        public static T SetPaddingX<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(
                right: value,
                left: value);
        }

        /// <summary>
        /// Sets the <c>padding-top</c> and <c>padding-bottom</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The vertical padding to set.</param>
        public static T SetPaddingY<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(
                top: value,
                bottom: value);
        }

        /// <summary>
        /// Sets the <c>padding-top</c> CSS property.
        /// </summary>
        /// <param name="value">The top padding to set.</param>
        public static T SetPaddingTop<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(top: value);
        }

        /// <summary>
        /// Sets the <c>padding-right</c> CSS property.
        /// </summary>
        /// <param name="value">The right padding to set.</param>
        public static T SetPaddingRight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(right: value);
        }

        /// <summary>
        /// Sets the <c>padding-bottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom padding to set.</param>
        public static T SetPaddingBottom<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(bottom: value);
        }

        /// <summary>
        /// Sets the <c>padding-left</c> CSS property.
        /// </summary>
        /// <param name="value">The left padding to set.</param>
        public static T SetPaddingLeft<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetPadding(left: value);
        }
        #endregion

        #region Display
        /// <summary>
        /// Defines how an element is displayed in the layout.
        /// </summary>
        public static T SetDisplay<T>(
            this T style,
            DisplayStyle value)
            where T : IStyle
        {
            style.display = value;
            return style;
        }
        #endregion

        #region Overflow
        /// <summary>
        /// How a container behaves if its content overflows its own box.
        /// </summary>
        public static T SetOverflow<T>(
            this T style,
            StyleEnum<Overflow> value)
            where T : IStyle
        {
            style.overflow = value;
            return style;
        }

        /// <summary>
        /// Specifies which box the element content is clipped against.
        /// </summary>
        public static T SetUnityOverflowClipBox<T>(
            this T style,
            StyleEnum<OverflowClipBox> value)
            where T : IStyle
        {
            style.unityOverflowClipBox = value;
            return style;
        }
        #endregion

        #region Distance
        /// <summary>
        /// Sets the <c>top</c>, <c>right</c>, <c>bottom</c>, and <c>left</c> CSS properties to the same value for absolute/relative positioning.
        /// </summary>
        /// <param name="value">The distance to apply to all sides.</param>
        public static T SetDistance<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(
                top: value,
                right: value,
                bottom: value,
                left: value);
        }

        /// <summary>
        /// Sets the positional offset CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left offset, or <see langword="null"/> to leave unchanged.</param>
        public static T SetDistance<T>(
            this T style,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null,
            StyleLength? left = null)
            where T : IStyle
        {
            if (top.HasValue) style.top = top.Value;
            if (right.HasValue) style.right = right.Value;
            if (bottom.HasValue) style.bottom = bottom.Value;
            if (left.HasValue) style.left = left.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>left</c> and <c>right</c> CSS positional properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal offset to set.</param>
        public static T SetDistanceX<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(
                right: value,
                left: value);
        }

        /// <summary>
        /// Sets the <c>top</c> and <c>bottom</c> CSS positional properties to the same value.
        /// </summary>
        /// <param name="value">The vertical offset to set.</param>
        public static T SetDistanceY<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(
                top: value,
                bottom: value);
        }

        /// <summary>
        /// Sets the <c>top</c> CSS property.
        /// </summary>
        /// <param name="value">The top offset to set.</param>
        public static T SetTop<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(top: value);
        }

        /// <summary>
        /// Sets the <c>right</c> CSS property.
        /// </summary>
        /// <param name="value">The right offset to set.</param>
        public static T SetRight<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(right: value);
        }

        /// <summary>
        /// Sets the <c>bottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom offset to set.</param>
        public static T SetBottom<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(bottom: value);
        }

        /// <summary>
        /// Sets the <c>left</c> CSS property.
        /// </summary>
        /// <param name="value">The left offset to set.</param>
        public static T SetLeft<T>(
            this T style,
            StyleLength value)
            where T : IStyle
        {
            return style.SetDistance(left: value);
        }
        #endregion

        #region Material
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Custom material to use on the element.
        /// </summary>
        public static T SetUnityMaterial<T>(
            this T style,
            StyleMaterialDefinition value)
            where T : IStyle
        {
            style.unityMaterial = value;
            return style;
        }
#endif
        #endregion

        #region Transform
        /// <summary>
        /// A scaling transformation.
        /// </summary>
        public static T SetScale<T>(
            this T style,
            StyleScale value)
            where T : IStyle
        {
            style.scale = value;
            return style;
        }

        /// <summary>
        /// A rotation transformation.
        /// </summary>
        public static T SetRotate<T>(
            this T style,
            StyleRotate value)
            where T : IStyle
        {
            style.rotate = value;
            return style;
        }

        /// <summary>
        /// A translate transformation.
        /// </summary>
        public static T SetTranslate<T>(
            this T style,
            StyleTranslate value)
            where T : IStyle
        {
            style.translate = value;
            return style;
        }

        /// <summary>
        /// Element's positioning in its parent container.
        /// </summary>
        public static T SetPosition<T>(
            this T style,
            StyleEnum<Position> value)
            where T : IStyle
        {
            style.position = value;
            return style;
        }

        /// <summary>
        /// The transformation origin is the point around which a transformation is applied.
        /// </summary>
        public static T SetTransformOrigin<T>(
            this T style,
            StyleTransformOrigin value)
            where T : IStyle
        {
            style.transformOrigin = value;
            return style;
        }
        #endregion

        #region Background
        /// <summary>
        /// Background color to paint in the element's box.
        /// </summary>
        public static T SetBackgroundColor<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            style.backgroundColor = value;
            return style;
        }

        /// <summary>
        /// Background image to paint in the element's box.
        /// </summary>
        public static T SetBackgroundImage<T>(
            this T style,
            StyleBackground value)
            where T : IStyle
        {
            style.backgroundImage = value;
            return style;
        }

        /// <summary>
        /// Background image size value. Transitions are fully supported only when using size in pixels or percentages, such as pixel-to-pixel or percentage-to-percentage transitions.
        /// </summary>
        public static T SetBackgroundSize<T>(
            this T style,
            StyleBackgroundSize value)
            where T : IStyle
        {
            style.backgroundSize = value;
            return style;
        }

        /// <summary>
        /// Background image repeat value.
        /// </summary>
        public static T SetBackgroundRepeat<T>(
            this T style,
            StyleBackgroundRepeat value)
            where T : IStyle
        {
            style.backgroundRepeat = value;
            return style;
        }

        /// <summary>
        /// Tinting color for the element's backgroundImage.
        /// </summary>
        public static T SetUnityBackgroundImageTintColor<T>(
            this T style,
            StyleColor value)
            where T : IStyle
        {
            style.unityBackgroundImageTintColor = value;
            return style;
        }

        /// <summary>
        /// Sets both the <c>background-position-x</c> and <c>background-position-y</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The background position to apply to both axes.</param>
        public static T SetBackgroundPosition<T>(
            this T style,
            StyleBackgroundPosition value)
            where T : IStyle
        {
            return style.SetBackgroundPosition(
                x: value,
                y: value);
        }

        /// <summary>
        /// Sets the <c>background-position-x</c> and/or <c>background-position-y</c> CSS properties.
        /// </summary>
        /// <param name="x">The horizontal background position, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="y">The vertical background position, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBackgroundPosition<T>(
            this T style,
            StyleBackgroundPosition? x = null,
            StyleBackgroundPosition? y = null)
            where T : IStyle
        {
            if (x.HasValue) style.backgroundPositionX = x.Value;
            if (y.HasValue) style.backgroundPositionY = y.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>background-position-x</c> CSS property.
        /// </summary>
        /// <param name="value">The horizontal background position to set.</param>
        public static T SetBackgroundPositionX<T>(
            this T style,
            StyleBackgroundPosition value)
            where T : IStyle
        {
            return style.SetBackgroundPosition(x: value);
        }

        /// <summary>
        /// Sets the <c>background-position-y</c> CSS property.
        /// </summary>
        /// <param name="value">The vertical background position to set.</param>
        public static T SetBackgroundPositionY<T>(
            this T style,
            StyleBackgroundPosition value)
            where T : IStyle
        {
            return style.SetBackgroundPosition(y: value);
        }
        #endregion

        #region Transition
        /// <summary>
        /// Duration to wait before starting a property's transition effect when its value changes.
        /// </summary>
        public static T SetTransitionDelay<T>(
            this T style,
            StyleList<TimeValue> value)
            where T : IStyle
        {
            style.transitionDelay = value;
            return style;
        }

        /// <summary>
        /// Time a transition animation should take to complete.
        /// </summary>
        public static T SetTransitionDuration<T>(
            this T style,
            StyleList<TimeValue> value)
            where T : IStyle
        {
            style.transitionDuration = value;
            return style;
        }

        /// <summary>
        /// Properties to which a transition effect should be applied.
        /// </summary>
        public static T SetTransitionProperty<T>(
            this T style,
            StyleList<StylePropertyName> value)
            where T : IStyle
        {
            style.transitionProperty = value;
            return style;
        }

        /// <summary>
        /// Determines how intermediate values are calculated for properties modified by a transition effect.
        /// </summary>
        public static T SetTransitionTimingFunction<T>(
            this T style,
            StyleList<EasingFunction> value)
            where T : IStyle
        {
            style.transitionTimingFunction = value;
            return style;
        }
        #endregion

        #region UnitySlice
        /// <summary>
        /// Sets the <c>-unity-slice-scale</c> CSS property controlling the scale of sliced borders.
        /// </summary>
        /// <param name="value">The slice scale to set.</param>
        public static T SetUnitySliceScale<T>(
            this T style,
            StyleFloat value)
            where T : IStyle
        {
            style.unitySliceScale = value;
            return style;
        }

        /// <summary>
        /// Sets all four <c>-unity-slice-*</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The slice width to apply to all sides.</param>
        public static T SetUnitySlice<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(
                top: value,
                right: value,
                bottom: value,
                left: value);
        }

        /// <summary>
        /// Sets the <c>-unity-slice-*</c> CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left slice width, or <see langword="null"/> to leave unchanged.</param>
        public static T SetUnitySlice<T>(
            this T style,
            StyleInt? top = null,
            StyleInt? right = null,
            StyleInt? bottom = null,
            StyleInt? left = null)
            where T : IStyle
        {
            if (top.HasValue) style.unitySliceTop = top.Value;
            if (right.HasValue) style.unitySliceRight = right.Value;
            if (bottom.HasValue) style.unitySliceBottom = bottom.Value;
            if (left.HasValue) style.unitySliceLeft = left.Value;

            return style;
        }

        /// <summary>
        /// Sets the <c>-unity-slice-left</c> and <c>-unity-slice-right</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal slice width to set.</param>
        public static T SetUnitySliceX<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(
                right: value,
                left: value);
        }

        /// <summary>
        /// Sets the <c>-unity-slice-top</c> and <c>-unity-slice-bottom</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The vertical slice width to set.</param>
        public static T SetUnitySliceY<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(
                top: value,
                bottom: value);
        }

        /// <summary>
        /// Sets the <c>-unity-slice-top</c> CSS property.
        /// </summary>
        /// <param name="value">The top slice width to set.</param>
        public static T SetUnitySliceTop<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(top: value);
        }

        /// <summary>
        /// Sets the <c>-unity-slice-right</c> CSS property.
        /// </summary>
        /// <param name="value">The right slice width to set.</param>
        public static T SetUnitySliceRight<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(right: value);
        }

        /// <summary>
        /// Sets the <c>-unity-slice-bottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom slice width to set.</param>
        public static T SetUnitySliceBottom<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(bottom: value);
        }

        /// <summary>
        /// Sets the <c>-unity-slice-left</c> CSS property.
        /// </summary>
        /// <param name="value">The left slice width to set.</param>
        public static T SetUnitySliceLeft<T>(
            this T style,
            StyleInt value)
            where T : IStyle
        {
            return style.SetUnitySlice(left: value);
        }

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Specifies the type of sclicing.
        /// </summary>
        public static T SetUnitySliceType<T>(
            this T style,
            StyleEnum<SliceType> value)
            where T : IStyle
        {
            style.unitySliceType = value;
            return style;
        }
#endif
        #endregion

        #region Visibility
        /// <summary>
        /// Specifies whether an element is visible.
        /// </summary>
        public static T SetVisibility<T>(
            this T style,
            StyleEnum<Visibility> value)
            where T : IStyle
        {
            style.visibility = value;
            return style;
        }
        #endregion

        #region WhiteSpace
        /// <summary>
        /// Word wrap over multiple lines if not enough space is available to draw the text of an element.
        /// </summary>
        public static T SetWhiteSpace<T>(
            this T style,
            StyleEnum<WhiteSpace> value)
            where T : IStyle
        {
            style.whiteSpace = value;
            return style;
        }
        #endregion

        #region JustifyContent
        /// <summary>
        /// Justification of children on the main axis of this container.
        /// </summary>
        public static T SetJustifyContent<T>(
            this T style,
            StyleEnum<Justify> value)
            where T : IStyle
        {
            style.justifyContent = value;
            return style;
        }
        #endregion
    }
}