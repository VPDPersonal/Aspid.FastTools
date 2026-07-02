using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // A plain [Serializable] container — NOT a managed reference itself — pairing a polymorphic
    // weapon with some metadata. The [SerializeReference] weapon inside it is still a full
    // hierarchical picker (TUTORIAL.md, Lesson 7). Shared by TypeSelectorTutorial and SlottedLoadout.
    [Serializable]
    public sealed class WeaponSlot
    {
        public string label = "Slot";

        [Min(0)] public int priority;

        [SerializeReference] [TypeSelector]
        private IWeapon _weapon;
    }
}
