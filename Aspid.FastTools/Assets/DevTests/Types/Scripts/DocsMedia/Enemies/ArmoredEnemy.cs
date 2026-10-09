using UnityEngine;

// Serialized fields are read by the Inspector only.
#pragma warning disable CS0414

// ReSharper disable once CheckNamespace
namespace Game.Enemies
{
    public sealed class ArmoredEnemy : EnemyBase
    {
        [SerializeField] private int _armor = 10;
    }
}
#pragma warning restore CS0414
