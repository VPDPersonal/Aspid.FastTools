using UnityEngine;
using Aspid.FastTools.Types;

// Docs-media harness for Documentation/04-serialize-reference-selector.md: Images/aspid_fasttools_serialize_reference_selector.gif
// switches _primary from a Pistol with Damage 37 to Shotgun. The classes mirror the SerializeReferences sample's fields and
// initial values, which the page's tables quote, without its other Loadout fields.

// ReSharper disable once CheckNamespace
namespace Game.Weapons
{
    public sealed class Loadout : MonoBehaviour
    {
        [TypeSelector]
        [SerializeReference] private IWeapon _primary;
    }
}
