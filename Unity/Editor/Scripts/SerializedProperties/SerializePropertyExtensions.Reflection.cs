using System;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Aspid.FastTools.Reflection;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static partial class SerializePropertyExtensions
    {
        private const BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Public
            | System.Reflection.BindingFlags.NonPublic;

        /// <summary>
        /// Returns the <see cref="Type"/> of the field or property that backs this <see cref="SerializedProperty"/>.
        /// For an array/list element property the element type is returned.
        /// </summary>
        /// <param name="serializedProperty">The property to inspect.</param>
        /// <returns>
        /// The <see cref="FieldInfo.FieldType"/> or <see cref="PropertyInfo.PropertyType"/> of the backing member
        /// (the element type when the property is an array/list element),
        /// or <see langword="null"/> if the member cannot be resolved.
        /// </returns>
        public static Type GetPropertyType(this SerializedProperty serializedProperty)
        {
            var type = GetMemberInfo(serializedProperty) switch
            {
                FieldInfo field => field.FieldType,
                PropertyInfo property => property.PropertyType,
                _ => null
            };

            return IsArrayElement(serializedProperty) ? GetElementType(type) : type;
        }

        /// <summary>
        /// Uses reflection to find the <see cref="MemberInfo"/> (field or property) on the owning class
        /// that corresponds to this <see cref="SerializedProperty"/>.
        /// For an array/list element property the collection field itself is returned.
        /// </summary>
        /// <param name="serializedProperty">The property whose backing member should be located.</param>
        /// <returns>
        /// The <see cref="MemberInfo"/> whose name matches <see cref="SerializedProperty.name"/>
        /// (the collection field when the property is an array/list element),
        /// or <see langword="null"/> if it cannot be found.
        /// </returns>
        public static MemberInfo GetMemberInfo(this SerializedProperty serializedProperty)
        {
            var instance = serializedProperty.GetClassInstance();
            if (instance is null) return null;

            var memberName = GetMemberName(serializedProperty);
            return instance.GetType()
                .GetMembersInfosIncludingBaseClasses(BindingFlags)
                .FirstOrDefault(member => member.Name == memberName);
        }

        /// <summary>
        /// Traverses the <see cref="SerializedProperty.propertyPath"/> to return the runtime object instance
        /// that directly owns this property (i.e., the containing class instance, not the root target).
        /// Supports nested objects, arrays, and generic <see cref="List{T}"/> fields;
        /// for an array/list element property the instance owning the collection field is returned.
        /// </summary>
        /// <param name="property">The property whose owning instance should be resolved.</param>
        /// <returns>The runtime object that contains the field represented by <paramref name="property"/>.</returns>
        public static object GetClassInstance(this SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            var path = property.propertyPath.Replace(".Array.data[", "[");

            var lastDotIndex = path.LastIndexOf('.');
            if (lastDotIndex < 0) return target;

            path = path.Remove(lastDotIndex);

            object current = target;

            foreach (var part in path.Split('.'))
            {
                if (part.Contains("["))
                {
                    var startPartIndex = part.IndexOf("[", StringComparison.Ordinal) + 1;
                    var length = part.IndexOf("]", StringComparison.Ordinal) - startPartIndex;

                    var index = int.Parse(part.Substring(startPartIndex, length));
                    current = FindInstance(part[..(startPartIndex - 1)], index);
                }
                else
                {
                    current = FindInstance(part);
                }
            }

            return current;

            object FindInstance(string name, int index = -1)
            {
                if (current is null) return null;

                var field = FindField(current.GetType(), name);
                if (field is null) return null;

                var value = field.GetValue(current);
                return index > -1 && value is IList list
                    ? list[index]
                    : value;
            }

            FieldInfo FindField(Type type, string name)
            {
                return type?.GetMembersInfosIncludingBaseClasses(BindingFlags)
                    .OfType<FieldInfo>()
                    .FirstOrDefault(field => field.Name == name);
            }
        }

        /// <summary>
        /// The name of the member that backs <paramref name="property"/>: <see cref="SerializedProperty.name"/>
        /// for a regular property, the collection field's name for an array/list element
        /// (whose path ends with <c>Array.data[i]</c> while its name is just <c>data</c>).
        /// </summary>
        private static string GetMemberName(SerializedProperty property)
        {
            if (!IsArrayElement(property)) return property.name;

            var path = property.propertyPath.Replace(".Array.data[", "[");
            var lastDotIndex = path.LastIndexOf('.');
            var lastSegment = lastDotIndex < 0 ? path : path[(lastDotIndex + 1)..];

            return lastSegment[..lastSegment.IndexOf('[')];
        }

        /// <summary>
        /// True when <paramref name="property"/> is itself an element of an array or <see cref="List{T}"/>
        /// (its <see cref="SerializedProperty.propertyPath"/> ends with an <c>Array.data[i]</c> segment).
        /// </summary>
        private static bool IsArrayElement(SerializedProperty property) =>
            property.propertyPath.EndsWith("]", StringComparison.Ordinal);

        /// <summary>
        /// Unwraps the element type of an array or single-argument generic collection type;
        /// returns <paramref name="collectionType"/> unchanged when it is not one.
        /// </summary>
        private static Type GetElementType(Type collectionType)
        {
            if (collectionType is null) return null;
            if (collectionType.IsArray) return collectionType.GetElementType();

            return collectionType.IsGenericType && collectionType.GetGenericArguments() is { Length: 1 } arguments
                ? arguments[0]
                : collectionType;
        }
    }
}
