using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class StringIdRegistryValidator
    {
        public static HashSet<string> GetDuplicates(SerializedProperty entriesProp)
        {
            var seen  = new HashSet<string>();
            var dupes = new HashSet<string>();
            
            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var val = entriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (!string.IsNullOrEmpty(val) && !seen.Add(val))
                    dupes.Add(val);
            }
            
            return dupes;
        }

        public static bool HasDuplicate(StringIdRegistry registry, string entryName)
        {
            var seen = false;
            foreach (var name in registry.IdNames)
            {
                if (name != entryName) continue;
                if (seen) return true;
                seen = true;
            }
            return false;
        }

        public static void CleanUpInvalid(Object target)
        {
            var so       = new SerializedObject(target);
            var entries  = so.FindProperty("_entries");
            var seen     = new HashSet<string>();
            var toRemove = new List<int>();

            for (int i = 0; i < entries.arraySize; i++)
            {
                var val = entries.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue;
                if (string.IsNullOrEmpty(val) || !seen.Add(val))
                    toRemove.Add(i);
            }

            for (var i = toRemove.Count - 1; i >= 0; i--)
                entries.DeleteArrayElementAtIndex(toRemove[i]);

            if (toRemove.Count > 0)
                so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
