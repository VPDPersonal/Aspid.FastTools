using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness: mirrors the member-reference example in Documentation/EN|RU/Types.md —
// _category drives _weaponType's picker live in the Inspector.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public sealed class Loadout : MonoBehaviour
    {
        // The category chosen here drives the picker of _weaponType below.
        [SerializeField] private SerializableType<Weapon> _category;

        // Constrained live to whatever _category currently holds.
        [TypeSelector(nameof(_category))]
        [SerializeField] private string _weaponType;
    }
}
