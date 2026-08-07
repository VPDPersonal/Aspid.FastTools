namespace Aspid.FastTools.Types.Editors.Tests
{
    /// <summary>
    /// Host for <see cref="MonoScriptLookupTests"/>. Lives in its own file because the lookup under test searches
    /// script assets by file name: the nested type's name deliberately shares no substring with this file, so the
    /// asset search can only reach it by walking out to the declaring type. The type is <c>partial</c> so a second
    /// nested type can live in a file this one does not resolve to — see <c>MonoScriptLookupSecondPart.cs</c>.
    /// </summary>
    internal sealed partial class MonoScriptLookupFixtures
    {
        internal sealed class Concealed { }
    }
}
