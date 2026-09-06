using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Orbits the center at a fixed distance.
    public class Archer : Enemy
    {
        [SerializeField] [Min(1f)] private float _keepDistance = 6f;

        protected override Color Tint => new(0.3f, 0.7f, 0.4f);

        protected override void Move(float deltaTime)
        {
            var position = transform.position;
            var toCenter = -position;
            var distance = toCenter.magnitude;
            if (distance < 0.01f) return;

            var radial = toCenter / distance * Mathf.Clamp(distance - _keepDistance, -1f, 1f);
            var tangent = Vector3.Cross(Vector3.up, toCenter / distance);
            transform.position = position + (radial + tangent) * (Speed * deltaTime);
        }
    }
}
