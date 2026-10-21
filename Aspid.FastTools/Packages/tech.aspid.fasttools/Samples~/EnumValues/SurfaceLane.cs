using UnityEngine;

namespace Aspid.FastTools.Samples.EnumValues
{
    public sealed class SurfaceLane : MonoBehaviour
    {
        private const float Length = 9f;

        [SerializeField] private SurfaceRules _rules;
        [SerializeField] private SurfaceType _surface;
        [SerializeField] private TerrainFlags _terrain;

        [SerializeField] private Renderer _track;
        [SerializeField] private Transform _runner;

        private float _distance;

        private void Update()
        {
            if (_rules == null)
                return;

            _track.material.color = _rules.GetColor(_surface);
            _distance += _rules.GetSpeed(_terrain) * Time.deltaTime;

            var position = _runner.localPosition;
            position.x = Mathf.PingPong(t: _distance, length: Length);
            _runner.localPosition = position;
        }
    }
}
