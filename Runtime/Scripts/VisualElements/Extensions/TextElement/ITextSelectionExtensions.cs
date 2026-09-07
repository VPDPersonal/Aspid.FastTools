using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="ITextSelection"/>.
    /// </summary>
    public static class ITextSelectionExtensions
    {
        #region OnCursorIndexChange
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Subscribes to the <see cref="ITextSelection.OnCursorIndexChange"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnCursorIndexChange<T>(this T element, Action value)
            where T : ITextSelection
        {
            element.OnCursorIndexChange += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ITextSelection.OnCursorIndexChange"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnCursorIndexChange<T>(this T element, Action value)
            where T : ITextSelection
        {
            element.OnCursorIndexChange -= value;
            return element;
        }
#endif
        #endregion

        #region OnSelectIndexChange
#if UNITY_6000_3_OR_NEWER
        /// <summary>
        /// Subscribes to the <see cref="ITextSelection.OnSelectIndexChange"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnSelectIndexChange<T>(this T element, Action value)
            where T : ITextSelection
        {
            element.OnSelectIndexChange += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="ITextSelection.OnSelectIndexChange"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnSelectIndexChange<T>(this T element, Action value)
            where T : ITextSelection
        {
            element.OnSelectIndexChange -= value;
            return element;
        }
#endif
        #endregion

        /// <summary>
        /// Sets <see cref="ITextSelection.cursorIndex"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The cursor index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetCursorIndex<T>(this T element, int value)
            where T : ITextSelection
        {
            element.cursorIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.selectIndex"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The selection index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectIndex<T>(this T element, int value)
            where T : ITextSelection
        {
            element.selectIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.isSelectable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the text can be selected.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectable<T>(this T element, bool value)
            where T : ITextSelection
        {
            element.isSelectable = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.selectAllOnFocus"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the whole text is selected when the element receives focus.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectAllOnFocus<T>(this T element, bool value)
            where T : ITextSelection
        {
            element.selectAllOnFocus = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.selectAllOnMouseUp"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the whole text is selected on the first mouse up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectAllOnMouseUp<T>(this T element, bool value)
            where T : ITextSelection
        {
            element.selectAllOnMouseUp = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.doubleClickSelectsWord"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a double click selects the word under the pointer.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDoubleClickSelectsWord<T>(this T element, bool value)
            where T : ITextSelection
        {
            element.doubleClickSelectsWord = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="ITextSelection.tripleClickSelectsLine"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a triple click selects the line under the pointer.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTripleClickSelectsLine<T>(this T element, bool value)
            where T : ITextSelection
        {
            element.tripleClickSelectsLine = value;
            return element;
        }
    }
}
