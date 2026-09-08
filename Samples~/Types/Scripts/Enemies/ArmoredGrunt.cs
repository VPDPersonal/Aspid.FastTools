using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    /// <summary>
    /// <see cref="Grunt"/> with a larger body and an armor value for display.
    /// </summary>
    public sealed class ArmoredGrunt : Grunt
    {
        [Tooltip("Armor value shown in the enemy description.")]
        [SerializeField, Min(0f)] private float _armor = 50f;

        /// <summary>
        /// Gets the color applied to the enemy renderer.
        /// </summary>
        protected override Color Tint => new(0.55f, 0.2f, 0.15f);

        /// <summary>
        /// Scales the body before applying its ground placement and tint.
        /// </summary>
        protected override void Start()
        {
            transform.localScale = Vector3.one * 1.4f;
            base.Start();
        }

        /// <summary>
        /// Returns the enemy type and health, followed by its armor value.
        /// </summary>
        /// <returns>Enemy description.</returns>
        public override string ToString() =>
            $"{base.ToString()}, armor {_armor}";
    }
}
