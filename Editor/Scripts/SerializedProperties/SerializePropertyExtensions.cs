using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// Provides extension methods for synchronizing and assigning <see cref="SerializedProperty"/> values.
    /// </summary>
    public static partial class SerializePropertyExtensions
    {
        /// <summary>
        /// Calls <see cref="SerializedObject.Update"/> on the property's serialized object and returns the property for chaining.
        /// </summary>
        /// <typeparam name="T">Concrete <see cref="SerializedProperty"/> type.</typeparam>
        /// <param name="property">The property whose serialized object should be updated.</param>
        /// <returns>The same <paramref name="property"/> instance.</returns>
        public static T Update<T>(this T property)
            where T : SerializedProperty
        {
            property.serializedObject.Update();
            return property;
        }

        /// <summary>
        /// Calls <see cref="SerializedObject.UpdateIfRequiredOrScript"/> on the property's serialized object and returns the property for chaining.
        /// </summary>
        /// <typeparam name="T">Concrete <see cref="SerializedProperty"/> type.</typeparam>
        /// <param name="property">The property whose serialized object should be conditionally updated.</param>
        /// <returns>The same <paramref name="property"/> instance.</returns>
        public static T UpdateIfRequiredOrScript<T>(this T property)
            where T : SerializedProperty
        {
            property.serializedObject.UpdateIfRequiredOrScript();
            return property;
        }

        /// <summary>
        /// Calls <see cref="SerializedObject.ApplyModifiedProperties"/> on the property's serialized object and returns the property for chaining.
        /// </summary>
        /// <typeparam name="T">Concrete <see cref="SerializedProperty"/> type.</typeparam>
        /// <param name="property">The property whose serialized object changes should be applied.</param>
        /// <returns>The same <paramref name="property"/> instance.</returns>
        public static T ApplyModifiedProperties<T>(this T property)
            where T : SerializedProperty
        {
            property.serializedObject.ApplyModifiedProperties();
            return property;
        }

        /// <summary>
        /// Calls <see cref="SerializedObject.ApplyModifiedPropertiesWithoutUndo"/> on the property's serialized object and returns the property for chaining.
        /// </summary>
        /// <typeparam name="T">Concrete <see cref="SerializedProperty"/> type.</typeparam>
        /// <param name="property">The property whose serialized object changes should be applied without registering an undo step.</param>
        /// <returns>The same <paramref name="property"/> instance.</returns>
        public static T ApplyModifiedPropertiesWithoutUndo<T>(this T property)
            where T : SerializedProperty
        {
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return property;
        }

        /// <summary>
        /// Returns the property at the same path on an independent <see cref="SerializedObject"/>.
        /// </summary>
        /// <param name="property">The source property.</param>
        /// <returns>The independent property; otherwise, <see langword="null"/> if the path no longer exists on the targets.</returns>
        /// <remarks>
        /// The caller owns the new serialized object and must dispose it when finished.
        /// Pending changes on the source are not copied until they have been applied to its targets.
        /// </remarks>
        public static SerializedProperty Persistent(this SerializedProperty property) =>
            new SerializedObject(property.serializedObject.targetObjects).FindProperty(property.propertyPath);
    }
}
