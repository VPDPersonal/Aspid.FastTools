using System;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="EnumField"/>.
    /// </summary>
    public static class EnumFieldExtensions
    {
        /// <summary>
        /// Initializes the field with a default enum value via <see cref="EnumField.Init(Enum, bool)"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="defaultValue">The default enum value to display.</param>
        /// <param name="includeObsoleteValues">When <see langword="true"/>, obsolete enum values are included in the choices.</param>
        /// <returns>The element, for chaining.</returns>
        public static T Initialize<T>(this T element, Enum defaultValue, bool includeObsoleteValues = false)
            where T : EnumField
        {
            element.Init(defaultValue, includeObsoleteValues);
            return element;
        }
    }
}
