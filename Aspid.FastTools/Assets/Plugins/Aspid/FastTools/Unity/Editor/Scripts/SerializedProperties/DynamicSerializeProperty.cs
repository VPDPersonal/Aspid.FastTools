using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public sealed class DynamicSerializeProperty
    {
        private readonly string _propertyName;
        private readonly Object _targetObject;
        
        public DynamicSerializeProperty(SerializedProperty property)
            : this(property.name, property.serializedObject) { }
        
        public DynamicSerializeProperty(string propertyName, SerializedObject serializedObject)
            : this(propertyName, serializedObject.targetObject) { }
        
        public DynamicSerializeProperty(string propertyName, Object targetObject)
        {
            _propertyName = propertyName;
            _targetObject = targetObject;
        }
        
        public SerializedProperty GetProperty() => 
            new SerializedObject(_targetObject).FindProperty(_propertyName);
        
        public static implicit operator DynamicSerializeProperty(SerializedProperty property) => new(property);
        
        public static implicit operator SerializedProperty(DynamicSerializeProperty property) => property.GetProperty();
    }
}
