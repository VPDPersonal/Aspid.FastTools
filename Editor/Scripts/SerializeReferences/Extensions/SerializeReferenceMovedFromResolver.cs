using System;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Resolves a stored, no longer loadable type identity to the type declaring it as its old name via [MovedFrom].
    // Unity migrates such references in memory at load, but the YAML keeps the old name until the asset is re-saved,
    // so a YAML-level scan keeps seeing the stale identity. This is what lets those entries read as a pending
    // migration instead of a breakage, and backs the bulk "Migrate all" that bakes the rename into the files.
    internal static class SerializeReferenceMovedFromResolver
    {
        // Stored-type key -> the single authoritative target; null means no claimant or an ambiguous pair. Negative
        // results are cached too, since the breakage paths probe every unresolved entry. [MovedFrom] declarations
        // only change with a recompile, which resets this with the domain, so nothing invalidates it.
        private static readonly Dictionary<string, Type> Cache = new(StringComparer.Ordinal);

        private static readonly char[] NestedSeparators = { '/', '+' };

        // True when exactly one eligible type declares a [MovedFrom] matching the stored identity. Two claimants
        // make the rename non-authoritative, so the resolver refuses to pick between them.
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

        // Matches the candidate's recorded old identity against the stored class and, when declared, namespace and
        // assembly. storedClass must already be normalized. The attribute's data is not public API, so it is read
        // reflectively and any failure counts as "no match".
        public static bool MatchesOldIdentity(Type candidate, ManagedTypeName stored, string storedClass)
        {
            try
            {
                foreach (var attribute in candidate.GetCustomAttributes(inherit: false))
                {
                    var attributeType = attribute.GetType();
                    if (attributeType.Name != "MovedFromAttribute") continue;

                    var data = attributeType
                        .GetField("data", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        ?.GetValue(attribute);
                    if (data is null) continue;

                    var dataType = data.GetType();

                    // A false "*HasChanged" flag means the old value equals the current one, matching how Unity's
                    // own updater resolves the old name.
                    var oldClass = NormalizeClassName(ReadMovedSlot(dataType, data, "className", "classHasChanged", candidate.Name));
                    if (!string.Equals(oldClass, storedClass, StringComparison.Ordinal)) continue;

                    if (!string.IsNullOrEmpty(stored.Namespace))
                    {
                        var oldNamespace = ReadMovedSlot(dataType, data, "nameSpace", "nameSpaceHasChanged", candidate.Namespace);
                        if (!string.Equals(oldNamespace ?? string.Empty, stored.Namespace, StringComparison.Ordinal)) continue;
                    }

                    if (!string.IsNullOrEmpty(stored.Assembly))
                    {
                        var oldAssembly = ReadMovedSlot(dataType, data, "assembly", "assemblyHasChanged", candidate.Assembly.GetName().Name);
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

        // Strips generic-arity decoration and nesting so both sides compare on the bare simple name:
        // "Modifier`1[[System.Single, mscorlib]]" and "Outer/Modifier" both reduce to "Modifier".
        public static string NormalizeClassName(string className)
        {
            if (string.IsNullOrEmpty(className)) return string.Empty;

            var bracket = className.IndexOf('[');
            if (bracket >= 0) className = className[..bracket];

            var tick = className.IndexOf('`');
            if (tick >= 0) className = className[..tick];

            var slash = className.LastIndexOfAny(NestedSeparators);
            if (slash >= 0) className = className[(slash + 1)..];

            return className.Trim();
        }

        // Returns the recorded old value when the slot's "*HasChanged" flag is set, and the current one otherwise.
        private static string ReadMovedSlot(Type dataType, object data, string valueField, string changedField, string current)
        {
            var changed = dataType.GetField(changedField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var hasChanged = changed?.GetValue(data) is true;
            if (!hasChanged) return current;

            var value = dataType.GetField(valueField, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return value?.GetValue(data) as string;
        }
    }
}
