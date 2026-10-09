using System;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct MissingReferenceLocation
    {
        public readonly string AssetPath;
        public readonly MissingReferenceEntry Entry;

        public MissingReferenceLocation(string assetPath, MissingReferenceEntry entry)
        {
            AssetPath = assetPath;
            Entry = entry;
        }
    }

    internal sealed class MissingReferenceGroup
    {
        public readonly ManagedTypeName StoredType;
        public readonly List<MissingReferenceLocation> Entries = new();

        // Shared by every group and kept across renders, since a render of the window reads the constraint of every
        // entry. Import and Rescan clear it.
        private static readonly SerializeReferenceConstraintCache _constraints = new();

        private readonly HashSet<string> _files = new(StringComparer.Ordinal);

        public MissingReferenceGroup(ManagedTypeName storedType)
        {
            StoredType = storedType;
        }

        public int FileCount => _files.Count;

        public string DisplayName => StoredType.DisplayName;

        public static void ClearConstraintCache() => _constraints.Clear();

        public static List<MissingReferenceGroup> CollectFromIndex()
        {
            var byType = new Dictionary<string, MissingReferenceGroup>(StringComparer.Ordinal);

            foreach (var usage in SerializeReferenceTypeUsageIndex.EnumerateUnresolved())
            {
                // Fix all rewrites RefIds entries; an override has none, so it is listed by CollectOverridesFromIndex.
                if (usage.IsOverride) continue;

                var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                if (string.IsNullOrEmpty(path)) continue;

                var key = SerializeReferenceHelpers.StoredTypeKey(usage.StoredType);
                if (!byType.TryGetValue(key, out var group))
                {
                    group = new MissingReferenceGroup(usage.StoredType);
                    byType.Add(key, group);
                }

                group.Add(path, new MissingReferenceEntry(usage.FileId, usage.Rid, usage.StoredType));
            }

            var groups = byType.Values.ToList();
            groups.Sort((a, b) => b.Entries.Count.CompareTo(a.Entries.Count));
            return groups;
        }

        // Missing types set by prefab instance overrides. They are reported but not repaired: the fix is a new type or
        // Revert on the instance itself. An old name a [MovedFrom] claims stays listed (see OverrideMigrationTarget).
        public static List<MissingReferenceLocation> CollectOverridesFromIndex()
        {
            var result = new List<MissingReferenceLocation>();

            foreach (var usage in SerializeReferenceTypeUsageIndex.EnumerateUnresolved())
            {
                if (!usage.IsOverride) continue;

                var path = AssetDatabase.GUIDToAssetPath(usage.Guid);
                if (string.IsNullOrEmpty(path)) continue;

                result.Add(new MissingReferenceLocation(path,
                    new MissingReferenceEntry(usage.FileId, usage.Rid, usage.StoredType, isOverride: true)));
            }

            result.Sort((a, b) => string.CompareOrdinal(a.AssetPath, b.AssetPath));
            return result;
        }

        // The type a [MovedFrom] maps an override's old name to, or null. The gate treats it as a pending migration
        // (an override has no field constraint to reject the target), but Migrate all rewrites RefIds entries only, so
        // the instance keeps the old name, and needs the attribute, until it is saved with the new one.
        public static Type OverrideMigrationTarget(MissingReferenceLocation location) =>
            SerializeReferenceMovedFromResolver.TryResolve(location.Entry.StoredType, out var target) ? target : null;

        public void Add(string assetPath, MissingReferenceEntry entry)
        {
            Entries.Add(new MissingReferenceLocation(assetPath, entry));
            _files.Add(assetPath);
        }

        // Ranked against the constraint-filtered pool, so the suggestion is always assignable — which is what lets a
        // quick-apply bypass the picker. Every entry stores the same broken type, so the first one ranks the same
        // candidates as any other.
        public bool TryGetSuggestion(Type constraint, out SerializeReferenceRepairSuggestions.RepairCandidate suggestion)
        {
            suggestion = default;

            var first = Entries[0];
            var fieldNames = SerializeReferenceYamlEditor.GetReferenceFieldNames(first.AssetPath, first.Entry.FileId, first.Entry.Rid);

            var ranked = SerializeReferenceRepairSuggestions.Rank(StoredType, fieldNames, constraint);
            if (ranked.Count == 0) return false;

            suggestion = ranked[0];
            return true;
        }

        public Type ResolveConstraint() => ResolveConstraint(out _);

        // mixedFieldTypes separates a fallback caused by disagreeing field types from an unrecoverable one; the
        // bulk-fix confirmation warns only on the former.
        public Type ResolveConstraint(out bool mixedFieldTypes)
        {
            mixedFieldTypes = false;
            Type common = null;

            foreach (var entry in Entries)
            {
                // An unrecoverable field type leaves the group unconstrained: a tighter guess could hide a valid
                // pick.
                var fieldType = _constraints.Resolve(entry.AssetPath, entry.Entry.FileId, entry.Entry.Rid);
                if (fieldType is null) return typeof(object);

                if (common is null)
                {
                    common = fieldType;
                }
                else if (common != fieldType)
                {
                    mixedFieldTypes = true;
                    return typeof(object);
                }
            }

            return common ?? typeof(object);
        }
    }

    internal readonly struct MissingReferenceMigration
    {
        public readonly Type Constraint;
        public readonly bool IsMigration;

        public readonly Type Target;

        public MissingReferenceMigration(MissingReferenceGroup group)
        {
            Constraint = group.ResolveConstraint();
            IsMigration = SerializeReferenceMovedFromResolver.TryResolve(group.StoredType, out var target) &&
                (Constraint == typeof(object) || Constraint.IsAssignableFrom(target));
            Target = IsMigration ? target : null;
        }
    }
}
