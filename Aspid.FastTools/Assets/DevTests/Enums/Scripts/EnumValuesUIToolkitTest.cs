using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Enums
{
    // Dev-only UIToolkit counterpart to EnumValuesIMGUITest: identical fields (reusing its nested
    // enum/struct types so the serialized data is interchangeable), default inspector — the
    // reference rendering the forced-IMGUI component is compared against.
    public sealed class EnumValuesUIToolkitTest : MonoBehaviour
    {
        [SerializeField] private EnumValues<float> _untypedSingleLine;

        [SerializeField] private EnumValues<EnumValuesIMGUITest.Element, Color> _typedSingleLine;

        [SerializeField] private EnumValues<EnumValuesIMGUITest.Element, EnumValuesIMGUITest.Profile> _typedFoldout;

        [SerializeField] private EnumValues<EnumValuesIMGUITest.Modifier, float> _typedFlags;
    }
}
