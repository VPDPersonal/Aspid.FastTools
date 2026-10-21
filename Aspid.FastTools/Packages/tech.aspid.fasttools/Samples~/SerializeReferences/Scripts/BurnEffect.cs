using System;
using UnityEngine;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class BurnEffect : StatusEffect
    {
        [SerializeField, Min(0)] private int _damagePerSecond = 5;

        public override void Apply(TrainingDummy target) =>
            target.Burn(damagePerSecond: _damagePerSecond, duration: Duration);
    }
}
