using System;
using UnityEditor;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal static class SerializeReferenceLinker
    {
        public readonly struct LinkCandidate
        {
            public readonly long Rid;
            public readonly Type Type;
            public readonly string Path;

            public LinkCandidate(long rid, Type type, string path)
            {
                Rid = rid;
                Type = type;
                Path = path;
            }
        }

        // Every other managed reference in the object assignable to this field, minus this property and its
        // ancestors and descendants, which would form a self-cycle.
        public static List<LinkCandidate> CollectLinkCandidates(SerializedProperty property)
        {
            var result = new List<LinkCandidate>();
            if (property is null) return result;

            var fieldType = SerializeReferenceHelpers.GetFieldType(property);
            var selfPath = property.propertyPath;
            var seen = new HashSet<long>();

            // The exclusion works by identity, not path: an aliased sibling is an ancestor instance under another
            // name, and its path shares no prefix with this property's, so the checks below cannot see the cycle.
            var ancestorRids = CollectAncestorRids(property);

            using var iterator = property.serializedObject.GetIterator();
            if (!iterator.Next(enterChildren: true)) return result;

            // A cyclic graph would loop the iterator forever, so never re-enter an instance already seen.
            var visitedChildren = new HashSet<long>();
            bool enterChildren;

            do
            {
                enterChildren = true;
                if (iterator.propertyType != SerializedPropertyType.ManagedReference) continue;

                var rid = iterator.managedReferenceId;
                if (rid >= 0 && !visitedChildren.Add(rid)) enterChildren = false;

                var path = iterator.propertyPath;
                if (path == selfPath) continue;
                if (IsDescendant(path, selfPath) || IsDescendant(selfPath, path)) continue;
                if (ancestorRids.Contains(rid)) continue;

                var value = iterator.managedReferenceValue;
                if (value is null) continue;

                var type = value.GetType();
                if (fieldType != null && !fieldType.IsAssignableFrom(type)) continue;

                if (!seen.Add(rid)) continue;

                result.Add(new LinkCandidate(rid, type, path));
            }
            while (iterator.Next(enterChildren));

            return result;
        }

        private static HashSet<long> CollectAncestorRids(SerializedProperty property)
        {
            var rids = new HashSet<long>();
            var serializedObject = property.serializedObject;
            var path = property.propertyPath;

            for (var dot = path.LastIndexOf('.'); dot > 0; dot = path.LastIndexOf('.'))
            {
                path = path[..dot];

                using var ancestor = serializedObject.FindProperty(path);
                if (ancestor is { propertyType: SerializedPropertyType.ManagedReference })
                {
                    var rid = ancestor.managedReferenceId;
                    if (rid >= 0) rids.Add(rid);
                }
            }

            return rids;
        }

        public static bool LinkTo(SerializedProperty property, string sourcePath)
        {
            if (property is null || string.IsNullOrEmpty(sourcePath)) return false;

            // Read and write through the SAME SerializedObject: a value pulled through a separate one deserializes a
            // fresh copy that gets a new rid on apply, so the two fields would not share one.
            var serializedObject = property.serializedObject;
            var value = serializedObject.FindProperty(sourcePath)?.managedReferenceValue;
            if (value is null) return false;

            property.managedReferenceValue = value;
            serializedObject.ApplyModifiedProperties();
            return true;
        }

        private static bool IsDescendant(string candidate, string ancestor) =>
            candidate.StartsWith(ancestor + ".", StringComparison.Ordinal);
    }
}
