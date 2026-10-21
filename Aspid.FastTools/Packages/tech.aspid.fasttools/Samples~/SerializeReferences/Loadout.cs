using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    public sealed class Loadout : ScriptableObject
    {
        [TypeSelector]
        [SerializeReference] private IWeapon _primary;

        [TypeSelector]
        [SerializeReference] private List<IWeapon> _sidearms = new();

        public IWeapon Primary => _primary;

        public IReadOnlyList<IWeapon> Sidearms => _sidearms;
    }
}
