using System;
using System.Collections.Generic;
using UnityEngine;
using Aspid.FastTools.SerializeReferences;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Demonstrates [SerializeReferenceSelector] on references that live INSIDE plain [Serializable]
    // containers — a single container field and a List<T> of them — instead of directly on the component.
    //
    // Everything works at this depth exactly as for a top-level field: the type-picker dropdown, the inline
    // child properties of the chosen type, and the missing-type warning with its inline Fix. A renamed or
    // removed weapon type nested in a slot is detected and re-pointed in place (keeping its data), so the
    // asset-level Repair window is only needed for things the Inspector cannot reach at all.
    public sealed class SlottedLoadout : MonoBehaviour
    {
        // A plain [Serializable] container (NOT a managed reference itself) pairing a polymorphic weapon with
        // some metadata. The [SerializeReference] weapon inside it is still a full hierarchical picker.
        [Serializable]
        public sealed class WeaponSlot
        {
            public string label;

            [Min(0)] public int priority;

            // Polymorphic weapon nested one level inside the container — picker, inline fields and Fix all apply.
            [SerializeReference] [SerializeReferenceSelector]
            private IWeapon _weapon;
        }

        // A reference nested inside a single container field (path "_primarySlot._weapon").
        [SerializeField] private WeaponSlot _primarySlot = new();

        // References nested inside each element of a List of containers (path "_slots.Array.data[i]._weapon").
        [SerializeField] private List<WeaponSlot> _slots = new();
    }
}
