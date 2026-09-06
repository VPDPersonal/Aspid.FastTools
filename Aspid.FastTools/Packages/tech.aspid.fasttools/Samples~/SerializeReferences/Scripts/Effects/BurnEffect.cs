using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class BurnEffect : StatusEffect
    {
        [SerializeField] [Min(0)] private int _damagePerSecond = 5;

        public override string Name => "Burn";

        public override void Apply(TrainingDummy target) =>
            target.Burn(_damagePerSecond, Duration);
    }
}
