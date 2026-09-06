using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // IMGUI-side group navigation for shared references: clicking the "Shared reference #N" message picks the
    // group's next member in document order, expands the parents hiding it, scrolls to it once painted, and pulses
    // every member in the group color. The UIToolkit field navigates its element tree instead; only the pulse timings
    // and the document-order cycling are kept in lock-step between the two.
    internal static class SerializeReferenceSharedNavigation
    {
        // Full tint for the hold fraction, then a linear fade. Mirrors the UIToolkit field so both pulses match.
        private const float FlashAlpha = 0.25f;
        private const double FlashSeconds = 1.6;
        private const float FlashHoldFraction = 0.35f;

        // Scrolls the revealed member a quarter down, so a line or two of context stays above it.
        private const float RevealViewportFraction = 0.25f;

        // A revealed member only gets a rect once painted; the reveal is dropped if no repaint reports one in time.
        private const double RevealTimeoutSeconds = 1.0;

        // The group member the next repaint should scroll to.
        private static int _revealTarget;
        private static long _revealRid;
        private static string _revealPath;
        private static double _revealUntil;

        // Advancing from a per-group cursor rather than the clicked field lets repeated clicks on the same notice
        // walk the whole group.
        private static readonly Dictionary<(int target, long rid), string> NavigationCursor = new();

        // The active pulse: every drawn member of the group except the clicked one tints until the deadline.
        private static int _flashTarget;
        private static long _flashRid;
        private static string _flashExceptPath;
        private static double _flashUntil;

        // Handles a click on the shared-reference message. Call with the inspector's LIVE property, never a
        // persistent copy: isExpanded is cached per SerializedObject, so the ancestor expansion only reaches the
        // inspector's foldouts when written through its own.
        public static void NavigateFrom(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            if (target == null) return;

            var rid = property.managedReferenceId;
            if (rid < 0) return;

            var group = SerializeReferenceHelpers.GetSharedReferenceGroupPaths(property);
            if (group.Count < 2) return;

            var selfPath = property.propertyPath;

            // The pulse covers every drawn member, so the whole group is revealed, not just the scroll target.
            foreach (var path in group)
                if (path != selfPath)
                    ExpandAncestors(property.serializedObject, path);

            var key = (target.GetInstanceID(), rid);
            var start = NavigationCursor.TryGetValue(key, out var cursor) ? IndexOf(group, cursor) : -1;
            if (start < 0) start = IndexOf(group, selfPath);

            string nextPath = null;
            for (var step = 1; step <= group.Count && nextPath is null; step++)
            {
                var candidate = group[(start + step) % group.Count];
                if (candidate != selfPath) nextPath = candidate;
            }

            if (nextPath is null) return;
            NavigationCursor[key] = nextPath;

            StartFlash(target.GetInstanceID(), rid, selfPath);

            _revealTarget = target.GetInstanceID();
            _revealRid = rid;
            _revealPath = nextPath;
            _revealUntil = EditorApplication.timeSinceStartup + RevealTimeoutSeconds;
        }

        // Reports where a shared field was painted; call with its FULL rect, header and children. The inspector
        // scrolls to it when it is the pending reveal's member.
        public static void RevealIfPending(SerializedProperty property, Rect fieldRect)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (_revealPath is null || EditorApplication.timeSinceStartup > _revealUntil) return;

            var target = property.serializedObject.targetObject;
            if (target == null || target.GetInstanceID() != _revealTarget) return;
            if (property.managedReferenceId != _revealRid) return;
            if (property.propertyPath != _revealPath) return;

            _revealPath = null;
            var screenRect = GUIUtility.GUIToScreenRect(fieldRect);

            // Scrolling would mutate the host ScrollView mid-Repaint, so defer it a tick.
            EditorApplication.delayCall += () => ScrollTo(screenRect);
        }

        // True while the pulse covers this field, with the overlay's current fade alpha.
        public static bool TryGetFlashAlpha(SerializedProperty property, out float alpha)
        {
            alpha = 0f;

            var remaining = _flashUntil - EditorApplication.timeSinceStartup;
            if (remaining <= 0) return false;

            var target = property.serializedObject.targetObject;
            if (target == null || target.GetInstanceID() != _flashTarget) return false;
            if (property.managedReferenceId != _flashRid) return false;
            if (property.propertyPath == _flashExceptPath) return false;

            var t = 1f - (float)(remaining / FlashSeconds);
            alpha = t < FlashHoldFraction
                ? FlashAlpha
                : FlashAlpha * (1f - (t - FlashHoldFraction) / (1f - FlashHoldFraction));
            return true;
        }

        private static int IndexOf(IReadOnlyList<string> paths, string path)
        {
            for (var i = 0; i < paths.Count; i++)
                if (paths[i] == path)
                    return i;

            return -1;
        }

        // Expands every ancestor so the property is drawn at all; the member itself is left alone, since revealing it
        // must not toggle its own foldout. Prefixes that are not real properties resolve to null and are skipped.
        private static void ExpandAncestors(SerializedObject serializedObject, string path)
        {
            for (var dot = path.IndexOf('.'); dot >= 0; dot = path.IndexOf('.', dot + 1))
            {
                using var ancestor = serializedObject.FindProperty(path[..dot]);
                if (ancestor != null) ancestor.isExpanded = true;
            }
        }

        private static void StartFlash(int target, long rid, string exceptPath)
        {
            _flashTarget = target;
            _flashRid = rid;
            _flashExceptPath = exceptPath;
            _flashUntil = EditorApplication.timeSinceStartup + FlashSeconds;

            // IMGUI only repaints on events, so the fade needs its own repaint driver.
            EditorApplication.update -= DriveFlashRepaints;
            EditorApplication.update += DriveFlashRepaints;
        }

        private static void DriveFlashRepaints()
        {
            InternalEditorUtility.RepaintAllViews();
            if (EditorApplication.timeSinceStartup < _flashUntil) return;

            EditorApplication.update -= DriveFlashRepaints;
            InternalEditorUtility.RepaintAllViews(); // one final repaint erases the last visible tint
        }

        // The inspector hosts IMGUI editors inside a UIToolkit ScrollView, which GUI.ScrollTo cannot reach, so the
        // scroll goes through the window's element tree. Screen rects and worldBound share the window's origin, so
        // the conversion is a plain offset.
        private static void ScrollTo(Rect screenRect)
        {
            var window = EditorWindow.mouseOverWindow != null ? EditorWindow.mouseOverWindow : EditorWindow.focusedWindow;
            if (window == null) return;

            var scrollView = window.rootVisualElement?.Q<ScrollView>();
            if (scrollView == null) return;

            var viewport = scrollView.contentViewport.worldBound;
            var targetY = screenRect.y - window.position.y;

            // Already inside the viewport, so the pulse alone is enough.
            if (targetY >= viewport.yMin + 4f && targetY + screenRect.height <= viewport.yMax - 4f) return;

            var offset = scrollView.scrollOffset;
            offset.y += targetY - (viewport.yMin + viewport.height * RevealViewportFraction);
            scrollView.scrollOffset = offset; // the ScrollView clamps to its content bounds
        }
    }
}
