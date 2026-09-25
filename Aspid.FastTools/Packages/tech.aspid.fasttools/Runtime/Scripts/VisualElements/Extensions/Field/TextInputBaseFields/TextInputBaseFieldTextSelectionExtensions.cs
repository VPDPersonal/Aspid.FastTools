using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides text-selection extension methods for <see cref="TextInputBaseField{TValueType}"/>.
    /// </summary>
    public static class TextInputBaseFieldTextSelectionExtensions
    {
        #region OnCursorIndexChange
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Subscribes to the <see cref="ITextSelection.OnCursorIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField AddOnCursorIndexChange<TField, TValue>(this TField element, Action value)
            where TField : TextInputBaseField<TValue>
        {
            element.textSelection.OnCursorIndexChange += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ITextSelection.OnCursorIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField RemoveOnCursorIndexChange<TField, TValue>(this TField element, Action value)
            where TField : TextInputBaseField<TValue>
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
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField AddOnSelectIndexChange<TField, TValue>(this TField element, Action value)
            where TField : TextInputBaseField<TValue>
        {
            element.textSelection.OnSelectIndexChange += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ITextSelection.OnSelectIndexChange"/> event of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField RemoveOnSelectIndexChange<TField, TValue>(this TField element, Action value)
            where TField : TextInputBaseField<TValue>
        {
            element.textSelection.OnSelectIndexChange -= value;
            return element;
        }
#endif
        #endregion

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.cursorIndex"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The cursor index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetCursorIndex<TField, TValue>(this TField element, int value)
            where TField : TextInputBaseField<TValue>
        {
            element.cursorIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.selectIndex"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The selection index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetSelectIndex<TField, TValue>(this TField element, int value)
            where TField : TextInputBaseField<TValue>
        {
            element.selectIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.isSelectable"/> of <see cref="TextInputBaseField{TValueType}.textSelection"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the text can be selected.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetSelectable<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.textSelection.isSelectable = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.selectAllOnFocus"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the whole text is selected when the field receives focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetSelectAllOnFocus<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.selectAllOnFocus = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.selectAllOnMouseUp"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the whole text is selected on the first mouse up.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetSelectAllOnMouseUp<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.selectAllOnMouseUp = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.doubleClickSelectsWord"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a double click selects the word under the pointer.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetDoubleClickSelectsWord<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.doubleClickSelectsWord = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="TextInputBaseField{TValueType}.tripleClickSelectsLine"/>.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a triple click selects the line under the pointer.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetTripleClickSelectsLine<TField, TValue>(this TField element, bool value)
            where TField : TextInputBaseField<TValue>
        {
            element.tripleClickSelectsLine = value;
            return element;
        }
    }
}
