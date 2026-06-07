// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Non-generic entry point for the generic [SerializeReference] sample.
    //
    // A field typed as IModifier lets [SerializeReferenceSelector] offer both:
    //   - every concrete subclass that closes Modifier<T> over a real type argument
    //     (DamageModifier : Modifier<float>, AmmoModifier : Modifier<int>, NameModifier : Modifier<string>), and
    //   - the open generic Modifier<T> itself — picking it opens a second window to choose the argument T,
    //     then instantiates Modifier<string> / Modifier<float> / etc.
    public interface IModifier
    {
        string Describe();
    }
}
