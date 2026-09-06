using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    // IMGUI twin of InspectorNotice, so both inspector modes render the same notice rows.
    internal static class InspectorNoticeGUI
    {
        // Mirrors the UIToolkit palette: --aspid-colors-status-warning-text-light / -lightness.
        internal static readonly Color NoticeColor = new(245f / 255f, 185f / 255f, 85f / 255f);
        internal static readonly Color NoticeColorHover = new(255f / 255f, 235f / 255f, 175f / 255f);

        // The rid color is dynamic, so hover lightens it instead of applying a static USS brighten.
        private const float ActionHoverLighten = 0.35f;

        private const float DotSize = 8f;

        private static readonly Color _infoNoticeColor = new(150f / 255f, 150f / 255f, 150f / 255f);

        private static GUIStyle _messageStyle;
        private static GUIStyle _actionStyle;
        private static GUIStyle _infoMessageStyle;

        // Non-actionable info row: icon plus a dim message whose full detail rides the tooltip.
        internal static void DrawInfoNotice(Rect rect, string message, string detail)
        {
            _infoMessageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _infoMessageStyle.normal.textColor = _infoNoticeColor;

            const float iconSize = 16f;
            var iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            GUI.Label(iconRect, EditorGUIUtility.IconContent("console.infoicon"));

            var messageContent = new GUIContent(message, detail);
            var messageRect = new Rect(iconRect.xMax + 4f, rect.y, rect.xMax - iconRect.xMax - 4f, rect.height);
            GUI.Label(messageRect, messageContent, _infoMessageStyle);
        }

        // Single-row notice: a message, a right-pinned action word and an optional Smart Fix suggestion after it.
        // Without ridColor the row is an amber warning with a triangle icon; with it the row is the shared-reference
        // variant — a rid-colored swatch instead of the icon, message and action tinted that color so aliased fields
        // match at a glance. onMessageClick makes the message itself clickable.
        internal static void DrawNotice(Rect rect, string message, string actionText, string detail, Action onClick,
            string suggestionText = null, string suggestionDetail = null, Action onSuggestion = null,
            Color? ridColor = null, Action onMessageClick = null)
        {
            var shared = ridColor.HasValue;
            var baseColor = shared ? ridColor.Value : NoticeColor;
            var hoverColor = shared ? Color.Lerp(baseColor, Color.white, ActionHoverLighten) : NoticeColorHover;

            _messageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _actionStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            _messageStyle.normal.textColor = baseColor;

            float messageX;
            if (shared)
            {
                DrawDot(rect.x, rect, baseColor);
                messageX = rect.x + DotSize + 6f;
            }
            else
            {
                const float iconSize = 16f;
                var iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
                GUI.Label(iconRect, EditorGUIUtility.IconContent("console.warnicon"));
                messageX = iconRect.xMax + 4f;
            }

            var messageContent = new GUIContent(message, detail);
            var messageWidth = _messageStyle.CalcSize(messageContent).x;
            var messageRect = new Rect(messageX, rect.y, messageWidth, rect.height);
            if (onMessageClick is not null)
            {
                var messageHover = messageRect.Contains(Event.current.mousePosition);
                var messageColor = messageHover ? hoverColor : baseColor;
                _messageStyle.normal.textColor = messageColor;
                _messageStyle.hover.textColor = messageColor;

                EditorGUIUtility.AddCursorRect(messageRect, MouseCursor.Link);
                if (GUI.Button(messageRect, messageContent, _messageStyle)) onMessageClick();
            }
            else
            {
                // The style is shared across notices — reset the tint a clickable message may have left behind.
                _messageStyle.hover.textColor = baseColor;
                GUI.Label(messageRect, messageContent, _messageStyle);
            }

            if (string.IsNullOrEmpty(actionText) || onClick is null) return;

            var actionContent = new GUIContent(actionText, detail);
            var actionWidth = _actionStyle.CalcSize(actionContent).x;

            var hasSuggestion = !string.IsNullOrEmpty(suggestionText) && onSuggestion is not null;
            var suggestionContent = hasSuggestion ? new GUIContent(suggestionText, suggestionDetail) : null;
            var suggestionWidth = hasSuggestion ? _actionStyle.CalcSize(suggestionContent).x : 0f;
            const float suggestionGap = 6f;

            var separatorContent = hasSuggestion ? new GUIContent("·") : null;
            var separatorWidth = hasSuggestion ? _actionStyle.CalcSize(separatorContent).x : 0f;

            // Pin the action cluster to the right edge, but never let it overlap the message.
            var clusterWidth = actionWidth +
                (hasSuggestion ? suggestionGap + separatorWidth + suggestionGap + suggestionWidth : 0f);
            var actionX = Mathf.Max(messageRect.xMax + 6f, rect.xMax - clusterWidth);

            DrawLink(new Rect(actionX, rect.y, actionWidth, rect.height), actionContent, baseColor, hoverColor, onClick);

            if (hasSuggestion)
            {
                // The separator is decoration, not an action — a plain label with no link affordance.
                _actionStyle.normal.textColor = baseColor;
                _actionStyle.hover.textColor = baseColor;
                GUI.Label(new Rect(actionX + actionWidth + suggestionGap, rect.y, separatorWidth, rect.height),
                    separatorContent, _actionStyle);

                DrawLink(new Rect(actionX + actionWidth + suggestionGap + separatorWidth + suggestionGap, rect.y,
                    suggestionWidth, rect.height), suggestionContent, baseColor, hoverColor, onSuggestion);
            }
        }

        internal static void DrawRequiredNotice(Rect rect, string message, string detail) =>
            DrawNotice(rect, message, actionText: string.Empty, detail: detail, onClick: null);

        private static void DrawLink(Rect linkRect, GUIContent content, Color color, Color hoverColor, Action onClick)
        {
            var hover = linkRect.Contains(Event.current.mousePosition);
            var drawColor = hover ? hoverColor : color;
            _actionStyle.normal.textColor = drawColor;
            _actionStyle.hover.textColor = drawColor;

            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);

            // IMGUI rich text has no <u>, so the underline is a hand-drawn 1px line.
            EditorGUI.DrawRect(new Rect(linkRect.x + 1f, linkRect.yMax - 3f, linkRect.width - 2f, 1f), drawColor);

            if (GUI.Button(linkRect, content, _actionStyle)) onClick();
        }

        // IMGUI has no circle primitive, so the swatch is a tinted, fully rounded 1x1 white texture.
        private static void DrawDot(float x, Rect rect, Color color)
        {
            var dotRect = new Rect(x, rect.y + (rect.height - DotSize) * 0.5f, DotSize, DotSize);
            GUI.DrawTexture(dotRect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                alphaBlend: true, imageAspect: 0f, color: color, borderWidth: 0f, borderRadius: DotSize * 0.5f);
        }
    }
}
