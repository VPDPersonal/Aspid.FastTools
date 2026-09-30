using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Plain data, so the host window can keep it while the Project References view is rebuilt on a tab switch.
    internal sealed class RepairSummary
    {
        public readonly string Title;
        public readonly string Message;

        // Null when the edit cannot be undone.
        public readonly RepairReceipt Receipt;

        public RepairSummary(string title, string message, RepairReceipt receipt)
        {
            Title = title;
            Message = message;
            Receipt = receipt;
        }
    }

    // What Undo needs to re-point a rewrite back to the missing type.
    internal sealed class RepairReceipt
    {
        public readonly IReadOnlyList<MissingReferenceLocation> Entries;
        public readonly ManagedTypeName OriginalType;
        public readonly ManagedTypeName AppliedType;
        public readonly string MissingName;
        public readonly string AppliedName;

        public RepairReceipt(IReadOnlyList<MissingReferenceLocation> entries, ManagedTypeName originalType,
            ManagedTypeName appliedType, string missingName, string appliedName)
        {
            Entries = entries;
            OriginalType = originalType;
            AppliedType = appliedType;
            MissingName = missingName;
            AppliedName = appliedName;
        }
    }
}
