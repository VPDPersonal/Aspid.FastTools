using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Aspid.FastTools.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    [CustomPropertyDrawer(typeof(ComponentTypeSelector))]
    internal sealed class ComponentTypeSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currentType = property.serializedObject.targetObject.GetType();
            var rowHeight = EditorGUIUtility.singleLineHeight;

            var dropdownRect = new Rect(position.x, position.y, position.width - rowHeight - 2f, rowHeight);
            var openButtonRect = new Rect(dropdownRect.xMax + 2f, position.y, rowHeight, rowHeight);

            if (EditorGUI.DropdownButton(dropdownRect,
                    new GUIContent(TypeSelectorHelpers.GetTypeSelectorTitle(currentType)), FocusType.Passive))
            {
                var persistent = property.Persistent();

                TypeSelectorWindow.Show(
                    GUIUtility.GUIToScreenRect(dropdownRect),
                    CreateFilter(),
                    currentType.AssemblyQualifiedName,
                    onSelected: aqn => ReplaceComponentScript(persistent, currentType, TypeUtility.GetTypeOrNull(aqn)));
            }

            TypeIMGUIPropertyDrawer.DrawOpenScriptButton(openButtonRect, currentType);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var currentType = property.serializedObject.targetObject.GetType();
            var persistent = property.Persistent();
            var filter = CreateFilter();

            var field = new InspectorTypeField(label: null, defaultValue: currentType)
            {
                Types = filter.Types,
                Allow = filter.Allow,
                HideNoneOption = filter.HideNoneOption,
            };

            field.RegisterValueChangedCallback(evt =>
            {
                // The swap is applied on the next editor tick (see ReplaceComponentScript); until then — and for good
                // when no script is found — the caption must keep showing the type the object actually is.
                if (!ReplaceComponentScript(persistent, currentType, evt.newValue))
                    field.SetValueWithoutNotify(currentType);
            });

            field.RegisterCallback<AttachToPanelEvent>(_ => HideScriptField(field));

            return field;
        }

        private TypeSelectorFilter CreateFilter() => new()
        {
            Types = new[] { fieldInfo.DeclaringType },
            Allow = TypeAllow.None,
            HideNoneOption = true,
        };

        private static void HideScriptField(VisualElement field)
        {
            var inspector = field.GetFirstAncestorOfType<InspectorElement>();
            if (inspector is null) return;

            inspector.Query<PropertyField>()
                .Where(propertyField => propertyField.bindingPath == "m_Script")
                .ForEach(propertyField => propertyField.style.display = DisplayStyle.None);
        }

        internal static bool ReplaceComponentScript(SerializedProperty property, Type oldType, Type newType)
        {
            if (newType is null || newType == oldType) return false;

            var script = newType.FindMonoScript();

            // FindMonoScript answers with the file a type is DECLARED in, which for a nested type is the declaring
            // type's file. m_Script must name the script whose own class is the component, so a script reporting a
            // different class is a miss: writing it would silently swap the component for another class.
            if (script is null || script.GetClass() != newType)
            {
                Debug.LogWarning($"[ComponentTypeSelector] MonoScript not found for type: {newType.AssemblyQualifiedName}");
                return false;
            }

            // m_Script is written directly, so Unity checks none of the attributes AddComponent would: refuse the
            // swaps AddComponent or Remove Component would refuse, and add what the new class requires in SwapScript.
            foreach (var target in property.serializedObject.targetObjects)
            {
                if (target is not Component component) continue;

                var conflict = FindSwapConflict(component, newType);
                if (conflict is null) continue;

                Debug.LogWarning($"[ComponentTypeSelector] Cannot change {component.name} to {newType.Name}: {conflict}", component);
                return false;
            }

            // Deferred: the swap rebuilds the inspector, which must not happen from inside the current GUI/event pass.
            EditorApplication.delayCall += () => SwapScript(property.serializedObject, script, newType);

            return true;
        }

        internal static string FindSwapConflict(Component component, Type newType)
        {
            var disallowingType = GetTypeDisallowingMultiple(newType);

            foreach (var other in component.GetComponents<Component>())
            {
                // A component whose script is missing comes back as null.
                if (!other || other == component) continue;

                if (disallowingType is not null && disallowingType.IsInstanceOfType(other))
                    return $"{disallowingType.Name} is [DisallowMultipleComponent] and the GameObject already has {other.GetType().Name}.";

                foreach (var required in GetRequiredTypes(other.GetType()))
                {
                    if (!required.IsInstanceOfType(component) || required.IsAssignableFrom(newType)) continue;
                    if (HasOtherComponent(component, required)) continue;

                    return $"{other.GetType().Name} requires {required.Name}.";
                }
            }

            foreach (var required in GetRequiredTypes(newType))
            {
                if (IsRequirementMet(component, newType, required) || !IsAbstractComponent(required)) continue;
                return $"{newType.Name} requires {required.Name}, which is abstract and cannot be added.";
            }

            return null;
        }

        internal static void SwapScript(SerializedObject serializedObject, MonoScript script, Type newType)
        {
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName($"Change Type to {newType.Name}");
            var undoGroup = Undo.GetCurrentGroup();

            // As AddComponent does, the requirements come first: applying m_Script already runs the new class's
            // OnValidate (and Awake/OnEnable where they run), which must find them.
            foreach (var component in serializedObject.targetObjects.OfType<Component>())
            {
                foreach (var required in GetRequiredTypes(newType))
                {
                    if (IsRequirementMet(component, newType, required)) continue;
                    if (Undo.AddComponent(component.gameObject, required)) continue;

                    // Unity has logged why (e.g. it conflicts with an existing component); keep the object as it was.
                    Undo.RevertAllDownToGroup(undoGroup);
                    Debug.LogWarning($"[ComponentTypeSelector] Cannot change {component.name} to {newType.Name}: it requires {required.Name}, which cannot be added.", component);
                    return;
                }
            }

            serializedObject.FindProperty("m_Script").SetObjectReferenceAndApply(script);
            Undo.CollapseUndoOperations(undoGroup);
        }

        // The component being swapped does not count: it is about to become newType.
        private static bool IsRequirementMet(Component component, Type newType, Type required) =>
            required.IsAssignableFrom(newType) || HasOtherComponent(component, required);

        // Engine base classes such as Collider, Renderer or Joint are abstract only natively; on the managed side they
        // are the built-in components that other component classes derive from. (Transform matches too, but every
        // GameObject already has one.)
        private static bool IsAbstractComponent(Type type) =>
            type.IsAbstract
            || (!typeof(MonoBehaviour).IsAssignableFrom(type) && TypeCache.GetTypesDerivedFrom(type).Count > 0);

        private static bool HasOtherComponent(Component component, Type type)
        {
            foreach (var other in component.GetComponents(type))
            {
                if (other != component) return true;
            }

            return false;
        }

        private static IEnumerable<Type> GetRequiredTypes(Type type)
        {
            foreach (RequireComponent attribute in type.GetCustomAttributes(typeof(RequireComponent), inherit: true))
            {
                if (IsComponentType(attribute.m_Type0)) yield return attribute.m_Type0;
                if (IsComponentType(attribute.m_Type1)) yield return attribute.m_Type1;
                if (IsComponentType(attribute.m_Type2)) yield return attribute.m_Type2;
            }

            static bool IsComponentType(Type type) =>
                type is not null && typeof(Component).IsAssignableFrom(type);
        }

        // Unity's own rule: the topmost class in the hierarchy that declares the attribute; no component derived
        // from it may be added twice.
        private static Type GetTypeDisallowingMultiple(Type type)
        {
            Type result = null;

            for (; type is not null && type != typeof(MonoBehaviour); type = type.BaseType)
            {
                if (Attribute.IsDefined(type, typeof(DisallowMultipleComponent), inherit: false))
                    result = type;
            }

            return result;
        }
    }
}
