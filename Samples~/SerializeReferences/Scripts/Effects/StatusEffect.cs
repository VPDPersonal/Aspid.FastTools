using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Abstract base: a field declared as StatusEffect offers only the concrete subclasses.
    [Serializable]
    public abstract class StatusEffect
    {
        [SerializeField] [Min(0f)] private float _duration = 3f;

        public abstract string Name { get; }

        public float Duration => _duration;

        public abstract void Apply(TrainingDummy target);
    }
}
