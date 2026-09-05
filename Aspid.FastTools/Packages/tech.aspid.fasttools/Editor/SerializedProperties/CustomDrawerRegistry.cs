using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    /// <summary>
    /// Answers whether the project registers a <see cref="PropertyDrawer"/> for a given field type or property
    /// attribute — what a custom field has to know before drawing a property itself instead of handing it to a
    /// <c>PropertyField</c>.
    /// </summary>
    /// <remarks>
    /// Unity resolves drawers through internal machinery with no public entry point, so the answer is rebuilt from
    /// the source that machinery reads: every <see cref="CustomPropertyDrawer"/> in the domain and the target type
    /// it names. <see cref="DecoratorDrawer"/> registrations are ignored — a decorator adds chrome around a
    /// property rather than a body for it, so it never decides who draws.
    /// </remarks>
    internal static class CustomDrawerRegistry
    {
        // CustomPropertyDrawer exposes neither field publicly; both have carried these names since the attribute
        // was introduced. A rename in a future Unity degrades this to "no drawer registered", which only costs the
        // deferral — never a wrong drawing.
        private static readonly FieldInfo _targetField =
            typeof(CustomPropertyDrawer).GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo _useForChildrenField =
            typeof(CustomPropertyDrawer).GetField("m_UseForChildren", BindingFlags.Instance | BindingFlags.NonPublic);

        // Built once per domain: the sweep is over drawer types only (a few dozen), and static state is cleared on
        // every reload, so a newly added drawer is picked up without an explicit invalidation.
        private static List<(Type Target, bool UseForChildren)> _registrations;

        private static List<(Type Target, bool UseForChildren)> Registrations => _registrations ??= Collect();

        /// <summary>
        /// Returns <see langword="true"/> when a <see cref="PropertyDrawer"/> is registered for
        /// <paramref name="type"/> itself, or for one of its base types / interfaces with
        /// <c>useForChildren</c> set.
        /// </summary>
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

        /// <summary>
        /// Returns <see langword="true"/> when <paramref name="field"/> declares a <see cref="PropertyAttribute"/>
        /// that has a <see cref="PropertyDrawer"/> of its own — the attribute the drawer exists to serve, which
        /// only runs through Unity's own property path.
        /// </summary>
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
