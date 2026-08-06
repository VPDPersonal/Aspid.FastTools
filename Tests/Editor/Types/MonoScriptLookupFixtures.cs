namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Host for <see cref="MonoScriptLookupTests"/>. Lives in its own file because the lookup under test searches
    /// script assets by file name: the nested type's name deliberately shares no substring with this file, so the
    /// asset search can only reach it by walking out to the declaring type.
    /// </summary>
    internal sealed class MonoScriptLookupFixtures
    {
        internal sealed class Concealed { }
    }
}
