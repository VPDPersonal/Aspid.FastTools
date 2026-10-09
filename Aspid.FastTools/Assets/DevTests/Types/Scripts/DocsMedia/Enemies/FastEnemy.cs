using UnityEngine;

// Serialized fields are read by the Inspector only.
#pragma warning disable CS0414

// ReSharper disable once CheckNamespace
namespace Game.Enemies
{
    public sealed class FastEnemy : EnemyBase
    {
        [SerializeField] private float _speed = 25f;
    }
}
#pragma warning restore CS0414
