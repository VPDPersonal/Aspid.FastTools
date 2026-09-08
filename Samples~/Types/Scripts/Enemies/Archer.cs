using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    /// <summary>
    /// <see cref="Enemy"/> that orbits the arena center at a preferred distance.
    /// </summary>
    public class Archer : Enemy
    {
        [Tooltip("Preferred distance from the arena center.")]
        [SerializeField, Min(1f)] private float _keepDistance = 6f;

        /// <summary>
        /// Gets the color applied to the enemy renderer.
        /// </summary>
        protected override Color Tint => new(0.3f, 0.7f, 0.4f);

        /// <summary>
        /// Moves tangentially while approaching the preferred orbit distance.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time in seconds.</param>
        protected override void Move(float deltaTime)
        {
            var position = transform.position;
            var toCenter = new Vector3(-position.x, 0f, -position.z);
            var distance = toCenter.magnitude;
            if (distance < 0.01f)
                return;

            var radial = toCenter / distance * Mathf.Clamp(distance - _keepDistance, -1f, 1f);
            var tangent = Vector3.Cross(Vector3.up, toCenter / distance);
            transform.position = position + (radial + tangent) * (Speed * deltaTime);
        }
    }
}
