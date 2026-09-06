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

        // Tooltip override from TypeSelectorDisplayAttribute.Tooltip; falls back to
        // FullName when no override is supplied.
        internal readonly string Tooltip;

        // Raw icon identifier from TypeSelectorDisplayAttribute.Icon; null
        // when no icon was requested.
        internal readonly string Icon;

        // Normalized display-name override from TypeSelectorDisplayAttribute.Name;
        // null when the type keeps its real name.
        internal readonly string CustomName;

        // Normalized TypeSelectorDisplayAttribute.Group path segments (split on /,
        // trimmed, empty segments dropped); null when the type stays under its namespace.
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

        // The types shown in the selector. Additional types are appended verbatim, bypassing the base-type, name
        // and allow checks, so a caller can inject entries the assignability scan cannot match.
        //
        // A hidden type is dropped from both paths, since opting out of the picker must hold however a type reaches
        // it. A repair picker passes includeHidden: its job is to re-point data that already holds such a type, and
        // filtering it out would leave the reference unfixable from the editor at all.
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
                    // A static class is abstract+sealed to the CLR yet never a type a field can meaningfully name.
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
