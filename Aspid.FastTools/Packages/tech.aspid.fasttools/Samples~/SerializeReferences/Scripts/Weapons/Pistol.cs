using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences.Weapons
{
    [Serializable]
    public sealed class Pistol : IWeapon
    {
        [SerializeField] [Min(0)] private int _damage = 10;
        [SerializeField] [Min(1)] private int _magazineSize = 12;

        public int MagazineSize => _magazineSize;

        public int Fire() =>
            _damage;
    }
}
