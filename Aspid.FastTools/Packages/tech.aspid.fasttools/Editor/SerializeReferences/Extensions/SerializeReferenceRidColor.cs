using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // Deterministic colors for shared-reference visuals, both entry points sharing one green->magenta palette.
    // ForRid hashes the rid, for the window's SHARED chip where there is no per-object badge; ForIndex walks the
    // badge number around the band instead, so consecutive badges are maximally separated.
    internal static class SerializeReferenceRidColor
    {
        // Adding this fraction per step drops each next hue into the largest remaining gap.
        private const float GoldenRatioConjugate = 0.618033988749895f;

        // Rid colors must never read as the inspector's error (red) or warning (yellow), so the hue is remapped into
        // a green->magenta band; the remap is linear, so it keeps the spread.
        private const float SafeHueMin = 0.32f; // green — past yellow-green/lime
        private const float SafeHueMax = 0.90f; // magenta/pink — before it reddens

        private const float Saturation = 0.55f;

        // HSV is not perceptually uniform — a fixed value makes greens glow "acid" — so each hue's value is scaled
        // to hit a common perceived luminance instead.
        private const float TargetLuminance = 0.6f;

        // A hue the eye sees as dark is lifted toward mid-brightness, never blown out chasing the target.
        private const float MaxValue = 0.92f;

        // A Knuth multiplicative hash spreads the rid across the hue circle before the golden-ratio rotation.
        public static Color ForRid(long rid)
        {
            var hash = unchecked((uint)(rid * 2654435761));
            var fraction = (hash / (float)uint.MaxValue + GoldenRatioConjugate * (hash & 0xFF)) % 1f;
            return FromFraction(fraction);
        }

        // The 1-based badge number, so its color and its number stay in lock-step.
        public static Color ForIndex(int index)
        {
            var fraction = (index * GoldenRatioConjugate) % 1f;
            return FromFraction(fraction);
        }

        // Hue into the safe green→magenta band, value normalized to the common perceived luminance.
        private static Color FromFraction(float fraction)
        {
            var hue = SafeHueMin + fraction * (SafeHueMax - SafeHueMin);

            // Luminance is linear in HSV's value for a fixed hue, so measure this hue at full value and take the
            // value that lands it on the target.
            var full = Color.HSVToRGB(hue, Saturation, 1f);
            var luminance = 0.2126f * full.r + 0.7152f * full.g + 0.0722f * full.b;
            var value = Mathf.Min(TargetLuminance / luminance, MaxValue);

            return Color.HSVToRGB(hue, Saturation, value);
        }
    }
}
