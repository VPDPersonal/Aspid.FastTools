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

        // The stored assembly-qualified name of a MissingTypeName violation; empty otherwise.
        public readonly string TypeName;

        public GateViolation(
            string assetPath,
            long fileId,
            long rid,
            ManagedTypeName storedType,
            GateViolationKind kind,
            string fieldPath,
            bool isOverride = false,
            string typeName = null)
        {
            Rid = rid;
            Kind = kind;
            FileId = fileId;
            AssetPath = assetPath;
            StoredType = storedType;
            FieldPath = fieldPath;
            IsOverride = isOverride;
            TypeName = typeName ?? string.Empty;
        }

        public static GateViolation ForTypeName(string assetPath, StoredTypeNameEntry entry) =>
            new(assetPath, entry.FileId, entry.Rid, default, GateViolationKind.MissingTypeName, entry.FieldPath,
                entry.IsOverride, entry.TypeName);

        // The report's stored-name column: the class of a managed reference, or the whole stored type name.
        public string StoredName => Kind == GateViolationKind.MissingTypeName ? TypeName : StoredType.Class ?? string.Empty;

        // The report's trailing columns: where the stored class of a MissingType row lived; empty for the other kinds.
        public string StoredNamespace => StoredType.Namespace ?? string.Empty;

        public string StoredAssembly => StoredType.Assembly ?? string.Empty;

        public override string ToString()
        {
            var where = string.IsNullOrEmpty(FieldPath) ? $"rid {Rid}" : FieldPath;
            if (Kind == GateViolationKind.MissingTypeName && Rid != 0) where = $"rid {Rid} {where}";

            var what = Kind switch
            {
                GateViolationKind.MissingType => $"missing type {StoredType.DisplayName}",
                GateViolationKind.MissingTypeName => $"missing type name {MissingTypeNames.FullName(TypeName)}",
                _ => "required value not set",
            };
            if (IsOverride) what += " (prefab instance override)";

            return $"{AssetPath} : {where} -> {what}";
        }
    }
}
