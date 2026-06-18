using UnityEngine;
using Aspid.FastTools.UIElements.Editors.Internal;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// Status-driven tones for the windows' animated dots canvas. The whole dotted backdrop takes a single dim,
    /// desaturated hue that reflects the view's current state — blue while idle/informational, green when clean,
    /// amber when something needs attention, red on failure — so the canvas reads as a calm status wash rather than
    /// the component's default green→amber→red gradient (which implied a good→bad scale that mapped to nothing).
    /// </summary>
    internal static class SerializeReferenceCanvasStyle
    {
        /// <summary>Idle / informational backdrop (no asset, empty graph, canceled scan).</summary>
        public static readonly Color Info = new(0.15f, 0.21f, 0.30f);

        /// <summary>Healthy backdrop (a clean graph, a clean project).</summary>
        public static readonly Color Success = new(0.16f, 0.28f, 0.21f);

        /// <summary>Attention backdrop (missing references / orphans present).</summary>
        public static readonly Color Warning = new(0.30f, 0.25f, 0.12f);

        /// <summary>Failure backdrop, reserved for hard-error states.</summary>
        public static readonly Color Error = new(0.32f, 0.16f, 0.16f);

        // The component's own default blob colours (the green→amber→red "traffic light"), mirroring the
        // --aspid-colors-status-{success,warning,error}-text-dark palette tokens that the canvas USS resolves to.
        // Used to restore the multi-tone gradient on the shared canvas for a screen that carries no single status.
        private static readonly Color SignalSuccess = new(85f / 255f, 175f / 255f, 100f / 255f);
        private static readonly Color SignalWarning = new(185f / 255f, 135f / 255f, 60f / 255f);
        private static readonly Color SignalError = new(185f / 255f, 65f / 255f, 65f / 255f);

        /// <summary>
        /// Paints every blob of <paramref name="background"/> the one <paramref name="tone"/>. Set inline (via the
        /// component's <c>SetColorN</c>) so it wins over the component's USS defaults — a preset/constructor colour is
        /// not flagged inline and would be overwritten when the stylesheet resolves.
        /// </summary>
        public static void SetTone(this AspidAnimatedDotsBackground background, Color tone) =>
            background.SetColor1(tone).SetColor2(tone).SetColor3(tone);

        /// <summary>
        /// Restores the green→amber→red "traffic light" gradient (the component's default three-blob look). Once a
        /// view has toned the shared canvas to a single colour via <see cref="SetTone"/>, the inline override hides
        /// the USS defaults; this re-applies them as explicit inline colours so a no-status screen (the Welcome home
        /// tab) reads as the multi-tone gradient again rather than one flat colour.
        /// </summary>
        public static void SetSignalGradient(this AspidAnimatedDotsBackground background) =>
            background.SetColor1(SignalSuccess).SetColor2(SignalWarning).SetColor3(SignalError);
    }
}
