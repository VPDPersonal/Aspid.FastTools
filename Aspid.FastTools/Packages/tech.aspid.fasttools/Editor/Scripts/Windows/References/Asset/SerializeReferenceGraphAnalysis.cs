using System;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
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

        // The type name fields that get a card, in field order. An empty required one is drawn from its violation, which
        // is always kept, so it does not count; the rest share what the budget leaves.
        public static List<TypeNameField> SelectTypeNameCards(IReadOnlyList<TypeNameField> fields, int budget, out int hidden)
        {
            var cards = new List<TypeNameField>();
            hidden = 0;

            foreach (var field in fields)
            {
                if (!field.Required || !string.IsNullOrEmpty(field.TypeName))
                {
                    if (budget <= 0)
                    {
                        hidden++;
                        continue;
                    }

                    budget--;
                }

                cards.Add(field);
            }

            return cards;
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
