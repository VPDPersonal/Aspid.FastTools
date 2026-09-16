using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness: the component the picker in Images/aspid_fasttools_type_selector_display.png is
// opened from. Allow = TypeAllow.None keeps the abstract CombatModifier out, so the page shows exactly
// the three decorated candidates.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public sealed class ModifierRack : MonoBehaviour
    {
        [TypeSelector(typeof(CombatModifier), Allow = TypeAllow.None)]
        [SerializeField] private string _modifierType;
    }
}
