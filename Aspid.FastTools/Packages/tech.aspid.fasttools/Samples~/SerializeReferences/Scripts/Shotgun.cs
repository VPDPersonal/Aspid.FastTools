using System;
using UnityEngine;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    public sealed class Shotgun : IWeapon
    {
        [SerializeField, Min(0)] private int _damage = 6;
        [SerializeField, Min(1)] private int _pellets = 8;

        public int Fire() =>
            _damage * UnityEngine.Random.Range(minInclusive: _pellets / 2, maxExclusive: _pellets + 1);
    }
}
