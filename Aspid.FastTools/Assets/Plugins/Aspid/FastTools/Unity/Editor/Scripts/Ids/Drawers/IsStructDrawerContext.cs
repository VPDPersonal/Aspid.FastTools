using System;
using UnityEditor;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal sealed class IsStructDrawerContext
    {
        private readonly string _propertyPath;
            
        public string Label { get; }
        
        public Type FieldType { get; }   
        
        public Type DeclaringType { get; }
        
        public SerializedObject SerializedObject { get; }
            
        public SerializedProperty Property => SerializedObject.FindProperty(_propertyPath);
            
        public SerializedProperty IntIdProperty => Property.FindPropertyRelative(Constants.IntIdFieldName);
            
        public SerializedProperty StringIdProperty => Property.FindPropertyRelative(Constants.StringIdFieldName);

        public IsStructDrawerContext(
            string label, 
            FieldInfo fieldInfo,
            SerializedProperty property)
        {
            Label = label;
            FieldType = fieldInfo.FieldType;
            _propertyPath = property.propertyPath;
            DeclaringType = fieldInfo.DeclaringType;
            SerializedObject = property.serializedObject;
        }

        public void OpenRegistryAsset()
        {
            var registry = FindRegistry();
            if (registry is null) return;
            
            EditorGUIUtility.PingObject(registry);
            Selection.activeObject = registry;
        }
        
        public IdRegistry GetOrCreate() =>
            IdRegistryResolver.GetOrCreate(FieldType);
        
        public IdRegistry FindRegistry() =>
            IdRegistryResolver.Find(FieldType);
        
        public IdRegistry Create() =>
            IdRegistryResolver.Create(FieldType);
        
        public string GetCurrentAssetGuid()
        {
            var path = AssetDatabase.GetAssetPath(SerializedObject.targetObject);
            return string.IsNullOrEmpty(path) ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }
    }
}
