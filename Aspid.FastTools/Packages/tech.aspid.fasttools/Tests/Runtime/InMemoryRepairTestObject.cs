using UnityEngine;

namespace Aspid.FastTools.Tests
{
    // An asset for the in-memory missing-type repair tests. In its own file because an asset reloads its
    // ScriptableObject only when the script file carries the class name.
    public sealed class InMemoryRepairTestObject : ScriptableObject
    {
        [SerializeReference] public object value;

        // A field that can share a reference with value.
        [SerializeReference] public IInMemoryRepairShape shape;
    }
}
