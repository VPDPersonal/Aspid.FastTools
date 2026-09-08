using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    // In the CPU Profiler, expand Update.ScriptRunBehaviourUpdate > Flock.Update to inspect the marker tree.
    /// <summary>
    /// <see cref="MonoBehaviour"/> that visualizes a flock simulation with nested profiler markers.
    /// </summary>
    public sealed class Flock : MonoBehaviour
    {
        [Tooltip("Number of simulated agents.")]
        [SerializeField, Range(8, 400)] private int _count = 120;

        [Tooltip("Distance from the origin at which agents turn back.")]
        [SerializeField, Min(1f)] private float _bounds = 12f;

        [Tooltip("Distance within which agents influence steering.")]
        [SerializeField, Min(0.1f)] private float _neighborRadius = 3f;

        [Tooltip("Maximum agent speed in world units per second.")]
        [SerializeField, Min(0.1f)] private float _maxSpeed = 6f;

        [Tooltip("Material applied to generated sample objects.")]
        [SerializeField, HideInInspector] private Material _presentationMaterial;

        private Transform[] _agents;
        private FlockSimulation _simulation;

        private void Start() => InitializeAgents();

        private void InitializeAgents()
        {
            if (_agents is not null)
            {
                foreach (var agent in _agents)
                {
                    agent.gameObject.SetActive(false);
                    Destroy(agent.gameObject);
                }
            }

            _count = Mathf.Clamp(_count, 8, 400);
            _simulation = new FlockSimulation(_count, _bounds);
            _agents = new Transform[_count];

            // A local function resolves to the enclosing method: "Flock.InitializeAgents (line)".
            for (var i = 0; i < _count; i++)
                _agents[i] = CreateAgent(i);

            Transform CreateAgent(int index)
            {
                using var _ = this.Marker();
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = $"Agent {index}";
                var renderer = go.GetComponent<Renderer>();
                if (_presentationMaterial != null)
                    renderer.sharedMaterial = _presentationMaterial;
                var block = new MaterialPropertyBlock();
                var tint = Color.Lerp(new Color(0.25f, 0.85f, 1f), new Color(0.65f, 1f, 0.35f), (float)index / _count);
                block.SetColor("_Color", tint);
                block.SetColor("_BaseColor", tint);
                renderer.SetPropertyBlock(block);
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

            if (_agents.Length != Mathf.Clamp(_count, 8, 400))
                InitializeAgents();

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
