using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
        #region Flex
        /// <summary>
        /// Sets the <c>style.flexBasis</c> CSS property defining the initial main-axis size of a flex item.
        /// </summary>
        /// <param name="value">The flex basis value to set.</param>
        public static T SetFlexBasis<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetFlexBasis(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.flexGrow</c> CSS property defining how much a flex item can grow relative to others.
        /// </summary>
        /// <param name="value">The flex grow factor to set.</param>
        public static T SetFlexGrow<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetFlexGrow(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.flexShrink</c> CSS property defining how much a flex item can shrink relative to others.
        /// </summary>
        /// <param name="value">The flex shrink factor to set.</param>
        public static T SetFlexShrink<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetFlexShrink(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.flexWrap</c> CSS property controlling whether flex items wrap onto multiple lines.
        /// </summary>
        /// <param name="value">The wrap mode to set.</param>
        public static T SetFlexWrap<T>(
            this T element,
            StyleEnum<Wrap> value)
            where T : VisualElement
        {
            element.style.SetFlexWrap(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.flexDirection</c> CSS property defining the main axis of the flex container.
        /// </summary>
        /// <param name="value">The flex direction to set.</param>
        public static T SetFlexDirection<T>(
            this T element,
            FlexDirection value)
            where T : VisualElement
        {
            element.style.SetFlexDirection(value);
            return element;
        }
        #endregion

        #region Size
        /// <summary>
        /// Sets both the <c>style.width</c> and <c>style.height</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The size to apply to both width and height.</param>
        public static T SetSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetSize(
                width: value,
                height: value);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.width</c> and/or <c>style.height</c> CSS properties.
        /// </summary>
        /// <param name="width">The width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The height to set, or <see langword="null"/> to leave unchanged.</param>
        public static T SetSize<T>(
            this T element,
            StyleLength? width = null,
            StyleLength? height = null)
            where T : VisualElement
        {
            element.style.SetSize(
                width: width,
                height: height);

            return element;
        }

        /// <summary>
        /// Sets both the <c>style.minWidth</c> and <c>style.minHeight</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The minimum size to apply to both width and height.</param>
        public static T SetMinSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinSize(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.minWidth</c> and/or <c>style.minHeight</c> CSS properties.
        /// </summary>
        /// <param name="width">The minimum width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The minimum height to set, or <see langword="null"/> to leave unchanged.</param>
        public static T SetMinSize<T>(
            this T element,
            StyleLength? width = null,
            StyleLength? height = null)
            where T : VisualElement
        {
            element.style.SetMinSize(
                width: width,
                height: height);

            return element;
        }

        /// <summary>
        /// Sets both the <c>style.maxWidth</c> and <c>style.maxHeight</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The maximum size to apply to both width and height.</param>
        public static T SetMaxSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMaxSize(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.maxWidth</c> and/or <c>style.maxHeight</c> CSS properties.
        /// </summary>
        /// <param name="width">The maximum width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The maximum height to set, or <see langword="null"/> to leave unchanged.</param>
        public static T SetMaxSize<T>(
            this T element,
            StyleLength? width = null,
            StyleLength? height = null)
            where T : VisualElement
        {
            element.style.SetMaxSize(
                width: width,
                height: height);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.width</c> CSS property.
        /// </summary>
        /// <param name="value">The width to set.</param>
        public static T SetWidth<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.minWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The minimum width to set.</param>
        public static T SetMinWidth<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.maxWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The maximum width to set.</param>
        public static T SetMaxWidth<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMaxWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.height</c> CSS property.
        /// </summary>
        /// <param name="value">The height to set.</param>
        public static T SetHeight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetHeight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.minHeight</c> CSS property.
        /// </summary>
        /// <param name="value">The minimum height to set.</param>
        public static T SetMinHeight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinHeight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.maxHeight</c> CSS property.
        /// </summary>
        /// <param name="value">The maximum height to set.</param>
        public static T SetMaxHeight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMaxHeight(value);
            return element;
        }
        #endregion

        #region Font
        /// <summary>
        /// Sets the <c>style.unityFont</c> CSS property.
        /// </summary>
        /// <param name="value">The font asset to set.</param>
        public static T SetUnityFont<T>(
            this T element,
            StyleFont value)
            where T : VisualElement
        {
            element.style.SetUnityFont(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.fontSize</c> CSS property.
        /// </summary>
        /// <param name="value">The font size to set.</param>
        public static T SetFontSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetFontSize(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityFontDefinition</c> CSS property.
        /// </summary>
        /// <param name="value">The font definition to set.</param>
        public static T SetUnityFontDefinition<T>(
            this T element,
            StyleFontDefinition value)
            where T : VisualElement
        {
            element.style.SetUnityFontDefinition(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityFontStyleAndWeight</c> CSS property controlling bold and italic style.
        /// </summary>
        /// <param name="value">The font style and weight to set.</param>
        public static T SetUnityFontStyleAndWeight<T>(
            this T element,
            StyleEnum<FontStyle> value)
            where T : VisualElement
        {
            element.style.SetUnityFontStyleAndWeight(value);
            return element;
        }
        #endregion

        #region Text
        /// <summary>
        /// Sets the <c>style.wordSpacing</c> CSS property defining additional space between words.
        /// </summary>
        /// <param name="value">The word spacing to set.</param>
        public static T SetWorldSpacing<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetWorldSpacing(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.letterSpacing</c> CSS property defining additional space between characters.
        /// </summary>
        /// <param name="value">The letter spacing to set.</param>
        public static T SetLetterSpacing<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetLetterSpacing(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityTextAlign</c> CSS property controlling text alignment within the element.
        /// </summary>
        /// <param name="value">The text alignment anchor to set.</param>
        public static T SetUnityTextAlign<T>(
            this T element,
            TextAnchor value)
            where T : VisualElement
        {
            element.style.SetUnityTextAlign(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.textShadow</c> CSS property.
        /// </summary>
        /// <param name="value">The text shadow to set.</param>
        public static T SetTextShadow<T>(
            this T element,
            StyleTextShadow value)
            where T : VisualElement
        {
            element.style.SetTextShadow(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityTextOutlineColor</c> CSS property.
        /// </summary>
        /// <param name="value">The text outline color to set.</param>
        public static T SetUnityTextOutlineColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineColor(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityTextOutlineWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The text outline width to set.</param>
        public static T SetUnityTextOutlineWidth<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityParagraphSpacing</c> CSS property defining space between paragraphs.
        /// </summary>
        /// <param name="value">The paragraph spacing to set.</param>
        public static T SetUnityParagraphSpacing<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetUnityParagraphSpacing(value);
            return element;
        }

#if UNITY_6000_2_OR_NEWER
        /// <summary>
        /// Sets the <c>style.unityTextAutoSize</c> CSS property controlling automatic text sizing.
        /// </summary>
        /// <param name="value">The text auto-size settings to set.</param>
        public static T SetUnityTextAutoSize<T>(
            this T element,
            StyleTextAutoSize value)
            where T : VisualElement
        {
            element.style.SetUnityTextAutoSize(value);
            return element;
        }
#endif

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the <c>style.unityTextGenerator</c> CSS property controlling which text generator is used.
        /// </summary>
        /// <param name="value">The text generator type to set.</param>
        public static T SetUnityTextGenerator<T>(
            this T element,
            TextGeneratorType value)
            where T : VisualElement
        {
            element.style.SetUnityTextGenerator(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityEditorTextRenderingMode</c> CSS property.
        /// </summary>
        /// <param name="value">The editor text rendering mode to set.</param>
        public static T SetUnityEditorTextRenderingMode<T>(
            this T element,
            EditorTextRenderingMode value)
            where T : VisualElement
        {
            element.style.SetUnityEditorTextRenderingMode(value);
            return element;
        }
#endif

        /// <summary>
        /// Sets the <c>style.textOverflow</c> CSS property controlling how overflowing text is displayed.
        /// </summary>
        /// <param name="value">The text overflow mode to set.</param>
        public static T SetTextOverflow<T>(
            this T element,
            StyleEnum<TextOverflow> value)
            where T : VisualElement
        {
            element.style.SetTextOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityTextOverflowPosition</c> CSS property controlling where the ellipsis is placed.
        /// </summary>
        /// <param name="value">The overflow position to set.</param>
        public static T SetUnityTextOverflowPosition<T>(
            this T element,
            TextOverflowPosition value)
            where T : VisualElement
        {
            element.style.SetUnityTextOverflowPosition(value);
            return element;
        }
        #endregion

        #region Color
        /// <summary>
        /// Sets the <c>style.color</c> CSS property defining the text color.
        /// </summary>
        /// <param name="value">The color to set.</param>
        public static T SetColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetColor(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.opacity</c> CSS property.
        /// </summary>
        /// <param name="value">The opacity value between 0 (transparent) and 1 (opaque).</param>
        public static T SetOpacity<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetOpacity(value);
            return element;
        }
        #endregion

        #region Align
        /// <summary>
        /// Sets the <c>style.alignSelf</c> CSS property overriding the container's align-items for this element.
        /// </summary>
        /// <param name="value">The alignment to set.</param>
        public static T SetAlignSelf<T>(
            this T element,
            StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignSelf(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.alignItems</c> CSS property controlling how flex children are aligned on the cross axis.
        /// </summary>
        /// <param name="value">The alignment to set.</param>
        public static T SetAlignItems<T>(
            this T element,
            StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignItems(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.alignContent</c> CSS property controlling how flex lines are aligned when there is extra space.
        /// </summary>
        /// <param name="value">The alignment to set.</param>
        public static T SetAlignContent<T>(
            this T element,
            StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignContent(value);
            return element;
        }
        #endregion

        #region Aspect
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Sets the <c>style.aspectRatio</c> CSS property.
        /// </summary>
        /// <param name="value">The aspect ratio to set.</param>
        public static T SetAspectRation<T>(
            this T element,
            StyleRatio value)
            where T : VisualElement
        {
            element.style.SetAspectRation(value);
            return element;
        }
#endif
        #endregion

        #region Filter
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Sets the <c>style.filter</c> CSS property applying graphical effects such as blur or color shift.
        /// </summary>
        /// <param name="value">The list of filter functions to apply.</param>
        public static T SetFilter<T>(
            this T element,
            StyleList<FilterFunction> value)
            where T : VisualElement
        {
            element.style.SetFilter(value);
            return element;
        }
#endif
        #endregion

        #region Border
        /// <summary>
        /// Sets all four border color CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border color to apply to all sides.</param>
        public static T SetBorderColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColor(
                top: value,
                right: value,
                bottom: value,
                left: value);

            return element;
        }

        /// <summary>
        /// Sets the border color CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left border color, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBorderColor<T>(
            this T element,
            StyleColor? top = null,
            StyleColor? right = null,
            StyleColor? bottom = null,
            StyleColor? left = null)
            where T : VisualElement
        {
            element.style.SetBorderColor(
                top: top,
                right: right,
                bottom: bottom,
                left: left);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderLeftColor</c> and <c>style.borderRightColor</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border color to apply to the left and right sides.</param>
        public static T SetBorderColorX<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopColor</c> and <c>style.borderBottomColor</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border color to apply to the top and bottom sides.</param>
        public static T SetBorderColorY<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorY(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopColor</c> CSS property.
        /// </summary>
        /// <param name="value">The top border color to set.</param>
        public static T SetBorderColorTop<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderRightColor</c> CSS property.
        /// </summary>
        /// <param name="value">The right border color to set.</param>
        public static T SetBorderColorRight<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderBottomColor</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom border color to set.</param>
        public static T SetBorderColorBottom<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderLeftColor</c> CSS property.
        /// </summary>
        /// <param name="value">The left border color to set.</param>
        public static T SetBorderColorLeft<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorLeft(value);
            return element;
        }

        /// <summary>
        /// Sets all four border radius CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border radius to apply to all corners.</param>
        public static T SetBorderRadius<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadius(value);
            return element;
        }

        /// <summary>
        /// Sets the border radius CSS properties for individual corners.
        /// </summary>
        /// <param name="topLeft">The top-left radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="topRight">The top-right radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottomRight">The bottom-right radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottomLeft">The bottom-left radius, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBorderRadius<T>(
            this T element,
            StyleLength? topLeft = null,
            StyleLength? topRight = null,
            StyleLength? bottomRight = null,
            StyleLength? bottomLeft = null)
            where T : VisualElement
        {
            element.style.SetBorderRadius(
                topLeft: topLeft,
                topRight: topRight,
                bottomRight: bottomRight,
                bottomLeft: bottomLeft);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopLeftRadius</c> and <c>style.borderTopRightRadius</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The radius to apply to both top corners.</param>
        public static T SetBorderRadiusTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderBottomRightRadius</c> and <c>style.borderBottomLeftRadius</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The radius to apply to both bottom corners.</param>
        public static T SetBorderRadiusBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopLeftRadius</c> CSS property.
        /// </summary>
        /// <param name="value">The top-left corner radius to set.</param>
        public static T SetBorderRadiusTopLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusTopLeft(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopRightRadius</c> CSS property.
        /// </summary>
        /// <param name="value">The top-right corner radius to set.</param>
        public static T SetBorderRadiusTopRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusTopRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderBottomRightRadius</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom-right corner radius to set.</param>
        public static T SetBorderRadiusBottomRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusBottomRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderBottomLeftRadius</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom-left corner radius to set.</param>
        public static T SetBorderRadiusBottomLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusBottomLeft(value);
            return element;
        }

        /// <summary>
        /// Sets all four border width CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border width to apply to all sides.</param>
        public static T SetBorderWidth<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidth(value);
            return element;
        }

        /// <summary>
        /// Sets the border width CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left border width, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBorderWidth<T>(this T element,
            StyleFloat? top = null,
            StyleFloat? right = null,
            StyleFloat? bottom = null,
            StyleFloat? left = null)
            where T : VisualElement
        {
            element.style.SetBorderWidth(
                top: top,
                right: right,
                bottom: bottom,
                left: left);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderLeftWidth</c> and <c>style.borderRightWidth</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border width to apply to the left and right sides.</param>
        public static T SetBorderWidthX<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopWidth</c> and <c>style.borderBottomWidth</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The border width to apply to the top and bottom sides.</param>
        public static T SetBorderWidthY<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthY(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderTopWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The top border width to set.</param>
        public static T SetBorderWidthTop<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderRightWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The right border width to set.</param>
        public static T SetBorderWidthRight<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderBottomWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom border width to set.</param>
        public static T SetBorderWidthBottom<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.borderLeftWidth</c> CSS property.
        /// </summary>
        /// <param name="value">The left border width to set.</param>
        public static T SetBorderWidthLeft<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthLeft(value);
            return element;
        }
        #endregion

        #region Cursor
        /// <summary>
        /// Sets the <c>style.cursor</c> CSS property.
        /// </summary>
        /// <param name="value">The cursor to display when hovering over the element.</param>
        public static T SetCursor<T>(
            this T element,
            StyleCursor value)
            where T : VisualElement
        {
            element.style.SetCursor(value);
            return element;
        }
        #endregion

        #region Margin
        /// <summary>
        /// Sets all four margin CSS properties to the same value.
        /// </summary>
        /// <param name="value">The margin to apply to all sides.</param>
        public static T SetMargin<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMargin(value);
            return element;
        }

        /// <summary>
        /// Sets the margin CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left margin, or <see langword="null"/> to leave unchanged.</param>
        public static T SetMargin<T>(
            this T element,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null,
            StyleLength? left = null)
            where T : VisualElement
        {
            element.style.SetMargin(
                top: top,
                right: right,
                bottom: bottom,
                left: left);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.marginLeft</c> and <c>style.marginRight</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal margin to set.</param>
        public static T SetMarginX<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.marginTop</c> and <c>style.marginBottom</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The vertical margin to set.</param>
        public static T SetMarginY<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginY(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.marginTop</c> CSS property.
        /// </summary>
        /// <param name="value">The top margin to set.</param>
        public static T SetMarginTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.marginRight</c> CSS property.
        /// </summary>
        /// <param name="value">The right margin to set.</param>
        public static T SetMarginRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.marginBottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom margin to set.</param>
        public static T SetMarginBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.marginLeft</c> CSS property.
        /// </summary>
        /// <param name="value">The left margin to set.</param>
        public static T SetMarginLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginLeft(value);
            return element;
        }
        #endregion

        #region Padding
        /// <summary>
        /// Sets all four padding CSS properties to the same value.
        /// </summary>
        /// <param name="value">The padding to apply to all sides.</param>
        public static T SetPadding<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPadding(value);
            return element;
        }

        /// <summary>
        /// Sets the padding CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left padding, or <see langword="null"/> to leave unchanged.</param>
        public static T SetPadding<T>(
            this T element,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null,
            StyleLength? left = null)
            where T : VisualElement
        {
            element.style.SetPadding(
                top: top,
                right: right,
                bottom: bottom,
                left: left);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.paddingLeft</c> and <c>style.paddingRight</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal padding to set.</param>
        public static T SetPaddingX<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.paddingTop</c> and <c>style.paddingBottom</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The vertical padding to set.</param>
        public static T SetPaddingY<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingY(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.paddingTop</c> CSS property.
        /// </summary>
        /// <param name="value">The top padding to set.</param>
        public static T SetPaddingTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.paddingRight</c> CSS property.
        /// </summary>
        /// <param name="value">The right padding to set.</param>
        public static T SetPaddingRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.paddingBottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom padding to set.</param>
        public static T SetPaddingBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.paddingLeft</c> CSS property.
        /// </summary>
        /// <param name="value">The left padding to set.</param>
        public static T SetPaddingLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingLeft(value);
            return element;
        }
        #endregion

        #region Display
        /// <summary>
        /// Sets the <c>style.display</c> CSS property controlling whether the element participates in layout.
        /// </summary>
        /// <param name="value">The display style to set.</param>
        public static T SetDisplay<T>(
            this T element,
            DisplayStyle value)
            where T : VisualElement
        {
            element.style.SetDisplay(value);
            return element;
        }
        #endregion

        #region Overflow
        /// <summary>
        /// Sets the <c>style.overflow</c> CSS property controlling how content that exceeds the element's bounds is handled.
        /// </summary>
        /// <param name="value">The overflow mode to set.</param>
        public static T SetOverflow<T>(
            this T element,
            StyleEnum<Overflow> value)
            where T : VisualElement
        {
            element.style.SetOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityOverflowClipBox</c> CSS property controlling the clipping region for overflow.
        /// </summary>
        /// <param name="value">The overflow clip box to set.</param>
        public static T SetUnityOverflowClipBox<T>(
            this T element,
            StyleEnum<OverflowClipBox> value)
            where T : VisualElement
        {
            element.style.SetUnityOverflowClipBox(value);
            return element;
        }
        #endregion

        #region Distance
        /// <summary>
        /// Sets the <c>style.top</c>, <c>style.right</c>, <c>style.bottom</c>, and <c>style.left</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The distance to apply to all sides.</param>
        public static T SetDistance<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistance(value);
            return element;
        }

        /// <summary>
        /// Sets the positional offset CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left offset, or <see langword="null"/> to leave unchanged.</param>
        public static T SetDistance<T>(
            this T element,
            StyleLength? top = null,
            StyleLength? right = null,
            StyleLength? bottom = null,
            StyleLength? left = null)
            where T : VisualElement
        {
            element.style.SetDistance(
                top: top,
                right: right,
                bottom: bottom,
                left: left);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.left</c> and <c>style.right</c> CSS positional properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal offset to set.</param>
        public static T SetDistanceX<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistanceX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.top</c> and <c>style.bottom</c> CSS positional properties to the same value.
        /// </summary>
        /// <param name="value">The vertical offset to set.</param>
        public static T SetDistanceY<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistanceY(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.top</c> CSS property.
        /// </summary>
        /// <param name="value">The top offset to set.</param>
        public static T SetTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.right</c> CSS property.
        /// </summary>
        /// <param name="value">The right offset to set.</param>
        public static T SetRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.bottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom offset to set.</param>
        public static T SetBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.left</c> CSS property.
        /// </summary>
        /// <param name="value">The left offset to set.</param>
        public static T SetLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetLeft(value);
            return element;
        }
        #endregion

        #region Material
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Sets the <c>style.unityMaterial</c> CSS property.
        /// </summary>
        /// <param name="value">The material definition to set.</param>
        public static T SetUnityMaterial<T>(
            this T element,
            StyleMaterialDefinition value)
            where T : VisualElement
        {
            element.style.SetUnityMaterial(value);
            return element;
        }
#endif
        #endregion

        #region Transform
        /// <summary>
        /// Sets the <c>style.scale</c> CSS property.
        /// </summary>
        /// <param name="value">The scale transform to set.</param>
        public static T SetScale<T>(
            this T element,
            StyleScale value)
            where T : VisualElement
        {
            element.style.SetScale(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.rotate</c> CSS property.
        /// </summary>
        /// <param name="value">The rotation transform to set.</param>
        public static T SetRotate<T>(
            this T element,
            StyleRotate value)
            where T : VisualElement
        {
            element.style.SetRotate(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.translate</c> CSS property.
        /// </summary>
        /// <param name="value">The translation transform to set.</param>
        public static T SetTranslate<T>(
            this T element,
            StyleTranslate value)
            where T : VisualElement
        {
            element.style.SetTranslate(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.position</c> CSS property controlling whether the element uses absolute or relative positioning.
        /// </summary>
        /// <param name="value">The position type to set.</param>
        public static T SetPosition<T>(
            this T element,
            StyleEnum<Position> value)
            where T : VisualElement
        {
            element.style.SetPosition(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.transformOrigin</c> CSS property defining the pivot point for transforms.
        /// </summary>
        /// <param name="value">The transform origin to set.</param>
        public static T SetTransformOrigin<T>(
            this T element,
            StyleTransformOrigin value)
            where T : VisualElement
        {
            element.style.SetTransformOrigin(value);
            return element;
        }
        #endregion

        #region Background
        /// <summary>
        /// Sets the <c>style.backgroundColor</c> CSS property.
        /// </summary>
        /// <param name="value">The background color to set.</param>
        public static T SetBackgroundColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBackgroundColor(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.backgroundImage</c> CSS property.
        /// </summary>
        /// <param name="value">The background image to set.</param>
        public static T SetBackgroundImage<T>(
            this T element,
            StyleBackground value)
            where T : VisualElement
        {
            element.style.SetBackgroundImage(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.backgroundSize</c> CSS property controlling how the background image is scaled.
        /// </summary>
        /// <param name="value">The background size to set.</param>
        public static T SetBackgroundSize<T>(
            this T element,
            StyleBackgroundSize value)
            where T : VisualElement
        {
            element.style.SetBackgroundSize(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.backgroundRepeat</c> CSS property controlling how the background image is tiled.
        /// </summary>
        /// <param name="value">The background repeat mode to set.</param>
        public static T SetBackgroundRepeat<T>(
            this T element,
            StyleBackgroundRepeat value)
            where T : VisualElement
        {
            element.style.SetBackgroundRepeat(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unityBackgroundImageTintColor</c> CSS property.
        /// </summary>
        /// <param name="value">The tint color to apply to the background image.</param>
        public static T SetUnityBackgroundImageTintColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetUnityBackgroundImageTintColor(value);
            return element;
        }

        /// <summary>
        /// Sets both the <c>style.backgroundPositionX</c> and <c>style.backgroundPositionY</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The background position to apply to both axes.</param>
        public static T SetBackgroundPosition<T>(
            this T element,
            StyleBackgroundPosition value)
            where T : VisualElement
        {
            element.style.SetBackgroundPosition(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.backgroundPositionX</c> and/or <c>style.backgroundPositionY</c> CSS properties.
        /// </summary>
        /// <param name="x">The horizontal background position, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="y">The vertical background position, or <see langword="null"/> to leave unchanged.</param>
        public static T SetBackgroundPosition<T>(
            this T element,
            StyleBackgroundPosition? x = null,
            StyleBackgroundPosition? y = null)
            where T : VisualElement
        {
            element.style.SetBackgroundPosition(x, y);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.backgroundPositionX</c> CSS property.
        /// </summary>
        /// <param name="value">The horizontal background position to set.</param>
        public static T SetBackgroundPositionX<T>(
            this T element,
            StyleBackgroundPosition value)
            where T : VisualElement
        {
            element.style.SetBackgroundPositionX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.backgroundPositionY</c> CSS property.
        /// </summary>
        /// <param name="value">The vertical background position to set.</param>
        public static T SetBackgroundPositionY<T>(
            this T element,
            StyleBackgroundPosition value)
            where T : VisualElement
        {
            element.style.SetBackgroundPositionY(value);
            return element;
        }
        #endregion

        #region Transition
        /// <summary>
        /// Sets the <c>style.transitionDelay</c> CSS property.
        /// </summary>
        /// <param name="value">The list of delay values for each transition.</param>
        public static T SetTransitionDelay<T>(
            this T element,
            StyleList<TimeValue> value)
            where T : VisualElement
        {
            element.style.SetTransitionDelay(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.transitionDuration</c> CSS property.
        /// </summary>
        /// <param name="value">The list of duration values for each transition.</param>
        public static T SetTransitionDuration<T>(
            this T element,
            StyleList<TimeValue> value)
            where T : VisualElement
        {
            element.style.SetTransitionDuration(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.transitionProperty</c> CSS property specifying which properties are transitioned.
        /// </summary>
        /// <param name="value">The list of property names to transition.</param>
        public static T SetTransitionProperty<T>(
            this T element,
            StyleList<StylePropertyName> value)
            where T : VisualElement
        {
            element.style.SetTransitionProperty(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.transitionTimingFunction</c> CSS property.
        /// </summary>
        /// <param name="value">The list of easing functions for each transition.</param>
        public static T SetTransitionTimingFunction<T>(
            this T element,
            StyleList<EasingFunction> value)
            where T : VisualElement
        {
            element.style.SetTransitionTimingFunction(value);
            return element;
        }
        #endregion

        #region UnitySlice
        /// <summary>
        /// Sets the <c>style.unitySliceScale</c> CSS property controlling the scale of sliced borders.
        /// </summary>
        /// <param name="value">The slice scale to set.</param>
        public static T SetUnitySliceScale<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetUnitySliceScale(value);
            return element;
        }

        /// <summary>
        /// Sets all four <c>style.unitySlice*</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The slice width to apply to all sides.</param>
        public static T SetUnitySlice<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySlice(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySlice*</c> CSS properties for individual sides.
        /// </summary>
        /// <param name="top">The top slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left slice width, or <see langword="null"/> to leave unchanged.</param>
        public static T SetUnitySlice<T>(
            this T element,
            StyleInt? top = null,
            StyleInt? right = null,
            StyleInt? bottom = null,
            StyleInt? left = null)
            where T : VisualElement
        {
            element.style.SetUnitySlice(
                top: top,
                right: right,
                bottom: bottom,
                left: left);

            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySliceLeft</c> and <c>style.unitySliceRight</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The horizontal slice width to set.</param>
        public static T SetUnitySliceX<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceX(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySliceTop</c> and <c>style.unitySliceBottom</c> CSS properties to the same value.
        /// </summary>
        /// <param name="value">The vertical slice width to set.</param>
        public static T SetUnitySliceY<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceY(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySliceTop</c> CSS property.
        /// </summary>
        /// <param name="value">The top slice width to set.</param>
        public static T SetUnitySliceTop<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceTop(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySliceRight</c> CSS property.
        /// </summary>
        /// <param name="value">The right slice width to set.</param>
        public static T SetUnitySliceRight<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceRight(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySliceBottom</c> CSS property.
        /// </summary>
        /// <param name="value">The bottom slice width to set.</param>
        public static T SetUnitySliceBottom<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the <c>style.unitySliceLeft</c> CSS property.
        /// </summary>
        /// <param name="value">The left slice width to set.</param>
        public static T SetUnitySliceLeft<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceLeft(value);
            return element;
        }

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the <c>style.unitySliceType</c> CSS property.
        /// </summary>
        /// <param name="value">The slice type to set.</param>
        public static T SetUnitySliceType<T>(
            this T element,
            StyleEnum<SliceType> value)
            where T : VisualElement
        {
            element.style.SetUnitySliceType(value);
            return element;
        }
#endif
        #endregion

        #region Visibility
        /// <summary>
        /// Sets the <c>style.visibility</c> CSS property controlling whether the element is visible without affecting layout.
        /// </summary>
        /// <param name="value">The visibility mode to set.</param>
        public static T SetVisibility<T>(
            this T element,
            StyleEnum<Visibility> value)
            where T : VisualElement
        {
            element.style.SetVisibility(value);
            return element;
        }
        #endregion

        #region WhiteSpace
        /// <summary>
        /// Sets the <c>style.whiteSpace</c> CSS property controlling text wrapping behaviour.
        /// </summary>
        /// <param name="value">The white space mode to set.</param>
        public static T SetWhiteSpace<T>(
            this T element,
            StyleEnum<WhiteSpace> value)
            where T : VisualElement
        {
            element.style.SetWhiteSpace(value);
            return element;
        }
        #endregion

        #region JustifyContent
        /// <summary>
        /// Sets the <c>style.justifyContent</c> CSS property controlling alignment of flex children along the main axis.
        /// </summary>
        /// <param name="value">The justify content mode to set.</param>
        public static T SetJustifyContent<T>(
            this T element,
            StyleEnum<Justify> value)
            where T : VisualElement
        {
            element.style.SetJustifyContent(value);
            return element;
        }
        #endregion
    }
}