using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    // The file name matches the class name on purpose.
    // A MonoScript reports its class only then, and a drop carries only script assets.
    [Serializable]
    internal sealed class IMGUIDropWeapon : ITestWeapon { }
}
