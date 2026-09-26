using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal sealed class TypeInfo
    {
        // A type's picker data changes only with a recompile, which reloads the domain and resets these caches.
        private static readonly Dictionary<Type, TypeInfo> _cache = new();
        private static readonly Dictionary<System.Reflection.Assembly, string> _assemblyNames = new();

        internal readonly string Name;
        internal readonly string Assembly;
        internal readonly string Namespace;
        internal readonly string AssemblyQualifiedName;

        // Namespace.Name without the assembly part, which search matches instead of the assembly-qualified name.
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
            QualifiedName = string.IsNullOrEmpty(type.Namespace) ? Name : $"{type.Namespace}.{Name}";
            Namespace = string.IsNullOrEmpty(type.Namespace) ? TypeSelectorHelpers.GlobalNamespace : type.Namespace;

            var item = type.GetCustomAttribute<TypeSelectorDisplayAttribute>(inherit: false);

            Tooltip = type.FullName;
            Icon = null;
            CustomName = TypeSelectorHelpers.GetCustomDisplayName(type);
            GroupPath = null;

            if (item is null) return;

            Icon = string.IsNullOrWhiteSpace(item.Icon) ? null : item.Icon;
            GroupPath = ParseGroupPath(item.Group);

            if (!string.IsNullOrWhiteSpace(item.Tooltip))
                Tooltip = item.Tooltip;
        }

        private static string GetAssemblyName(System.Reflection.Assembly assembly)
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
        // stored in a runtime object, since a player cannot resolve them. The cheap name and modifier checks run
        // before the attribute lookups, which matters for an unconstrained picker scanning the whole domain.
        internal static List<TypeInfo> GetAllTypeInfos(
            Type[] baseTypes,
            TypeAllow allow,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null,
            bool includeHidden = false,
            bool excludeEditorOnly = false)
        {
            var result = new List<TypeInfo>();

            result.AddRange(TypeUtility.DomainTypes
                .Where(t => !t.Name.Contains("<") &&
                    !t.Name.Contains(">") &&
                    !(t.IsAbstract && t.IsSealed) &&
                    (allow.HasFlag(TypeAllow.Abstract) || t.IsInterface || !t.IsAbstract) &&
                    (allow.HasFlag(TypeAllow.Interface) || !t.IsInterface) &&
                    baseTypes.All(baseType => baseType.IsAssignableFrom(t)) &&
                    (!excludeEditorOnly || !TypeUtility.IsEditorOnlyAssembly(t.Assembly)) &&
                    !t.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                    (includeHidden || !TypeSelectorHelpers.IsHiddenFromPicker(t)) &&
                    (filter is null || filter(t)))
                .Select(Get));

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
    }
}
