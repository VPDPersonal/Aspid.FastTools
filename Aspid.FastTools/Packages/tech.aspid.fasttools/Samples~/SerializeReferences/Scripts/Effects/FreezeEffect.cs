using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences.Effects
{
    [Serializable]
    public sealed class FreezeEffect : StatusEffect
    {
        [SerializeField] [Range(0f, 1f)] private float _slow = 0.5f;

        public override void Apply(TrainingDummy target) =>
            target.Freeze(slow: _slow, duration: Duration);
    }
}
