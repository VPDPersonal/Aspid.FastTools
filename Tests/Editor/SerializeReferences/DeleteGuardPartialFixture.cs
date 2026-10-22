using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    // DeleteGuardShotgun has another part in DeleteGuardPartialFixture.Part.cs, so deleting one file keeps it;
    // DeleteGuardRevolver has no other part and can be a stored managed reference (SerializeReferenceDeleteGuardTests).
    [Serializable]
    internal sealed partial class DeleteGuardShotgun { }

    [Serializable]
    internal sealed partial class DeleteGuardRevolver : ITestWeapon { }
}
