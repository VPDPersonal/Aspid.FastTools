using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class AmmoModifier : Modifier<int>
    {
        public override string Describe() => $"+{Value} ammo";
    }
}
