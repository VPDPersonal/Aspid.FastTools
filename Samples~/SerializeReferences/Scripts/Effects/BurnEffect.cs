using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class BurnEffect : StatusEffect
    {
        [SerializeField] [Min(0f)] private float _damagePerSecond = 5f;

        public override string Describe() => $"Burn — {_damagePerSecond} dps for {Duration}s";
    }
}
