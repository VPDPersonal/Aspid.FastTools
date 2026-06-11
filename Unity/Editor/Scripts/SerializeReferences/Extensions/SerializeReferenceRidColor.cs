using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Deterministic per-rid colour helper shared by the inspector field stripe/chip and the Managed
    /// References window SHARED chip, so the same rid always renders the same colour across both surfaces.
    /// </summary>
    internal static class SerializeReferenceRidColor
    {
        // Golden-ratio hue rotation: a Knuth multiplicative hash spreads the integer rid across the full
        // hue circle, and adding the golden-ratio conjugate fraction of its low byte pushes consecutive
        // ids further apart. Saturation/value are fixed so the chip reads legibly on the dark inspector
        // and the Managed References window's dark canvas alike.
        private const float GoldenRatioConjugate = 0.618033988749895f;

        /// <summary>
        /// Returns a deterministic, visually distinct colour for <paramref name="rid"/>. The same rid
        /// always maps to the same colour; nearby rids are separated by the golden-ratio hue rotation.
        /// </summary>
        public static Color ForRid(long rid)
        {
            var hash = unchecked((uint)(rid * 2654435761));
            var hue = (hash / (float)uint.MaxValue + GoldenRatioConjugate * (hash & 0xFF)) % 1f;
            return Color.HSVToRGB(hue, 0.55f, 0.85f);
        }
    }
}
