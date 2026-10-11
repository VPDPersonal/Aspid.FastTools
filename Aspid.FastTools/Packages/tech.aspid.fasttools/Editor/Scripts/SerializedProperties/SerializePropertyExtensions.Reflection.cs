using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

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
        /// <remarks>For an array or list element, returns the collection field.</remarks>
        /// <param name="property">The property whose backing field to locate.</param>
        /// <returns>The backing field; otherwise, <see langword="null"/> if it cannot be resolved.</returns>
        public static FieldInfo GetFieldInfo(this SerializedProperty property)
        {
            var owner = property.GetDeclaringInstance();
            return owner is null ? null : GetFieldIncludingBaseClasses(owner.GetType(), property.GetMemberName());
        }

        /// <summary>
        /// Returns the instance declaring the backing field, including the collection owner for array or list elements.
        /// </summary>
        /// <remarks>A struct instance is a boxed copy; modifying it does not update the serialized object.</remarks>
        /// <param name="property">The property whose declaring instance to resolve.</param>
        /// <returns>The declaring instance; otherwise, <see langword="null"/> if the path cannot be resolved.</returns>
        public static object GetDeclaringInstance(this SerializedProperty property)
        {
            object current = property.serializedObject.targetObject;

            var path = property.SimplifyPropertyPath();
            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex < 0) return current;

            // Walks the owner path in place: IMGUI resolves it on every event of a nested reference.
            for (var start = 0; start < lastDotIndex;)
            {
                if (current is null) return null;

                var separator = path.IndexOf('.', start, lastDotIndex - start);
                var end = separator < 0 ? lastDotIndex : separator;

                var bracket = path.IndexOf('[', start, end - start);
                var nameEnd = bracket < 0 ? end : bracket;

                current = GetFieldIncludingBaseClasses(current.GetType(), path.Substring(start, nameEnd - start))?.GetValue(current);

                if (bracket >= 0 && current is IList list)
                {
                    var index = int.Parse(path.AsSpan(bracket + 1, end - bracket - 2));
                    current = index < list.Count ? list[index] : null;
                }

                start = end + 1;
            }

            return current;
        }

        // Unity's own list honors [NonReorderable]; a list the package draws in its place must too.
        internal static bool IsNonReorderable(this SerializedProperty listProperty) =>
            listProperty.GetFieldInfo()?.IsDefined(typeof(NonReorderableAttribute), inherit: true) ?? false;

        // Fields change only with a domain reload, which also clears this cache. A missing field is cached too.
        private static readonly Dictionary<(Type, string), FieldInfo> FieldCache = new();

        private static FieldInfo GetFieldIncludingBaseClasses(Type type, string name)
        {
            if (FieldCache.TryGetValue((type, name), out var cached)) return cached;
            return FieldCache[(type, name)] = FindFieldIncludingBaseClasses(type, name);
        }

        private static FieldInfo FindFieldIncludingBaseClasses(Type type, string name)
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
