// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    internal readonly struct GateViolation
    {
        public readonly long Rid;
        public readonly long FileId;
        public readonly string AssetPath;
        public readonly string FieldPath;
        public readonly GateViolationKind Kind;
        public readonly ManagedTypeName StoredType;

        // A missing type set by a prefab instance override; FileId is then the PrefabInstance document.
        public readonly bool IsOverride;

        public GateViolation(
            string assetPath,
            long fileId,
            long rid,
            ManagedTypeName storedType,
            GateViolationKind kind,
            string fieldPath,
            bool isOverride = false)
        {
            Rid = rid;
            Kind = kind;
            FileId = fileId;
            AssetPath = assetPath;
            StoredType = storedType;
            FieldPath = fieldPath;
            IsOverride = isOverride;
        }

        public override string ToString()
        {
            var where = string.IsNullOrEmpty(FieldPath) ? $"rid {Rid}" : FieldPath;
            var what = Kind == GateViolationKind.MissingType ? $"missing type {StoredType.Class}" : "required value not set";
            if (IsOverride) what += " (prefab instance override)";

            return $"{AssetPath} : {where} -> {what}";
        }
    }
}
