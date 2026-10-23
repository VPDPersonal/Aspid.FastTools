using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.Editors.SerializeReferences
{
    // A second weapon type for SerializeReferenceWriterTests. In its own file because a dropped script resolves its
    // class only when the file carries the class name.
    [Serializable]
    internal sealed class WriterTestBow : ITestWeapon
    {
        public int damage;
    }
}
