using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Concrete subclass closing Modifier<T> over int.
    // Offered for an IModifier field, but NOT for a Modifier<float> field —
    // it is Modifier<int>, which is not assignable to Modifier<float>.
    [Serializable]
    public sealed class AmmoModifier : Modifier<int>
    {
        public override string Describe() => $"+{Value} ammo";
    }
}
