using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomEditor(typeof(IdRegistry))]
    internal sealed class StringIdRegistryEditor : Editor
    {
        private SerializedProperty _targetTypeProp;
        private SerializedProperty _entriesProp;

        private void OnEnable()
        {
            _targetTypeProp = serializedObject.FindProperty("_targetStructType");
            _entriesProp    = serializedObject.FindProperty("_entries");
        }

        private void OnDisable()
        {
            if (target == null) return;
            CleanUpInvalid();
        }

        private const string StyleSheetPath = "Styles/Aspid-FastTools-Id";

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement()
                .AddStyleSheetsFromResource(StyleSheetPath)
                .AddClass("aspid-fasttools-id-registry");

            root.Add(new PropertyField(_targetTypeProp, label: string.Empty));

            var spacer = new VisualElement();
            spacer.style.height = 8;
            root.Add(spacer);

            var idsLabel = new Label("IDs");
            idsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            idsLabel.style.marginBottom = 2;
            root.Add(idsLabel);

            var entriesContainer = new VisualElement();
            root.Add(entriesContainer);
            root.Add(BuildRegistryAddRow());

            root.TrackSerializedObjectValue(serializedObject, _ => RebuildEntries(entriesContainer));
            RebuildEntries(entriesContainer);

            return root;
        }

        private void RebuildEntries(VisualElement container)
        {
            container.Clear();
            var duplicates = GetDuplicates();

            for (int i = 0; i < _entriesProp.arraySize; i++)
            {
                var element     = _entriesProp.GetArrayElementAtIndex(i);
                var name        = element.FindPropertyRelative("Name").stringValue;
                var id          = element.FindPropertyRelative("Id").intValue;
                var isDuplicate = duplicates.Contains(name);
                container.Add(BuildRegistryEntryRow(i, name, id, isDuplicate));
            }
        }

        private VisualElement BuildRegistryEntryRow(int index, string name, int id, bool isDuplicate)
        {
            var container = new VisualElement().AddClass("aspid-fasttools-id-registry-entry");
            var row       = new VisualElement().AddClass("aspid-fasttools-id-registry-row");

            var nameField = new TextField { value = name };
            nameField.AddClass("aspid-fasttools-id-registry-name");
            if (isDuplicate)
            {
                nameField.AddClass("aspid-fasttools-id-registry-name--duplicate");
                row.Add(nameField);
                row.Add(new Label("(duplicate)").AddClass("aspid-fasttools-id-registry-duplicate-label"));
            }
            else
            {
                row.Add(nameField);
            }

            var idField = new IntegerField(string.Empty) { value = id };
            idField.SetEnabled(false);
            idField.AddClass("aspid-fasttools-id-registry-id");
            row.Add(idField);

            var deleteButton = new Button { text = "×" };
            deleteButton.AddClass("aspid-fasttools-id-registry-delete");
            row.Add(deleteButton);

            container.Add(row);

            var errorLabel = new Label().AddClass("aspid-fasttools-id-drawer-error");
            if (isDuplicate)
            {
                errorLabel.text = "Name already exists.";
                errorLabel.SetDisplay(DisplayStyle.Flex);
            }
            container.Add(errorLabel);

            nameField.RegisterCallback<FocusInEvent>(_ =>
            {
                if (HasDuplicate(name))
                {
                    errorLabel.text = "Name already exists.";
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                }
            });

            nameField.RegisterValueChangedCallback(e =>
            {
                var t        = e.newValue?.Trim() ?? string.Empty;
                var registry = (IdRegistry)target;
                if (string.IsNullOrEmpty(t))
                {
                    errorLabel.text = "Name cannot be empty.";
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                }
                else if (HasDuplicate(t) || (t != name && registry.Contains(t)))
                {
                    errorLabel.text = $"'{t}' already exists.";
                    errorLabel.SetDisplay(DisplayStyle.Flex);
                }
                else
                {
                    errorLabel.SetDisplay(DisplayStyle.None);
                }
            });

            nameField.RegisterCallback<FocusOutEvent>(_ =>
            {
                var t        = nameField.value?.Trim() ?? string.Empty;
                var registry = (IdRegistry)target;
                if (!string.IsNullOrEmpty(t) && t != name && !registry.Contains(t))
                {
                    serializedObject.ApplyModifiedProperties();
                    registry.Rename(name, t);
                    EditorUtility.SetDirty(registry);
                    serializedObject.Update();
                    errorLabel.SetDisplay(DisplayStyle.None);
                }
                else
                {
                    nameField.SetValueWithoutNotify(name);
                    if (HasDuplicate(name))
                    {
                        errorLabel.text = "Name already exists.";
                        errorLabel.SetDisplay(DisplayStyle.Flex);
                    }
                    else
                    {
                        errorLabel.SetDisplay(DisplayStyle.None);
                    }
                }
            });

            deleteButton.clicked += () =>
            {
                TryDeleteEntry(index);
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
            };

            return container;
        }

        private VisualElement BuildRegistryAddRow()
        {
            var row = new VisualElement().AddClass("aspid-fasttools-id-registry-add-row");

            var inputField = new TextField();
            inputField.AddClass("aspid-fasttools-id-registry-add-input");

            var addButton = new Button { text = "+" };
            addButton.AddClass("aspid-fasttools-id-registry-add-button");
            addButton.SetEnabled(false);

            inputField.RegisterValueChangedCallback(e =>
            {
                var val      = e.newValue?.Trim() ?? string.Empty;
                var registry = (IdRegistry)target;
                addButton.SetEnabled(!string.IsNullOrEmpty(val) && !registry.Contains(val));
            });

            addButton.clicked += () =>
            {
                var val = inputField.value?.Trim();
                if (string.IsNullOrEmpty(val)) return;
                var registry = (IdRegistry)target;
                serializedObject.ApplyModifiedProperties();
                registry.Add(val);
                EditorUtility.SetDirty(registry);
                serializedObject.Update();
                inputField.SetValueWithoutNotify(string.Empty);
                addButton.SetEnabled(false);
            };

            row.Add(inputField);
            row.Add(addButton);
            return row;
        }
        
        private void TryDeleteEntry(int index)
        {
            var nameProp   = _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Name");
            var nameToDelete = nameProp.stringValue;
            var usageCount   = CountUsages(nameToDelete);

            var message = usageCount == 0
                ? $"Delete '{nameToDelete}'?"
                : $"'{nameToDelete}' is used in {usageCount} asset(s).\n\nFields referencing this ID will show <Missing> after deletion.\n\nDelete anyway?";

            if (EditorUtility.DisplayDialog("Delete ID", message, "Delete", "Cancel"))
                _entriesProp.DeleteArrayElementAtIndex(index);
        }
        
        private int CountUsages(string name)
        {
            var structType  = GetStructType();
            var idFieldName = structType != null ? GetStringIdFieldName(structType) : null;
            if (idFieldName == null) return 0;

            try
            {
                return FindUsages(idFieldName, name, showProgress: true).Count;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static List<(UnityEngine.Object asset, string path)> FindUsages(
            string idFieldName, string name, bool showProgress = false)
        {
            var results = new List<(UnityEngine.Object, string)>();
            ScanAssets("t:ScriptableObject", idFieldName, name, results, showProgress, "Scanning ScriptableObjects");
            ScanPrefabs(idFieldName, name, results, showProgress);
            ScanScenes(idFieldName, name, results, showProgress);
            return results;
        }

        private static void ScanAssets(string filter, string idFieldName, string name,
            List<(UnityEngine.Object, string)> results, bool showProgress, string progressTitle)
        {
            var guids = AssetDatabase.FindAssets(filter, new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (showProgress)
                    EditorUtility.DisplayProgressBar(progressTitle, assetPath, (float)i / guids.Length);

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset == null) continue;

                ScanObject(asset, idFieldName, name, results);
            }
        }

        private static void ScanPrefabs(string idFieldName, string name,
            List<(UnityEngine.Object, string)> results, bool showProgress)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (showProgress)
                    EditorUtility.DisplayProgressBar("Scanning Prefabs", assetPath, (float)i / guids.Length);

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null) continue;

                foreach (var component in prefab.GetComponentsInChildren<Component>(includeInactive: true))
                {
                    if (component != null)
                        ScanObject(component, idFieldName, name, results);
                }
            }
        }

        private static void ScanObject(UnityEngine.Object obj, string idFieldName, string name,
            List<(UnityEngine.Object, string)> results)
        {
            var so       = new SerializedObject(obj);
            var iterator = so.GetIterator();

            while (iterator.Next(enterChildren: true))
            {
                if (iterator.propertyType == SerializedPropertyType.String
                    && iterator.name == idFieldName
                    && iterator.stringValue == name)
                {
                    results.Add((obj, iterator.propertyPath));
                }
            }
        }

        private static void ScanScenes(string idFieldName, string name,
            List<(UnityEngine.Object asset, string path)> results, bool showProgress)
        {
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < guids.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (showProgress)
                    EditorUtility.DisplayProgressBar("Scanning Scenes", scenePath, (float)i / guids.Length);

                var alreadyOpen = false;
                for (int s = 0; s < SceneManager.sceneCount; s++)
                {
                    if (SceneManager.GetSceneAt(s).path == scenePath)
                    {
                        alreadyOpen = true;
                        break;
                    }
                }

                var scene = alreadyOpen
                    ? SceneManager.GetSceneByPath(scenePath)
                    : EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                if (!scene.IsValid())
                {
                    if (!alreadyOpen) EditorSceneManager.CloseScene(scene, true);
                    continue;
                }

                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var component in root.GetComponentsInChildren<Component>(includeInactive: true))
                    {
                        if (component != null)
                            ScanObject(component, idFieldName, name, results);
                    }
                }

                if (!alreadyOpen)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }
        
        private void CleanUpInvalid()
        {
            var so      = new SerializedObject(target);
            var entries = so.FindProperty("_entries");
            var seen    = new HashSet<string>();
            var toRemove = new List<int>();

            for (int i = 0; i < entries.arraySize; i++)
            {
                var val = entries.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (string.IsNullOrEmpty(val) || !seen.Add(val))
                    toRemove.Add(i);
            }

            for (int i = toRemove.Count - 1; i >= 0; i--)
                entries.DeleteArrayElementAtIndex(toRemove[i]);

            if (toRemove.Count > 0)
                so.ApplyModifiedPropertiesWithoutUndo();
        }

        private Type GetStructType()
        {
            var aqn = _targetTypeProp.stringValue;
            return string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, throwOnError: false);
        }

        private static string GetStringIdFieldName(Type structType)
        {
            // IId-generated structs use __stringId
            return typeof(IId).IsAssignableFrom(structType) 
                ? "__stringId"
                : null;
        }

        private HashSet<string> GetDuplicates()
        {
            var seen  = new HashSet<string>();
            var dupes = new HashSet<string>();
            for (int i = 0; i < _entriesProp.arraySize; i++)
            {
                var val = _entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (!string.IsNullOrEmpty(val) && !seen.Add(val))
                    dupes.Add(val);
            }
            return dupes;
        }

        // Reads directly from the C# object — reliable in UIToolkit callbacks
        // where _entriesProp may be stale.
        private bool HasDuplicate(string entryName)
        {
            var count = 0;
            foreach (var e in ((IdRegistry)target).Entries)
                if (e.Name == entryName) count++;
            return count > 1;
        }
    }
}
