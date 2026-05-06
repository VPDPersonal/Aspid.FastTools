using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public sealed class IMGUITankEnemy : IMGUIEnemyBase
    {
        [SerializeField] [Min(0)] private float _armor = 50f;
        
        public override void Attack() =>
            Debug.Log("IMGUI tank attacks!");
    }
}
