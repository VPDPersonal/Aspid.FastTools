using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="Modifier{T}"/> that scales attack damage.
    /// </summary>
    [Serializable]
    public sealed class DamageModifier : Modifier<float>
    {
        /// <summary>
        /// Returns a description of the damage multiplier.
        /// </summary>
        /// <returns>Modifier description.</returns>
        public override string Describe() => $"damage x{Value:0.##}";

        /// <summary>
        /// Returns incoming damage scaled by the stored multiplier and rounded to an integer.
        /// </summary>
        /// <param name="damage">Incoming attack damage.</param>
        /// <returns>Modified attack damage.</returns>
        public override int ModifyDamage(int damage) => Mathf.RoundToInt(damage * Value);
    }
}
