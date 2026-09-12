using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // The elite picker offers ArmoredGrunt when the regular enemy type is Grunt.
    /// <summary>
    /// <see cref="Enemy"/> that moves directly toward the arena center.
    /// </summary>
    public class Grunt : Enemy
    {
        /// <summary>
        /// Gets the color applied to the enemy renderer.
        /// </summary>
        protected override Color Tint => new(0.85f, 0.35f, 0.25f);

        /// <summary>
        /// Moves toward the arena center while preserving height.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time in seconds.</param>
        protected override void Move(float deltaTime) =>
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(0f, transform.position.y, 0f), Speed * deltaTime);
    }
}
