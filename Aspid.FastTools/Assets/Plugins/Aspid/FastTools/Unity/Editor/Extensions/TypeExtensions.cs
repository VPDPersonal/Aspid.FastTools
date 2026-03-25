using System;
using System.Linq;
using UnityEditor;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors
{
    public static class TypeExtensions
    {
        public static MonoScript FindMonoScript(this Type type)
        {
            var isEnum = type.IsEnum;
            var typeName = type.Name;
            var typeNamespace = type.Namespace;
            
            var scripts = AssetDatabase.FindAssets(filter: $"t:MonoScript {typeName}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                .Where(script => script is not null)
                .ToArray();
            
            var regex = new Regex(GetPattern(isEnum, typeName));

            foreach (var script in scripts)
            {
                if (script.GetClass() != type) continue;
                return script;
            }
            
            foreach (var script in scripts)
            {
                var text = script.text;
                if (string.IsNullOrWhiteSpace(text)) continue;
                if (!string.IsNullOrWhiteSpace(typeNamespace) && !text.Contains($"namespace {typeNamespace}")) continue;
                if (!regex.IsMatch(text)) continue;
                
                return script;
            }
            
            return null;
        }
        
        public static (MonoScript script, int line) FindMonoScriptWithLine(this Type type)
        {
            var script = type.FindMonoScript();
            if (script is null) return (script: null, line: 0);
            
            var line = FindTypeLineNumber(script.text, type.Name, type.IsEnum);
            return (script, line);
        }
        
        private static int FindTypeLineNumber(string text, string typeName, bool isEnum)
        {
            if (string.IsNullOrEmpty(text)) return 1;
            
            var regex = new Regex(GetPattern(isEnum, typeName));
            var lines = text.Split('\n');
            
            for (var i = 0; i < lines.Length; i++)
            {
                if (regex.IsMatch(lines[i]))
                    return i + 1;
            }
            
            return 1;
        }
        
        private static string GetPattern(bool isEnum, string typeName) => isEnum 
            ? $@"\benum\s+{Regex.Escape(typeName)}\b"
            : $@"\b(class|struct|record)\s+{Regex.Escape(typeName)}\b";
    }
}