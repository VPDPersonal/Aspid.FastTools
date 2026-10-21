using System;
using UnityEngine;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class Shotgun : IWeapon
    {
        [SerializeField] private int _damage = 6;
        [SerializeField] private int _pellets = 8;

        public int Damage => _damage * _pellets;
    }
}
