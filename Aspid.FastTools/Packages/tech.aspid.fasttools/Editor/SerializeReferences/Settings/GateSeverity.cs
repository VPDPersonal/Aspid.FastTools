// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Build/CI gate severity for missing or unset-required managed references.
    internal enum GateSeverity
    {
        Off,
        Warn,
        Fail,
    }
}
