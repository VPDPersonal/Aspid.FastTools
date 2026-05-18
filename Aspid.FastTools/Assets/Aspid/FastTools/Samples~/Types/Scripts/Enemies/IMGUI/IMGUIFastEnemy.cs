using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public sealed class IMGUIFastEnemy : IMGUIEnemyBase
    {
        [SerializeField] [Min(0)] private float _speed = 25f;
        
        public override void Attack() =>
            Debug.Log("Fast IMGUI enemy strikes!");
    }
}
