#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomEditor(typeof(StringIdRegistry))]
    internal sealed class StringIdRegistryEditor : Editor
    {
        private SerializedProperty _targetTypeProp = null!;
        private SerializedProperty _entriesProp = null!;

        private string _addInput = string.Empty;
        private int _renamingIndex = -1;
        private string _renameInput = string.Empty;

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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_targetTypeProp, new GUIContent("Target Struct Type"));
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("IDs", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            var duplicates = GetDuplicates();
            int indexToDelete = -1;

            for (int i = 0; i < _entriesProp.arraySize; i++)
            {
                var element     = _entriesProp.GetArrayElementAtIndex(i);
                var nameProp    = element.FindPropertyRelative("Name");
                var idProp      = element.FindPropertyRelative("Id");
                var name        = nameProp.stringValue;
                var id          = idProp.intValue;
                var isDuplicate = duplicates.Contains(name);

                if (_renamingIndex == i)
                    DrawRenameRow(i, name, duplicates);
                else
                {
                    if (DrawIdRow(name, id, isDuplicate, out bool deleteRequested))
                        indexToDelete = i;
                }
            }

            if (indexToDelete >= 0)
                TryDeleteEntry(indexToDelete);

            DrawAddRow();

            serializedObject.ApplyModifiedProperties();
        }

        // ─────────────────────────────────────────────
        // Row renderers
        // ─────────────────────────────────────────────

        private bool DrawIdRow(string name, int id, bool isDuplicate, out bool deleteRequested)
        {
            deleteRequested = false;

            using var h = new EditorGUILayout.HorizontalScope();

            if (isDuplicate)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                EditorGUILayout.LabelField(name);
                GUI.color = prev;
                EditorGUILayout.LabelField("(duplicate)", GUILayout.Width(72));
            }
            else
            {
                EditorGUILayout.LabelField(name);
            }

            // Read-only int ID
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.IntField(id, GUILayout.Width(40));

            if (GUILayout.Button("Rename", GUILayout.Width(58)) && _renamingIndex < 0)
            {
                _renamingIndex = Array.IndexOf(GetAllNames(), name);
                _renameInput   = name;
            }

            if (GUILayout.Button("×", GUILayout.Width(20)))
                deleteRequested = true;

            return deleteRequested;
        }

        private void DrawRenameRow(int index, string oldName, HashSet<string> duplicates)
        {
            using var h = new EditorGUILayout.HorizontalScope();

            _renameInput = EditorGUILayout.TextField(_renameInput);

            var trimmed    = _renameInput?.Trim() ?? string.Empty;
            var canConfirm = !string.IsNullOrEmpty(trimmed)
                          && trimmed != oldName
                          && !duplicates.Contains(trimmed);

            using (new EditorGUI.DisabledScope(!canConfirm))
            {
                if (GUILayout.Button("✓", GUILayout.Width(24)))
                    ConfirmRename(index, oldName, trimmed);
            }

            if (GUILayout.Button("✗", GUILayout.Width(24)))
            {
                _renamingIndex = -1;
                _renameInput   = string.Empty;
            }
        }

        private void DrawAddRow()
        {
            EditorGUILayout.Space(4);
            using var h = new EditorGUILayout.HorizontalScope();

            _addInput = EditorGUILayout.TextField(_addInput, GUILayout.ExpandWidth(true));

            var registry = (StringIdRegistry)target;
            var trimmed  = _addInput?.Trim() ?? string.Empty;
            var canAdd   = !string.IsNullOrEmpty(trimmed) && !registry.Contains(trimmed);

            using (new EditorGUI.DisabledScope(!canAdd))
            {
                if (GUILayout.Button("Add", GUILayout.Width(40)))
                {
                    serializedObject.ApplyModifiedProperties();
                    registry.Add(trimmed);
                    EditorUtility.SetDirty(registry);
                    serializedObject.Update();
                    _addInput = string.Empty;
                }
            }
        }

        // ─────────────────────────────────────────────
        // Delete
        // ─────────────────────────────────────────────

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

        // ─────────────────────────────────────────────
        // Rename
        // ─────────────────────────────────────────────

        private void ConfirmRename(int index, string oldName, string newName)
        {
            var structType  = GetStructType();
            var idFieldName = structType != null ? GetStringIdFieldName(structType) : null;

            StringIdRenameDialog.Show(oldName, newName, idFieldName != null, choice =>
            {
                if (choice == StringIdRenameDialog.RenameChoice.Cancel) return;

                var nameProp = _entriesProp.GetArrayElementAtIndex(index).FindPropertyRelative("Name");
                nameProp.stringValue = newName;
                serializedObject.ApplyModifiedProperties();

                _renamingIndex = -1;
                _renameInput   = string.Empty;

                if (choice == StringIdRenameDialog.RenameChoice.RenameEverywhere && idFieldName != null)
                {
                    int replaced = ReplaceInAssets(idFieldName, oldName, newName);
                    if (replaced > 0)
                        Debug.Log($"[StringIdRegistry] Renamed '{oldName}' → '{newName}' in {replaced} asset(s).");
                }
            });
        }

        // ─────────────────────────────────────────────
        // Usage scanning
        // ─────────────────────────────────────────────

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

        private List<(UnityEngine.Object asset, string path)> FindUsages(
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

        private static int ReplaceInAssets(string idFieldName, string oldName, string newName)
        {
            var replaced = 0;

            try
            {
                AssetDatabase.StartAssetEditing();
                replaced += ReplaceInFilter("t:ScriptableObject", idFieldName, oldName, newName, "Replacing in ScriptableObjects");
                replaced += ReplaceInFilter("t:Prefab",           idFieldName, oldName, newName, "Replacing in Prefabs");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
            }

            replaced += ReplaceInScenes(idFieldName, oldName, newName);

            return replaced;
        }

        private static int ReplaceInFilter(string filter, string idFieldName,
            string oldName, string newName, string progressTitle)
        {
            var replaced = 0;
            var guids    = AssetDatabase.FindAssets(filter, new[] { "Assets" });

            for (int i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(progressTitle, assetPath, (float)i / guids.Length);

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset == null) continue;

                if (ReplaceInObject(asset, idFieldName, oldName, newName))
                {
                    EditorUtility.SetDirty(asset);
                    replaced++;
                }
            }

            return replaced;
        }

        private static int ReplaceInScenes(string idFieldName, string oldName, string newName)
        {
            var replaced = 0;
            var guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });

            for (int i = 0; i < guids.Length; i++)
            {
                var scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar("Replacing in Scenes", scenePath, (float)i / guids.Length);

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

                var sceneChanged = false;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var component in root.GetComponentsInChildren<Component>(includeInactive: true))
                    {
                        if (component != null && ReplaceInSerializedObject(new SerializedObject(component), idFieldName, oldName, newName))
                            sceneChanged = true;
                    }
                }

                if (sceneChanged)
                {
                    EditorSceneManager.SaveScene(scene);
                    replaced++;
                }

                if (!alreadyOpen)
                    EditorSceneManager.CloseScene(scene, true);
            }

            EditorUtility.ClearProgressBar();
            return replaced;
        }

        private static bool ReplaceInObject(UnityEngine.Object obj, string idFieldName, string oldName, string newName)
        {
            if (obj is GameObject go)
            {
                var changed = false;
                foreach (var c in go.GetComponentsInChildren<Component>(includeInactive: true))
                {
                    if (c != null)
                        changed |= ReplaceInSerializedObject(new SerializedObject(c), idFieldName, oldName, newName);
                }
                return changed;
            }

            return ReplaceInSerializedObject(new SerializedObject(obj), idFieldName, oldName, newName);
        }

        private static bool ReplaceInSerializedObject(SerializedObject so, string idFieldName,
            string oldName, string newName)
        {
            var iterator = so.GetIterator();
            var changed  = false;

            while (iterator.Next(enterChildren: true))
            {
                if (iterator.propertyType == SerializedPropertyType.String
                    && iterator.name == idFieldName
                    && iterator.stringValue == oldName)
                {
                    iterator.stringValue = newName;
                    changed = true;
                }
            }

            if (changed)
                so.ApplyModifiedPropertiesWithoutUndo();

            return changed;
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

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

        private Type? GetStructType()
        {
            var aqn = _targetTypeProp.stringValue;
            return string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, throwOnError: false);
        }

        /// <summary>Returns the serialized field name used for string display — __stringId (IId structs) or [IdDropdown] field.</summary>
        private static string? GetStringIdFieldName(Type structType)
        {
            // IId-generated structs use __stringId
            if (typeof(IId).IsAssignableFrom(structType))
                return "__stringId";

            // Legacy: [IdDropdown] attribute
            foreach (var field in structType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<IdDropdownAttribute>() != null)
                    return field.Name;
            }

            return null;
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

        private string[] GetAllNames()
        {
            var vals = new string[_entriesProp.arraySize];
            for (int i = 0; i < _entriesProp.arraySize; i++)
                vals[i] = _entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
            return vals;
        }
    }
}
