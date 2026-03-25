using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
	[CustomPropertyDrawer(typeof(ComponentTypeSelectorAttribute))]
	public sealed class ComponentTypeSelectorPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var currentType = property.serializedObject.targetObject.GetType();

			EditorGUI.BeginProperty(position, label, property);
			{
				var buttonRect = EditorGUI.PrefixLabel(position, label);
				var buttonLabel = new GUIContent(currentType.Name);

				if (GUI.Button(buttonRect, buttonLabel, EditorStyles.popup))
				{
					var screenRect = GUIUtility.GUIToScreenRect(buttonRect);
					
					TypeSelectorWindow.Show(
						new[] { fieldInfo.DeclaringType },
						screenRect,
						currentType.AssemblyQualifiedName,
						aqn => OnTypeSelected(property, aqn, currentType)
					);
				}
			}
			EditorGUI.EndProperty();
		}
		
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var currentType = property.serializedObject.targetObject.GetType();

			var container = new VisualElement();
			container.AddToClassList(BaseField<string>.ussClassName);

			var label = new Label(property.displayName);
			label.AddToClassList(BaseField<string>.labelUssClassName);

			var button = new Button()
				.SetFlexGrow(1)
				.SetText(currentType.Name);
			
			button.AddToClassList(BaseField<string>.inputUssClassName);
			button.clicked += () =>
			{
				var window = EditorWindow.focusedWindow;
				var worldBound = button.worldBound;
				var screenRect = new Rect(window.position.x + worldBound.xMin, window.position.y + worldBound.yMin, worldBound.width, worldBound.height);
				
				TypeSelectorWindow.Show(
					new[] { fieldInfo.DeclaringType },
					screenRect,
					currentType.AssemblyQualifiedName,
					aqn => OnTypeSelected(property, aqn, currentType)
				);
			};

			return container
				.AddChild(label)
				.AddChild(button);
		}

		private static void OnTypeSelected(SerializedProperty property, string aqn, Type currentType)
		{
			var newType = Type.GetType(aqn);
			if (newType is null || newType == currentType) return;

			var script = newType.FindMonoScript();

			if (script is null)
			{
				Debug.LogWarning($"[SubclassDropdown] MonoScript not found for type: {aqn}");
				return;
			}

			property.serializedObject.FindProperty("m_Script").SetObjectReferenceAndApply(script);
		}
	}
}