using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness for Documentation/02-serializable-types.md, each capture showing one field (drop the other for the
// shot): Images/serializable-type-quick-start.gif picks _primaryWeapon, Images/serializable-type-missing.png stores a
// removed Game.Combat.Spear in it, Images/type-selector-required.png shows _secondaryWeapon empty with the inline
// "required" notice, and Images/type-selector-constraint-warning.png swaps both for
// [TypeSelector("Spear, Assembly-CSharp")] string _weaponName, whose type name resolves to nothing.
// Images/type-selector-member-constraint.gif swaps them for the page's SerializableType<Weapon> _weaponClass and
// [TypeSelector(nameof(_weaponClass), Allow = TypeAllow.None)] string _weaponName.
// Images/type-selector-display.png keeps only _primaryWeapon and decorates Sword in Weapons.cs with the page's
// [TypeSelectorDisplay(Name = "Longsword", Group = "Weapons/Melee", ...)] for the shot.
// Images/type-selector-window.png keeps only _primaryWeapon with Sword in Favorites and Axe, Bow in Recent, and
// Images/type-selector-generic.gif adds the page's Enchantment, Fire, Frost and Enchanted<T> : MeleeWeapon to
// Weapons.cs for the shot. Favorites and Recent live in EditorPrefs shared with every checkout: restore them after.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public sealed class WeaponMount : MonoBehaviour
    {
        [TypeSelector(Allow = TypeAllow.None)]
        [SerializeField] private SerializableType<Weapon> _primaryWeapon;

        [TypeSelector(typeof(Weapon), Required = true)]
        [SerializeField] private string _secondaryWeapon;
    }
}
