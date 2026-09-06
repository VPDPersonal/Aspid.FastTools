using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // [Serializable] is what lets Unity store the instance; the serialized fields are drawn inline under the
    // type dropdown once the weapon is picked. Field names matter: switching Pistol → Shotgun and back keeps
    // the values of fields both types declare under the same name.
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon")]
    public sealed class Pistol : IRanged
    {
        [SerializeField] [Min(0)] private int _damage = 10;
        [SerializeField] [Min(1)] private int _magazineSize = 12;

        private int _rounds;

        public string Name => "Pistol";

        public int Fire()
        {
            if (_rounds <= 0) _rounds = _magazineSize;
            _rounds--;
            return _damage;
        }
    }
}
