using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // The target the Loadout shoots at. Shrinks as health drops, tints while an effect is active and resets
    // when destroyed so the scene keeps running.
    public sealed class TrainingDummy : MonoBehaviour
    {
        [SerializeField] [Min(1)] private int _maxHealth = 500;

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

        public bool IsFrozen => Time.time < _freezeUntil;

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

        public void TakeDamage(int damage, string source)
        {
            _health -= damage;
            Debug.Log($"{source}: -{damage} → {Mathf.Max(_health, 0)} HP", this);

            if (_health > 0) return;
            Debug.Log("Dummy destroyed, resetting.", this);
            _health = _maxHealth;
            _burnUntil = _freezeUntil = 0f;
        }

        public void Burn(int damagePerSecond, float duration)
        {
            _burnPerSecond = damagePerSecond;
            _burnUntil = Time.time + duration;
        }

        public void Freeze(float slow, float duration)
        {
            _slow = slow;
            _freezeUntil = Time.time + duration;
        }
    }
}
