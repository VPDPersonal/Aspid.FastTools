using UnityEditor;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    // Detects double-clicks based on the elapsed time between consecutive calls to Detect. Uses timeSinceStartup, so
    // it is editor-only and does not require frame updates.
    internal struct DoubleClickTracker
    {
        private const float DefaultThresholdSeconds = 0.3f;

        private float _lastClickTime;

        public bool Detect(float thresholdSeconds = DefaultThresholdSeconds)
        {
            var currentTime = (float)EditorApplication.timeSinceStartup;
            var isDouble = _lastClickTime > 0f && currentTime - _lastClickTime < thresholdSeconds;

            _lastClickTime = isDouble ? 0f : currentTime;
            return isDouble;
        }
    }
}
