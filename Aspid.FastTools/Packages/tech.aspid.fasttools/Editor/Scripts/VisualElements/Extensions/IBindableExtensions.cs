using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors
{
    /// <summary>
    /// Provides binding extension methods for <see cref="IBindable"/> elements.
    /// </summary>
    public static class IBindableExtensions
    {
        /// <summary>
        /// Sets the binding path and binds the element to <paramref name="serializedObject"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to bind.</param>
        /// <param name="serializedObject">The serialized object to bind to.</param>
        /// <param name="propertyPath">The property path to bind to.</param>
        /// <returns>The element, for chaining.</returns>
        public static T BindTo<T>(this T element, SerializedObject serializedObject, string propertyPath)
            where T : VisualElement, IBindable
        {
            element.bindingPath = propertyPath;
            element.Bind(serializedObject);
            return element;
        }

        /// <summary>
        /// Binds the element to <paramref name="property"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to bind.</param>
        /// <param name="property">The property to bind to.</param>
        /// <returns>The element, for chaining.</returns>
        public static T BindPropertyTo<T>(this T element, SerializedProperty property)
            where T : VisualElement, IBindable
        {
            element.BindProperty(property);
            return element;
        }

        /// <summary>
        /// Sets the element's binding path.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="value">The binding path to set.</param>
        /// <returns>The element, for chaining.</returns>
        public static T SetBindingPath<T>(this T element, string value)
            where T : VisualElement, IBindable
        {
            element.bindingPath = value;
            return element;
        }
    }
}
