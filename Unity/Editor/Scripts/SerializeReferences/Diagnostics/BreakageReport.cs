using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>One managed reference that just became missing, plus its pre-computed best fix candidate (if any).</summary>
    internal readonly struct BreakageEntry
    {
        public readonly string AssetPath;
        public readonly long FileId;
        public readonly long Rid;
        public readonly ManagedTypeName StoredType;

        /// <summary>False for entries the per-asset repair flow cannot reach (currently scene-hosted references).</summary>
        public readonly bool IsRepairable;

        /// <summary>The top Smart-Fix suggestion (e.g. a declared <c>[MovedFrom]</c> rename), pre-ranked at detection time.</summary>
        public readonly SerializeReferenceRepairSuggestions.RepairCandidate? TopSuggestion;

        /// <summary>
        /// The authoritative <c>[MovedFrom]</c> rename target of the stored type (see
        /// <see cref="SerializeReferenceMovedFromResolver"/>), or <see langword="null"/>. Non-null means the
        /// reference is not really broken — Unity migrates it in memory at load; only the file is stale.
        /// </summary>
        public readonly Type MigrationTarget;

        public BreakageEntry(
            string assetPath,
            long fileId,
            long rid,
            ManagedTypeName storedType,
            bool isRepairable,
            SerializeReferenceRepairSuggestions.RepairCandidate? topSuggestion,
            Type migrationTarget)
        {
            AssetPath = assetPath;
            FileId = fileId;
            Rid = rid;
            StoredType = storedType;
            IsRepairable = isRepairable;
            TopSuggestion = topSuggestion;
            MigrationTarget = migrationTarget;
        }

        public string TypeName => StoredType.DisplayName;
    }

    /// <summary>The set of managed references that became missing since the last scan, grouped count metadata included.</summary>
    internal readonly struct BreakageReport
    {
        public readonly IReadOnlyList<BreakageEntry> Entries;
        public readonly int TypeCount;

        public BreakageReport(IReadOnlyList<BreakageEntry> entries, int typeCount)
        {
            Entries = entries;
            TypeCount = typeCount;
        }

        public bool HasAny => Entries is { Count: > 0 };
    }
}
