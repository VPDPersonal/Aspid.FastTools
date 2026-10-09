using System;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace UnityEditor.Compilation
{
    // Stands in for the part of UnityEditor.Compilation that TypeUtility.IsEditorOnlyAssembly reads. Outside Unity no
    // assembly comes from Unity's compilation pipeline, so it lists none.
    internal enum AssembliesType
    {
        Editor,
    }

    [Flags]
    internal enum AssemblyFlags
    {
        None = 0,
        EditorAssembly = 1,
    }

    internal sealed class Assembly
    {
        public string name { get; }

        public AssemblyFlags flags { get; }
    }

    internal static class CompilationPipeline
    {
        public static Assembly[] GetAssemblies(AssembliesType assembliesType) =>
            Array.Empty<Assembly>();
    }
}
