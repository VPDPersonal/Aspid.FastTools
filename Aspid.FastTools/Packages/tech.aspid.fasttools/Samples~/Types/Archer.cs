using UnityEngine;

namespace Aspid.FastTools.Samples.Types
{
    public sealed class Archer : Enemy
    {
        [SerializeField] private float _range = 3f;

        public float Range => _range;

        private void Start() =>
            GetComponent<Renderer>().material.color = new Color(r: 0.65f, g: 0.95f, b: 0.4f);
    }
}
