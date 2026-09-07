using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="BaseField{TValueType}"/>.
    /// </summary>
    public static class BaseFieldExtensions
    {
        /// <summary>
        /// Sets <see cref="BaseField{TValueType}.label"/> displayed next to the field.
        /// </summary>
        /// <typeparam name="TField">The field type.</typeparam>
        /// <typeparam name="TValue">The value type held by the field.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The label text to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static TField SetLabel<TField, TValue>(this TField element, string value)
            where TField : BaseField<TValue>
        {
            element.label = value;
            return element;
        }
    }
}
