using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Game.Enemies
{
    public sealed class ArmoredEnemy : EnemyBase
    {
        [SerializeField] private int _armor = 10;
    }
}
