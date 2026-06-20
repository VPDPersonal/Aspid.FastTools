using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UnityEngine.UIElements;
using System.Collections.Generic;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Property drawer for <see cref="TypeSelectorAttribute"/>. Dispatches on the property kind: a
    /// <c>[SerializeReference]</c> managed reference gets the hierarchical instance selector (the attribute's
    /// base types optionally narrow the candidates below the declared field type), while a <c>string</c> field
    /// gets the assembly-qualified-name picker constrained by the attribute's base types and <see cref="TypeAllow"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
    internal sealed class TypeSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ThrowExceptionIfInvalidProperty(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                SerializeReferenceIMGUIPropertyDrawer.Draw(
                    position: position,
                    label: label,
                    property: property,
                    baseTypes: GetTypesFromAttribute(property));
                return;
            }

            TypeIMGUIPropertyDrawer.Draw(
                position: position,
                property: property,
                label: label,
                allow: GetTypeAllow(),
                types: GetTypesFromAttribute(property));
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            property.propertyType == SerializedPropertyType.ManagedReference
                ? SerializeReferenceIMGUIPropertyDrawer.GetHeight(property)
                : TypeIMGUIPropertyDrawer.GetHeight(property);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            ThrowExceptionIfInvalidProperty(property);

            if (property.propertyType == SerializedPropertyType.ManagedReference)
                return SerializeReferenceUIToolkitPropertyDrawer.Draw(
                    label: preferredLabel,
                    property: property,
                    baseTypes: GetTypesFromAttribute(property));

            return TypeUIToolkitPropertyDrawer.Draw(
                label: preferredLabel,
                property: property,
                allow: GetTypeAllow(),
                types: GetTypesFromAttribute(property));
        }

        private TypeAllow GetTypeAllow()
        {
            var typeSelectorAttribute = (TypeSelectorAttribute)attribute;
            return typeSelectorAttribute.Allow;
        }

        private Type[] GetTypesFromAttribute(SerializedProperty property)
        {
            var typeSelectorAttribute = (TypeSelectorAttribute)attribute;
            
            var assemblyQualifiedNames = typeSelectorAttribute.AssemblyQualifiedNames
                .Where(assemblyQualifiedName => !string.IsNullOrWhiteSpace(assemblyQualifiedName))
                .ToArray();
            
            if (assemblyQualifiedNames.Length is 0)
                return Array.Empty<Type>();
            
            var targetObject = property.serializedObject.targetObject;
            var targetType = targetObject.GetType();
            var types = new List<Type>();

            foreach (var name in assemblyQualifiedNames)
            {
                var member = GetMemberFromHierarchy(targetType, name);
                
                if (member is not null)
                {
                    AddTypesFromMember(types, member, targetObject);
                }
                else
                {
                    var type = Type.GetType(name, throwOnError: false);
                    
                    if (type is not null)
                        types.Add(type);
                }
            }

            return types.ToArray();
        }

        private static MemberInfo GetMemberFromHierarchy(Type type, string memberName)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;
            
            var currentType = type;
            while (currentType is not null)
            {
                var members = currentType.GetMember(memberName, bindingAttr);
                if (members.Length > 0)
                    return members[0];
                
                currentType = currentType.BaseType;
            }
            
            return null;
        }

        private static void AddTypesFromMember(List<Type> types, MemberInfo member, object targetObject)
        {
            var value = member switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(targetObject),
                PropertyInfo propertyInfo => propertyInfo.GetValue(targetObject),
                _ => null
            };

            switch (value)
            {
                case null: return;
                
                case Type type:
                    types.Add(type);
                    return;
                
                case Type[] typeArray:
                    types.AddRange(typeArray);
                    return;
                
                case string assemblyQualifiedName:
                {
                    var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
                    if (type is not null)
                        types.Add(type);
                    return;
                }
                
                case string[] assemblyQualifiedNames:
                {
                    foreach (var assemblyQualifiedName in assemblyQualifiedNames)
                    {
                        if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) continue;
                        
                        var type = Type.GetType(assemblyQualifiedName, throwOnError: false);
                        if (type is not null)
                            types.Add(type);
                    }
                    return;
                }
            }
        }
        
        private static void ThrowExceptionIfInvalidProperty(SerializedProperty property)
        {
            if (property.propertyType is not (SerializedPropertyType.String or SerializedPropertyType.ManagedReference))
                throw new ArgumentException(
                    "[TypeSelector] can only be applied to a string field or a [SerializeReference] managed-reference field.",
                    nameof(property));
        }
    }
}
