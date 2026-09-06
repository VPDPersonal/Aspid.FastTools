using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class FreezeEffect : StatusEffect
    {
        [SerializeField] [Range(0f, 1f)] private float _slow = 0.5f;

        public override string Name => "Freeze";

        public override void Apply(TrainingDummy target) =>
            target.Freeze(_slow, Duration);
    }
}
