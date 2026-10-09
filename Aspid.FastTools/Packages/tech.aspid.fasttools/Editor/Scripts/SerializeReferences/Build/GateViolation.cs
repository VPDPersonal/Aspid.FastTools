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

        // The class of the component or asset that holds a required field, read while the scan has it loaded so a
        // window never loads the file again to label a row; empty for a scene and for the other kinds.
        public readonly string ComponentName;

        public GateViolation(
            string assetPath,
            long fileId,
            long rid,
            ManagedTypeName storedType,
            GateViolationKind kind,
            string fieldPath,
            bool isOverride = false,
            string typeName = null,
            string componentName = null)
        {
            Rid = rid;
            Kind = kind;
            FileId = fileId;
            AssetPath = assetPath;
            StoredType = storedType;
            FieldPath = fieldPath;
            IsOverride = isOverride;
            TypeName = typeName ?? string.Empty;
            ComponentName = componentName ?? string.Empty;
        }

        public static GateViolation ForTypeName(string assetPath, StoredTypeNameEntry entry) =>
            new(assetPath, entry.FileId, entry.Rid, default, GateViolationKind.MissingTypeName, entry.FieldPath,
                entry.IsOverride, entry.TypeName);

        // The row label of a field: "Component.field", or the bare path when the component is unknown.
        public string FieldLabel => string.IsNullOrEmpty(ComponentName) ? FieldPath : $"{ComponentName}.{FieldPath}";

        // The report's stored-name column: the class of a managed reference, or the whole stored type name.
        public string StoredName => Kind == GateViolationKind.MissingTypeName ? TypeName : StoredType.Class ?? string.Empty;

        public override string ToString()
        {
            var where = string.IsNullOrEmpty(FieldPath) ? $"rid {Rid}" : FieldPath;
            if (Kind == GateViolationKind.MissingTypeName && Rid != 0) where = $"rid {Rid} {where}";

            var what = Kind switch
            {
                GateViolationKind.MissingType => $"missing type {StoredType.Class}",
                GateViolationKind.MissingTypeName => $"missing type name {MissingTypeNames.FullName(TypeName)}",
                _ => "required value not set",
            };
            if (IsOverride) what += " (prefab instance override)";

            return $"{AssetPath} : {where} -> {what}";
        }
    }
}
