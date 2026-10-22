using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors
{
    namespace SerializeReferences
    {
        // A generic namesake of DeleteGuardPistol, declared in nested namespace blocks: deleting this script covers only
        // DeleteGuardPistol<T>, and deleting the fixtures script only DeleteGuardPistol (SerializeReferenceDeleteGuardTests).
        [Serializable]
        internal sealed class DeleteGuardPistol<T> : ITestWeapon { }
    }
}
