using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Melee", Icon = "d_Transform Icon")]
    public sealed class Sword : IMelee
    {
        [SerializeField] [Min(0)] private int _damage = 30;
        [SerializeField] [Range(0f, 1f)] private float _critChance = 0.25f;

        public string Name => "Sword";

        public int Fire() =>
            UnityEngine.Random.value < _critChance ? _damage * 2 : _damage;
    }
}
