using UnityEngine;

namespace Aspid.FastTools.Samples.Types
{
    public sealed class Grunt : Enemy
    {
        private void Start() =>
            GetComponent<Renderer>().material.color = new Color(r: 1f, g: 0.5f, b: 0.35f);
    }
}
