using System;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Reads a scanned reference graph without drawing it: what is broken, what is merely a pending rename, which
    // slots sit empty and what a broken node's best repair guess is. The migration checks need the declared field
    // type behind a rid, so they take the caller's constraint cache and one asset scan serves the whole render pass.
    internal static class SerializeReferenceGraphAnalysis
    {
        public static string CombinePath(string parent, string child)
        {
            if (string.IsNullOrEmpty(child)) return parent;
            return string.IsNullOrEmpty(parent) ? child : $"{parent}.{child}";
        }

        // Every empty slot's normalized field path, across every document, root and nested edge — the card-building
        // walk minus the cards. An empty slot's required badge is checked against this set, which is how the graph
        // tells "already badged on a card" apart from "no card exists for this field", as a required string field
        // has no rid and so no node.
        public static HashSet<(long fileId, string path)> CollectEmptySlotPaths(List<ReferenceGraphDocument> documents)
        {
            var paths = new HashSet<(long, string)>();

            foreach (var document in documents)
            {
                foreach (var root in document.Roots)
                {
                    if (root.IsEmpty)
                        paths.Add((document.FileId, SerializeReferenceGraphEditor.ToSerializedPropertyPath(root.Label)));
                    else
                        WalkForEmptySlots(document, root.Rid, root.Label, new HashSet<long>(), paths);
                }
            }

            return paths;
        }

        // Used only for the overview hint; an empty slot is not an issue.
        public static int CountEmptySlots(ReferenceGraphDocument document)
        {
            var count = document.Roots.Count(root => root.IsEmpty);

            foreach (var pair in document.Edges)
            {
                count += pair.Value.Count(edge => edge.IsEmpty);
            }

            return count;
        }

        // Splits a document's unresolved nodes into genuinely broken ones and pending migrations. An orphaned rid
        // always counts as broken, since nothing loads an orphan and in-memory migration cannot apply, and it stays
        // out of the migration tally because the orphan counters already own it.
        public static (int broken, int migrations) CountUnresolved(string assetPath, ReferenceGraphDocument document,
            SerializeReferenceConstraintCache constraints)
        {
            var broken = 0;
            var migrations = 0;

            foreach (var node in document.Nodes)
            {
                if (node.Resolves || node.StoredType.IsEmpty) continue;
                if (document.Orphans.Contains(node.Rid)) continue;

                if (IsPendingMigration(assetPath, document.FileId, node.Rid, node.StoredType, constraints, out _))
                    migrations++;
                else
                    broken++;
            }

            return (broken, migrations);
        }

        // Drives the amber tint and the missing-first root ordering.
        public static bool RootIsMissing(ReferenceGraphDocument document, long rid)
        {
            var node = document.FindNode(rid);
            return node is { Resolves: false, StoredType: { IsEmpty: false } };
        }

        // Whether exactly one [MovedFrom] target claims the stored type and fits the field's declared type, meaning
        // Unity already migrates it in memory and only the file is stale. An unrecoverable constraint lets it
        // through.
        public static bool IsPendingMigration(string assetPath, long fileId, long rid, ManagedTypeName storedType,
            SerializeReferenceConstraintCache constraints, out Type target)
        {
            if (!SerializeReferenceMovedFromResolver.TryResolve(storedType, out target)) return false;

            var constraint = constraints.Resolve(assetPath, fileId, rid);
            return constraint is null || constraint == typeof(object) || constraint.IsAssignableFrom(target);
        }

        // Goes through the shared per-(path, fileId, rid) cache, so a rescan and the inline drawer reuse one
        // computation. A parse miss just means no suggestion row.
        public static bool TryGetSuggestion(string assetPath, long fileId, long rid, ManagedTypeName storedType,
            SerializeReferenceConstraintCache constraints, out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            try
            {
                var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(assetPath, fileId, rid);
                var constraint = constraints.Resolve(assetPath, fileId, rid) ?? typeof(object);

                var ranked = SerializeReferenceRepairSuggestions.GetCached(assetPath, fileId, rid,
                    () => SerializeReferenceRepairSuggestions.Rank(storedType, fieldNames, constraint));
                if (ranked.Count == 0) return false;

                suggestion = ranked[0];
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void WalkForEmptySlots(ReferenceGraphDocument document, long rid, string pathLabel,
            HashSet<long> visited, HashSet<(long fileId, string path)> paths)
        {
            if (!visited.Add(rid)) return;

            foreach (var edge in document.ChildrenOf(rid))
            {
                var childPath = CombinePath(pathLabel, edge.Label);
                if (edge.IsEmpty)
                    paths.Add((document.FileId, SerializeReferenceGraphEditor.ToSerializedPropertyPath(childPath)));
                else
                    WalkForEmptySlots(document, edge.Rid, childPath, visited, paths);
            }

            visited.Remove(rid);
        }
    }
}
