using System;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // DeleteGuardShotgun has another part in DeleteGuardPartialFixture.Part.cs, so deleting one file keeps it;
    // DeleteGuardRevolver has no other part and can be a stored managed reference (SerializeReferenceDeleteGuardTests).
    [Serializable]
    internal sealed partial class DeleteGuardShotgun { }

    [Serializable]
    internal sealed partial class DeleteGuardRevolver : ITestWeapon { }
}
