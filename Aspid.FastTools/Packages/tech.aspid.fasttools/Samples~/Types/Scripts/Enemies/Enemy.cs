using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // ComponentTypeSelector swaps the component subtype while preserving shared serialized fields.
    /// <summary>
    /// <see cref="MonoBehaviour"/> that colors, moves and expires a spawned enemy.
    /// </summary>
    public abstract class Enemy : MonoBehaviour
    {
        [Tooltip("Enemy subtype used by this component.")]
        [SerializeField] private ComponentTypeSelector _kind;

        [Tooltip("Health value shown in the enemy description.")]
        [SerializeField, Min(1f)] private float _health = 100f;

        [Tooltip("Movement speed in world units per second.")]
        [SerializeField, Min(0.1f)] private float _speed = 3f;

        [Tooltip("Seconds before the enemy is removed.")]
        [SerializeField, Min(1f)] private float _lifetime = 12f;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        private float _age;
        private MaterialPropertyBlock _block;

        /// <summary>
        /// Gets movement speed in world units per second.
        /// </summary>
        protected float Speed => _speed;

        /// <summary>
        /// Gets the color applied to the enemy renderer.
        /// </summary>
        protected abstract Color Tint { get; }

        /// <summary>
        /// Called before the first frame update. Override to customize initial placement and appearance.
        /// </summary>
        /// <remarks>Overrides must call the base implementation to apply ground placement and tint.</remarks>
        protected virtual void Start()
        {
            // Capsule pivots are centered; keep the feet on the arena after subtype scaling.
            transform.position -= Vector3.up * GetComponent<Renderer>().bounds.min.y;
            _block = new MaterialPropertyBlock();
            _block.SetColor(_baseColorId, Tint);
            _block.SetColor(_colorId, Tint);
            GetComponent<Renderer>().SetPropertyBlock(_block);
        }

        private void Update()
        {
            _age += Time.deltaTime;
            if (_age >= _lifetime)
            {
                Destroy(gameObject);
                return;
            }

            Move(Time.deltaTime);
        }

        /// <summary>
        /// Called each frame before expiration. Override to move the enemy.
        /// </summary>
        /// <param name="deltaTime">Elapsed frame time in seconds.</param>
        protected abstract void Move(float deltaTime);

        /// <summary>
        /// Returns the enemy type and health.
        /// </summary>
        /// <returns>Enemy description.</returns>
        public override string ToString() =>
            $"{GetType().Name} (HP {_health})";
    }
}
