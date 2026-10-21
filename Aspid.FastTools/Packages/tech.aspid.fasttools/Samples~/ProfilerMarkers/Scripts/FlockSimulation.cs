using UnityEngine;

// ReSharper disable CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    public sealed class FlockSimulation
    {
        private readonly Vector3[] _positions;
        private readonly Vector3[] _velocities;
        private readonly Vector3[] _steering;
        private readonly float _bounds;

        public FlockSimulation(int count, float bounds)
        {
            _bounds = bounds;
            _positions = new Vector3[count];
            _velocities = new Vector3[count];
            _steering = new Vector3[count];

            for (var i = 0; i < count; i++)
            {
                _positions[i] = Random.insideUnitSphere * bounds * 0.5f;
                _velocities[i] = Random.onUnitSphere * 2f;
            }
        }

        public Vector3 GetPosition(int index) =>
            _positions[index];

        public Vector3 GetVelocity(int index) =>
            _velocities[index];

        public void Step(float deltaTime, float neighborRadius, float maxSpeed)
        {
            using var _ = this.Marker();

            using (this.Marker().WithName("Steering"))
                ComputeSteering(neighborRadius);

            using (this.Marker().WithName("Integrate"))
                Integrate(deltaTime: deltaTime, maxSpeed: maxSpeed);
        }

        private void ComputeSteering(float neighborRadius)
        {
            var radiusSq = neighborRadius * neighborRadius;

            for (var i = 0; i < _positions.Length; i++)
            {
                using var _ = this.Marker().WithName("Steering.Agent");

                var center = Vector3.zero;
                var heading = Vector3.zero;
                var separation = Vector3.zero;
                var neighbors = 0;

                for (var j = 0; j < _positions.Length; j++)
                {
                    var offset = _positions[j] - _positions[i];
                    var distanceSq = offset.sqrMagnitude;
                    if (i == j || distanceSq > radiusSq)
                        continue;

                    neighbors++;
                    center += _positions[j];
                    heading += _velocities[j];
                    separation -= offset / Mathf.Max(a: distanceSq, b: 0.01f);
                }

                var toHome = -_positions[i] * 0.05f;
                _steering[i] = neighbors is 0
                    ? toHome
                    : (center / neighbors - _positions[i]) * 0.4f + (heading / neighbors - _velocities[i]) * 0.6f + separation * 1.5f + toHome;
            }
        }

        private void Integrate(float deltaTime, float maxSpeed)
        {
            for (var i = 0; i < _positions.Length; i++)
            {
                _velocities[i] = Vector3.ClampMagnitude(vector: _velocities[i] + _steering[i] * deltaTime, maxLength: maxSpeed);
                _positions[i] += _velocities[i] * deltaTime;

                if (_positions[i].magnitude > _bounds)
                    _velocities[i] = Vector3.Reflect(inDirection: _velocities[i], inNormal: -_positions[i].normalized);
            }
        }
    }
}
