using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types.Enemies
{
    public sealed class Archer : Enemy
    {
        [SerializeField] [Min(1f)] private float _keepDistance = 6f;

        protected override Color Tint => new(r: 0.3f, g: 0.7f, b: 0.4f);

        protected override void Move(float deltaTime)
        {
            var position = transform.position;
            var toCenter = new Vector3(x: -position.x, y: 0f, z: -position.z);
            var distance = toCenter.magnitude;
            if (distance < 0.01f)
                return;

            var radial = toCenter / distance * Mathf.Clamp(value: distance - _keepDistance, min: -1f, max: 1f);
            var tangent = Vector3.Cross(lhs: Vector3.up, rhs: toCenter / distance);
            transform.position = position + (radial + tangent) * (Speed * deltaTime);
        }
    }
}
