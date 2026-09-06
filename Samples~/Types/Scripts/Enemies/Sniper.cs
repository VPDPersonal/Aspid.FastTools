using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    public sealed class Sniper : Archer
    {
        protected override Color Tint => new(0.15f, 0.45f, 0.25f);

        protected override void Start()
        {
            base.Start();
            transform.localScale = new Vector3(0.6f, 1.6f, 0.6f);
        }
    }
}
