using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal sealed class TypeInfo
    {
        // A type's picker data changes only with a recompile, which reloads the domain and resets these caches.
        private static readonly Dictionary<Type, TypeInfo> _cache = new();
        private static readonly Dictionary<Assembly, string> _assemblyNames = new();

        // The types that pass every check of GetAllTypeInfos except the predicate, per key. They depend on the loaded
        // types alone, so they are reused until a new domain sweep replaces DomainTypes (it does so after any assembly
        // load; a recompile reloads the domain and resets the cache). Like the other caches here, it is read from the
        // main thread only.
        private static IReadOnlyList<Type> _candidatesDomainTypes;
        private static readonly Dictionary<CandidatesKey, Type[]> _candidates = new();

        internal readonly string Name;
        internal readonly string Assembly;
        internal readonly string Namespace;
        internal readonly string AssemblyQualifiedName;

        // Outer.Inner for the types enclosing a nested type, null for a top-level one: it tells apart nested types
        // of the same name.
        internal readonly string DeclaringName;

        // Namespace.Outer.Name without the assembly part, which search matches instead of the assembly-qualified name.
        internal readonly string QualifiedName;

        internal readonly string Tooltip;

        internal readonly string Icon;

        internal readonly string CustomName;

        internal readonly string[] GroupPath;

        internal string Label => CustomName ?? Name;

        internal static TypeInfo Get(Type type)
        {
            if (_cache.TryGetValue(type, out var info)) return info;

            info = new TypeInfo(type);
            _cache[type] = info;

            return info;
        }

        private TypeInfo(Type type)
        {
            Name = TypeUtility.FormatGenericName(type);
            Assembly = GetAssemblyName(type.Assembly);
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            DeclaringName = TypeUtility.FormatDeclaringName(type);
            QualifiedName = TypeUtility.FormatQualifiedName(type);
            Namespace = string.IsNullOrEmpty(type.Namespace) ? TypeSelectorHelpers.GlobalNamespace : type.Namespace;

            var item = type.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);

            // The text of TypeSelectorHelpers.GetTypeSelectorTooltip, built from the cached parts.
            Tooltip = $"{QualifiedName}, {Assembly}";
            Icon = null;
            CustomName = TypeSelectorHelpers.GetCustomDisplayName(type);
            GroupPath = null;

            if (item is null) return;

            Icon = string.IsNullOrWhiteSpace(item.Icon) ? null : item.Icon;
            GroupPath = ParseGroupPath(item.Group);

            if (!string.IsNullOrWhiteSpace(item.Tooltip))
                Tooltip = item.Tooltip;
        }

        private static string GetAssemblyName(Assembly assembly)
        {
            if (_assemblyNames.TryGetValue(assembly, out var name)) return name;

            name = assembly.GetName().Name;
            _assemblyNames[assembly] = name;

            return name;
        }

        // "Combat / Melee //" → ["Combat", "Melee"]; null when nothing survives, so a blank-only Group degrades to
        // the namespace placement. Sentinel segments are dropped too — the picker keys off DisplayName == "<None>",
        // so a group node named after a sentinel would impersonate it.
        private static string[] ParseGroupPath(string group)
        {
            if (string.IsNullOrWhiteSpace(group)) return null;

            var segments = group.Split('/')
                .Select(segment => segment.Trim())
                .Where(segment => segment.Length > 0 &&
                    segment != TypeSelectorHelpers.NoneOption &&
                    segment != TypeSelectorHelpers.GlobalNamespace)
                .ToArray();

            return segments.Length > 0 ? segments : null;
        }

        // Additional candidates bypass ordinary constraints, but hidden types remain excluded unless the repair
        // picker explicitly includes them. Types from editor-only assemblies are left out of both when the value is
        // stored in a runtime object, since a player cannot resolve them. Everything but the predicate is decided once
        // per key by GetCandidates, so a picker with a predicate, which cannot be cached between openings because it
        // may differ per field, runs only the predicate over the loaded types, in the order of the domain sweep.
        internal static List<TypeInfo> GetAllTypeInfos(
            Type[] baseTypes,
            TypeAllow allow,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null,
            bool includeHidden = false,
            bool excludeEditorOnly = false)
        {
            var result = new List<TypeInfo>();

            foreach (var type in GetCandidates(baseTypes, allow, includeHidden, excludeEditorOnly))
                if (filter is null || filter(type)) result.Add(Get(type));

            if (additionalTypes is not null)
            {
                var existing = new HashSet<string>(result.Select(info => info.AssemblyQualifiedName));

                result.AddRange(additionalTypes
                    .Where(type => type is not null &&
                        (!excludeEditorOnly || !TypeUtility.IsEditorOnlyAssembly(type.Assembly)) &&
                        (includeHidden || !TypeSelectorHelpers.IsHiddenFromPicker(type)) &&
                        existing.Add(type.AssemblyQualifiedName))
                    .Select(Get));
            }

            return result;
        }

        // The loaded types a picker with this key may offer before its predicate is asked. The base type check runs
        // first, because it rejects most of the domain for a narrow base, and the cheap modifier checks run before
        // the attribute lookups, which matters for an unconstrained picker scanning the whole domain. The result is
        // the array the cache holds, so the caller must not change it.
        internal static Type[] GetCandidates(Type[] baseTypes, TypeAllow allow, bool includeHidden, bool excludeEditorOnly)
        {
            var domainTypes = TypeUtility.DomainTypes;

            if (!ReferenceEquals(_candidatesDomainTypes, domainTypes))
            {
                _candidates.Clear();
                _candidatesDomainTypes = domainTypes;
            }

            var key = new CandidatesKey(baseTypes, allow, includeHidden, excludeEditorOnly);
            if (_candidates.TryGetValue(key, out var cached)) return cached;

            var allowAbstract = allow.HasFlag(TypeAllow.Abstract);
            var allowInterface = allow.HasFlag(TypeAllow.Interface);
            var candidates = new List<Type>();

            foreach (var type in domainTypes)
            {
                if (IsAssignableToAll(baseTypes, type) &&
                    !(type.IsAbstract && type.IsSealed) &&
                    (allowAbstract || type.IsInterface || !type.IsAbstract) &&
                    (allowInterface || !type.IsInterface) &&
                    (!excludeEditorOnly || !TypeUtility.IsEditorOnlyAssembly(type.Assembly)) &&
                    !TypeUtility.IsCompilerGenerated(type) &&
                    (includeHidden || !TypeSelectorHelpers.IsHiddenFromPicker(type)))
                {
                    candidates.Add(type);
                }
            }

            var result = candidates.ToArray();
            _candidates[key.WithOwnBaseTypes()] = result;

            return result;
        }

        private static bool IsAssignableToAll(Type[] baseTypes, Type type)
        {
            foreach (var baseType in baseTypes)
                if (!baseType.IsAssignableFrom(type)) return false;

            return true;
        }

        // The base types compare in the order given and by type, so two pickers with the same constraints share a list.
        private readonly struct CandidatesKey : IEquatable<CandidatesKey>
        {
            private readonly Type[] _baseTypes;
            private readonly TypeAllow _allow;
            private readonly bool _includeHidden;
            private readonly bool _excludeEditorOnly;

            internal CandidatesKey(Type[] baseTypes, TypeAllow allow, bool includeHidden, bool excludeEditorOnly)
            {
                _baseTypes = baseTypes;
                _allow = allow;
                _includeHidden = includeHidden;
                _excludeEditorOnly = excludeEditorOnly;
            }

            // The array of a lookup belongs to the caller, so a stored key keeps a copy that the caller cannot change.
            internal CandidatesKey WithOwnBaseTypes() =>
                new((Type[])_baseTypes.Clone(), _allow, _includeHidden, _excludeEditorOnly);

            public bool Equals(CandidatesKey other)
            {
                if (_allow != other._allow ||
                    _includeHidden != other._includeHidden ||
                    _excludeEditorOnly != other._excludeEditorOnly ||
                    _baseTypes.Length != other._baseTypes.Length) return false;

                for (var i = 0; i < _baseTypes.Length; i++)
                    if (_baseTypes[i] != other._baseTypes[i]) return false;

                return true;
            }

            public override bool Equals(object obj) =>
                obj is CandidatesKey other && Equals(other);

            public override int GetHashCode()
            {
                var hash = new HashCode();

                hash.Add(_allow);
                hash.Add(_includeHidden);
                hash.Add(_excludeEditorOnly);

                foreach (var baseType in _baseTypes)
                    hash.Add(baseType);

                return hash.ToHashCode();
            }
        }
    }
}
