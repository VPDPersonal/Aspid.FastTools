using System;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    // A second weapon type for SerializeReferenceWriterTests. In its own file because a dropped script resolves its
    // class only when the file carries the class name.
    [Serializable]
    internal sealed class WriterTestBow : ITestWeapon
    {
        public int damage;
    }
}
