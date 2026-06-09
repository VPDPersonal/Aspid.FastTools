using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Same fields as Loadout, but the companion editor (IMGUILoadoutEditor) overrides
    // OnInspectorGUI without CreateInspectorGUI, forcing the entire inspector — and every
    // nested [TypeSelector] field — through the IMGUI path
    // (SerializeReferenceIMGUIPropertyDrawer) instead of the UIToolkit one.
    //
    // Use this to verify both rendering paths stay visually and behaviourally aligned.
    public sealed class IMGUILoadout : MonoBehaviour
    {
        [SerializeReference] [TypeSelector]
        private IWeapon _primaryWeapon;

        [SerializeReference] [TypeSelector]
        private List<IWeapon> _sidearms = new();

        [SerializeReference] [TypeSelector]
        private StatusEffect _onHitEffect;

        [SerializeReference] [TypeSelector]
        private IModifier _modifier;

        [SerializeReference] [TypeSelector]
        private Modifier<float> _floatModifier;

        [SerializeReference] [TypeSelector]
        private List<IModifier> _modifiers = new();
    }
}
