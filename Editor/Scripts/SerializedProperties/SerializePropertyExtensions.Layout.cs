using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static partial class SerializePropertyExtensions
    {
        /// <summary>
        /// Determines whether a generic serialized value has visible children.
        /// </summary>
        /// <param name="property">The property to inspect.</param>
        /// <returns><see langword="true"/> for a generic value with visible children; otherwise, <see langword="false"/>.</returns>
        /// <remarks>Managed references and custom drawer layouts are not covered by this check.</remarks>
        public static bool HasFoldout(this SerializedProperty property) =>
            property.propertyType is SerializedPropertyType.Generic && property.hasVisibleChildren;
    }
}
