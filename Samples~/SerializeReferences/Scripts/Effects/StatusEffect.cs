using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // The abstract base is excluded from the picker; only concrete effects are offered.
    /// <summary>
    /// Represents a timed effect applied to a training dummy.
    /// </summary>
    [Serializable]
    public abstract class StatusEffect
    {
        [Tooltip("Effect duration in seconds.")]
        [SerializeField, Min(0f)] private float _duration = 3f;

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the effect duration in seconds.
        /// </summary>
        public float Duration => _duration;

        /// <summary>
        /// Called after a hit. Override to apply the effect to the target.
        /// </summary>
        /// <param name="target">Training dummy receiving the effect.</param>
        public abstract void Apply(TrainingDummy target);
    }
}
