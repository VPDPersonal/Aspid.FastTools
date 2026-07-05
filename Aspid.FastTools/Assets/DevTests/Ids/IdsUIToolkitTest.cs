using System;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Ids
{
    // Dev-only UIToolkit counterpart to IdsIMGUITest: identical fields, default inspector —
    // the reference rendering the forced-IMGUI component is compared against.
    public sealed class IdsUIToolkitTest : MonoBehaviour
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
