using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness: Required = true demo for the Types.md screenshot — _primaryWeapon is
// filled in the shot, _secondaryWeapon stays empty to show the inline "required" notice.

// ReSharper disable once CheckNamespace
namespace Game.Combat
{
    public sealed class WeaponMount : MonoBehaviour
    {
        [TypeSelector(typeof(Weapon))]
        [SerializeField] private string _primaryWeapon;

        [TypeSelector(typeof(Weapon), Required = true)]
        [SerializeField] private string _secondaryWeapon;
    }
}
