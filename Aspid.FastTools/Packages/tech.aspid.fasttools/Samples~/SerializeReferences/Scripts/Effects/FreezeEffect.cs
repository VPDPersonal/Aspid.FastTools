using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="StatusEffect"/> that increases the interval between loadout attacks.
    /// </summary>
    [Serializable]
    public sealed class FreezeEffect : StatusEffect
    {
        [Tooltip("Fraction added to the interval between attacks.")]
        [SerializeField, Range(0f, 1f)] private float _slow = 0.5f;

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string Name => "Freeze";

        /// <summary>
        /// Applies slowing for the configured duration.
        /// </summary>
        /// <param name="target">Training dummy receiving the effect.</param>
        public override void Apply(TrainingDummy target) =>
            target.Freeze(_slow, Duration);
    }
}
