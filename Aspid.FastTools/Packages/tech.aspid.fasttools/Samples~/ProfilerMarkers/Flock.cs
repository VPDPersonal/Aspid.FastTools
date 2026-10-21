using UnityEngine;
using System.Collections.Generic;

namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    public sealed class Flock : MonoBehaviour
    {
        [SerializeField] private Transform _agentPrefab;
        [SerializeField, Range(10, FlockSimulation.Capacity)] private int _count = 120;

        private readonly List<Transform> _agents = new();
        private FlockSimulation _simulation;

        private void Awake() => _simulation = new FlockSimulation();

        private void Update()
        {
            using var _ = this.Marker();

            ResizeAgents();

            _simulation.Count = _count;
            _simulation.Step(Time.deltaTime);

            using (this.Marker().WithName("ApplyTransforms"))
            {
                for (var i = 0; i < _agents.Count; i++)
                    _agents[i].localPosition = _simulation.GetPosition(i);
            }
        }

        private void ResizeAgents()
        {
            while (_agents.Count < _count)
                _agents.Add(Instantiate(original: _agentPrefab, parent: transform));

            while (_agents.Count > _count)
            {
                Destroy(_agents[^1].gameObject);
                _agents.RemoveAt(_agents.Count - 1);
            }
        }
    }
}
