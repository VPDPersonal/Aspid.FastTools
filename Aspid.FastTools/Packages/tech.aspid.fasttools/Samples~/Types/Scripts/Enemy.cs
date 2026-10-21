using UnityEngine;
using Aspid.FastTools.Types;

namespace Aspid.FastTools.Samples.Types
{
    public abstract class Enemy : MonoBehaviour
    {
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        [SerializeField] private ComponentTypeSelector _kind;
        [SerializeField, Min(1f)] private float _health = 100f;
        [SerializeField, Min(0.1f)] private float _speed = 3f;
        [SerializeField, Min(1f)] private float _lifetime = 12f;

        private float _age;

        public float Health => _health;

        protected float Speed => _speed;

        protected abstract Color Tint { get; }

        protected virtual void Start()
        {
            var body = GetComponent<Renderer>();
            transform.position -= Vector3.up * body.bounds.min.y;

            var block = new MaterialPropertyBlock();
            block.SetColor(nameID: _colorId, value: Tint);
            body.SetPropertyBlock(block);
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

        protected abstract void Move(float deltaTime);
    }
}
