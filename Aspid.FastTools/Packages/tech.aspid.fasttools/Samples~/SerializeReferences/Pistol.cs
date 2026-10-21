using System;
using UnityEngine;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class Pistol : IWeapon
    {
        [SerializeField] private int _damage = 10;
        [SerializeField] private int _magazineSize = 12;

        public int Damage => _damage;

        public int MagazineSize => _magazineSize;
    }
}
