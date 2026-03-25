#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    [CustomEditor(typeof(StringIdRegistry))]
    internal sealed class StringIdRegistryEditor : Editor
    {
        private SerializedProperty _targetTypeProp = null!;
        private SerializedProperty _idsProp = null!;

        private string _addInput = string.Empty;
        private int _renamingIndex = -1;
        private string _renameInput = string.Empty;

        private void OnEnable()
        {
            _targetTypeProp = serializedObject.FindProperty("_targetStructType");
            _idsProp        = serializedObject.FindProperty("_ids");
        }

        private void OnDisable()
        {
            if (target == null) return;
            CleanUpInvalid();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Target struct type (uses existing TypeSelector drawer)
            EditorGUILayout.PropertyField(_targetTypeProp, new GUIContent("Target Struct Type"));
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("IDs", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            var duplicates = GetDuplicates();
            int indexToDelete = -1;

            for (int i = 0; i < _idsProp.arraySize; i++)
            {
                var element   = _idsProp.GetArrayElementAtIndex(i);
                var value     = element.stringValue;
                var isDuplicate = duplicates.Contains(value);

                if (_renamingIndex == i)
                {
                    DrawRenameRow(i, value, duplicates);
                }
                else
                {
                    if (DrawIdRow(value, isDuplicate, out bool deleteRequested))
                        indexToDelete = i;
                }
            }

            if (indexToDelete >= 0)
                TryDeleteId(indexToDelete);

            DrawAddRow();

            serializedObject.ApplyModifiedProperties();
        }

        // ─────────────────────────────────────────────
        // Row renderers
        // ─────────────────────────────────────────────

        private bool DrawIdRow(string value, bool isDuplicate, out bool deleteRequested)
        {
            deleteRequested = false;

            using var h = new EditorGUILayout.HorizontalScope();

            if (isDuplicate)
            {
                var prev = GUI.color;
                GUI.color = new Color(1f, 0.4f, 0.4f);
                EditorGUILayout.LabelField(value);
                GUI.color = prev;
                EditorGUILayout.LabelField("(duplicate)", GUILayout.Width(72));
            }
            else
            {
                EditorGUILayout.LabelField(value);
            }

            if (GUILayout.Button("Rename", GUILayout.Width(58)) && _renamingIndex < 0)
            {
                _renamingIndex = Array.IndexOf(GetAllValues(), value);
                _renameInput   = value;
            }

            if (GUILayout.Button("×", GUILayout.Width(20)))
                deleteRequested = true;

            return deleteRequested;
        }

        private void DrawRenameRow(int index, string oldValue, HashSet<string> duplicates)
        {
            using var h = new EditorGUILayout.HorizontalScope();

            _renameInput = EditorGUILayout.TextField(_renameInput);

            var trimmed    = _renameInput?.Trim() ?? string.Empty;
            var canConfirm = !string.IsNullOrEmpty(trimmed)
                          && trimmed != oldValue
                          && !duplicates.Contains(trimmed);

            using (new EditorGUI.DisabledScope(!canConfirm))
            {
                if (GUILayout.Button("✓", GUILayout.Width(24)))
                    ConfirmRename(index, oldValue, trimmed);
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
                    var size = _idsProp.arraySize;
                    _idsProp.InsertArrayElementAtIndex(size);
                    _idsProp.GetArrayElementAtIndex(size).stringValue = trimmed;
                    serializedObject.ApplyModifiedProperties();
                    _addInput = string.Empty;
                }
            }
        }

        // ─────────────────────────────────────────────
        // Delete
        // ─────────────────────────────────────────────

        private void TryDeleteId(int index)
        {
            var idToDelete  = _idsProp.GetArrayElementAtIndex(index).stringValue;
            var usageCount  = CountUsages(idToDelete);

            var message = usageCount == 0
                ? $"Delete '{idToDelete}'?"
                : $"'{idToDelete}' is used in {usageCount} asset(s).\n\nFields referencing this ID will show <Missing> after deletion.\n\nDelete anyway?";

            if (EditorUtility.DisplayDialog("Delete ID", message, "Delete", "Cancel"))
                _idsProp.DeleteArrayElementAtIndex(index);
        }

        // ─────────────────────────────────────────────
        // Rename
        // ─────────────────────────────────────────────

        private void ConfirmRename(int index, string oldId, string newId)
        {
            _idsProp.GetArrayElementAtIndex(index).stringValue = newId;
            serializedObject.ApplyModifiedProperties();

            _renamingIndex = -1;
            _renameInput   = string.Empty;

            var structType  = GetStructType();
            var idFieldName = structType != null ? GetIdFieldName(structType) : null;

            if (idFieldName == null) return;

            int replaced = ReplaceInAssets(idFieldName, oldId, newId);
            if (replaced > 0)
                Debug.Log($"[StringIdRegistry] Renamed '{oldId}' → '{newId}' in {replaced} asset(s).");
        }

        // ─────────────────────────────────────────────
        // Usage scanning
        // ─────────────────────────────────────────────

        private int CountUsages(string id)
        {
            var structType  = GetStructType();
            var idFieldName = structType != null ? GetIdFieldName(structType) : null;
            if (idFieldName == null) return 0;

            try
            {
                return FindUsages(idFieldName, id, showProgress: true).Count;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private List<(UnityEngine.Object asset, string path)> FindUsages(
            string idFieldName, string id, bool showProgress = false)
        {
            var results = new List<(UnityEngine.Object, string)>();
            ScanAssets("t:ScriptableObject", idFieldName, id, results, showProgress, "Scanning ScriptableObjects");
            ScanPrefabs(idFieldName, id, results, showProgress);
            return results;
        }

        private static void ScanAssets(string filter, string idFieldName, string id,
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

                ScanObject(asset, idFieldName, id, results);
            }
        }

        private static void ScanPrefabs(string idFieldName, string id,
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
                        ScanObject(component, idFieldName, id, results);
                }
            }
        }

        private static void ScanObject(UnityEngine.Object obj, string idFieldName, string id,
            List<(UnityEngine.Object, string)> results)
        {
            var so       = new SerializedObject(obj);
            var iterator = so.GetIterator();

            while (iterator.Next(enterChildren: true))
            {
                if (iterator.propertyType == SerializedPropertyType.String
                    && iterator.name == idFieldName
                    && iterator.stringValue == id)
                {
                    results.Add((obj, iterator.propertyPath));
                }
            }
        }

        private static int ReplaceInAssets(string idFieldName, string oldId, string newId)
        {
            var replaced = 0;

            try
            {
                AssetDatabase.StartAssetEditing();
                replaced += ReplaceInFilter("t:ScriptableObject", idFieldName, oldId, newId, "Replacing in ScriptableObjects");
                replaced += ReplaceInFilter("t:Prefab",           idFieldName, oldId, newId, "Replacing in Prefabs");
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                EditorUtility.ClearProgressBar();
                AssetDatabase.SaveAssets();
            }

            return replaced;
        }

        private static int ReplaceInFilter(string filter, string idFieldName,
            string oldId, string newId, string progressTitle)
        {
            var replaced = 0;
            var guids    = AssetDatabase.FindAssets(filter, new[] { "Assets" });

            for (int i = 0; i < guids.Length; i++)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(progressTitle, assetPath, (float)i / guids.Length);

                var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset == null) continue;

                if (ReplaceInObject(asset, idFieldName, oldId, newId))
                {
                    EditorUtility.SetDirty(asset);
                    replaced++;
                }
            }

            return replaced;
        }

        private static bool ReplaceInObject(UnityEngine.Object obj, string idFieldName, string oldId, string newId)
        {
            if (obj is GameObject go)
            {
                var changed = false;
                foreach (var c in go.GetComponentsInChildren<Component>(includeInactive: true))
                {
                    if (c != null)
                        changed |= ReplaceInSerializedObject(new SerializedObject(c), idFieldName, oldId, newId);
                }
                return changed;
            }

            return ReplaceInSerializedObject(new SerializedObject(obj), idFieldName, oldId, newId);
        }

        private static bool ReplaceInSerializedObject(SerializedObject so, string idFieldName,
            string oldId, string newId)
        {
            var iterator = so.GetIterator();
            var changed  = false;

            while (iterator.Next(enterChildren: true))
            {
                if (iterator.propertyType == SerializedPropertyType.String
                    && iterator.name == idFieldName
                    && iterator.stringValue == oldId)
                {
                    iterator.stringValue = newId;
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
            var so   = new SerializedObject(target);
            var ids  = so.FindProperty("_ids");
            var seen = new HashSet<string>();
            var toRemove = new List<int>();

            for (int i = 0; i < ids.arraySize; i++)
            {
                var val = ids.GetArrayElementAtIndex(i).stringValue;
                if (string.IsNullOrEmpty(val) || !seen.Add(val))
                    toRemove.Add(i);
            }

            for (int i = toRemove.Count - 1; i >= 0; i--)
                ids.DeleteArrayElementAtIndex(toRemove[i]);

            if (toRemove.Count > 0)
                so.ApplyModifiedPropertiesWithoutUndo();
        }

        private Type? GetStructType()
        {
            var aqn = _targetTypeProp.stringValue;
            return string.IsNullOrEmpty(aqn) ? null : Type.GetType(aqn, throwOnError: false);
        }

        private static string? GetIdFieldName(Type structType)
        {
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
            for (int i = 0; i < _idsProp.arraySize; i++)
            {
                var val = _idsProp.GetArrayElementAtIndex(i).stringValue;
                if (!string.IsNullOrEmpty(val) && !seen.Add(val))
                    dupes.Add(val);
            }
            return dupes;
        }

        private string[] GetAllValues()
        {
            var vals = new string[_idsProp.arraySize];
            for (int i = 0; i < _idsProp.arraySize; i++)
                vals[i] = _idsProp.GetArrayElementAtIndex(i).stringValue;
            return vals;
        }
    }
}
