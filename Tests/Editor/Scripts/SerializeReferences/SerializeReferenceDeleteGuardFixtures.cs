using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    // Fixtures for SerializeReferenceDeleteGuardTests. No type here is named after the file, so MonoScript.GetClass()
    // resolves nothing — the "several small implementations in one file" layout the delete guard must still cover.

    [Serializable]
    internal sealed class DeleteGuardPistol : ITestWeapon { }

    [Serializable]
    internal sealed class DeleteGuardRifle : ITestWeapon { }

    // Never a stored managed-reference type itself.
    internal abstract class DeleteGuardWeaponBase : ITestWeapon { }

    // A ScriptableObject is never a managed reference, but a type nested in it is (stored as "DeleteGuardArmory/Crate").
    internal sealed class DeleteGuardArmory : ScriptableObject
    {
        [Serializable]
        internal sealed class Crate : ITestWeapon { }
    }
}
