using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceMovedFromResolver
    {
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Cache unresolved and ambiguous identities too; domain reloads invalidate rename metadata.
        private static readonly Dictionary<string, Type> Cache = new(StringComparer.Ordinal);

        private static readonly char[] _nestedSeparators = { '/', '+' };

        // Unity keeps the rename metadata in non-public fields, so they are resolved once. When a Unity version
        // renames them, the resolver warns once and reports no match.
        private static readonly MovedFromFields _fields = LoadFields();

        // A rename is authoritative only when exactly one eligible type claims the stored identity.
        public static bool TryResolve(ManagedTypeName stored, out Type target)
        {
            target = null;
            if (string.IsNullOrEmpty(stored.Class)) return false;

            // A stored closed generic can only be claimed by an arity-stripped name collision — a guess, not a
            // rename — so it stays with the scored Smart Fix path.
            if (stored.Class.IndexOf('`') >= 0) return false;

            var key = SerializeReferenceHelpers.StoredTypeKey(stored);
            if (Cache.TryGetValue(key, out target)) return target is not null;

            target = ResolveUncached(stored);
            Cache[key] = target;
            return target is not null;
        }

        // Only Unity's own attribute is authoritative, since it is the one its serialization honors at load.
        // TypeCache is index-backed, so scanning just its carriers is cheap.
        private static Type ResolveUncached(ManagedTypeName stored)
        {
            var storedClass = NormalizeClassName(stored.Class);
            if (storedClass.Length == 0) return null;

            Type found = null;

            foreach (var candidate in TypeCache.GetTypesWithAttribute<MovedFromAttribute>())
            {
                if (!SerializeReferenceHelpers.IsAssignableManagedReference(candidate)) continue;
                if (!MatchesOldIdentity(candidate, stored, storedClass)) continue;

                if (found is not null && found != candidate) return null;
                found = candidate;
            }

            return found;
        }

        // Read non-public rename metadata reflectively; failures leave the identity unmatched.
        public static bool MatchesOldIdentity(Type candidate, ManagedTypeName stored, string storedClass)
        {
            if (_fields.Missing is not null) return false;

            try
            {
                foreach (var attribute in candidate.GetCustomAttributes(typeof(MovedFromAttribute), inherit: false))
                {
                    var data = _fields.Data.GetValue(attribute);
                    if (data is null) continue;

                    // A false "*HasChanged" flag means the old value equals the current one, matching how Unity's
                    // own updater resolves the old name.
                    var oldClass = NormalizeClassName(_fields.Class.Read(data, current: candidate.Name));
                    if (!string.Equals(oldClass, storedClass, StringComparison.Ordinal)) continue;

                    // An empty stored namespace is the global namespace: it must match too, or a stored global
                    // type would match any renamed class with the same old name.
                    var oldNamespace = _fields.Namespace.Read(data, current: candidate.Namespace);
                    if (!string.Equals(oldNamespace ?? string.Empty, stored.Namespace ?? string.Empty, StringComparison.Ordinal)) continue;

                    if (!string.IsNullOrEmpty(stored.Assembly))
                    {
                        var oldAssembly = _fields.Assembly.Read(data, current: candidate.Assembly.GetName().Name);
                        if (!string.Equals(oldAssembly ?? string.Empty, stored.Assembly, StringComparison.Ordinal)) continue;
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                // The data struct is not public API, so a reflection failure just means "no match".
            }

            return false;
        }

        public static string NormalizeClassName(string className)
        {
            if (string.IsNullOrEmpty(className)) return string.Empty;

            var bracket = className.IndexOf('[');
            if (bracket >= 0) className = className[..bracket];

            var tick = className.IndexOf('`');
            if (tick >= 0) className = className[..tick];

            var slash = className.LastIndexOfAny(_nestedSeparators);
            if (slash >= 0) className = className[(slash + 1)..];

            return className.Trim();
        }

        private static MovedFromFields LoadFields()
        {
            var fields = new MovedFromFields(typeof(MovedFromAttribute));
            if (fields.Missing is null) return fields;

            Debug.LogWarning(
                $"[Aspid.FastTools] Unity's MovedFromAttribute has no '{fields.Missing}' field in this Editor version, " +
                "so [MovedFrom] renames are not recognized: such types are reported as missing instead of pending " +
                "migrations, and Smart Fix does not rank them first. Update Aspid.FastTools or report the issue.");

            return fields;
        }

        // Reflection handles to the attribute's data; Missing names the first field Unity no longer has.
        internal sealed class MovedFromFields
        {
            public readonly FieldInfo Data;
            public readonly MovedSlot Class;
            public readonly MovedSlot Namespace;
            public readonly MovedSlot Assembly;
            public readonly string Missing;

            public MovedFromFields(Type attributeType)
            {
                Data = attributeType.GetField("data", InstanceFlags);

                var dataType = Data?.FieldType;
                Class = new MovedSlot(dataType, valueName: "className", changedName: "classHasChanged");
                Namespace = new MovedSlot(dataType, valueName: "nameSpace", changedName: "nameSpaceHasChanged");
                Assembly = new MovedSlot(dataType, valueName: "assembly", changedName: "assemblyHasChanged");

                Missing = Data is null ? "data" : Class.Missing ?? Namespace.Missing ?? Assembly.Missing;
            }
        }

        // One old-identity part (class, namespace or assembly) and the flag that says it changed.
        internal readonly struct MovedSlot
        {
            private readonly FieldInfo _value;
            private readonly FieldInfo _changed;

            public readonly string Missing;

            public MovedSlot(Type dataType, string valueName, string changedName)
            {
                _value = dataType?.GetField(valueName, InstanceFlags);
                _changed = dataType?.GetField(changedName, InstanceFlags);

                Missing = dataType is null ? null : _value is null ? valueName : _changed is null ? changedName : null;
            }

            public string Read(object data, string current) =>
                _changed.GetValue(data) is true ? _value.GetValue(data) as string : current;
        }
    }
}
