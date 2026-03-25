using UnityEngine;

namespace Aspid.FastTools.Samples.IdDropdown
{
    /// <summary>
    /// ScriptableObject where the EnemyId must be unique among all EnemyDefinition assets.
    /// </summary>
    [CreateAssetMenu(menuName = "Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        [UniqueId]
        public EnemyId _id;

        [SerializeField] private string _displayName = string.Empty;
    }
}