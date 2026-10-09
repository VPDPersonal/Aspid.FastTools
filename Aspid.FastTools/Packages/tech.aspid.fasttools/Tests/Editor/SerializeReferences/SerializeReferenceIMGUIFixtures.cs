using System;
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Shared fixture types of the IMGUI drawer tests (height, draw and drop).

    // A second concrete type, so a multi-object selection can hold different types.
    [Serializable]
    internal sealed class IMGUIShield : ITestWeapon { }

    // A value with a managed-reference field of its own: the drawer paints that child with its own header.
    [Serializable]
    internal sealed class IMGUINestingWeapon : ITestWeapon
    {
        public int level;
        [SerializeReference] public ITestWeapon inner;
    }

    // The attribute routes Unity's own IMGUI path (PropertyField, GetPropertyHeight) into the drawer.
    internal sealed class IMGUIDrawerTestObject : ScriptableObject
    {
        [TypeSelector] [SerializeReference] public ITestWeapon weapon;
        [TypeSelector] [SerializeReference] public List<ITestWeapon> weapons = new();
    }
}
