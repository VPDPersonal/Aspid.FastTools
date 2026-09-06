using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public sealed class ArmoredGrunt : Grunt
    {
        [SerializeField] [Min(0f)] private float _armor = 50f;

        protected override Color Tint => new(0.55f, 0.2f, 0.15f);

        protected override void Start()
        {
            base.Start();
            transform.localScale = Vector3.one * 1.4f;
        }

        public override string ToString() =>
            $"{base.ToString()}, armor {_armor}";
    }
}
