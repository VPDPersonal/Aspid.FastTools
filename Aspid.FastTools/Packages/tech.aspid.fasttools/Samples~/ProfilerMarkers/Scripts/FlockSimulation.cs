using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    // Each this.Marker() call site generates a static marker, including in plain C# classes.
    /// <summary>
    /// Represents a flock with separation, alignment and cohesion steering.
    /// </summary>
    public sealed class FlockSimulation
    {
        private readonly Vector3[] _positions;
        private readonly Vector3[] _velocities;
        private readonly Vector3[] _steering;
        private readonly float _bounds;

        /// <param name="count">Number of agents to create.</param>
        /// <param name="bounds">Distance from the origin at which agents turn back.</param>
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

        /// <summary>
        /// Gets the number of agents in the simulation.
        /// </summary>
        public int Count => _positions.Length;

        /// <summary>
        /// Returns the agent position.
        /// </summary>
        /// <param name="index">Zero-based agent index.</param>
        /// <returns>Agent position in world space.</returns>
        public Vector3 GetPosition(int index) => _positions[index];

        /// <summary>
        /// Returns the agent velocity.
        /// </summary>
        /// <param name="index">Zero-based agent index.</param>
        /// <returns>Agent velocity in world units per second.</returns>
        public Vector3 GetVelocity(int index) => _velocities[index];

        /// <summary>
        /// Advances steering and movement by one simulation step.
        /// </summary>
        /// <param name="deltaTime">Elapsed time in seconds.</param>
        /// <param name="neighborRadius">Distance within which agents influence steering.</param>
        /// <param name="maxSpeed">Maximum speed in world units per second.</param>
        public void Step(float deltaTime, float neighborRadius, float maxSpeed)
        {
            using var _ = this.Marker(); // Wraps the whole method: "FlockSimulation.Step (line)".

            using (this.Marker().WithName("Steering")) // "FlockSimulation.Steering (line)", nested under Step.
                ComputeSteering(neighborRadius);

            using (this.Marker().WithName("Integrate"))
                Integrate(deltaTime, maxSpeed);
        }

        private void ComputeSteering(float neighborRadius)
        {
            var radiusSq = neighborRadius * neighborRadius;

            for (var i = 0; i < _positions.Length; i++)
            {
                // One marker with N samples per frame, not N markers: the name is fixed per call site.
                using var _ = this.Marker().WithName("Steering.Agent");

                var center = Vector3.zero;
                var heading = Vector3.zero;
                var separation = Vector3.zero;
                var neighbors = 0;

                for (var j = 0; j < _positions.Length; j++)
                {
                    if (i == j)
                        continue;
                    var offset = _positions[j] - _positions[i];
                    var distanceSq = offset.sqrMagnitude;
                    if (distanceSq > radiusSq)
                        continue;

                    neighbors++;
                    center += _positions[j];
                    heading += _velocities[j];
                    separation -= offset / Mathf.Max(distanceSq, 0.01f);
                }

                var toHome = -_positions[i] * 0.05f;
                if (neighbors is 0)
                {
                    _steering[i] = toHome;
                    continue;
                }

                center = center / neighbors - _positions[i];
                heading = heading / neighbors - _velocities[i];
                _steering[i] = center * 0.4f + heading * 0.6f + separation * 1.5f + toHome;
            }
        }

        private void Integrate(float deltaTime, float maxSpeed)
        {
            for (var i = 0; i < _positions.Length; i++)
            {
                _velocities[i] = Vector3.ClampMagnitude(_velocities[i] + _steering[i] * deltaTime, maxSpeed);
                _positions[i] += _velocities[i] * deltaTime;

                if (_positions[i].magnitude > _bounds)
                    _velocities[i] = Vector3.Reflect(_velocities[i], -_positions[i].normalized);
            }
        }
    }
}
