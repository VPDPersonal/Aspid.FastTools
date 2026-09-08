using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> that visualizes damage and status effects and resets when defeated.
    /// </summary>
    public sealed class TrainingDummy : MonoBehaviour
    {
        [Tooltip("Health restored when the dummy is defeated.")]
        [SerializeField, Min(1)] private int _maxHealth = 500;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        private int _health;
        private float _burnUntil;
        private int _burnPerSecond;
        private float _burnAccumulator;
        private float _freezeUntil;
        private float _slow;
        private Renderer _renderer;
        private MaterialPropertyBlock _block;

        /// <summary>
        /// Gets a value indicating whether the slowing effect is active.
        /// </summary>
        public bool IsFrozen => Time.time < _freezeUntil;

        /// <summary>
        /// Gets the attack-interval multiplier increment, or zero when the effect has expired.
        /// </summary>
        public float Slow => IsFrozen ? _slow : 0f;

        private void Awake()
        {
            _health = _maxHealth;
            _renderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (Time.time < _burnUntil)
            {
                _burnAccumulator += _burnPerSecond * Time.deltaTime;
                var whole = Mathf.FloorToInt(_burnAccumulator);
                if (whole > 0)
                {
                    _burnAccumulator -= whole;
                    TakeDamage(whole, "burn");
                }
            }

            var t = (float)_health / _maxHealth;
            transform.localScale = Vector3.one * Mathf.Lerp(0.4f, 2f, t);

            var color = Time.time < _burnUntil ? new Color(1f, 0.45f, 0.1f)
                : IsFrozen ? new Color(0.4f, 0.8f, 1f)
                : Color.Lerp(new Color(0.6f, 0.1f, 0.1f), new Color(0.8f, 0.8f, 0.8f), t);
            _block.SetColor(_baseColorId, color);
            _block.SetColor(_colorId, color);
            _renderer.SetPropertyBlock(_block);
        }

        /// <summary>
        /// Subtracts health and resets the dummy when defeated.
        /// </summary>
        /// <param name="damage">Health to subtract.</param>
        /// <param name="source">Attack name included in the log message.</param>
        public void TakeDamage(int damage, string source)
        {
            _health -= damage;
            Debug.Log($"{source}: -{damage} → {Mathf.Max(_health, 0)} HP", this);

            if (_health > 0)
                return;

            Debug.Log("Dummy destroyed, resetting.", this);
            _health = _maxHealth;
            _burnUntil = _freezeUntil = 0f;
        }

        /// <summary>
        /// Replaces the active burn damage and expiration time.
        /// </summary>
        /// <param name="damagePerSecond">Damage accumulated each second.</param>
        /// <param name="duration">Burn duration in seconds.</param>
        public void Burn(int damagePerSecond, float duration)
        {
            _burnPerSecond = damagePerSecond;
            _burnUntil = Time.time + duration;
        }

        /// <summary>
        /// Replaces the active slowing amount and expiration time.
        /// </summary>
        /// <param name="slow">Fraction added to the interval between loadout attacks.</param>
        /// <param name="duration">Slowing duration in seconds.</param>
        public void Freeze(float slow, float duration)
        {
            _slow = slow;
            _freezeUntil = Time.time + duration;
        }
    }
}
