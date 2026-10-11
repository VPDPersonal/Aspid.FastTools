using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="RadioButtonGroup"/>.
    /// </summary>
    public static class RadioButtonGroupExtensions
    {
        /// <summary>
        /// Sets <see cref="RadioButtonGroup.choices"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The labels of the radio buttons to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetChoices<T>(this T element, IEnumerable<string> value)
            where T : RadioButtonGroup
        {
            element.choices = value;
            return element;
        }
    }
}
