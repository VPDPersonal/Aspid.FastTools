using System;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
	[CustomPropertyDrawer(typeof(ComponentTypeSelector))]
	internal sealed class ComponentTypeSelectorPropertyDrawer : PropertyDrawer
	{
		private const string StyleSheetPath = "Styles/Aspid-FastTools-ComponentTypeSelector";
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var currentType = property.serializedObject.targetObject.GetType();
			var buttonRow = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

			if (GUI.Button(buttonRow, new GUIContent(currentType.Name), EditorStyles.popup))
			{
				TypeSelectorWindow.Show
				(
					GUIUtility.GUIToScreenRect(buttonRow),
					types: new[] { fieldInfo.DeclaringType },
					currentType.AssemblyQualifiedName,
					onSelected: aqn => OnTypeSelected(property, aqn, currentType)
				);
			}
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
			EditorGUIUtility.singleLineHeight;

		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var currentType = property.serializedObject.targetObject.GetType();

			var button = new Button()
				.SetText(currentType.Name)
				.AddStyleSheetsFromResource(StyleSheetPath);
			
			button.clicked += () =>
			{
				var window = EditorWindow.focusedWindow;
				var worldBound = button.worldBound;
				var screenRect = new Rect(window.position.x + worldBound.xMin, window.position.y + worldBound.yMin, worldBound.width, worldBound.height);
				var currentT = property.serializedObject.targetObject.GetType();

				TypeSelectorWindow.Show
				(
					screenRect: screenRect,
					types: new[] { fieldInfo.DeclaringType },
					currentAqn: currentT.AssemblyQualifiedName,
					onSelected: aqn => OnTypeSelected(property, aqn, currentT)
				);
			};

			return button;
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