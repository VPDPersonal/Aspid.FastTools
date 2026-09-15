using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    /// <summary>
    /// <see cref="Archer"/> with a taller, narrower body and a darker tint.
    /// </summary>
    public sealed class Sniper : Archer
    {
        /// <summary>
        /// Gets the color applied to the enemy renderer.
        /// </summary>
        protected override Color Tint => new(0.15f, 0.45f, 0.25f);

        /// <summary>
        /// Scales the body before applying its ground placement and tint.
        /// </summary>
        protected override void Start()
        {
            transform.localScale = new Vector3(0.6f, 1.6f, 0.6f);
            base.Start();
        }
    }
}
