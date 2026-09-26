using UnityEngine;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // Kept out of SerializeReferenceTestFixtures.cs on purpose: a ScriptableObject asset reloads from disk only when
    // its class lives in a file of the same name, and the unsaved-asset repair tests reimport their probe.
    internal sealed class UnsavedRepairTestObject : ScriptableObject
    {
        [SerializeReference] public ITestWeapon a;
        [SerializeReference] public ITestWeapon b;
    }
}
