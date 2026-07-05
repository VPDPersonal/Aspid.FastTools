using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Ids
{
    // Dev-only harness for the IMGUI rendering path of the IId struct drawer — NOT part of the
    // package or its samples. The companion editor (Editor/IdsIMGUITestEditor.cs) forces the
    // whole inspector through IMGUI, routing every field below through
    // IdStructIMGUIPropertyDrawer instead of the UIToolkit path.
    //
    // To test: open Prefabs — IdsDevTest.prefab carries this component next to IdsUIToolkitTest
    // (same fields, default UIToolkit inspector) so both paths sit in one Inspector for
    // side-by-side comparison.
    public sealed class IdsIMGUITest : MonoBehaviour
    {
        [Serializable]
        public sealed class Nested
        {
            [SerializeField] private DevEnemyId _id;
        }

        [SerializeField] private DevEnemyId _single;
        [SerializeField] private DevEnemyId[] _array;
        [SerializeField] private Nested _nested;
    }
}
