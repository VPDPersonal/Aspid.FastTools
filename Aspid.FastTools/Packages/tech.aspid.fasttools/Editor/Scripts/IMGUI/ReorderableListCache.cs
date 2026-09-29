using System;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    // ReorderableList holds per-list UI state (selection, drag), so a list drawn from IMGUI must survive across
    // OnGUI calls, keyed by the property it draws.
    internal static class ReorderableListCache
    {
        private static readonly Dictionary<string, ReorderableList> Lists = new();

        // The returned list is re-pointed at listProperty: the property instance is rebuilt on every OnGUI, and a
        // cached list must never touch a disposed one.
        internal static TList GetOrCreate<TList>(SerializedProperty listProperty, Func<TList> create)
            where TList : ReorderableList
        {
            var serializedObject = listProperty.serializedObject;

            // The SerializedObject is part of the key: an Inspector plus a locked Inspector hold two distinct ones for
            // the same (target, path), and a shared key would rebuild the list on every alternating repaint.
            var key = $"{RuntimeHelpers.GetHashCode(serializedObject)}/" +
                      $"{RuntimeHelpers.GetHashCode(serializedObject.targetObject)}/{listProperty.propertyPath}";

            // A cached list bound to a stale SerializedObject (e.g. after a domain reload) must be rebuilt, not reused.
            if (Lists.TryGetValue(key, out var cached) && cached is TList typed &&
                cached.serializedProperty.serializedObject == serializedObject)
            {
                typed.serializedProperty = listProperty;
                return typed;
            }

            // Entries pin their SerializedObject, so a closed editor's entry would live until the next domain reload.
            // Swept on cache misses only, which are already the slow path.
            EvictDeadEntries();

            var list = create();
            Lists[key] = list;
            return list;
        }

        private static void EvictDeadEntries()
        {
            List<string> dead = null;

            foreach (var pair in Lists)
            {
                bool alive;
                try
                {
                    alive = pair.Value.serializedProperty.serializedObject.targetObject != null;
                }
                catch (Exception)
                {
                    // A disposed SerializedObject throws on access — the entry is dead either way.
                    alive = false;
                }

                if (!alive) (dead ??= new List<string>()).Add(pair.Key);
            }

            if (dead is null) return;
            foreach (var key in dead) Lists.Remove(key);
        }
    }
}
