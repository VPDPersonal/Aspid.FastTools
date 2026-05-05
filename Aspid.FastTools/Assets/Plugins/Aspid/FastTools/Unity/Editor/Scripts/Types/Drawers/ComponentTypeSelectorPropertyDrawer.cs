using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;
using Aspid.FastTools.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
	[CustomPropertyDrawer(typeof(ComponentTypeSelector))]
	internal sealed class ComponentTypeSelectorPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var declaringType = fieldInfo.DeclaringType;
			var currentType = property.serializedObject.targetObject.GetType();
			var buttonRow = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

			if (GUI.Button(buttonRow, new GUIContent(currentType.Name), EditorStyles.popup))
			{
				TypeSelectorWindow.Show(
					GUIUtility.GUIToScreenRect(buttonRow),
					types: new[] { declaringType },
					currentType.AssemblyQualifiedName,
					onSelected: aqn => OnTypeSelected(property, Type.GetType(aqn), currentType));
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			EditorGUIUtility.singleLineHeight;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var declaringType = fieldInfo.DeclaringType;
			var dynamicProperty = new DynamicSerializeProperty(property);
			var currentType = property.serializedObject.targetObject.GetType();
            
			var field = new TypeField(null, currentType)
				{
					Types = new[] { declaringType },
				}
				.AddClass(PropertyField.ussClassName) 
				.AddClass(TypeField.alignedFieldUssClassName);

			field.RegisterValueChangedCallback(evt =>
			{
				OnTypeSelected(dynamicProperty, currentType, evt.newValue);
			});
            
			return field;
		}

		private static void OnTypeSelected(SerializedProperty property, Type oldType, Type newType)
		{
			if (newType is null || newType == oldType) return;

			var script = newType.FindMonoScript();

			if (script is null)
			{
				Debug.LogWarning($"[SubclassDropdown] MonoScript not found for type: {newType.AssemblyQualifiedName}");
				return;
			}

			EditorApplication.delayCall += () =>
				property.serializedObject.FindProperty("m_Script").SetObjectReferenceAndApply(script);
		}
	}
}
