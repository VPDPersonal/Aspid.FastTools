using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Editor-session clipboard behind the drawer's Copy/Paste entries. The value is kept as JSON plus its concrete
    // type, so a paste builds an independent instance instead of aliasing the source.
    internal static class SerializeReferenceClipboard
    {
        private static bool _hasContent;
        private static string _json;

        // Null when an empty reference was copied.
        public static Type Type { get; private set; }

        // Copying null is meaningful: the next paste clears the target field.
        public static void Copy(object value)
        {
            _hasContent = true;
            Type = value?.GetType();
            _json = value is null ? null : JsonUtility.ToJson(value);
        }

        // The filter applies the same [TypeSelector] narrowing the picker enforces, so paste cannot assign a type the
        // dropdown would hide. An empty reference always pastes, since it clears the field.
        public static bool CanPasteInto(Type fieldType, Func<Type, bool> filter = null)
        {
            if (!_hasContent) return false;
            if (Type is null) return true;
            if (fieldType is not null && !fieldType.IsAssignableFrom(Type)) return false;
            return filter is null || filter(Type);
        }

        // Builds an instance independent of the copied object; null when an empty reference was copied.
        public static object CreateInstance()
        {
            if (!_hasContent || Type is null) return null;

            return string.IsNullOrEmpty(_json)
                ? SerializeReferenceHelpers.CreateInstance(Type)
                : JsonUtility.FromJson(_json, Type);
        }
    }
}
