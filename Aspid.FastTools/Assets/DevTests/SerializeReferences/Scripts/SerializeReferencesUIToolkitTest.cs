using UnityEngine;
using Game.Gear;
using System.Collections.Generic;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.SerializeReferences
{
    // Dev-only UIToolkit counterpart to SerializeReferencesIMGUITest: identical fields, default inspector —
    // the reference rendering the forced-IMGUI component is compared against on
    // Prefabs/SerializeReferencesDevTest.prefab. Keep the two field lists in step.
    public sealed class SerializeReferencesUIToolkitTest : MonoBehaviour
    {
        [TypeSelector(Required = true)]
        [SerializeReference] private IWeapon _required;

        [TypeSelector]
        [SerializeReference] private IWeapon _sharedLeft;

        [TypeSelector]
        [SerializeReference] private IWeapon _sharedRight;

        [TypeSelector]
        [SerializeReference] private IWeapon _missing;

        [TypeSelector]
        [SerializeReference] private List<IWeapon> _missingInList;
    }
}
