using System;
using UnityEditor.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    /// <summary>
    /// Provides extension methods for <see cref="EnumFlagsField"/>.
    /// </summary>
    public static class EnumFlagsFieldExtensions
    {
        /// <summary>
        /// Initializes the field with a default flags value.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="defaultValue">The flags value shown initially.</param>
        /// <param name="includeObsoleteValues">When <see langword="true"/>, obsolete enum values appear in the
        /// choices.</param>
        /// <returns>The element, for chaining.</returns>
        public static T Initialize<T>(this T element, Enum defaultValue, bool includeObsoleteValues = false)
            where T : EnumFlagsField
        {
            element.Init(defaultValue, includeObsoleteValues);
            return element;
        }
    }
}
