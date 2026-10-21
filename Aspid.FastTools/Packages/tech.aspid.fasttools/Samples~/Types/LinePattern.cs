using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.Types
{
    [TypeSelectorDisplay(Name = "Line", Group = "Spawn Patterns", Icon = "d_BoxCollider Icon")]
    public sealed class LinePattern : ISpawnPattern
    {
        public Vector3 GetPosition(int index, int count) =>
            new Vector3(x: (index - (count - 1) / 2f) * 2f, y: 0f, z: 0f);
    }
}
