using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    [TypeSelectorDisplay(Hidden = true)]
    public sealed class OriginPattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count, float radius) =>
            Vector3.zero;
    }
}
