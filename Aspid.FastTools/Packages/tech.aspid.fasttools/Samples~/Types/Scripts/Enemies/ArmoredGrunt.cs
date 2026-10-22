using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Enemies
{
    public sealed class ArmoredGrunt : Grunt
    {
        protected override Color Tint => new(r: 0.55f, g: 0.2f, b: 0.15f);

        protected override void Start()
        {
            transform.localScale = Vector3.one * 1.4f;
            base.Start();
        }
    }
}
