using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static partial class SerializePropertyExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when the property renders as an expandable foldout — a
        /// <see cref="SerializedPropertyType.Generic"/> value (a serializable struct/class) that
        /// exposes visible children. Single-line values (primitives, object references, and
        /// child-less generics) return <c>false</c>.
        /// </summary>
        /// <param name="property">The property whose drawing shape is being queried.</param>
        /// <returns><c>true</c> if the property draws with a foldout arrow; otherwise <c>false</c>.</returns>
        public static bool HasFoldout(this SerializedProperty property) =>
            property.propertyType is SerializedPropertyType.Generic && property.hasVisibleChildren;
    }
}
