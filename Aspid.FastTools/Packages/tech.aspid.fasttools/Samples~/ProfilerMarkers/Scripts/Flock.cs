using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    public sealed class Flock : MonoBehaviour
    {
        private static readonly Color _cyan = new(r: 0.25f, g: 0.85f, b: 1f);
        private static readonly Color _green = new(r: 0.65f, g: 1f, b: 0.35f);

        [SerializeField] [Range(8, 400)] private int _count = 120;
        [SerializeField] [Min(1f)] private float _bounds = 12f;
        [SerializeField] [Min(0.1f)] private float _neighborRadius = 3f;
        [SerializeField] [Min(0.1f)] private float _maxSpeed = 6f;
        [SerializeField] private Material _presentationMaterial;

        private Transform[] _agents = { };
        private FlockSimulation _simulation;

        private void Update()
        {
            using var _ = this.Marker();

            if (_agents.Length != _count)
                InitializeAgents();

            _simulation.Step(deltaTime: Time.deltaTime, neighborRadius: _neighborRadius, maxSpeed: _maxSpeed);

            using (this.Marker().WithName("ApplyTransforms"))
            {
                for (var i = 0; i < _agents.Length; i++)
                {
                    var velocity = _simulation.GetVelocity(i);
                    var rotation = velocity.sqrMagnitude > 0.001f
                        ? Quaternion.LookRotation(velocity)
                        : _agents[i].rotation;

                    _agents[i].SetPositionAndRotation(position: _simulation.GetPosition(i), rotation: rotation);
                }
            }
        }

        private void InitializeAgents()
        {
            foreach (var agent in _agents)
                Destroy(agent.gameObject);

            _simulation = new FlockSimulation(count: _count, bounds: _bounds);
            _agents = new Transform[_count];

            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            for (var i = 0; i < _count; i++)
                _agents[i] = CreateAgent(i);
            return;

            Transform CreateAgent(int index)
            {
                using var _ = this.Marker();

                var agent = new GameObject(name: $"Agent {index}", components: new[] { typeof(MeshFilter), typeof(MeshRenderer) });
                agent.GetComponent<MeshFilter>().sharedMesh = mesh;

                var block = new MaterialPropertyBlock();
                block.SetColor(name: "_Color", value: Color.Lerp(a: _cyan, b: _green, t: (float)index / _count));
                var renderer = agent.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = _presentationMaterial;
                renderer.SetPropertyBlock(block);

                agent.transform.SetParent(transform);
                agent.transform.localScale = new Vector3(x: 0.3f, y: 0.3f, z: 0.8f);

                return agent.transform;
            }
        }
    }
}
