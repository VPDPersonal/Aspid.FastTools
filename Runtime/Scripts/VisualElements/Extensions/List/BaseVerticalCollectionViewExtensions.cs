using System;
using System.Collections;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseVerticalCollectionView"/>.
    /// </summary>
    public static class BaseVerticalCollectionViewExtensions
    {
        #region ItemsChosen
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.itemsChosen"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItemsChosen<T>(this T element, Action<IEnumerable<object>> value)
            where T : BaseVerticalCollectionView
        {
            element.itemsChosen += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.itemsChosen"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItemsChosen<T>(this T element, Action<IEnumerable<object>> value)
            where T : BaseVerticalCollectionView
        {
            element.itemsChosen -= value;
            return element;
        }
        #endregion

        #region CanStartDrag
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.canStartDrag"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddCanStartDrag<T>(this T element, Func<CanStartDragArgs, bool> value)
            where T : BaseVerticalCollectionView
        {
            element.canStartDrag += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.canStartDrag"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveCanStartDrag<T>(this T element, Func<CanStartDragArgs, bool> value)
            where T : BaseVerticalCollectionView
        {
            element.canStartDrag -= value;
            return element;
        }
        #endregion

        #region SelectionChanged
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.selectionChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddSelectionChanged<T>(this T element, Action<IEnumerable<object>> value)
            where T : BaseVerticalCollectionView
        {
            element.selectionChanged += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.selectionChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveSelectionChanged<T>(this T element, Action<IEnumerable<object>> value)
            where T : BaseVerticalCollectionView
        {
            element.selectionChanged -= value;
            return element;
        }
        #endregion

        #region ItemIndexChanged
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.itemIndexChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItemIndexChanged<T>(this T element, Action<int, int> value)
            where T : BaseVerticalCollectionView
        {
            element.itemIndexChanged += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.itemIndexChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItemIndexChanged<T>(this T element, Action<int, int> value)
            where T : BaseVerticalCollectionView
        {
            element.itemIndexChanged -= value;
            return element;
        }
        #endregion

        #region SetupDragAndDrop
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.setupDragAndDrop"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddSetupDragAndDrop<T>(this T element, Func<SetupDragAndDropArgs, StartDragArgs> value)
            where T : BaseVerticalCollectionView
        {
            element.setupDragAndDrop += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.setupDragAndDrop"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveSetupDragAndDrop<T>(this T element, Func<SetupDragAndDropArgs, StartDragArgs> value)
            where T : BaseVerticalCollectionView
        {
            element.setupDragAndDrop -= value;
            return element;
        }
        #endregion

        #region DragAndDropUpdate
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.dragAndDropUpdate"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddDragAndDropUpdate<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value)
            where T : BaseVerticalCollectionView
        {
            element.dragAndDropUpdate += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.dragAndDropUpdate"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveDragAndDropUpdate<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value)
            where T : BaseVerticalCollectionView
        {
            element.dragAndDropUpdate -= value;
            return element;
        }
        #endregion

        #region HandleDrop
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.handleDrop"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddHandleDrop<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value)
            where T : BaseVerticalCollectionView
        {
            element.handleDrop += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.handleDrop"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveHandleDrop<T>(this T element, Func<HandleDragAndDropArgs, DragVisualMode> value)
            where T : BaseVerticalCollectionView
        {
            element.handleDrop -= value;
            return element;
        }
        #endregion

        #region ItemsSourceChanged
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.itemsSourceChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItemsSourceChanged<T>(this T element, Action value)
            where T : BaseVerticalCollectionView
        {
            element.itemsSourceChanged += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.itemsSourceChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItemsSourceChanged<T>(this T element, Action value)
            where T : BaseVerticalCollectionView
        {
            element.itemsSourceChanged -= value;
            return element;
        }
        #endregion

        #region SelectedIndicesChanged
        /// <summary>
        /// Subscribes to the <see cref="BaseVerticalCollectionView.selectedIndicesChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddSelectedIndicesChanged<T>(this T element, Action<IEnumerable<int>> value)
            where T : BaseVerticalCollectionView
        {
            element.selectedIndicesChanged += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseVerticalCollectionView.selectedIndicesChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveSelectedIndicesChanged<T>(this T element, Action<IEnumerable<int>> value)
            where T : BaseVerticalCollectionView
        {
            element.selectedIndicesChanged -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.itemsSource"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The items source to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetItemsSource<T>(this T element, IList value)
            where T : BaseVerticalCollectionView
        {
            element.itemsSource = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.reorderable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, items can be reordered by dragging.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetReorderable<T>(this T element, bool value)
            where T : BaseVerticalCollectionView
        {
            element.reorderable = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.selectedIndex"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The selected index to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectedIndex<T>(this T element, int value)
            where T : BaseVerticalCollectionView
        {
            element.selectedIndex = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.fixedItemHeight"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The fixed item height in pixels.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetFixedItemHeight<T>(this T element, float value)
            where T : BaseVerticalCollectionView
        {
            element.fixedItemHeight = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.selectionType"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The selection type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetSelectionType<T>(this T element, SelectionType value)
            where T : BaseVerticalCollectionView
        {
            element.selectionType = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.horizontalScrollingEnabled"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, a horizontal scroll bar is shown when the content does not fit.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHorizontalScrollingEnabled<T>(this T element, bool value)
            where T : BaseVerticalCollectionView
        {
            element.horizontalScrollingEnabled = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.virtualizationMethod"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The virtualization method to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVirtualizationMethod<T>(this T element, CollectionVirtualizationMethod value)
            where T : BaseVerticalCollectionView
        {
            element.virtualizationMethod = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseVerticalCollectionView.showAlternatingRowBackgrounds"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The alternating row background mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetShowAlternatingRowBackgrounds<T>(this T element, AlternatingRowBackground value)
            where T : BaseVerticalCollectionView
        {
            element.showAlternatingRowBackgrounds = value;
            return element;
        }
    }
}
