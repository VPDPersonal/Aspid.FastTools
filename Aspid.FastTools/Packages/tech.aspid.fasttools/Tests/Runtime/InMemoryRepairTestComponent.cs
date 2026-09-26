using System;
using UnityEngine;

namespace Aspid.FastTools.Tests
{
    // A scene component for the in-memory missing-type repair tests. It lives in a runtime assembly and in its own file
    // because a scene only serializes a non-editor MonoBehaviour whose script file carries the class name.
    public sealed class InMemoryRepairTestComponent : MonoBehaviour
    {
        [SerializeReference] public object value;
    }

    // Saved under this name, then renamed in the scene file so the reference loads as a missing type.
    [Serializable]
    public sealed class InMemoryRepairPayload
    {
        public int x;
    }

    [Serializable]
    public sealed class InMemoryRepairReplacement
    {
        public int x;
    }
}
