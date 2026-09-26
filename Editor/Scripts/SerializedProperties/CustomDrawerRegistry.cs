using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class CustomDrawerRegistry
    {
        // Unity exposes no public drawer registry; missing internal attribute fields disable this lookup.
        internal static readonly FieldInfo TargetField =
            typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static readonly FieldInfo UseForChildrenField =
            typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);

        private static List<(Type Target, bool UseForChildren)> _registrations;

        private static readonly Dictionary<(Type, bool), bool> Cache = new();

        private static List<(Type Target, bool UseForChildren)> Registrations => _registrations ??= Collect();

        // Mirrors Unity's lookup: the type and its base classes, then its interfaces, each also by generic definition.
        // An ancestor's drawer applies with useForChildren or, as Unity treats managed references, without it.
        internal static bool HasDrawerFor(Type type, bool isManagedReference = false)
        {
            if (type is null) return false;

            // Drawers only change with a domain reload, which also clears this cache.
            if (Cache.TryGetValue((type, isManagedReference), out var hasDrawer)) return hasDrawer;
            return Cache[(type, isManagedReference)] = Lookup(type, isManagedReference);
        }

        private static bool Lookup(Type type, bool isManagedReference)
        {
            for (var current = type; current is not null; current = current.BaseType)
                if (Matches(current, requested: current == type, isManagedReference))
                    return true;

            // An interface itself was checked above, so every interface listed here is an ancestor.
            foreach (var @interface in type.GetInterfaces())
                if (Matches(@interface, requested: false, isManagedReference))
                    return true;

            return false;
        }

        internal static bool DeclaresDrawnAttribute(FieldInfo field)
        {
            if (field is null) return false;

            foreach (var attribute in field.GetCustomAttributes<PropertyAttribute>(inherit: true))
                if (HasDrawerFor(attribute.GetType()))
                    return true;

            return false;
        }

        private static bool Matches(Type type, bool requested, bool isManagedReference)
        {
            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : null;

            foreach (var (target, useForChildren) in Registrations)
            {
                if (target != type && target != definition) continue;
                if (requested || useForChildren || isManagedReference) return true;
            }

            return false;
        }

        private static List<(Type Target, bool UseForChildren)> Collect()
        {
            var result = new List<(Type, bool)>();
            if (TargetField is null) return result;

            foreach (var drawer in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
            {
                if (!typeof(PropertyDrawer).IsAssignableFrom(drawer)) continue;

                foreach (var registration in drawer.GetCustomAttributes<CustomPropertyDrawer>(inherit: true))
                {
                    if (TargetField.GetValue(registration) is not Type target) continue;
                    var useForChildren = UseForChildrenField?.GetValue(registration) is true;

                    result.Add((target, useForChildren));
                }
            }

            return result;
        }
    }
}
