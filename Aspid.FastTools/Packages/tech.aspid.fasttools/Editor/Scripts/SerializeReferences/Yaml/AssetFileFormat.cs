// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // How a scan candidate is stored on disk; only TextYaml is readable by the YAML scanners.
    internal enum AssetFileFormat
    {
        TextYaml,
        Binary,
        LfsPointer,

        // Text YAML whose managed reference registry is not version 2: the scanners read the file but see none of its
        // managed references (SerializeReferenceYaml.HasUnsupportedReferencesVersion).
        UnsupportedReferencesVersion,
    }
}
