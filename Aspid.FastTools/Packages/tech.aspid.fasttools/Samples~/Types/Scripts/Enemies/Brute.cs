using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    /// <summary>
    /// <see cref="Enemy"/> that bobs while moving toward the arena center at half speed.
    /// </summary>
    public sealed class Brute : Enemy
    {
        [Tooltip("Vertical bobbing amplitude while moving.")]
        [SerializeField, Min(0f)] private float _stompHeight = 0.4f;

        /// <summary>
        /// Gets the color applied to the enemy renderer.
        /// </summary>
        protected override Color Tint => new(0.5f, 0.35f, 0.75f);

        /// <summary>
        /// Applies ground placement and tint, then enlarges the body.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            transform.localScale = Vector3.one * 2f;
        }

        /// <summary>
        /// Moves toward the arena center at half speed with vertical bobbing.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time in seconds.</param>
        protected override void Move(float deltaTime)
        {
            var position = Vector3.MoveTowards(transform.position, Vector3.zero, Speed * 0.5f * deltaTime);
            position.y = 1f + Mathf.Abs(Mathf.Sin(Time.time * 4f)) * _stompHeight;
            transform.position = position;
        }
    }
}
