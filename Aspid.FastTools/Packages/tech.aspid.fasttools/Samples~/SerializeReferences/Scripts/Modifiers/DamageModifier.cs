using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class DamageModifier : Modifier<float>
    {
        public override string Describe() => $"damage x{Value:0.##}";

        public override int ModifyDamage(int damage) => Mathf.RoundToInt(damage * Value);
    }
}
