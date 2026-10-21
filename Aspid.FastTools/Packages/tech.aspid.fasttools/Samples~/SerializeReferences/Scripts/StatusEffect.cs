using System;
using UnityEngine;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public abstract class StatusEffect
    {
        [SerializeField] [Min(0f)] private float _duration = 3f;

        public float Duration => _duration;

        public abstract void Apply(TrainingDummy target);
    }
}
