using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public sealed class TankEnemy : EnemyBase
    {
        [SerializeField] [Min(0)] private float _armor = 50f;
        
        public override void Attack() =>
            Debug.Log("Tank attacks!");
    }
}
