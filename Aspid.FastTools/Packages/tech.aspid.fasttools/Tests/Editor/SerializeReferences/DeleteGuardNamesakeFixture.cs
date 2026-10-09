using System;

// A global-namespace namesake of DeleteGuardPistol and a class named in Cyrillic: deleting this script covers exactly
// those two, and deleting the fixtures script never the global DeleteGuardPistol (SerializeReferenceDeleteGuardTests).
[Serializable]
internal sealed class DeleteGuardPistol { }

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    [Serializable]
    internal sealed class ОружиеDeleteGuard : ITestWeapon { }
}
