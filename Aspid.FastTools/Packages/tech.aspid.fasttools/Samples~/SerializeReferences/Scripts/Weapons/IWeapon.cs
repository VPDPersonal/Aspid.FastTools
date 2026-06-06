// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.SerializeReferences
{
    // Base interface for the polymorphic [SerializeReference] sample.
    //
    // [SerializeReferenceSelector] lists every concrete, non-UnityEngine.Object class
    // assignable to the field's declared type — here, every IWeapon implementation.
    public interface IWeapon
    {
        string Describe();
    }
}
