#nullable enable
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomPropertyDrawer(typeof(UniqueIdAttribute))]
    internal sealed class UniqueIdPropertyDrawer : PropertyDrawer
    {
        // Cache uniqueness per (objectId:propertyPath:value) → avoids AssetDatabase calls every frame
        private static readonly System.Collections.Generic.Dictionary<string, (double time, bool isUnique)> _cache = new();
        private const double CacheLifetime = 2.0;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var idField = FindIdField();
            if (idField == null)
                return EditorGUI.GetPropertyHeight(property, label);

            var idProp   = property.FindPropertyRelative(idField.Name);
            var baseH    = IdDropdownDrawer.GetIMGUIHeight(idProp);

            if (!string.IsNullOrEmpty(idProp?.stringValue) && !GetIsUnique(property, idProp))
                baseH += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            return baseH;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idField = FindIdField();
            if (idField == null)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var idProp  = property.FindPropertyRelative(idField.Name);
            var drawH   = IdDropdownDrawer.GetIMGUIHeight(idProp);
            var drawRect = new Rect(position.x, position.y, position.width, drawH);

            IdDropdownDrawer.DrawIMGUI(drawRect, idProp, label, fieldInfo.FieldType);

            if (string.IsNullOrEmpty(idProp.stringValue) || GetIsUnique(property, idProp)) return;

            var warnY    = position.y + drawH + EditorGUIUtility.standardVerticalSpacing;
            var warnRect = new Rect(position.x, warnY, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.HelpBox(warnRect, "ID is not unique among assets of this type", MessageType.Warning);
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var idField = FindIdField();
            if (idField == null)
                return new PropertyField(property, preferredLabel);

            var idProp        = property.FindPropertyRelative(idField.Name);
            var mainElement   = IdDropdownDrawer.DrawUIToolkit(idProp, preferredLabel, fieldInfo.FieldType);

            var warningLabel = new Label("⚠ ID is not unique among assets of this type")
                .SetDisplay(DisplayStyle.None)
                .SetMargin(top: 2);
            warningLabel.style.color    = new Color(1f, 0.75f, 0f);
            warningLabel.style.fontSize = 11;

            var declaringType = fieldInfo.DeclaringType;
            var propPath      = property.propertyPath;
            var idFieldName   = idField.Name;
            var so            = property.serializedObject;

            void Refresh()
            {
                var structProp = so.FindProperty(propPath);
                var ip         = structProp?.FindPropertyRelative(idFieldName);
                if (ip == null) return;

                var isUnique = string.IsNullOrEmpty(ip.stringValue) || CheckIsUnique(structProp!, idFieldName, ip.stringValue, declaringType);
                warningLabel.SetDisplay(isUnique ? DisplayStyle.None : DisplayStyle.Flex);
            }

            warningLabel.TrackPropertyValue(idProp, _ => Refresh());
            warningLabel.schedule.Execute(Refresh).StartingIn(0);

            return new VisualElement()
                .SetFlexDirection(FlexDirection.Column)
                .AddChild(mainElement)
                .AddChild(warningLabel);
        }

        private FieldInfo? FindIdField()
        {
            return Array.Find(
                fieldInfo.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                f => f.GetCustomAttribute<IdDropdownAttribute>() != null);
        }

        private bool GetIsUnique(SerializedProperty structProp, SerializedProperty idProp)
        {
            var key   = $"{structProp.serializedObject.targetObject.GetInstanceID()}:{structProp.propertyPath}:{idProp.stringValue}";
            var now   = EditorApplication.timeSinceStartup;

            if (_cache.TryGetValue(key, out var cached) && now - cached.time < CacheLifetime)
                return cached.isUnique;

            var result = CheckIsUnique(structProp, idProp.name, idProp.stringValue, fieldInfo.DeclaringType);
            _cache[key] = (now, result);
            return result;
        }

        private static bool CheckIsUnique(SerializedProperty structProp, string idFieldName, string idValue, Type? assetType)
        {
            if (assetType == null || string.IsNullOrEmpty(idValue)) return true;

            var currentObject = structProp.serializedObject.targetObject;
            var fullPath      = $"{structProp.propertyPath}.{idFieldName}";
            var guids         = AssetDatabase.FindAssets($"t:{assetType.Name}");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset     = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                if (asset == null || asset == currentObject) continue;

                var otherSo    = new SerializedObject(asset);
                var otherValue = otherSo.FindProperty(fullPath)?.stringValue;
                if (otherValue == idValue) return false;
            }

            return true;
        }
    }
}
