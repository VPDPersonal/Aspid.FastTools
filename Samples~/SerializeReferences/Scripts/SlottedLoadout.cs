using System.Collections.Generic;
using UnityEngine;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Demonstrates [TypeSelector] on references that live INSIDE plain [Serializable]
    // containers (the shared WeaponSlot.cs) — a single container field and a List<T> of them —
    // instead of directly on the component. See TUTORIAL.md, Lesson 7: everything (picker,
    // inline child fields, missing-type Fix) works at this depth exactly as for a top-level field.
    public sealed class SlottedLoadout : MonoBehaviour
    {
        // A reference nested inside a single container field (path "_primarySlot._weapon").
        [SerializeField] private WeaponSlot _primarySlot = new();

        // References nested inside each element of a List of containers (path "_slots.Array.data[i]._weapon").
        [SerializeField] private List<WeaponSlot> _slots = new();
    }
}
