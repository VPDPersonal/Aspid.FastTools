using System;
using UnityEngine;
using Aspid.FastTools;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.IdDropdown
{
    [Serializable]
    public struct ItemId
    {
        [IdDropdown]
        public string _id;

        public override string ToString() => _id ?? string.Empty;
    }

    [Serializable]
    public struct EnemyId
    {
        [IdDropdown]
        public string _id;

        public override string ToString() => _id ?? string.Empty;
    }

    /// <summary>
    /// ScriptableObject where the ItemId must be unique among all ItemDefinition assets.
    /// </summary>
    public class ItemDefinition : ScriptableObject
    {
        [UniqueId]
        public ItemId _id;

        [SerializeField] private string _displayName = string.Empty;
    }

    /// <summary>
    /// MonoBehaviour that uses IDs without uniqueness constraint.
    /// </summary>
    public class IdDropdownTest : MonoBehaviour
    {
        [Header("Item ID (no uniqueness check)")]
        [SerializeField] private ItemId _swordId;
        [SerializeField] private ItemId _shieldId;

        [Header("Enemy ID (no uniqueness check)")]
        [SerializeField] private EnemyId _bossId;
    }
}
