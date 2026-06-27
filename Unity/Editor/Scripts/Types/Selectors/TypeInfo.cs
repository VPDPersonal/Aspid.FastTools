using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    internal sealed class TypeInfo
    {
        public readonly string Name;
        public readonly string Assembly;
        public readonly string FullName;
        public readonly string Namespace;
        public readonly string AssemblyQualifiedName;

        /// <summary>
        /// Leaf display name shown in the picker. Falls back to <see cref="Name"/> when no
        /// <see cref="TypeSelectorItemAttribute"/> renames the type.
        /// </summary>
        public readonly string DisplayName;

        /// <summary>
        /// Category segments the type is re-homed under (from a <c>/</c>-separated
        /// <see cref="TypeSelectorItemAttribute.DisplayPath"/>); empty when the type keeps its
        /// namespace location.
        /// </summary>
        public readonly IReadOnlyList<string> CategoryPath;

        /// <summary>
        /// Tooltip override from <see cref="TypeSelectorItemAttribute.Tooltip"/>; falls back to
        /// <see cref="FullName"/> when no override is supplied.
        /// </summary>
        public readonly string Tooltip;

        /// <summary>
        /// Ordering hint from <see cref="TypeSelectorItemAttribute.Order"/> (lower = higher).
        /// </summary>
        public readonly int Order;

        /// <summary>
        /// Raw icon identifier from <see cref="TypeSelectorItemAttribute.Icon"/>; <see langword="null"/>
        /// when no icon was requested.
        /// </summary>
        public readonly string Icon;

        /// <summary>
        /// Whether <see cref="CategoryPath"/> re-homes the type under explicit category nodes
        /// (a <c>/</c>-separated display path) instead of its namespace hierarchy.
        /// </summary>
        public bool HasCategoryPath => CategoryPath.Count > 0;

        public TypeInfo(Type type)
        {
            Name = FormatName(type);
            FullName = type.FullName;
            Assembly = type.Assembly.GetName().Name;
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            Namespace = string.IsNullOrEmpty(type.Namespace) ? TypeSelectorHelpers.GlobalNamespace : type.Namespace;

            var item = type.GetCustomAttribute<TypeSelectorItemAttribute>(inherit: false);

            DisplayName = Name;
            CategoryPath = Array.Empty<string>();
            Tooltip = type.FullName;
            Order = 0;
            Icon = null;

            if (item is null) return;

            Order = item.Order;
            Icon = string.IsNullOrWhiteSpace(item.Icon) ? null : item.Icon;

            if (!string.IsNullOrWhiteSpace(item.Tooltip))
                Tooltip = item.Tooltip;

            if (string.IsNullOrWhiteSpace(item.DisplayPath)) return;

            if (item.DisplayPath.Contains('/'))
            {
                var segments = item.DisplayPath
                    .Split('/')
                    .Select(segment => segment.Trim())
                    .Where(segment => segment.Length > 0)
                    .ToArray();

                if (segments.Length > 0)
                {
                    DisplayName = segments[^1];
                    CategoryPath = segments.Take(segments.Length - 1).ToArray();
                }
            }
            else
            {
                DisplayName = item.DisplayPath.Trim();
            }
        }

        /// <summary>
        /// Collects the type infos shown in the selector. <paramref name="additionalTypes"/> are appended
        /// verbatim (bypassing the base-type, name and <paramref name="allow"/> checks) so callers can inject
        /// entries — such as open generic definitions — that the standard <see cref="Type.IsAssignableFrom"/>
        /// scan cannot match.
        /// </summary>
        public static List<TypeInfo> GetAllTypeInfos(
            Type[] baseTypes,
            TypeAllow allow,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null)
        {
            var result = new List<TypeInfo>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"[TypeSelector] Skipped assembly '{assembly.GetName().Name}': {ex.Message}");
                    types = ex.Types.Where(t => t is not null).ToArray();
                }

                result.AddRange(types
                    .Where(t => baseTypes.All(baseType => baseType.IsAssignableFrom(t)) &&
                        !t.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                        !t.Name.Contains("<") &&
                        !t.Name.Contains(">") &&
                        (allow.HasFlag(TypeAllow.Abstract) || !t.IsAbstract) &&
                        (allow.HasFlag(TypeAllow.Interface) || !t.IsInterface) &&
                        (filter is null || filter(t)))
                    .Select(type => new TypeInfo(type)));
            }

            if (additionalTypes is not null)
            {
                var existing = new HashSet<string>(result.Select(info => info.AssemblyQualifiedName));

                result.AddRange(additionalTypes
                    .Where(type => type is not null && existing.Add(type.AssemblyQualifiedName))
                    .Select(type => new TypeInfo(type)));
            }

            return result;
        }

        /// <summary>
        /// Short display name for a type. Open generic definitions are rendered with angle-bracket
        /// parameters (<c>Modifier&lt;T&gt;</c>) instead of Unity's raw arity form (<c>Modifier`1</c>);
        /// generic arguments are formatted recursively so nested closed generics render fully.
        /// </summary>
        private static string FormatName(Type type)
        {
            if (!type.IsGenericType) return type.Name;

            var name = type.Name;
            var tick = name.IndexOf('`');
            var baseName = tick >= 0 ? name[..tick] : name;
            var arguments = string.Join(", ", type.GetGenericArguments().Select(FormatName));
            return $"{baseName}<{arguments}>";
        }
    }
}
