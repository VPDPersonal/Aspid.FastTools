using System;
using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static partial class SerializePropertyExtensions
    {
        /// <summary>
        /// Returns the backing field name, including the collection field for an array or list element.
        /// </summary>
        /// <param name="property">The property to inspect.</param>
        /// <returns>The backing field name without collection indices.</returns>
        public static string GetMemberName(this SerializedProperty property)
        {
            var path = property.SimplifyPropertyPath();

            var lastSegment = path[(path.LastIndexOf('.') + 1)..];

            var bracket = lastSegment.IndexOf('[');
            return bracket < 0 ? lastSegment : lastSegment[..bracket];
        }

        /// <summary>
        /// Determines whether <paramref name="property"/> represents an array or list element.
        /// </summary>
        /// <param name="property">The property to inspect.</param>
        /// <returns><see langword="true"/> if the path ends with an element index; otherwise, <see langword="false"/>.</returns>
        public static bool IsArrayElement(this SerializedProperty property) =>
            property.propertyPath.EndsWith("]", StringComparison.Ordinal);

        internal static string SimplifyPropertyPath(string propertyPath) =>
            propertyPath.Replace(".Array.data[", "[");

        internal static string SimplifyPropertyPath(this SerializedProperty property) =>
            SimplifyPropertyPath(property.propertyPath);
    }
}
