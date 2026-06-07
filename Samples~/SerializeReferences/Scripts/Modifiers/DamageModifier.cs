using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Concrete subclass closing Modifier<T> over float.
    // Offered wherever the field type is IModifier or Modifier<float>.
    [Serializable]
    public sealed class DamageModifier : Modifier<float>
    {
        public override string Describe() => $"Damage ×{Value}";
    }
}
