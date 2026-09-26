using UnityEngine;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A script declaring only a UnityEngine.Object type, which can never be a managed reference: deleting it must not
    // start the project sweep (SerializeReferenceDeleteGuardTests).
    internal sealed class DeleteGuardSettingsFixture : ScriptableObject { }
}
