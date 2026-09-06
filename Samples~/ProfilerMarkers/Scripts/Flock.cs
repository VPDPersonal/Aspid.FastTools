using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    // Drives the simulation and moves one cube per agent. Open Window → Analysis → Profiler, enter Play Mode
    // and expand PlayerLoop → Update.ScriptRunBehaviourUpdate → Flock.Update to see the marker tree.
    public sealed class Flock : MonoBehaviour
    {
        [SerializeField] [Range(8, 400)] private int _count = 120;
        [SerializeField] [Min(1f)] private float _bounds = 12f;
        [SerializeField] [Min(0.1f)] private float _neighborRadius = 3f;
        [SerializeField] [Min(0.1f)] private float _maxSpeed = 6f;

        private Transform[] _agents;
        private FlockSimulation _simulation;

        private void Start()
        {
            _simulation = new FlockSimulation(_count, _bounds);
            _agents = new Transform[_count];

            // A local function resolves to the enclosing method: "Flock.Start (line)".
            for (var i = 0; i < _count; i++)
                _agents[i] = CreateAgent(i);

            Transform CreateAgent(int index)
            {
                using var _ = this.Marker();
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Agent {index}";
                go.transform.SetParent(transform);
                go.transform.localScale = new Vector3(0.3f, 0.3f, 0.8f);
                Destroy(go.GetComponent<Collider>());
                return go.transform;
            }
        }

        private void Update()
        {
            // "Flock.Update (line)" covers the whole frame step; the simulation adds its own markers below it.
            using var _ = this.Marker();

            _simulation.Step(Time.deltaTime, _neighborRadius, _maxSpeed);

            using (this.Marker().WithName("ApplyTransforms"))
            {
                for (var i = 0; i < _agents.Length; i++)
                {
                    var velocity = _simulation.GetVelocity(i);
                    _agents[i].SetPositionAndRotation(
                        _simulation.GetPosition(i),
                        velocity.sqrMagnitude > 0.001f ? Quaternion.LookRotation(velocity) : _agents[i].rotation);
                }
            }
        }
    }
}
