using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
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
