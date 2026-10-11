// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // How a scan candidate is stored on disk; only TextYaml is readable by the YAML scanners.
    internal enum AssetFileFormat
    {
        TextYaml,
        Binary,
        LfsPointer,

        // A file that could not be opened (another program has it locked, no permission, or it was removed during the
        // scan): its format is unknown, so it is not called Binary, and it was not checked.
        Unreadable,

        // Text YAML whose managed reference registry is not version 2: the scanners read the file but see none of its
        // managed references (SerializeReferenceYaml.HasUnsupportedReferencesVersion).
        UnsupportedReferencesVersion,
    }
}
