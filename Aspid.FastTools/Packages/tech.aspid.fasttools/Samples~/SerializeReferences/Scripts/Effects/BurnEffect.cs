using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="StatusEffect"/> that deals damage over time.
    /// </summary>
    [Serializable]
    public sealed class BurnEffect : StatusEffect
    {
        [Tooltip("Burn damage dealt per second.")]
        [SerializeField, Min(0)] private int _damagePerSecond = 5;

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string Name => "Burn";

        /// <summary>
        /// Applies burning for the configured duration.
        /// </summary>
        /// <param name="target">Training dummy receiving the effect.</param>
        public override void Apply(TrainingDummy target) =>
            target.Burn(_damagePerSecond, Duration);
    }
}
