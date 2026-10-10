// ReSharper disable once CheckNamespace
namespace UnityEditor
{
    // Stands in for UnityEditor.AssetDatabase. The engine calls MakeEditable before every write; without a version
    // control provider Unity returns true for any path, which is what the tests outside Unity need.
    internal static class AssetDatabase
    {
        public static bool MakeEditable(string path) => true;
    }
}
