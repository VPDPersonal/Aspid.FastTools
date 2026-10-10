using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    internal static class InspectorNoticeGUI
    {
        // Keep these colors aligned with the UI Toolkit notice: --unity-colors-warning-text, the hover tokens
        // --aspid-colors-status-warning-text-lightness / --aspid-colors-light-skin-status-warning-text-hover, and
        // --unity-colors-helpbox-text.
        internal static Color NoticeColor => EditorGUIUtility.isProSkin
            ? new Color32(244, 188, 2, 255)
            : new Color32(51, 51, 8, 255);

        internal static Color NoticeColorHover => EditorGUIUtility.isProSkin
            ? new Color32(255, 235, 175, 255)
            : new Color32(90, 65, 30, 255);

        internal static Color InfoNoticeColor => EditorGUIUtility.isProSkin
            ? new Color32(189, 189, 189, 255)
            : new Color32(22, 22, 22, 255);

        private const float ActionHoverLighten = 0.35f;

        // Unity's light-skin inspector background (--unity-colors-window-background); text needs 4.5:1, plus a margin.
        private static readonly Color LightSkinBackground = new Color32(200, 200, 200, 255);
        private const float MinTextContrast = 4.6f;

        private const float DotSize = 8f;

        private static GUIStyle _messageStyle;
        private static GUIStyle _actionStyle;
        private static GUIStyle _infoMessageStyle;

        internal static void DrawInfoNotice(Rect rect, string message, string detail)
        {
            _infoMessageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _infoMessageStyle.normal.textColor = InfoNoticeColor;

            const float iconSize = 16f;
            var iconRect = new Rect(rect.x, rect.y + (rect.height - iconSize) * 0.5f, iconSize, iconSize);
            GUI.Label(iconRect, EditorGUIUtility.IconContent("console.infoicon"));

            var messageContent = new GUIContent(message, detail);
            var messageRect = new Rect(iconRect.xMax + 4f, rect.y, rect.xMax - iconRect.xMax - 4f, rect.height);
            GUI.Label(messageRect, messageContent, _infoMessageStyle);
        }

        internal static void DrawNotice(Rect rect, string message, string actionText, string detail, Action onClick,
            string suggestionText = null, string suggestionDetail = null, Action onSuggestion = null,
            Color? ridColor = null, Action onMessageClick = null)
        {
            var shared = ridColor.HasValue;
            var lightSkin = !EditorGUIUtility.isProSkin;
            var baseColor = shared ? SharedTextColor(ridColor.Value, lightSkin) : NoticeColor;
            var hoverColor = shared ? SharedHoverColor(baseColor, lightSkin) : NoticeColorHover;

            _messageStyle ??= new GUIStyle(EditorStyles.label) { wordWrap = false };
            _actionStyle ??= new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
            _messageStyle.normal.textColor = baseColor;

            float messageX;
            if (shared)
            {
                DrawDot(rect.x, rect, ridColor.Value);
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
                // Reset the shared style tint left by a previously drawn clickable message.
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

            var clusterWidth = actionWidth +
                (hasSuggestion ? suggestionGap + separatorWidth + suggestionGap + suggestionWidth : 0f);
            var actionX = Mathf.Max(messageRect.xMax + 6f, rect.xMax - clusterWidth);

            DrawLink(new Rect(actionX, rect.y, actionWidth, rect.height), actionContent, baseColor, hoverColor, onClick);

            if (hasSuggestion)
            {
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

        // Rid colors are tuned for the dark skin. On the light skin the dot and stripe keep the rid color, while the
        // text and its link darken, keeping the hue, until they read against the inspector background.
        internal static Color SharedTextColor(Color ridColor, bool lightSkin)
        {
            if (!lightSkin) return ridColor;

            var maxLuminance = (Luminance(LightSkinBackground) + 0.05f) / MinTextContrast - 0.05f;
            var luminance = Luminance(ridColor);
            if (luminance <= maxLuminance) return ridColor;

            var scale = maxLuminance / luminance;
            var linear = ridColor.linear;
            return new Color(linear.r * scale, linear.g * scale, linear.b * scale, ridColor.a).gamma;
        }

        // A hovered link moves away from the background: lighter on the dark skin, darker on the light one.
        internal static Color SharedHoverColor(Color textColor, bool lightSkin) =>
            Color.Lerp(textColor, lightSkin ? Color.black : Color.white, ActionHoverLighten);

        private static void DrawLink(Rect linkRect, GUIContent content, Color color, Color hoverColor, Action onClick)
        {
            var hover = linkRect.Contains(Event.current.mousePosition);
            var drawColor = hover ? hoverColor : color;
            _actionStyle.normal.textColor = drawColor;
            _actionStyle.hover.textColor = drawColor;

            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);

            // IMGUI rich text has no underline tag; draw the underline explicitly.
            EditorGUI.DrawRect(new Rect(linkRect.x + 1f, linkRect.yMax - 3f, linkRect.width - 2f, 1f), drawColor);

            if (GUI.Button(linkRect, content, _actionStyle)) onClick();
        }

        private static float Luminance(Color color)
        {
            var linear = color.linear;
            return 0.2126f * linear.r + 0.7152f * linear.g + 0.0722f * linear.b;
        }

        // IMGUI has no circle primitive; round a tinted white texture instead.
        private static void DrawDot(float x, Rect rect, Color color)
        {
            var dotRect = new Rect(x, rect.y + (rect.height - DotSize) * 0.5f, DotSize, DotSize);
            GUI.DrawTexture(dotRect, Texture2D.whiteTexture, ScaleMode.StretchToFill,
                alphaBlend: true, imageAspect: 0f, color: color, borderWidth: 0f, borderRadius: DotSize * 0.5f);
        }
    }
}
