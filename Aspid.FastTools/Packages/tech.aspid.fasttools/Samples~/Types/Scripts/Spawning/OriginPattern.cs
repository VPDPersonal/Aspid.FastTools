using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Spawning
{
    [TypeSelectorDisplay(Hidden = true)]
    public sealed class OriginPattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius) =>
            Vector3.zero;
    }
}
