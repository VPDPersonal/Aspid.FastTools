using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // The melee branch of the IWeapon hierarchy, added for the tutorial's narrowing step.
    // It is the only IMelee, so [TypeSelector(typeof(IMelee))] resolves to exactly this type.
    [Serializable]
    public sealed class Sword : IMelee
    {
        [SerializeField] [Min(0)] private int _damage = 30;
        [SerializeField] [Range(0f, 5f)] private float _reach = 1.8f;

        public string Describe() => $"Sword — {_damage} dmg, {_reach}m reach";
    }
}
