using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class FreezeEffect : StatusEffect
    {
        [SerializeField] [Range(0f, 100f)] private float _slowPercent = 40f;

        public override string Describe() => $"Freeze — {_slowPercent}% slow for {Duration}s";
    }
}
