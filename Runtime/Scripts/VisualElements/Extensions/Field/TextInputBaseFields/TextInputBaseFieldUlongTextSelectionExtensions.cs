using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides text-selection extension methods for <see cref="TextInputBaseField{TValueType}"/> of <see cref="ulong"/>.
    /// </summary>
    public static class TextInputBaseFieldUlongTextSelectionExtensions
    {
        #region OnCursorIndexChange
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Subscribes to the <see cref="ITextSelection.OnCursorIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnCursorIndexChange<T>(this T element, Action value)
            where T : TextInputBaseField<ulong>
        {
            element.textSelection.OnCursorIndexChange += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ITextSelection.OnCursorIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnCursorIndexChange<T>(this T element, Action value)
            where T : TextInputBaseField<ulong>
        {
            element.textSelection.OnCursorIndexChange -= value;
            return element;
        }
#endif
        #endregion

        #region OnSelectIndexChange
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Subscribes to the <see cref="ITextSelection.OnSelectIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnSelectIndexChange<T>(this T element, Action value)
            where T : TextInputBaseField<ulong>
        {
            element.textSelection.OnSelectIndexChange += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ITextSelection.OnSelectIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnSelectIndexChange<T>(this T element, Action value)
            where T : TextInputBaseField<ulong>
        {
            element.textSelection.OnSelectIndexChange -= value;
            return element;
        }
#endif
        #endregion

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.cursorIndex"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The cursor index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetCursorIndex<T>(this T element, int value)
            where T : TextInputBaseField<ulong>
        {
            element.cursorIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.selectIndex"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The selection index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectIndex<T>(this T element, int value)
            where T : TextInputBaseField<ulong>
        {
            element.selectIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.isSelectable"/> of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the text can be selected.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectable<T>(this T element, bool value)
            where T : TextInputBaseField<ulong>
        {
            element.textSelection.isSelectable = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.selectAllOnFocus"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the whole text is selected when the field receives focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectAllOnFocus<T>(this T element, bool value)
            where T : TextInputBaseField<ulong>
        {
            element.selectAllOnFocus = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.selectAllOnMouseUp"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the whole text is selected on the first mouse up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectAllOnMouseUp<T>(this T element, bool value)
            where T : TextInputBaseField<ulong>
        {
            element.selectAllOnMouseUp = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.doubleClickSelectsWord"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a double click selects the word under the pointer.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDoubleClickSelectsWord<T>(this T element, bool value)
            where T : TextInputBaseField<ulong>
        {
            element.doubleClickSelectsWord = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.tripleClickSelectsLine"/>.
        /// </summary>
        /// <typeparam name="T">The field type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a triple click selects the line under the pointer.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTripleClickSelectsLine<T>(this T element, bool value)
            where T : TextInputBaseField<ulong>
        {
            element.tripleClickSelectsLine = value;
            return element;
        }
    }
}
