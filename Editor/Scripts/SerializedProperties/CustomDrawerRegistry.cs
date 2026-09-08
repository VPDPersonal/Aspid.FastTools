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
        private static readonly FieldInfo _targetField =
            typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo _useForChildrenField =
            typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);

        private static List<(Type Target, bool UseForChildren)> _registrations;

        private static List<(Type Target, bool UseForChildren)> Registrations => _registrations ??= Collect();

        internal static bool HasDrawerFor(Type type)
        {
            if (type is null) return false;

            foreach (var (target, useForChildren) in Registrations)
            {
                if (target == type) return true;
                if (useForChildren && target.IsAssignableFrom(type)) return true;
            }

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

        private static List<(Type Target, bool UseForChildren)> Collect()
        {
            var result = new List<(Type, bool)>();
            if (_targetField is null) return result;

            foreach (var drawer in TypeCache.GetTypesWithAttribute<CustomPropertyDrawer>())
            {
                if (!typeof(PropertyDrawer).IsAssignableFrom(drawer)) continue;

                foreach (var registration in drawer.GetCustomAttributes<CustomPropertyDrawer>(inherit: true))
                {
                    if (_targetField.GetValue(registration) is not Type target) continue;
                    var useForChildren = _useForChildrenField?.GetValue(registration) is true;

                    result.Add((target, useForChildren));
                }
            }

            return result;
        }
    }
}
