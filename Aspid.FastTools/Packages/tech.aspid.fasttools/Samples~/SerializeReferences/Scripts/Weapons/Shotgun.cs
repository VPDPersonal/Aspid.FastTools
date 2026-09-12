using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="IRanged"/> that randomizes the number of pellets hitting the target.
    /// </summary>
    [Serializable]
    [TypeSelectorDisplay(Group = "Weapons/Ranged", Icon = "d_Transform Icon")]
    public sealed class Shotgun : IRanged
    {
        [Tooltip("Damage dealt by each pellet that hits.")]
        [SerializeField, Min(0)] private int _damage = 6;

        [Tooltip("Maximum number of pellets that can hit the target.")]
        [SerializeField, Min(1)] private int _pellets = 8;

        /// <inheritdoc/>
        public string Name => "Shotgun";

        /// <inheritdoc/>
        public int Fire() =>
            _damage * UnityEngine.Random.Range(_pellets / 2, _pellets + 1);
    }
}
