using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="IMGUIContainer"/>.
    /// </summary>
    public static class IMGUIContainerExtensions
    {
        #region OnGUIHandler
        /// <summary>
        /// Sets the <see cref="IMGUIContainer.onGUIHandler"/> callback, replacing any existing handler.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The handler to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetOnGUIHandler<T>(this T element, Action value)
            where T : IMGUIContainer
        {
            element.onGUIHandler = value;
            return element;
        }

        /// <summary>
        /// Subscribes to the <see cref="IMGUIContainer.onGUIHandler"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The handler to add.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddOnGUIHandler<T>(this T element, Action value)
            where T : IMGUIContainer
        {
            element.onGUIHandler += value;
            return element;
        }

        /// <summary>
        /// Unsubscribes from the <see cref="IMGUIContainer.onGUIHandler"/> callback.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The handler to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveOnGUIHandler<T>(this T element, Action value)
            where T : IMGUIContainer
        {
            element.onGUIHandler -= value;
            return element;
        }
        #endregion

        /// <summary>
        /// Sets <see cref="IMGUIContainer.cullingEnabled"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the handler is not called while the element is outside the viewport.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetCullingEnabled<T>(this T element, bool value)
            where T : IMGUIContainer
        {
            element.cullingEnabled = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="IMGUIContainer.contextType"/>.
        /// </summary>
        /// <remarks>
        /// Only <see cref="ContextType.Editor"/> is currently supported by Unity.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The context type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetContextType<T>(this T element, ContextType value)
            where T : IMGUIContainer
        {
            element.contextType = value;
            return element;
        }

        /// <summary>
        /// Marks the IMGUI layout as dirty via <see cref="IMGUIContainer.MarkDirtyLayout"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T MarkDirtyLayout<T>(this T element)
            where T : IMGUIContainer
        {
            element.MarkDirtyLayout();
            return element;
        }
    }
}
