// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Non-generic entry point. On an IModifier field the picker offers the closed subclasses and the open
    // Modifier<T> itself; picking the latter asks for T on a second page.
    public interface IModifier
    {
        string Describe();

        int ModifyDamage(int damage);
    }
}
