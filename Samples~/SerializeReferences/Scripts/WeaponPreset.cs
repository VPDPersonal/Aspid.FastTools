using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // ScriptableObject host for [TypeSelector], used to demonstrate the missing-type repair flow.
    //
    // Why a ScriptableObject and not the Loadout MonoBehaviour? Unity preserves a managed reference whose type
    // went missing (renamed / moved / deleted) only on ScriptableObject assets — on GameObjects and prefabs the
    // reference is silently dropped to null on load (Unity bug UUM-129100). The "Fix" action that rewrites
    // the stored type therefore only has something to repair on assets like this one.
    //
    // See the bundled BrokenWeaponPreset.asset: its _weapon points at a type that does not exist (GhostWeapon),
    // so the Inspector shows a "Missing type" warning with a "Fix" button — set the class back to "Pistol"
    // to recover the reference and its data.
    [CreateAssetMenu(menuName = "Aspid/FastTools Samples/Weapon Preset", fileName = "WeaponPreset")]
    public sealed class WeaponPreset : ScriptableObject
    {
        [SerializeReference] [TypeSelector]
        private IWeapon _weapon;

        [SerializeReference] [TypeSelector]
        private List<IWeapon> _alternates = new();
    }
}
