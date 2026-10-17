using System;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A required type name field that holds a name. FieldPath is a property path, as in GateViolation.
    internal readonly struct RequiredTypeName
    {
        public readonly long FileId;
        public readonly string FieldPath;
        public readonly string ComponentName;
        public readonly string TypeName;

        public RequiredTypeName(long fileId, string fieldPath, string componentName, string typeName)
        {
            FileId = fileId;
            FieldPath = fieldPath;
            ComponentName = componentName;
            TypeName = typeName;
        }

        public string FieldLabel => string.IsNullOrEmpty(ComponentName) ? FieldPath : $"{ComponentName}.{FieldPath}";
    }

    internal static class SerializeReferenceGraphAnalysis
    {
        public static string CombinePath(string parent, string child)
        {
            if (string.IsNullOrEmpty(child)) return parent;
            return string.IsNullOrEmpty(parent) ? child : $"{parent}.{child}";
        }

        // Only the empty slots that the plans draw, so a required field counts as graphed only when its card exists.
        public static HashSet<(long fileId, string path)> CollectEmptySlotPaths(List<SerializeReferenceGraphPlan> plans)
        {
            var paths = new HashSet<(long, string)>();

            foreach (var plan in plans)
            {
                foreach (var card in plan.Cards)
                {
                    if (card.Kind != SerializeReferenceGraphPlan.CardKind.Empty) continue;
                    paths.Add((plan.Document.FileId, SerializeReferenceGraphEditor.ToSerializedPropertyPath(card.Path)));
                }
            }

            return paths;
        }

        // The violations that get their own required-field card. A required string field has no node in the graph, so it
        // always gets one; a required reference only while the budget lasts, like the cards of the plans.
        public static List<GateViolation> SelectRequiredCards(List<GateViolation> violations, int budget, out int hidden)
        {
            var cards = new List<GateViolation>();
            hidden = 0;

            foreach (var violation in violations)
            {
                if (violation.Rid != 0 && budget <= 0)
                {
                    hidden++;
                    continue;
                }

                cards.Add(violation);
                budget--;
            }

            return cards;
        }

        // The required scan reports only the empty ones, so a field would leave the map once it is set. A scene cannot be
        // object-loaded, so it has none here.
        public static List<RequiredTypeName> CollectSetRequiredTypeNames(string assetPath)
        {
            var fields = new List<RequiredTypeName>();
            if (SerializeReferenceHelpers.IsScene(assetPath)) return fields;

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset == null) continue;
                if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out _, out var fileId)) continue;

                using var serializedObject = new SerializedObject(asset);
                using var iterator = serializedObject.GetIterator();
                if (!iterator.Next(enterChildren: true)) continue;

                var visited = new HashSet<long>();
                bool enterChildren;

                do
                {
                    enterChildren = true;

                    if (iterator.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        var id = iterator.managedReferenceId;
                        if (id >= 0 && !visited.Add(id)) enterChildren = false;
                        continue;
                    }

                    if (iterator.propertyType != SerializedPropertyType.String || string.IsNullOrEmpty(iterator.stringValue)) continue;
                    if (!TypeSelectorRequiredGate.TryGetRequired(iterator, out _)) continue;

                    fields.Add(new RequiredTypeName(fileId, TypeSelectorRequiredGate.GetFieldPath(iterator),
                        asset.GetType().Name, iterator.stringValue));
                }
                while (iterator.Next(enterChildren));
            }

            return fields;
        }

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

        public static bool RootIsMissing(ReferenceGraphDocument document, long rid)
        {
            var node = document.FindNode(rid);
            return node is { Resolves: false, StoredType: { IsEmpty: false } };
        }

        public static bool IsPendingMigration(string assetPath, long fileId, long rid, ManagedTypeName storedType,
            SerializeReferenceConstraintCache constraints, out Type target)
        {
            if (!SerializeReferenceMovedFromResolver.TryResolve(storedType, out target)) return false;

            return SerializeReferenceHelpers.FitsConstraints(target, constraints.Resolve(assetPath, fileId, rid));
        }

        public static bool TryGetSuggestion(string assetPath, long fileId, long rid, ManagedTypeName storedType,
            SerializeReferenceConstraintCache constraints, out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            try
            {
                var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(assetPath, fileId, rid);
                var constraint = constraints.Resolve(assetPath, fileId, rid);

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
    }
}
