using UnityEngine;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A script declaring only a UnityEngine.Object type, which can never be a managed reference: deleting it must not
    // start the project sweep (SerializeReferenceDeleteGuardTests). Naming class DeleteGuardPistol in this comment and
    // DeleteGuardRifle in the string below declares neither.
    internal sealed class DeleteGuardSettingsFixture : ScriptableObject
    {
        private const string Note = "sealed class DeleteGuardRifle : ITestWeapon";
    }
}
