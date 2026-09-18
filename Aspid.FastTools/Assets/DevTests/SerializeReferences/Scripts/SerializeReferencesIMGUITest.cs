using UnityEngine;
using Game.Gear;
using System.Collections.Generic;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.SerializeReferences
{
    // Dev-only harness for the notice states of the SerializeReference drawers — NOT part of the package
    // or its samples, which cannot ship a deliberately broken asset. The SerializeReferences sample covers
    // ordinary selection in both UI paths; what is left is what only a hand-authored asset can produce.
    //
    // The companion editor (Editor/SerializeReferencesIMGUITestEditor.cs) forces the whole inspector
    // through IMGUI, so the notices come from SerializeReferenceIMGUIPropertyDrawer and InspectorNoticeGUI
    // instead of the InspectorNotice element.
    //
    // To test: open Prefabs/SerializeReferencesDevTest.prefab — this component sits next to
    // SerializeReferencesUIToolkitTest, which carries the same four states in the default inspector.
    public sealed class SerializeReferencesIMGUITest : MonoBehaviour
    {
        // Left null on the prefab: the "required reference is not set" notice.
        [TypeSelector(Required = true)]
        [SerializeReference] private IWeapon _required;

        // Both fields hold one instance on the prefab: the "shared reference" notice and Make unique.
        [TypeSelector]
        [SerializeReference] private IWeapon _sharedLeft;

        [TypeSelector]
        [SerializeReference] private IWeapon _sharedRight;

        // The prefab stores a class name no assembly resolves: the missing-type notice with Fix and
        // Smart Fix, whose suggestion comes from the one-letter difference to Pistol.
        [TypeSelector]
        [SerializeReference] private IWeapon _missing;

        // The same break inside a list, where the element notice and the list guard apply instead.
        [TypeSelector]
        [SerializeReference] private List<IWeapon> _missingInList;
    }
}
