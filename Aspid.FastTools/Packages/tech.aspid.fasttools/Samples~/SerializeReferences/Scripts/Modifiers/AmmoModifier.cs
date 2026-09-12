using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="Modifier{T}"/> that describes an ammunition bonus.
    /// </summary>
    [Serializable]
    public sealed class AmmoModifier : Modifier<int>
    {
        /// <summary>
        /// Returns a description of the ammunition bonus.
        /// </summary>
        /// <returns>Modifier description.</returns>
        public override string Describe() => $"+{Value} ammo";
    }
}
