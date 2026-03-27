using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static class ListViewExtensions
    {
        /// <summary>
        /// Sets the callback used to bind a data item to a visual element in the list.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetBindItem<T>(this T element, Action<VisualElement, int> bindItem)
            where T : ListView
        {
            element.bindItem = bindItem;
            return element;
        }

        #region Make
        /// <summary>
        /// Sets the factory callback used to create visual elements for list items.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMakeItem<T>(this T element, Func<VisualElement> makeItem)
            where T : ListView
        {
            element.makeItem = makeItem;
            return element;
        }

#if UNITY_6000_0_OR_NEWER
        /// <summary>
        /// Sets the factory callback used to create the footer element of the list.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMakeFooter<T>(this T element, Func<VisualElement> makeFooter)
            where T : ListView
        {
            element.makeFooter = makeFooter;
            return element;
        }

        /// <summary>
        /// Sets the factory callback used to create the header element of the list.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMakeHeader<T>(this T element, Func<VisualElement> makeHeader)
            where T : ListView
        {
            element.makeHeader = makeHeader;
            return element;
        }

        /// <summary>
        /// Sets the factory callback used to create the element displayed when the list is empty.
        /// </summary>
        /// <returns>The element for method chaining.</returns>
        public static T SetMakeNoneElement<T>(this T element, Func<VisualElement> makeNoneElement)
            where T : ListView
        {
            element.makeNoneElement = makeNoneElement;
            return element;
        }
#endif
        #endregion
    }
}