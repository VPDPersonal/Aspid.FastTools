using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public sealed class DynamicSerializeProperty
    {
        private readonly string _propertyPath;
        private readonly Object _targetObject;
        
        public DynamicSerializeProperty(SerializedProperty property)
            : this(property.propertyPath, property.serializedObject) { }
        
        public DynamicSerializeProperty(string propertyPath, SerializedObject serializedObject)
            : this(propertyPath, serializedObject.targetObject) { }
        
        public DynamicSerializeProperty(string propertyPath, Object targetObject)
        {
            _propertyPath = propertyPath;
            _targetObject = targetObject;
        }
        
        public SerializedProperty GetProperty() => 
            new SerializedObject(_targetObject).FindProperty(_propertyPath);
        
        public static implicit operator DynamicSerializeProperty(SerializedProperty property) => new(property);
        
        public static implicit operator SerializedProperty(DynamicSerializeProperty property) => property.GetProperty();
    }
}
