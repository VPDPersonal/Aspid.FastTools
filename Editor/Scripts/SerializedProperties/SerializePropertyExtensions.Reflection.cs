using System;
using UnityEditor;
using System.Reflection;
using System.Collections;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static partial class SerializePropertyExtensions
    {
        /// <summary>
        /// Returns the backing field type or, for an array or list element, its element type.
        /// </summary>
        /// <param name="serializedProperty">The property to inspect.</param>
        /// <returns>The declared type; otherwise, <see langword="null"/> if the backing field cannot be resolved.</returns>
        public static Type GetPropertyType(this SerializedProperty serializedProperty)
        {
            var type = serializedProperty.GetFieldInfo()?.FieldType;
            return IsArrayElement(serializedProperty) ? type?.GetCollectionElementTypeOrSelf() : type;
        }

        /// <summary>
        /// Resolves the backing field on the runtime type of the declaring instance or its base classes.
        /// </summary>
        /// <param name="property">The property whose backing field to locate.</param>
        /// <returns>The backing field; otherwise, <see langword="null"/> if it cannot be resolved.</returns>
        /// <remarks>For an array or list element, returns the collection field.</remarks>
        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            var owner = property.GetDeclaringInstance();
            return owner is null ? null : GetFieldIncludingBaseClasses(owner.GetType(), property.GetMemberName());
        }

        /// <summary>
        /// Returns the instance declaring the backing field, including the collection owner for array or list elements.
        /// </summary>
        /// <param name="property">The property whose declaring instance to resolve.</param>
        /// <returns>The declaring instance; otherwise, <see langword="null"/> if the path cannot be resolved.</returns>
        /// <remarks>A struct instance is a boxed copy; modifying it does not update the serialized object.</remarks>
        public static object GetDeclaringInstance(this SerializedProperty property)
        {
            object current = property.serializedObject.targetObject;

            var path = property.SimplifyPropertyPath();
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex < 0) return current;

            foreach (var part in path[..lastDotIndex].Split('.'))
            {
                if (current is null) return null;

                var bracket = part.IndexOf('[');
                var name = bracket < 0 ? part : part[..bracket];

                current = GetFieldIncludingBaseClasses(current.GetType(), name)?.GetValue(current);

                if (bracket >= 0 && current is IList list)
                {
                    var index = int.Parse(part[(bracket + 1)..^1]);
                    current = index < list.Count ? list[index] : null;
                }
            }

            return current;
        }

        private static FieldInfo GetFieldIncludingBaseClasses(Type type, string name)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (var current = type; current is not null; current = current.BaseType)
            {
                var field = current.GetField(name, bindingAttr);
                if (field is not null) return field;
            }

            return null;
        }
    }
}
