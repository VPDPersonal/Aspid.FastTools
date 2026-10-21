using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class Railgun : IWeapon
    {
        [SerializeField] [Min(0)] private int _damage = 45;

        [TypeSelector]
        [SerializeReference] private StatusEffect _chargeEffect;

        public StatusEffect ChargeEffect => _chargeEffect;

        public int Fire() =>
            _damage;
    }
}
