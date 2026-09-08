#nullable enable
using System;
using UnityEditor;
using Aspid.FastTools.Editors;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class SerializableTypeUtility
    {
        internal const string BackingFieldName = "_assemblyQualifiedName";

        // BaseType is an instance member, so reading it means instantiating the wrapper; the drawers ask on every
        // repaint, so the answer is memoized per wrapper type (stable until a domain reload clears statics).
        private static readonly Dictionary<Type, Type> _baseTypes = new();

        internal static bool IsSerializableTypeField(Type fieldType) =>
            typeof(ISerializableType).IsAssignableFrom(fieldType.GetCollectionElementTypeOrSelf());

        internal static bool TryGetBaseType(Type fieldType, out Type? baseType)
        {
            var type = fieldType.GetCollectionElementTypeOrSelf();

            if (!typeof(ISerializableType).IsAssignableFrom(type) || type.IsAbstract || type.IsInterface)
            {
                baseType = null;
                return false;
            }

            if (!_baseTypes.TryGetValue(type, out var cached))
            {
                cached = ((ISerializableType)Activator.CreateInstance(type, nonPublic: true)).BaseType;
                _baseTypes[type] = cached;
            }

            baseType = cached;
            return true;
        }

        internal static SerializedProperty? GetBackingProperty(SerializedProperty wrapperProperty) =>
            wrapperProperty.FindPropertyRelative(BackingFieldName);
    }
}
