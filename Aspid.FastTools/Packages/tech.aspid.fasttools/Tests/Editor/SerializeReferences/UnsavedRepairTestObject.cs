using System;
using UnityEngine;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Kept out of SerializeReferenceTestFixtures.cs on purpose: a ScriptableObject asset reloads from disk only when
    // its class lives in a file of the same name, and the unsaved-asset repair tests reimport their probe.
    internal sealed class UnsavedRepairTestObject : ScriptableObject
    {
        [SerializeReference] public ITestWeapon a;
        [SerializeReference] public ITestWeapon b;

        // A plain string field: the target of the required-type route the open-copy tests drive.
        public string requiredName = string.Empty;
    }

    // A sequence the in-memory recovery skips, so only the lossless file route keeps it.
    [Serializable]
    internal sealed class UnsavedRepairSpawnAction : ITestWeapon
    {
        public float delay;
        public List<int> waves = new();
    }
}
