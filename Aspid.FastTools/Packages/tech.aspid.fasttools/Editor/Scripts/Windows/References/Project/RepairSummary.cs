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

        // Set instead of Receipt for a rewrite of stored type names.
        public readonly TypeNameReceipt TypeNameReceipt;

        public RepairSummary(string title, string message, RepairReceipt receipt)
        {
            Title = title;
            Message = message;
            Receipt = receipt;
        }

        public RepairSummary(string title, string message, TypeNameReceipt typeNameReceipt)
        {
            Title = title;
            Message = message;
            TypeNameReceipt = typeNameReceipt;
        }
    }

    // What Undo needs to put the missing names back: the entries as they were read before the rewrite.
    internal sealed class TypeNameReceipt
    {
        public readonly IReadOnlyList<MissingTypeNameLocation> Entries;
        public readonly string AppliedName;
        public readonly string MissingName;
        public readonly string AppliedDisplayName;

        public TypeNameReceipt(IReadOnlyList<MissingTypeNameLocation> entries, string appliedName, string missingName,
            string appliedDisplayName)
        {
            Entries = entries;
            AppliedName = appliedName;
            MissingName = missingName;
            AppliedDisplayName = appliedDisplayName;
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
