using UnityEngine;

namespace Aspid.FastTools.Samples.Types
{
    public class Grunt : Enemy
    {
        protected override Color Tint => new(r: 0.85f, g: 0.35f, b: 0.25f);

        protected override void Move(float deltaTime)
        {
            var center = new Vector3(x: 0f, y: transform.position.y, z: 0f);
            transform.position = Vector3.MoveTowards(current: transform.position, target: center, maxDistanceDelta: Speed * deltaTime);
        }
    }
}
