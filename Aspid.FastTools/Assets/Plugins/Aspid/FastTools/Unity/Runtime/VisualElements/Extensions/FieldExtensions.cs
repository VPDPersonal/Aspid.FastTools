using UnityEngine.UIElements;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools
{
    public static class FieldExtensions
    {
        /// <summary>
        /// Sets the label text of the field.
        /// </summary>
        /// <returns>The field for method chaining.</returns>
        public static T SetLabel<T, TValueType>(this T field, string label)
            where T : BaseField<TValueType>
        {
            field.label = label;
            return field;
        }

        /// <summary>
        /// Sets the value of the field.
        /// </summary>
        /// <returns>The field for method chaining.</returns>
        public static T SetValue<T, TValueType>(this T field, TValueType value)
            where T : BaseField<TValueType>
        {
            field.value = value;
            return field;
        }
    }
}