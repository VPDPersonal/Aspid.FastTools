using UnityEngine;

namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    public sealed class FlockSimulation
    {
        public const int Capacity = 400;

        private readonly Vector3[] _positions = new Vector3[Capacity];
        private readonly Vector3[] _velocities = new Vector3[Capacity];

        public int Count { get; set; }

        public FlockSimulation()
        {
            for (var i = 0; i < Capacity; i++)
                _positions[i] = Random.insideUnitSphere;
        }

        public Vector3 GetPosition(int index) => _positions[index];

        public void Step(float deltaTime)
        {
            using var _ = this.Marker();

            using (this.Marker().WithName("Steering"))
                Steer(deltaTime);

            using (this.Marker().WithName("Integrate"))
                Integrate(deltaTime);
        }

        private void Steer(float deltaTime)
        {
            var pull = 1f + Mathf.Sin(Time.time * 2f) * 0.5f;
            var bait = new Vector3(x: Mathf.Sin(Time.time), y: 0f, z: Mathf.Cos(Time.time)) * 3f;

            for (var i = 0; i < Count; i++)
            {
                using var _ = this.Marker().WithName("Steering.Agent");

                var push = Vector3.zero;

                for (var j = 0; j < Count; j++)
                {
                    var offset = _positions[i] - _positions[j];
                    push += offset / (offset.sqrMagnitude + 0.1f);
                }

                _velocities[i] += (push * 0.1f + (bait - _positions[i]) * pull) * deltaTime;
            }
        }

        private void Integrate(float deltaTime)
        {
            for (var i = 0; i < Count; i++)
            {
                _velocities[i] *= 1f - deltaTime;
                _positions[i] += _velocities[i] * deltaTime;
            }
        }
    }
}
