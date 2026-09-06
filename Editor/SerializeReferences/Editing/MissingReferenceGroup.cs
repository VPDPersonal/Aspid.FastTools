using System;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // One broken managed-reference entry plus the asset it lives in.
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

    // Every broken reference sharing one stored type across the project — the unit the audit lists and bulk-fixes.
    // The picker constraint intersects the entries' declared field types, falling back to object when they disagree.
    internal sealed class MissingReferenceGroup
    {
        public readonly ManagedTypeName StoredType;
        public readonly List<MissingReferenceLocation> Entries = new();

        private readonly HashSet<string> _files = new(StringComparer.Ordinal);
        private readonly SerializeReferenceConstraintCache _constraints = new();

        public MissingReferenceGroup(ManagedTypeName storedType)
        {
            StoredType = storedType;
        }

        public int FileCount => _files.Count;

        public string DisplayName => StoredType.DisplayName;

        // Groups every unresolved reference by stored type, biggest group first. Cheap once the shared usage index
        // is warm, since it is an in-memory filter rather than a sweep.
        public static List<MissingReferenceGroup> CollectFromIndex()
        {
            var byType = new Dictionary<string, MissingReferenceGroup>(StringComparer.Ordinal);

            foreach (var usage in SerializeReferenceTypeUsageIndex.EnumerateUnresolved())
            {
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

        // The type every entry's field can hold, or object when that cannot be narrowed.
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

    // A group's picker constraint and whether it reads as a one-click [MovedFrom] migration, resolved once so the
    // audit's partition, card body and picker label can never disagree. A migration needs its rename target to fit
    // the constraint too: "Migrate all" bypasses the picker's assignability guarantee, and Unity would null an
    // incompatible target at load.
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
