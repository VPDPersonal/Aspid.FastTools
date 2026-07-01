using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Concrete IWeapon (ranged branch). Its serialized fields are drawn inline under the dropdown's
    // foldout once it is assigned. [Serializable] is conventional for managed-reference payloads.
    [Serializable]
    public sealed class Pistol : IRanged
    {
        [SerializeField] [Min(0)] private int _damage = 10;
        [SerializeField] [Min(0)] private int _magazineSize = 12;

        public string Describe() => $"Pistol — {_damage} dmg, {_magazineSize}-round mag";
    }
}
