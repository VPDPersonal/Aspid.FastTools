using System;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseListView"/>.
    /// </summary>
    public static class BaseListViewExtensions
    {
        #region OnAdd
        /// <summary>
        /// Sets <see cref="BaseListView.onAdd"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOnAdd<T>(this T element, Action<BaseListView> value)
            where T : BaseListView
        {
            element.onAdd = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="BaseListView.onAdd"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnAdd<T>(this T element, Action<BaseListView> value)
            where T : BaseListView
        {
            element.onAdd += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseListView.onAdd"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnAdd<T>(this T element, Action<BaseListView> value)
            where T : BaseListView
        {
            element.onAdd -= value;
            return element;
        }
        #endregion

        #region OnRemove
        /// <summary>
        /// Sets <see cref="BaseListView.onRemove"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOnRemove<T>(this T element, Action<BaseListView> value)
            where T : BaseListView
        {
            element.onRemove = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="BaseListView.onRemove"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnRemove<T>(this T element, Action<BaseListView> value)
            where T : BaseListView
        {
            element.onRemove += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseListView.onRemove"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnRemove<T>(this T element, Action<BaseListView> value)
            where T : BaseListView
        {
            element.onRemove -= value;
            return element;
        }
        #endregion

        #region ItemsAdded
        /// <summary>
        /// Subscribes to the <see cref="BaseListView.itemsAdded"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItemsAdded<T>(this T element, Action<IEnumerable<int>> value)
            where T : BaseListView
        {
            element.itemsAdded += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseListView.itemsAdded"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItemsAdded<T>(this T element, Action<IEnumerable<int>> value)
            where T : BaseListView
        {
            element.itemsAdded -= value;
            return element;
        }
        #endregion

        #region ItemsRemoved
        /// <summary>
        /// Subscribes to the <see cref="BaseListView.itemsRemoved"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddItemsRemoved<T>(this T element, Action<IEnumerable<int>> value)
            where T : BaseListView
        {
            element.itemsRemoved += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseListView.itemsRemoved"/> event.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveItemsRemoved<T>(this T element, Action<IEnumerable<int>> value)
            where T : BaseListView
        {
            element.itemsRemoved -= value;
            return element;
        }
        #endregion

        #region OverridingAddButtonBehavior
        /// <summary>
        /// Sets <see cref="BaseListView.overridingAddButtonBehavior"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOverridingAddButtonBehavior<T>(this T element, Action<BaseListView, Button> value)
            where T : BaseListView
        {
            element.overridingAddButtonBehavior = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="BaseListView.overridingAddButtonBehavior"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOverridingAddButtonBehavior<T>(this T element, Action<BaseListView, Button> value)
            where T : BaseListView
        {
            element.overridingAddButtonBehavior += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="BaseListView.overridingAddButtonBehavior"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOverridingAddButtonBehavior<T>(this T element, Action<BaseListView, Button> value)
            where T : BaseListView
        {
            element.overridingAddButtonBehavior -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="BaseListView.allowRemove"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the Remove button removes an item.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAllowRemove<T>(this T element, bool value)
            where T : BaseListView
        {
            element.allowRemove = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.allowAdd"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the Add button adds an item.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetAllowAdd<T>(this T element, bool value)
            where T : BaseListView
        {
            element.allowAdd = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.headerTitle"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The header title to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetHeaderTitle<T>(this T element, string value)
            where T : BaseListView
        {
            element.headerTitle = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.showFoldoutHeader"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the list is wrapped in a foldout header.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetShowFoldoutHeader<T>(this T element, bool value)
            where T : BaseListView
        {
            element.showFoldoutHeader = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.showAddRemoveFooter"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the footer with the Add and Remove buttons is shown.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetShowAddRemoveFooter<T>(this T element, bool value)
            where T : BaseListView
        {
            element.showAddRemoveFooter = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.showBoundCollectionSize"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the size of the bound collection is shown.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetShowBoundCollectionSize<T>(this T element, bool value)
            where T : BaseListView
        {
            element.showBoundCollectionSize = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.reorderMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The reorder mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetReorderMode<T>(this T element, ListViewReorderMode value)
            where T : BaseListView
        {
            element.reorderMode = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.bindingSourceSelectionMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The binding source selection mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBindingSourceSelectionMode<T>(this T element, BindingSourceSelectionMode value)
            where T : BaseListView
        {
            element.bindingSourceSelectionMode = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.makeFooter"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMakeFooter<T>(this T element, Func<VisualElement> value)
            where T : BaseListView
        {
            element.makeFooter = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.makeHeader"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMakeHeader<T>(this T element, Func<VisualElement> value)
            where T : BaseListView
        {
            element.makeHeader = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="BaseListView.makeNoneElement"/>, replacing any existing callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetMakeNoneElement<T>(this T element, Func<VisualElement> value)
            where T : BaseListView
        {
            element.makeNoneElement = value;
            return element;
        }
    }
}
