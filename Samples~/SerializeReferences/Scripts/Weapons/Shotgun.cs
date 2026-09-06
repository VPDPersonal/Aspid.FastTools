using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon")]
    public sealed class Shotgun : IRanged
    {
        [SerializeField] [Min(0)] private int _damage = 6;
        [SerializeField] [Min(1)] private int _pellets = 8;

        public string Name => "Shotgun";

        public int Fire() =>
            _damage * UnityEngine.Random.Range(_pellets / 2, _pellets + 1);
    }
}
