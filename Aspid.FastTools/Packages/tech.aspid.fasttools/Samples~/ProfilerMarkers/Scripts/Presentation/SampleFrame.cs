using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers.Presentation
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public sealed class SampleFrame : MonoBehaviour
    {
        private const float ReferenceSize = 20.7f;

        private Camera _camera;

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            RenderPipelineManager.beginCameraRendering += BeforeRender;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -= BeforeRender;
            if (_camera != null)
                _camera.orthographicSize = ReferenceSize;
        }

        private void OnPreCull() =>
            Frame();

        private void BeforeRender(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _camera)
                Frame();
        }

        private void Frame()
        {
            if (_camera == null)
                return;

            var target = _camera.targetTexture;
            var aspect = target != null
                ? (float)target.width / target.height
                : _camera.aspect;

            _camera.orthographicSize = ReferenceSize * Mathf.Max(a: 1f, b: 16f / 9f / Mathf.Max(a: aspect, b: 0.01f));
        }
    }
}
