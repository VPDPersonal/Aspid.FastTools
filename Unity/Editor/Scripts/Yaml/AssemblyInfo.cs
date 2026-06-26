using System.Runtime.CompilerServices;

// The YAML asset-editing engine ships as internal infrastructure: its types are an implementation detail shared with
// the SerializeReference editor feature (currently the sole consumer) and exercised by its own test assembly. Kept
// internal — rather than promoted to a public API surface — until a second consumer justifies it (see PLAN.md item 8).
[assembly: InternalsVisibleTo("Aspid.FastTools.Unity.Editor")]
[assembly: InternalsVisibleTo("Aspid.FastTools.Unity.Editor.Yaml.Tests")]
// The SerializeReference editor test assembly keeps a couple of integration tests that couple SR-specific code to this
// engine (FindUnsetRequiredFields, ManagedTypeName); they reach the same internals as the production consumer above.
[assembly: InternalsVisibleTo("Aspid.FastTools.Unity.Editor.Tests")]
