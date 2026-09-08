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
        internal readonly string Name;
        internal readonly string Assembly;
        internal readonly string Namespace;
        internal readonly string AssemblyQualifiedName;

        internal readonly string Tooltip;

        internal readonly string Icon;

        internal readonly string CustomName;

        internal readonly string[] GroupPath;

        internal string Label => CustomName ?? Name;

        internal TypeInfo(Type type)
        {
            Name = TypeUtility.FormatGenericName(type);
            Assembly = type.Assembly.GetName().Name;
            AssemblyQualifiedName = type.AssemblyQualifiedName;
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
        // picker explicitly includes them.
        internal static List<TypeInfo> GetAllTypeInfos(
            Type[] baseTypes,
            TypeAllow allow,
            Func<Type, bool> filter = null,
            IEnumerable<Type> additionalTypes = null,
            bool includeHidden = false)
        {
            var result = new List<TypeInfo>();

            result.AddRange(TypeUtility.DomainTypes
                .Where(t => baseTypes.All(baseType => baseType.IsAssignableFrom(t)) &&
                    !t.IsDefined(typeof(CompilerGeneratedAttribute), false) &&
                    !t.Name.Contains("<") &&
                    !t.Name.Contains(">") &&
                    !(t.IsAbstract && t.IsSealed) &&
                    (allow.HasFlag(TypeAllow.Abstract) || t.IsInterface || !t.IsAbstract) &&
                    (allow.HasFlag(TypeAllow.Interface) || !t.IsInterface) &&
                    (includeHidden || !TypeSelectorHelpers.IsHiddenFromPicker(t)) &&
                    (filter is null || filter(t)))
                .Select(type => new TypeInfo(type)));

            if (additionalTypes is not null)
            {
                var existing = new HashSet<string>(result.Select(info => info.AssemblyQualifiedName));

                result.AddRange(additionalTypes
                    .Where(type => type is not null &&
                        (includeHidden || !TypeSelectorHelpers.IsHiddenFromPicker(type)) &&
                        existing.Add(type.AssemblyQualifiedName))
                    .Select(type => new TypeInfo(type)));
            }

            return result;
        }
    }
}
