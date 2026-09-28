using System.Runtime.CompilerServices;

// Aspid.Ids and Aspid.MVVM build their editors on the internal Aspid* components, ThemeStyle and StatusStyle:
// renaming or removing their members breaks those packages, so check them when changing VisualElements/Internal.
[assembly: InternalsVisibleTo(assemblyName: "Aspid.Ids.Editor")]
[assembly: InternalsVisibleTo(assemblyName: "Aspid.MVVM.Unity.Editor")]
[assembly: InternalsVisibleTo(assemblyName: "Aspid.MVVM.StarterKit.Editor")]
[assembly: InternalsVisibleTo(assemblyName: "Aspid.FastTools.Editor.Tests")]
[assembly: InternalsVisibleTo(assemblyName: "Aspid.FastTools.Editor.SerializeReferences.Tests")]
[assembly: InternalsVisibleTo(assemblyName: "Aspid.FastTools.DevTests.CliCommands.Editor")]
