namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// The second half of <see cref="MonoScriptLookupFixtures"/>, deliberately in a file Unity does not associate
    /// with the type (the file name differs), so <see cref="Detached"/> is a nested type whose declaration is
    /// <b>not</b> in the file its declaring type resolves to — the shape that makes the declaring-type fallback
    /// answer with the wrong file unless it verifies the declaration is really there.
    /// </summary>
    internal sealed partial class MonoScriptLookupFixtures
    {
        internal sealed class Detached { }
    }
}
