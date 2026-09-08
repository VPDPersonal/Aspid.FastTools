using UnityEngine;
using UnityEngine.Rendering;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.EnumValues
{
    /// <summary>
    /// <see cref="MonoBehaviour"/> that preserves the authored 16:9 camera framing.
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(Camera)), AddComponentMenu("")]
    public sealed class SampleFrame : MonoBehaviour
    {
        [Tooltip("Orthographic size at the authored aspect ratio.")]
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
            if (_camera != null)
                _camera.orthographicSize = _referenceSize;
        }

        private void BeforeRender(ScriptableRenderContext context, Camera camera)
        {
            if (camera == _camera)
                Frame();
        }

        private void OnPreCull() => Frame();

        private void Frame()
        {
            if (_camera == null)
                return;

            var target = _camera.targetTexture;
            var aspect = target != null ? (float)target.width / target.height : _camera.aspect;
            _camera.orthographicSize = _referenceSize * Mathf.Max(1f, (16f / 9f) / Mathf.Max(aspect, 0.01f));
        }
    }
}
