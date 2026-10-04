// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // A new name for one stored type name field, applied only while the field still stores Entry.TypeName.
    internal readonly struct TypeNameEdit
    {
        public readonly StoredTypeNameEntry Entry;
        public readonly string NewName;

        // The inline mapping for the wrapper's _script ("{fileID: 0}" clears it); null leaves _script as it is.
        public readonly string NewScriptReference;

        public TypeNameEdit(StoredTypeNameEntry entry, string newName, string newScriptReference = null)
        {
            Entry = entry;
            NewName = newName ?? string.Empty;
            NewScriptReference = newScriptReference;
        }
    }
}
