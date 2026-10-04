using System;
using System.Linq;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal static class TypeMissingRepair
    {
        // The predicate is part of the key by delegate equality: pass a static method, not a fresh closure per call.
        private static readonly Dictionary<(string name, string constraints, TypeAllow allow, bool runtime, Func<Type, bool> predicate), Type> _suggestions = new();
        private static IReadOnlyList<Type> _domainTypes;

        internal static bool IsMissing(SerializedProperty property) =>
            !property.hasMultipleDifferentValues && !string.IsNullOrWhiteSpace(property.stringValue) &&
            TypeUtility.GetTypeOrNull(assemblyQualifiedName: property.stringValue) is null;

        // A SerializableMonoScript wrapper is missing only when neither its script nor its stored name gives a type.
        internal static bool IsMissingMonoScript(SerializedProperty wrapperProperty)
        {
            var nameProperty = SerializableTypeUtility.GetBackingProperty(wrapperProperty);
            if (nameProperty is null || wrapperProperty.hasMultipleDifferentValues || nameProperty.hasMultipleDifferentValues)
                return false;

            return !string.IsNullOrWhiteSpace(nameProperty.stringValue) &&
                   SerializableMonoScriptUtility.GetCurrentType(wrapperProperty, out _) is null;
        }

        internal static string GetDetail(string storedName) =>
            $"Missing type: {storedName}.\nClick Fix to choose an existing type and replace the stored type name.";

        internal static Type GetSuggestion(string storedName, Type[] types, TypeAllow allow, bool excludeEditorOnly,
            Func<Type, bool> predicate = null)
        {
            if (string.IsNullOrWhiteSpace(storedName)) return null;

            // Constructed generics and element types need more than a name match to preserve their shape.
            var fullName = storedName.Split(',')[0].Trim();
            if (fullName.IndexOfAny(new[] { '[', ']', '`', '*', '&', '\\' }) >= 0) return null;

            var domainTypes = TypeUtility.DomainTypes;
            if (!ReferenceEquals(_domainTypes, domainTypes))
            {
                _suggestions.Clear();
                _domainTypes = domainTypes;
            }

            types ??= new[] { typeof(object) };
            var constraints = string.Join("|", types.Select(type => type.AssemblyQualifiedName));
            var key = (storedName, constraints, allow, excludeEditorOnly, predicate);
            if (_suggestions.TryGetValue(key: key, value: out var cached)) return cached;

            var name = fullName[(Math.Max(fullName.LastIndexOf('.'), fullName.LastIndexOf('+')) + 1)..];
            var candidates = TypeInfo.GetAllTypeInfos(baseTypes: types, allow: allow,
                filter: type => !type.ContainsGenericParameters && type.Name == name && (predicate is null || predicate(type)),
                excludeEditorOnly: excludeEditorOnly);
            var suggestion = candidates.Count == 1
                ? TypeUtility.GetTypeOrNull(assemblyQualifiedName: candidates[0].AssemblyQualifiedName)
                : null;

            _suggestions[key] = suggestion;
            return suggestion;
        }
    }
}
