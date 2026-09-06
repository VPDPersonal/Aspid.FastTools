using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Base enemy. The ComponentTypeSelector field adds a dropdown at the top of the Inspector that swaps
    // this component to any Enemy subtype in place; fields shared with the new subtype keep their values.
    public abstract class Enemy : MonoBehaviour
    {
        [SerializeField] private ComponentTypeSelector _kind;
        [SerializeField] [Min(1f)] private float _health = 100f;
        [SerializeField] [Min(0.1f)] private float _speed = 3f;
        [SerializeField] [Min(1f)] private float _lifetime = 12f;

        private static readonly int _baseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int _colorId = Shader.PropertyToID("_Color");

        private float _age;
        private MaterialPropertyBlock _block;

        protected float Speed => _speed;

        protected abstract Color Tint { get; }

        protected virtual void Start()
        {
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

        protected abstract void Move(float deltaTime);

        public override string ToString() =>
            $"{GetType().Name} (HP {_health})";
    }
}
