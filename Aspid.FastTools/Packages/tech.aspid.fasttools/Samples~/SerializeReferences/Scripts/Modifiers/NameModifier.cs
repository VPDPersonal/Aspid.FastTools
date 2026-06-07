using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Concrete subclass closing Modifier<T> over string.
    // Offered for an IModifier field; excluded from a Modifier<float> field.
    [Serializable]
    public sealed class NameModifier : Modifier<string>
    {
        public override string Describe() => $"Renamed to \"{Value}\"";
    }
}
