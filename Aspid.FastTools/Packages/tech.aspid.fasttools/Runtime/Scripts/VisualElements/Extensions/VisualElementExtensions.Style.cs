using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static partial class VisualElementExtensions
    {
        #region Flex
        /// <summary>
        /// Sets <see cref="IStyle.flexBasis"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex basis to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFlexBasis<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetFlexBasis(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.flexGrow"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex grow factor to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFlexGrow<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetFlexGrow(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.flexShrink"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex shrink factor to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFlexShrink<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetFlexShrink(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.flexWrap"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex wrap mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFlexWrap<T>(
            this T element,
            StyleEnum<Wrap> value)
            where T : VisualElement
        {
            element.style.SetFlexWrap(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.flexWrap"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex wrap mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFlexWrap<T>(
            this T element,
            Wrap value)
            where T : VisualElement
        {
            element.style.SetFlexWrap(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.flexDirection"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex direction to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFlexDirection<T>(
            this T element,
            StyleEnum<FlexDirection> value)
            where T : VisualElement
        {
            element.style.SetFlexDirection(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.flexDirection"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The flex direction to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.width"/> and <see cref="IStyle.height"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The size to apply to both width and height.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            return element.SetSize(
                width: value,
                height: value);
        }

        /// <summary>
        /// Sets <see cref="IStyle.width"/> and <see cref="IStyle.height"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="width">The width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="height">The height to set, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.minWidth"/> and <see cref="IStyle.minHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The minimum size to apply to both width and height.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMinSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinSize(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.minWidth"/> and <see cref="IStyle.minHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="minWidth">The minimum width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="minHeight">The minimum height to set, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMinSize<T>(
            this T element,
            StyleLength? minWidth = null,
            StyleLength? minHeight = null)
            where T : VisualElement
        {
            element.style.SetMinSize(
                minWidth: minWidth,
                minHeight: minHeight);

            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.maxWidth"/> and <see cref="IStyle.maxHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The maximum size to apply to both width and height.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaxSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            return element.SetMaxSize(
                maxWidth: value,
                maxHeight: value);
        }

        /// <summary>
        /// Sets <see cref="IStyle.maxWidth"/> and <see cref="IStyle.maxHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="maxWidth">The maximum width to set, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="maxHeight">The maximum height to set, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaxSize<T>(
            this T element,
            StyleLength? maxWidth = null,
            StyleLength? maxHeight = null)
            where T : VisualElement
        {
            element.style.SetMaxSize(
                maxWidth: maxWidth,
                maxHeight: maxHeight);

            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.width"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetWidth<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetWidth(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.minWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The minimum width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMinWidth<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinWidth(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.maxWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The maximum width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMaxWidth<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMaxWidth(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.height"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The height to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHeight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetHeight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.minHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The minimum height to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMinHeight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMinHeight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.maxHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The maximum height to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.unityFont"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The font to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityFont<T>(
            this T element,
            StyleFont value)
            where T : VisualElement
        {
            element.style.SetUnityFont(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.fontSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The font size to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFontSize<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetFontSize(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityFontDefinition"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The font definition to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityFontDefinition<T>(
            this T element,
            StyleFontDefinition value)
            where T : VisualElement
        {
            element.style.SetUnityFontDefinition(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityFontStyleAndWeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The font style and weight to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityFontStyleAndWeight<T>(
            this T element,
            StyleEnum<FontStyle> value)
            where T : VisualElement
        {
            element.style.SetUnityFontStyleAndWeight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityFontStyleAndWeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The font style and weight to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityFontStyleAndWeight<T>(
            this T element,
            FontStyle value)
            where T : VisualElement
        {
            element.style.SetUnityFontStyleAndWeight(value);
            return element;
        }
        #endregion

        #region Text
        /// <summary>
        /// Sets <see cref="IStyle.wordSpacing"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The word spacing to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetWordSpacing<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetWordSpacing(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.letterSpacing"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The letter spacing to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLetterSpacing<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetLetterSpacing(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextAlign"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextAlign<T>(
            this T element,
            StyleEnum<TextAnchor> value)
            where T : VisualElement
        {
            element.style.SetUnityTextAlign(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextAlign"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextAlign<T>(
            this T element,
            TextAnchor value)
            where T : VisualElement
        {
            element.style.SetUnityTextAlign(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.textShadow"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text shadow to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextShadow<T>(
            this T element,
            StyleTextShadow value)
            where T : VisualElement
        {
            element.style.SetTextShadow(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextOutlineColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text outline color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextOutlineColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineColor(value);
            return element;
        }

        /// <summary>
        /// Sets the text outline color from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextOutlineColor<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextOutlineWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text outline width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextOutlineWidth<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetUnityTextOutlineWidth(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityParagraphSpacing"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The paragraph spacing to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.unityTextAutoSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text auto size settings to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextAutoSize<T>(
            this T element,
            StyleTextAutoSize value)
            where T : VisualElement
        {
            element.style.SetUnityTextAutoSize(value);
            return element;
        }
#endif

        /// <summary>
        /// Sets <see cref="IStyle.unityTextGenerator"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text generator type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextGenerator<T>(
            this T element,
            StyleEnum<TextGeneratorType> value)
            where T : VisualElement
        {
            element.style.SetUnityTextGenerator(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextGenerator"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text generator type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextGenerator<T>(
            this T element,
            TextGeneratorType value)
            where T : VisualElement
        {
            element.style.SetUnityTextGenerator(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityEditorTextRenderingMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The editor text rendering mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityEditorTextRenderingMode<T>(
            this T element,
            StyleEnum<EditorTextRenderingMode> value)
            where T : VisualElement
        {
            element.style.SetUnityEditorTextRenderingMode(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityEditorTextRenderingMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The editor text rendering mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityEditorTextRenderingMode<T>(
            this T element,
            EditorTextRenderingMode value)
            where T : VisualElement
        {
            element.style.SetUnityEditorTextRenderingMode(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.textOverflow"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text overflow mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextOverflow<T>(
            this T element,
            StyleEnum<TextOverflow> value)
            where T : VisualElement
        {
            element.style.SetTextOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.textOverflow"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text overflow mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTextOverflow<T>(
            this T element,
            TextOverflow value)
            where T : VisualElement
        {
            element.style.SetTextOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextOverflowPosition"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text overflow position to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityTextOverflowPosition<T>(
            this T element,
            StyleEnum<TextOverflowPosition> value)
            where T : VisualElement
        {
            element.style.SetUnityTextOverflowPosition(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityTextOverflowPosition"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text overflow position to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.color"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The text color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.color"/> from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetColor<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.opacity"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The opacity to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.alignSelf"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAlignSelf<T>(
            this T element,
            StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignSelf(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.alignSelf"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAlignSelf<T>(
            this T element,
            Align value)
            where T : VisualElement
        {
            element.style.SetAlignSelf(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.alignItems"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The children alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAlignItems<T>(
            this T element,
            StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignItems(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.alignItems"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The children alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAlignItems<T>(
            this T element,
            Align value)
            where T : VisualElement
        {
            element.style.SetAlignItems(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.alignContent"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The content alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAlignContent<T>(
            this T element,
            StyleEnum<Align> value)
            where T : VisualElement
        {
            element.style.SetAlignContent(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.alignContent"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The content alignment to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAlignContent<T>(
            this T element,
            Align value)
            where T : VisualElement
        {
            element.style.SetAlignContent(value);
            return element;
        }
        #endregion

        #region Aspect
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Sets <see cref="IStyle.aspectRatio"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The aspect ratio to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAspectRatio<T>(
            this T element,
            StyleRatio value)
            where T : VisualElement
        {
            element.style.SetAspectRatio(value);
            return element;
        }
#endif
        #endregion

        #region Filter
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Sets <see cref="IStyle.filter"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The filter effects to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.borderTopColor"/>, <see cref="IStyle.borderRightColor"/>,
        /// <see cref="IStyle.borderBottomColor"/> and <see cref="IStyle.borderLeftColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border color to apply to all sides.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets the border color on all sides from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColor<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopColor"/>, <see cref="IStyle.borderRightColor"/>,
        /// <see cref="IStyle.borderBottomColor"/> and <see cref="IStyle.borderLeftColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="top">The top border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom border color, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left border color, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.borderRightColor"/> and <see cref="IStyle.borderLeftColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border color to apply to the left and right sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorX<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorX(value);
            return element;
        }

        /// <summary>
        /// Sets the left and right border colors from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorX<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColorX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopColor"/> and <see cref="IStyle.borderBottomColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border color to apply to the top and bottom sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorY<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorY(value);
            return element;
        }

        /// <summary>
        /// Sets the top and bottom border colors from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorY<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColorY(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top border color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorTop<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorTop(value);
            return element;
        }

        /// <summary>
        /// Sets the top border color from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorTop<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColorTop(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderRightColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The right border color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorRight<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorRight(value);
            return element;
        }

        /// <summary>
        /// Sets the right border color from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorRight<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColorRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderBottomColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom border color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorBottom<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorBottom(value);
            return element;
        }

        /// <summary>
        /// Sets the bottom border color from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorBottom<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColorBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderLeftColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The left border color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorLeft<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBorderColorLeft(value);
            return element;
        }

        /// <summary>
        /// Sets the left border color from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderColorLeft<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBorderColorLeft(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopLeftRadius"/>, <see cref="IStyle.borderTopRightRadius"/>,
        /// <see cref="IStyle.borderBottomRightRadius"/> and <see cref="IStyle.borderBottomLeftRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border radius to apply to all corners.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadius<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadius(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopLeftRadius"/>, <see cref="IStyle.borderTopRightRadius"/>,
        /// <see cref="IStyle.borderBottomRightRadius"/> and <see cref="IStyle.borderBottomLeftRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="topLeft">The top-left radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="topRight">The top-right radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottomRight">The bottom-right radius, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottomLeft">The bottom-left radius, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.borderTopLeftRadius"/> and <see cref="IStyle.borderTopRightRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The radius to apply to both top corners.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusTop(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderBottomRightRadius"/> and <see cref="IStyle.borderBottomLeftRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The radius to apply to both bottom corners.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopLeftRadius"/> and <see cref="IStyle.borderBottomLeftRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The radius to apply to both left corners.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusLeft(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopRightRadius"/> and <see cref="IStyle.borderBottomRightRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The radius to apply to both right corners.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopLeftRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top-left corner radius to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusTopLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusTopLeft(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopRightRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top-right corner radius to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusTopRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusTopRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderBottomRightRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom-right corner radius to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusBottomRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusBottomRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderBottomLeftRadius"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom-left corner radius to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderRadiusBottomLeft<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBorderRadiusBottomLeft(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopWidth"/>, <see cref="IStyle.borderRightWidth"/>,
        /// <see cref="IStyle.borderBottomWidth"/> and <see cref="IStyle.borderLeftWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border width to apply to all sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderWidth<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidth(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopWidth"/>, <see cref="IStyle.borderRightWidth"/>,
        /// <see cref="IStyle.borderBottomWidth"/> and <see cref="IStyle.borderLeftWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="top">The top border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom border width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left border width, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.borderLeftWidth"/> and <see cref="IStyle.borderRightWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border width to apply to the left and right sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderWidthX<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderTopWidth"/> and <see cref="IStyle.borderBottomWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The border width to apply to the top and bottom sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderWidthY<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthY(value);
            return element;
        }
        /// <summary>
        /// Sets <see cref="IStyle.borderTopWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top border width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderWidthTop<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthTop(value);
            return element;
        }
        /// <summary>
        /// Sets <see cref="IStyle.borderRightWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The right border width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderWidthRight<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderBottomWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom border width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBorderWidthBottom<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetBorderWidthBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.borderLeftWidth"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The left border width to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.cursor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The cursor style to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.marginTop"/>, <see cref="IStyle.marginRight"/>,
        /// <see cref="IStyle.marginBottom"/> and <see cref="IStyle.marginLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The margin to apply to all sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMargin<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMargin(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.marginTop"/>, <see cref="IStyle.marginRight"/>,
        /// <see cref="IStyle.marginBottom"/> and <see cref="IStyle.marginLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="top">The top margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom margin, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left margin, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.marginRight"/> and <see cref="IStyle.marginLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The horizontal margin to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMarginX<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.marginTop"/> and <see cref="IStyle.marginBottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vertical margin to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMarginY<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginY(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.marginTop"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top margin to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMarginTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginTop(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.marginRight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The right margin to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMarginRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.marginBottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom margin to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMarginBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetMarginBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.marginLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The left margin to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.paddingTop"/>, <see cref="IStyle.paddingRight"/>,
        /// <see cref="IStyle.paddingBottom"/> and <see cref="IStyle.paddingLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The padding to apply to all sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPadding<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPadding(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.paddingTop"/>, <see cref="IStyle.paddingRight"/>,
        /// <see cref="IStyle.paddingBottom"/> and <see cref="IStyle.paddingLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="top">The top padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom padding, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left padding, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.paddingRight"/> and <see cref="IStyle.paddingLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The horizontal padding to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPaddingX<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.paddingTop"/> and <see cref="IStyle.paddingBottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vertical padding to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPaddingY<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingY(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.paddingTop"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top padding to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPaddingTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingTop(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.paddingRight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The right padding to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPaddingRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.paddingBottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom padding to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPaddingBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetPaddingBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.paddingLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The left padding to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.display"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The display mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDisplay<T>(
            this T element,
            StyleEnum<DisplayStyle> value)
            where T : VisualElement
        {
            element.style.SetDisplay(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.display"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The display mode to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.overflow"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The overflow behavior to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOverflow<T>(
            this T element,
            StyleEnum<Overflow> value)
            where T : VisualElement
        {
            element.style.SetOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.overflow"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The overflow behavior to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOverflow<T>(
            this T element,
            Overflow value)
            where T : VisualElement
        {
            element.style.SetOverflow(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityOverflowClipBox"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The overflow clip box to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityOverflowClipBox<T>(
            this T element,
            StyleEnum<OverflowClipBox> value)
            where T : VisualElement
        {
            element.style.SetUnityOverflowClipBox(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityOverflowClipBox"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The overflow clip box to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityOverflowClipBox<T>(
            this T element,
            OverflowClipBox value)
            where T : VisualElement
        {
            element.style.SetUnityOverflowClipBox(value);
            return element;
        }
        #endregion

        #region Distance
        /// <summary>
        /// Sets <see cref="IStyle.top"/>, <see cref="IStyle.right"/>,
        /// <see cref="IStyle.bottom"/> and <see cref="IStyle.left"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The distance to apply to all sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDistance<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistance(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.top"/>, <see cref="IStyle.right"/>,
        /// <see cref="IStyle.bottom"/> and <see cref="IStyle.left"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="top">The top offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom offset, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left offset, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.right"/> and <see cref="IStyle.left"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The horizontal offset to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDistanceX<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistanceX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.top"/> and <see cref="IStyle.bottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vertical offset to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDistanceY<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetDistanceY(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.top"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top offset to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTop<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetTop(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.right"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The right offset to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetRight<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.bottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom offset to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBottom<T>(
            this T element,
            StyleLength value)
            where T : VisualElement
        {
            element.style.SetBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.left"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The left offset to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.unityMaterial"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The material to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.scale"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The scale transformation to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetScale<T>(
            this T element,
            StyleScale value)
            where T : VisualElement
        {
            element.style.SetScale(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.rotate"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The rotation to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetRotate<T>(
            this T element,
            StyleRotate value)
            where T : VisualElement
        {
            element.style.SetRotate(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.translate"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The translation to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTranslate<T>(
            this T element,
            StyleTranslate value)
            where T : VisualElement
        {
            element.style.SetTranslate(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.position"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The position type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPosition<T>(
            this T element,
            StyleEnum<Position> value)
            where T : VisualElement
        {
            element.style.SetPosition(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.position"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The position type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPosition<T>(
            this T element,
            Position value)
            where T : VisualElement
        {
            element.style.SetPosition(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.transformOrigin"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The transform origin to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.backgroundColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The background color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetBackgroundColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundColor"/> from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundColor<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetBackgroundColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundImage"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The background image to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundImage<T>(
            this T element,
            StyleBackground value)
            where T : VisualElement
        {
            element.style.SetBackgroundImage(value);
            return element;
        }

        /// <summary>
        /// Loads a <see cref="Texture2D"/> from Resources and sets the <see cref="IStyle.backgroundImage"/> property.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources path of the texture to load.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundImageFromResources<T>(
            this T element,
            string path)
            where T : VisualElement
        {
            element.style.SetBackgroundImageFromResources(path);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The background size to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundSize<T>(
            this T element,
            StyleBackgroundSize value)
            where T : VisualElement
        {
            element.style.SetBackgroundSize(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundRepeat"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The background repeat mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundRepeat<T>(
            this T element,
            StyleBackgroundRepeat value)
            where T : VisualElement
        {
            element.style.SetBackgroundRepeat(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unityBackgroundImageTintColor"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The background image tint color to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityBackgroundImageTintColor<T>(
            this T element,
            StyleColor value)
            where T : VisualElement
        {
            element.style.SetUnityBackgroundImageTintColor(value);
            return element;
        }

        /// <summary>
        /// Sets the background image tint color from an HTML color string.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when <paramref name="value"/> cannot be parsed.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The HTML color string, such as <c>#FF0000</c> or <c>red</c>.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnityBackgroundImageTintColor<T>(
            this T element,
            string value)
            where T : VisualElement
        {
            element.style.SetUnityBackgroundImageTintColor(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundPositionX"/> and <see cref="IStyle.backgroundPositionY"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The background position to apply to both axes.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundPosition<T>(
            this T element,
            StyleBackgroundPosition value)
            where T : VisualElement
        {
            element.style.SetBackgroundPosition(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundPositionX"/> and <see cref="IStyle.backgroundPositionY"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="x">The horizontal background position, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="y">The vertical background position, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.backgroundPositionX"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The horizontal background position to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBackgroundPositionX<T>(
            this T element,
            StyleBackgroundPosition value)
            where T : VisualElement
        {
            element.style.SetBackgroundPositionX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.backgroundPositionY"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vertical background position to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.transitionDelay"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The transition delays to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTransitionDelay<T>(
            this T element,
            StyleList<TimeValue> value)
            where T : VisualElement
        {
            element.style.SetTransitionDelay(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.transitionDuration"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The transition durations to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTransitionDuration<T>(
            this T element,
            StyleList<TimeValue> value)
            where T : VisualElement
        {
            element.style.SetTransitionDuration(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.transitionProperty"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The transition properties to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTransitionProperty<T>(
            this T element,
            StyleList<StylePropertyName> value)
            where T : VisualElement
        {
            element.style.SetTransitionProperty(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.transitionTimingFunction"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The transition timing functions to set.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.unitySliceScale"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The slice scale to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceScale<T>(
            this T element,
            StyleFloat value)
            where T : VisualElement
        {
            element.style.SetUnitySliceScale(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceTop"/>, <see cref="IStyle.unitySliceRight"/>,
        /// <see cref="IStyle.unitySliceBottom"/> and <see cref="IStyle.unitySliceLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The slice width to apply to all sides.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySlice<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySlice(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceTop"/>, <see cref="IStyle.unitySliceRight"/>,
        /// <see cref="IStyle.unitySliceBottom"/> and <see cref="IStyle.unitySliceLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="top">The top slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="right">The right slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="bottom">The bottom slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <param name="left">The left slice width, or <see langword="null"/> to leave unchanged.</param>
        /// <returns>The element, for chaining.</returns>
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
        /// Sets <see cref="IStyle.unitySliceRight"/> and <see cref="IStyle.unitySliceLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The horizontal slice width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceX<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceX(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceTop"/> and <see cref="IStyle.unitySliceBottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The vertical slice width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceY<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceY(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceTop"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The top slice width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceTop<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceTop(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceRight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The right slice width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceRight<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceRight(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceBottom"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The bottom slice width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceBottom<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceBottom(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceLeft"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The left slice width to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceLeft<T>(
            this T element,
            StyleInt value)
            where T : VisualElement
        {
            element.style.SetUnitySliceLeft(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceType"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The slice type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceType<T>(
            this T element,
            StyleEnum<SliceType> value)
            where T : VisualElement
        {
            element.style.SetUnitySliceType(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.unitySliceType"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The slice type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUnitySliceType<T>(
            this T element,
            SliceType value)
            where T : VisualElement
        {
            element.style.SetUnitySliceType(value);
            return element;
        }
        #endregion

        #region Visibility
        /// <summary>
        /// Sets <see cref="IStyle.visibility"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The visibility to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVisibility<T>(
            this T element,
            StyleEnum<Visibility> value)
            where T : VisualElement
        {
            element.style.SetVisibility(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.visibility"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The visibility to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVisibility<T>(
            this T element,
            Visibility value)
            where T : VisualElement
        {
            element.style.SetVisibility(value);
            return element;
        }
        #endregion

        #region WhiteSpace
        /// <summary>
        /// Sets <see cref="IStyle.whiteSpace"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The white-space mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetWhiteSpace<T>(
            this T element,
            StyleEnum<WhiteSpace> value)
            where T : VisualElement
        {
            element.style.SetWhiteSpace(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.whiteSpace"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The white-space mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetWhiteSpace<T>(
            this T element,
            WhiteSpace value)
            where T : VisualElement
        {
            element.style.SetWhiteSpace(value);
            return element;
        }
        #endregion

        #region JustifyContent
        /// <summary>
        /// Sets <see cref="IStyle.justifyContent"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The justify content mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetJustifyContent<T>(
            this T element,
            StyleEnum<Justify> value)
            where T : VisualElement
        {
            element.style.SetJustifyContent(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="IStyle.justifyContent"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The justify content mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetJustifyContent<T>(
            this T element,
            Justify value)
            where T : VisualElement
        {
            element.style.SetJustifyContent(value);
            return element;
        }
        #endregion
    }
}
