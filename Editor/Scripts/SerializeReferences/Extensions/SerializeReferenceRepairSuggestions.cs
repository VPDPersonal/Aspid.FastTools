using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Aspid.FastTools.Types.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Ranking engine behind the missing-type Smart Fix suggestion: from the stored type identity, the field names
    // recorded for it and the field's declared constraint, it orders repair candidates and surfaces the best one.
    // The pool is the same set the type picker would offer, so a suggestion can never be a type the picker refuses,
    // and it is never auto-applied — the user always clicks.
    internal static class SerializeReferenceRepairSuggestions
    {
        // A scored repair candidate: the type the reference could be re-pointed to, its heuristic score (highest
        // wins) and a short human-readable reason.
        public readonly struct RepairCandidate
        {
            public readonly Type Type;
            public readonly float Score;
            public readonly string Reason;

            public RepairCandidate(Type type, float score, string reason)
            {
                Type = type;
                Score = score;
                Reason = reason;
            }
        }

        // Below this confidence the heuristics are too weak to offer at all.
        public const float MinScore = 0.6f;

        // How much the field-shape overlap can add, lifting a marginal name match over the threshold and breaking
        // ties between equally-named candidates.
        private const float FieldShapeBonus = 0.2f;

        // IMGUI repaints every frame, so the TypeCache-scanning ranking is cached per (asset, document, rid) with a
        // FIFO cap. The file id is part of the key because a rid is only unique within one host object.
        private const int CacheCapacity = 64;

        private static readonly Dictionary<(string assetPath, long fileId, long rid), IReadOnlyList<RepairCandidate>> Cache = new();
        private static readonly Queue<(string assetPath, long fileId, long rid)> CacheOrder = new();

        // Up to max candidates scoring at least MinScore, ordered by descending score. baseConstraint is the field's
        // declared element type, or typeof(object) when unconstrained.
        public static IReadOnlyList<RepairCandidate> Rank(
            ManagedTypeName stored,
            IReadOnlyCollection<string> storedFieldNames,
            Type baseConstraint,
            int max = 3)
        {
            if (stored.IsEmpty || max <= 0) return Array.Empty<RepairCandidate>();

            var constraint = baseConstraint ?? typeof(object);
            var storedClass = SerializeReferenceMovedFromResolver.NormalizeClassName(stored.Class);
            if (string.IsNullOrEmpty(storedClass)) return Array.Empty<RepairCandidate>();

            var hasFieldNames = storedFieldNames is { Count: > 0 };
            var storedFields = hasFieldNames
                ? new HashSet<string>(storedFieldNames, StringComparer.Ordinal)
                : null;

            var scored = new List<RepairCandidate>();

            foreach (var candidate in EnumerateCandidates(constraint))
            {
                var baseScore = ScoreCandidate(stored, storedClass, candidate, out var reason);
                if (baseScore <= 0f) continue;

                var bonus = hasFieldNames ? FieldShapeOverlap(storedFields, candidate) * FieldShapeBonus : 0f;
                var score = baseScore + bonus;
                if (score < MinScore) continue;

                scored.Add(new RepairCandidate(candidate, score, reason));
            }

            if (scored.Count == 0) return Array.Empty<RepairCandidate>();

            // Ties break on name then assembly, or the surfaced fix would follow TypeCache order and flip across
            // domain reloads.
            scored.Sort(static (a, b) =>
            {
                var byScore = b.Score.CompareTo(a.Score);
                if (byScore != 0) return byScore;

                var byName = string.CompareOrdinal(a.Type.FullName, b.Type.FullName);
                if (byName != 0) return byName;

                return string.CompareOrdinal(a.Type.Assembly.GetName().Name, b.Type.Assembly.GetName().Name);
            });
            return scored.Count <= max ? scored : scored.GetRange(0, max);
        }

        // The picker's own eligibility rules, so a suggestion can never be a type the field would refuse. Hidden
        // types are excluded even though the repair picker offers them: a suggestion is the package proposing a type,
        // and it does not get to volunteer one the author took out of circulation.
        private static IEnumerable<Type> EnumerateCandidates(Type constraint)
        {
            var pool = constraint == typeof(object)
                ? TypeCache.GetTypesDerivedFrom<object>()
                : TypeCache.GetTypesDerivedFrom(constraint);

            foreach (var type in pool)
            {
                if (!SerializeReferenceHelpers.IsAssignableManagedReference(type)) continue;
                if (TypeSelectorHelpers.IsHiddenFromPicker(type)) continue;
                if (constraint != typeof(object) && !constraint.IsAssignableFrom(type)) continue;
                yield return type;
            }
        }

        // Base score before the field-shape bonus; 0 means no match.
        private static float ScoreCandidate(ManagedTypeName stored, string storedClass, Type candidate, out string reason)
        {
            // A matching [MovedFrom] is an authoritative rename, so it tops the ranking.
            if (SerializeReferenceMovedFromResolver.MatchesOldIdentity(candidate, stored, storedClass))
            {
                reason = "declared [MovedFrom]";
                return 1f;
            }

            var candidateClass = SerializeReferenceMovedFromResolver.NormalizeClassName(candidate.Name);

            // Same class name in another namespace or assembly: the class was moved.
            if (string.Equals(candidateClass, storedClass, StringComparison.Ordinal))
            {
                reason = "same type name";
                return 0.8f;
            }

            if (string.Equals(candidateClass, storedClass, StringComparison.OrdinalIgnoreCase))
            {
                reason = "same name (case-insensitive)";
                return 0.6f;
            }

            // A near miss, surfaced only once the field-shape bonus lifts it over the threshold.
            if (LevenshteinAtMost(candidateClass, storedClass, 2))
            {
                reason = "similar name";
                return 0.5f;
            }

            reason = null;
            return 0f;
        }

        // Fraction of stored field names that exist on the candidate.
        private static float FieldShapeOverlap(HashSet<string> storedFields, Type candidate)
        {
            var candidateFields = GetSerializedFieldNames(candidate);
            if (candidateFields.Count == 0 || storedFields.Count == 0) return 0f;

            var matched = storedFields.Count(candidateFields.Contains);
            return (float)matched / storedFields.Count;
        }

        // Unity's rule: public instance fields plus private [SerializeField] ones, base chain included.
        private static HashSet<string> GetSerializedFieldNames(Type type)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
                foreach (var field in current.GetFields(flags))
                {
                    if (field.IsStatic || field.IsLiteral || field.IsInitOnly) continue;
                    if (field.IsNotSerialized) continue;

                    var serialized = field.IsPublic || field.IsDefined(typeof(SerializeField), inherit: false);
                    if (serialized) names.Add(field.Name);
                }
            }

            return names;
        }

        // Bounded Levenshtein with early bail-out once a row's best distance exceeds the bound.
        private static bool LevenshteinAtMost(string a, string b, int maxDistance)
        {
            if (a is null || b is null) return false;
            if (Math.Abs(a.Length - b.Length) > maxDistance) return false;
            if (a.Length == 0) return b.Length <= maxDistance;
            if (b.Length == 0) return a.Length <= maxDistance;

            var previous = new int[b.Length + 1];
            var current = new int[b.Length + 1];
            for (var j = 0; j <= b.Length; j++) previous[j] = j;

            for (var i = 1; i <= a.Length; i++)
            {
                current[0] = i;
                var rowBest = current[0];

                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    current[j] = Math.Min(Math.Min(previous[j] + 1, current[j - 1] + 1), previous[j - 1] + cost);
                    if (current[j] < rowBest) rowBest = current[j];
                }

                if (rowBest > maxDistance) return false;
                (previous, current) = (current, previous);
            }

            return previous[b.Length] <= maxDistance;
        }

        #region Cached ranking
        // The factory runs only on a cache miss.
        public static IReadOnlyList<RepairCandidate> GetCached(
            string assetPath,
            long fileId,
            long rid,
            Func<IReadOnlyList<RepairCandidate>> rank)
        {
            var key = (assetPath ?? string.Empty, fileId, rid);
            if (Cache.TryGetValue(key, out var cached)) return cached;

            var result = rank() ?? Array.Empty<RepairCandidate>();
            Cache[key] = result;
            CacheOrder.Enqueue(key);

            while (CacheOrder.Count > CacheCapacity)
            {
                var evicted = CacheOrder.Dequeue();
                Cache.Remove(evicted);
            }

            return result;
        }

        // Called after a repair, since the candidate set has changed.
        public static void ClearCache()
        {
            Cache.Clear();
            CacheOrder.Clear();
        }
        #endregion
    }
}
