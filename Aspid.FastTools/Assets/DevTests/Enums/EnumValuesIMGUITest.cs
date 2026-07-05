using System;
using UnityEngine;
using Aspid.FastTools.Enums;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Enums
{
    // Dev-only harness for the IMGUI rendering path of EnumValues drawers — NOT part of the
    // package or its samples. The companion editor (Editor/EnumValuesIMGUITestEditor.cs) forces
    // the whole inspector through IMGUI, routing every field below through
    // EnumValuesIMGUIPropertyDrawer/EnumValueIMGUIPropertyDrawer instead of the UIToolkit path.
    //
    // To test: drop this component on any GameObject in the dev project and compare against a
    // plain component with the same fields (default UIToolkit inspector).
    public sealed class EnumValuesIMGUITest : MonoBehaviour
    {
        public enum Element
        {
            Fire,
            Water,
            Earth,
        }

        [Flags]
        public enum Modifier
        {
            None = 0,
            Burning = 1 << 0,
            Frozen = 1 << 1,
            Slowed = 1 << 2,
        }

        // Multi-field value → the drawer's foldout path (own "Value" line under the key).
        [Serializable]
        public struct Profile
        {
            public float Damage;
            public Color Tint;
            public AnimationCurve Falloff;
        }

        // Untyped variant with the type-picker row in the header.
        [SerializeField] private EnumValues<float> _untypedSingleLine;

        // Typed variant, single-line values → the two-column row layout.
        [SerializeField] private EnumValues<Element, Color> _typedSingleLine;

        // Typed variant, multi-field values → the foldout row layout.
        [SerializeField] private EnumValues<Element, Profile> _typedFoldout;

        // [Flags] key handling.
        [SerializeField] private EnumValues<Modifier, float> _typedFlags;
    }
}
