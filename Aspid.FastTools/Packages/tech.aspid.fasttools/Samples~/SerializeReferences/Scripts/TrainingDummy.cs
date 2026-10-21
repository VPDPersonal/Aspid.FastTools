using UnityEngine;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    public sealed class TrainingDummy : MonoBehaviour
    {
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        [SerializeField] [Min(1)] private int _maxHealth = 500;

        private int _health;
        private float _burnUntil;
        private float _burnDamage;
        private int _burnPerSecond;
        private float _freezeUntil;
        private float _slow;
        private Renderer _renderer;
        private MaterialPropertyBlock _block;

        public float Slow => Time.time < _freezeUntil
            ? _slow
            : 0f;

        private void Awake()
        {
            _health = _maxHealth;
            _renderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            var burning = Time.time < _burnUntil;

            if (burning)
            {
                _burnDamage += _burnPerSecond * Time.deltaTime;
                var whole = Mathf.FloorToInt(_burnDamage);
                _burnDamage -= whole;
                TakeDamage(whole);
            }

            var health = (float)_health / _maxHealth;
            transform.localScale = Vector3.one * Mathf.Lerp(a: 0.4f, b: 2f, t: health);

            var color = burning ? new Color(r: 1f, g: 0.45f, b: 0.1f)
                : Time.time < _freezeUntil ? new Color(r: 0.4f, g: 0.8f, b: 1f)
                : Color.Lerp(a: new Color(r: 0.6f, g: 0.1f, b: 0.1f), b: new Color(r: 0.8f, g: 0.8f, b: 0.8f), t: health);

            _block.SetColor(nameID: _colorId, value: color);
            _renderer.SetPropertyBlock(_block);
        }

        public void TakeDamage(int damage)
        {
            _health -= damage;

            if (_health > 0)
                return;

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
