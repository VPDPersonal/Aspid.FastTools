using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="IMixedValueSupport"/>.
    /// </summary>
    public static class IMixedValueSupportExtensions
    {
        /// <summary>
        /// Sets <see cref="IMixedValueSupport.showMixedValue"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">When <see langword="true"/>, the mixed value state is shown.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetShowMixedValue<T>(this T element, bool value)
            where T : IMixedValueSupport
        {
            element.showMixedValue = value;
            return element;
        }
    }
}
