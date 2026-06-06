using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.SerializeReferences;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Same fields as Loadout, but the companion editor (IMGUILoadoutEditor) overrides
    // OnInspectorGUI without CreateInspectorGUI, forcing the entire inspector — and every
    // nested [SerializeReferenceSelector] field — through the IMGUI path
    // (SerializeReferenceIMGUIPropertyDrawer) instead of the UIToolkit one.
    //
    // Use this to verify both rendering paths stay visually and behaviourally aligned.
    public sealed class IMGUILoadout : MonoBehaviour
    {
        [SerializeReference] [SerializeReferenceSelector]
        private IWeapon _primaryWeapon;

        [SerializeReference] [SerializeReferenceSelector]
        private List<IWeapon> _sidearms = new();

        [SerializeReference] [SerializeReferenceSelector]
        private StatusEffect _onHitEffect;
    }
}
