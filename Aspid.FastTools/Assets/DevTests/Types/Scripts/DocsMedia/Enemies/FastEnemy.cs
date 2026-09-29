using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Game.Enemies
{
    public sealed class FastEnemy : EnemyBase
    {
        [SerializeField] private float _speed = 25f;
    }
}
