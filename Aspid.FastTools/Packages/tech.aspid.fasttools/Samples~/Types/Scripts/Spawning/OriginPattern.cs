using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // A test double that stacks everything at the center. Hidden keeps it out of the picker while code
    // (and any scene that already stores it) can still use it.
    [TypeSelectorDisplay(Hidden = true)]
    public sealed class OriginPattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius) => Vector3.zero;
    }
}
