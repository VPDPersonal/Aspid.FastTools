using System;
using Aspid.FastTools.Ids;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Ids
{
    // Dev-only id struct backing the Ids IMGUI harness — NOT part of the package or its samples.
    // IdStructGenerator emits __stringId, _id and the Id property; the registry asset lives at
    // Assets/DevTests/Ids/IdRegistry_DevEnemyId.asset.
    [Serializable]
    public partial struct DevEnemyId : IId { }
}
