using UnityEngine.UIElements;
using UnityEditor.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    /// <summary>
    /// Provides extension methods for <see cref="PropertyField"/>.
    /// </summary>
    public static class PropertyFieldExtensions
    {
        /// <summary>
        /// Subscribes to the element's value-changed event.
        /// </summary>
        /// <remarks>
        /// The callback stays registered until removed. Pass an equal delegate to
        /// <see cref="RemoveValueChanged{T}(T, EventCallback{SerializedPropertyChangeEvent})"/>: a method group or a delegate kept in a field.
        /// An inline lambda cannot be removed, and registering it again, for example in <c>ListView.bindItem</c>, adds one more.
        /// </remarks>
        /// <typeparam name="T">A <see cref="PropertyField"/> element to configure.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to subscribe.</param>
        /// <returns><paramref name="element"/> for chaining.</returns>
        public static T AddValueChanged<T>(this T element, EventCallback<SerializedPropertyChangeEvent> value)
            where T : PropertyField
        {
            element.RegisterCallback<SerializedPropertyChangeEvent>(value);
            return element;
        }
        
        /// <summary>
        /// Unsubscribes from the element's value-changed event.
        /// </summary>
        /// <typeparam name="T">A <see cref="PropertyField"/> element to configure.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The callback to remove.</param>
        /// <returns><paramref name="element"/> for chaining.</returns>
        public static T RemoveValueChanged<T>(this T element, EventCallback<SerializedPropertyChangeEvent> value)
            where T : PropertyField
        {
            element.UnregisterCallback(value);
            return element;
        }

        /// <summary>
        /// Sets the field's label.
        /// </summary>
        /// <typeparam name="T">A <see cref="PropertyField"/> element to configure.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The label text to set.</param>
        /// <returns><paramref name="element"/> for chaining.</returns>
        public static T SetLabel<T>(this T element, string value)
            where T : PropertyField
        {
            element.label = value;
            return element;
        }
    }
}
