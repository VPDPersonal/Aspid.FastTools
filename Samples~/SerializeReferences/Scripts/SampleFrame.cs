using UnityEngine;
using UnityEngine.Rendering;

namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Keeps the authored 16:9 composition visible in narrower Game views.
    [ExecuteAlways, RequireComponent(typeof(Camera)), AddComponentMenu("")]
    public sealed class SampleFrame : MonoBehaviour
    {
        [SerializeField, HideInInspector] private float _referenceSize = 11f;
        private Camera _camera;

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            RenderPipelineManager.beginCameraRendering += BeforeRender;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeforeRender;
            if (_camera != null) _camera.orthographicSize = _referenceSize;
        }

        private void BeforeRender(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _camera) Frame();
        }

        private void OnPreCull() => Frame();

        private void Frame()
        {
            if (_camera == null) return;
            var target = _camera.targetTexture;
            var aspect = target != null ? (float)target.width / target.height : _camera.aspect;
            _camera.orthographicSize = _referenceSize * Mathf.Max(1f, (16f / 9f) / Mathf.Max(aspect, 0.01f));
        }
    }
}
