using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.Types
{
    // Slow and large; bobs while walking so the swap from another type is visible at a glance.
    public sealed class Brute : Enemy
    {
        [SerializeField] [Min(0f)] private float _stompHeight = 0.4f;

        protected override Color Tint => new(0.5f, 0.35f, 0.75f);

        protected override void Start()
        {
            base.Start();
            transform.localScale = Vector3.one * 2f;
        }

        protected override void Move(float deltaTime)
        {
            var position = Vector3.MoveTowards(transform.position, Vector3.zero, Speed * 0.5f * deltaTime);
            position.y = 1f + Mathf.Abs(Mathf.Sin(Time.time * 4f)) * _stompHeight;
            transform.position = position;
        }
    }
}
