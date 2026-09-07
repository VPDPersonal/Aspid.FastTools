using System;
using Unity.Properties;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="VisualElement"/>.
    /// </summary>
    public static partial class VisualElementExtensions
    {
        /// <summary>
        /// Sets <see cref="VisualElement.name"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The name to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetName<T>(this T element, string value)
            where T : VisualElement
        {
            element.name = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.visible"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the element is rendered.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetVisible<T>(this T element, bool value)
            where T : VisualElement
        {
            element.visible = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.tooltip"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The tooltip text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetTooltip<T>(this T element, string value)
            where T : VisualElement
        {
            element.tooltip = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.userData"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The user data to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUserData<T>(this T element, object value)
            where T : VisualElement
        {
            element.userData = value;
            return element;
        }

        /// <summary>
        /// Enables or disables the element via <see cref="VisualElement.SetEnabled"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the element is enabled; a disabled element receives most events no longer.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetEnabledSelf<T>(this T element, bool value)
            where T : VisualElement
        {
            element.SetEnabled(value);
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.dataSource"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The data source to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDataSource<T>(this T element, object value)
            where T : VisualElement
        {
            element.dataSource = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.viewDataKey"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The view data key to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetViewDataKey<T>(this T element, string value)
            where T : VisualElement
        {
            element.viewDataKey = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.dataSourceType"/>.
        /// </summary>
        /// <remarks>
        /// The type is only a design-time hint for the UI Builder; it does not affect runtime binding.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The data source type to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDataSourceType<T>(this T element, Type value)
            where T : VisualElement
        {
            element.dataSourceType = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.usageHints"/>.
        /// </summary>
        /// <remarks>
        /// Must be called before the element is added to a panel; afterwards the property is effectively read-only and throws on assignment.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The usage hints to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetUsageHints<T>(this T element, UsageHints value)
            where T : VisualElement
        {
            element.usageHints = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.pickingMode"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The picking mode to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetPickingMode<T>(this T element, PickingMode value)
            where T : VisualElement
        {
            element.pickingMode = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.disablePlayModeTint"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the play-mode tint is not applied to the element and its children.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDisablePlayModeTint<T>(this T element, bool value)
            where T : VisualElement
        {
            element.disablePlayModeTint = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.dataSourcePath"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The data source path to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetDataSourcePath<T>(this T element, PropertyPath value)
            where T : VisualElement
        {
            element.dataSourcePath = value;
            return element;
        }

        /// <summary>
        /// Sets <see cref="VisualElement.languageDirection"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The language direction to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetLanguageDirection<T>(this T element, LanguageDirection value)
            where T : VisualElement
        {
            element.languageDirection = value;
            return element;
        }
    }
}
