using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Editor-session clipboard backing the Copy/Paste context-menu entries of the
    /// <c>[SerializeReferenceSelector]</c> drawers. Stores the copied managed-reference value as JSON plus its
    /// concrete <see cref="Type"/>, so a paste reconstructs an independent instance (rather than aliasing the
    /// source object) and survives across different fields, inspectors, and target objects within the session.
    /// </summary>
    internal static class SerializeReferenceClipboard
    {
        private static bool _hasContent;
        private static string _json;
        private static Type _type;

        /// <summary>The concrete type of the copied value, or <see langword="null"/> when an empty reference was copied.</summary>
        public static Type Type => _type;

        /// <summary>
        /// Captures <paramref name="value"/> into the clipboard. Copying <see langword="null"/> is meaningful — a
        /// subsequent paste clears the target field.
        /// </summary>
        public static void Copy(object value)
        {
            _hasContent = true;
            _type = value?.GetType();
            _json = value is null ? null : JsonUtility.ToJson(value);
        }

        /// <summary>
        /// Returns <see langword="true"/> when the clipboard holds content that can be pasted into a field whose
        /// declared managed-reference type is <paramref name="fieldType"/> (an empty reference always pastes —
        /// it clears the field).
        /// </summary>
        public static bool CanPasteInto(Type fieldType)
        {
            if (!_hasContent) return false;
            if (_type is null) return true;
            return fieldType is null || fieldType.IsAssignableFrom(_type);
        }

        /// <summary>
        /// Reconstructs a fresh instance from the clipboard contents for assignment to a managed reference, or
        /// <see langword="null"/> when an empty reference was copied. The result is independent of the copied object.
        /// </summary>
        public static object CreateInstance()
        {
            if (!_hasContent || _type is null) return null;

            return string.IsNullOrEmpty(_json)
                ? SerializeReferenceHelpers.CreateInstance(_type)
                : JsonUtility.FromJson(_json, _type);
        }
    }
}
