using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A managed-reference type's identity as Unity stores it in YAML. A reference whose type went missing is
    // repaired by rewriting that line directly, since the serialization API cannot reassign a missing type.
    internal readonly struct ManagedTypeName
    {
        private static readonly char[] _yamlReservedChars = { ',', '[', ']', '{', '}' };

        public readonly string Class;
        public readonly string Assembly;
        public readonly string Namespace;

        // Computed rather than stored, so a default instance — which never runs the constructor — still reports
        // empty instead of a stale false.
        public bool IsEmpty => string.IsNullOrWhiteSpace(Assembly)
            && string.IsNullOrWhiteSpace(Namespace)
            && string.IsNullOrWhiteSpace(Class);

        // For tooltips that need the assembly too.
        public string FullName => IsEmpty
            ? string.Empty
            : string.IsNullOrWhiteSpace(Assembly) ? DisplayName : $"{DisplayName}, {Assembly}";

        // The single source of the missing-type caption used by the repair dialog, the audit list and the graph
        // header, so a nested or generic class-name display fix lands in one place.
        public string DisplayName => IsEmpty
            ? string.Empty
            : string.IsNullOrWhiteSpace(Namespace) ? Class : $"{Namespace}.{Class}";

        public ManagedTypeName(string assembly, string @namespace, string className)
        {
            Class = className ?? string.Empty;
            Assembly = assembly ?? string.Empty;
            Namespace = @namespace ?? string.Empty;
        }

        // Includes the "Name`N[[arg, asm],…]" shape Unity uses for closed generics.
        public static ManagedTypeName FromType(Type type)
        {
            if (type is null) return default;
            var root = type.IsGenericType ? type.GetGenericTypeDefinition() : type;

            // Unity stores a nested type's class identity with its declaring types joined by '/' (Outer/Inner), but
            // Type.Name is only the leaf — mirror of the read side's '/'->'+' mapping in
            // SerializeReferenceHelpers.StoredTypeResolves. Without the prefix a repaired nested reference re-breaks.
            return new ManagedTypeName(
                assembly: root.Assembly.GetName().Name,
                @namespace: root.Namespace,
                className: NestedPrefix(type) + BuildClassName(type));
        }

        // The "Outer/" (or "Outer/Middle/") prefix Unity prepends to a nested type's class identity; empty for a
        // top-level type. Walks the declaring-type chain from the outermost inward.
        private static string NestedPrefix(Type type)
        {
            if (type.DeclaringType is null)
                return string.Empty;

            var prefix = string.Empty;
            for (var declaring = type.DeclaringType; declaring is not null; declaring = declaring.DeclaringType)
            {
                prefix = declaring.Name + "/" + prefix;
            }

            return prefix;
        }

        private static string BuildClassName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var definition = type.GetGenericTypeDefinition();
            var arguments = type.GetGenericArguments().Select(BuildGenericArgumentName);

            return $"{definition.Name}[[{string.Join("],[", arguments)}]]";
        }

        private static string BuildGenericArgumentName(Type type) =>
            $"{BuildFullClassName(type)}, {type.Assembly.GetName().Name}";

        private static string BuildFullClassName(Type type)
        {
            if (!type.IsGenericType)
                return type.FullName;

            var definition = type.GetGenericTypeDefinition();
            var prefix = string.IsNullOrEmpty(definition.Namespace) ? string.Empty : $"{definition.Namespace}.";

            return $"{prefix}{BuildClassName(type)}";
        }

        public string ToYamlType() =>
            $"{{class: {EscapeInline(Class)}, ns: {EscapeInline(Namespace)}, asm: {EscapeInline(Assembly)}}}";

        // A flow-scalar containing any of , [ ] { } would break the inline mapping, so single-quote it
        // (doubling embedded quotes) exactly as Unity does for generic class names like Foo`1[[…]].
        private static string EscapeInline(string value)
        {
            if (string.IsNullOrEmpty(value) || value.IndexOfAny(_yamlReservedChars) < 0)
                return value ?? string.Empty;

            return $"'{value.Replace("'", "''")}'";
        }
    }
}
