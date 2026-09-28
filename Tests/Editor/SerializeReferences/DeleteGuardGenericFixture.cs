using System;

namespace Aspid.FastTools.SerializeReferences.Editors
{
    namespace Tests
    {
        // A generic namesake of DeleteGuardPistol, declared in nested namespace blocks: deleting this script covers only
        // DeleteGuardPistol<T>, and deleting the fixtures script only DeleteGuardPistol (SerializeReferenceDeleteGuardTests).
        [Serializable]
        internal sealed class DeleteGuardPistol<T> : ITestWeapon { }
    }
}
