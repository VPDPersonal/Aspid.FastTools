using System;
using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Same fields as SlottedLoadout, but the companion editor (IMGUISlottedLoadoutEditor)
    // overrides OnInspectorGUI without CreateInspectorGUI, forcing the whole inspector —
    // and every nested [TypeSelector] field inside the [Serializable] containers — through
    // the IMGUI path (SerializeReferenceIMGUIPropertyDrawer) instead of the UIToolkit one.
    //
    // Use this to verify both rendering paths stay aligned even when the references live
    // one level inside a container rather than directly on the component.
    public sealed class IMGUISlottedLoadout : MonoBehaviour
    {
        [Serializable]
        public sealed class WeaponSlot
        {
            public string label;

            [Min(0)] public int priority;

            [SerializeReference] [TypeSelector]
            private IWeapon _weapon;
        }

        [SerializeField] private WeaponSlot _primarySlot = new();

        [SerializeField] private List<WeaponSlot> _slots = new();
    }
}
