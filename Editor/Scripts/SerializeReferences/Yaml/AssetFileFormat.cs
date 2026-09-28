// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // How a scan candidate is stored on disk; only TextYaml is readable by the YAML scanners.
    internal enum AssetFileFormat
    {
        TextYaml,
        Binary,
        LfsPointer,
    }
}
