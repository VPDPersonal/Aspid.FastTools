using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Walks straight to the arena center. Not sealed: ArmoredGrunt derives from it, which is what the
    // spawner's elite picker (constrained to the chosen enemy type) offers when Grunt is selected.
    public class Grunt : Enemy
    {
        protected override Color Tint => new(0.85f, 0.35f, 0.25f);

        protected override void Move(float deltaTime) =>
            transform.position = Vector3.MoveTowards(transform.position, Vector3.zero, Speed * deltaTime);
    }
}
