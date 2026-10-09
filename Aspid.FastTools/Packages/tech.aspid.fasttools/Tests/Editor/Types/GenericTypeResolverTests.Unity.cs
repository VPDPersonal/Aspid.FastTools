using System.Linq;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // The same shape twice, differing only in where T lands: one keeps it as serialized data, the other only ever
    // as a managed reference. What the field may close them over differs accordingly.
#pragma warning disable CS0169
    [System.Serializable]
    internal sealed class ResolverValueSequence<T> : IResolverConverter<T, T>
    {
        [UnityEngine.SerializeField] private T _value;
    }

    internal sealed class ResolverReferenceSequence<T> : IResolverConverter<T, T>
    {
        [UnityEngine.SerializeReference] private IResolverConverter<T, T>[] _items;
    }
#pragma warning restore CS0169

    // The tests of this fixture that need Unity or the package's Editor assembly. Aspid.FastTools.TypeTests runs the
    // rest without Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class GenericTypeResolverTests
    {
        [Test]
        public void GetAssignableGenericDefinitions_UnityNativeArgument_IsOfferedClosed()
        {
            // End-to-end with the real argument filter, on the shape the picker reported. ResolverValueSequence
            // stores T, so Vector2 has to clear the serializability bar — and before the engine's own types were
            // recognised it did not, leaving the row open with an argument page that refused Vector2 as well.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<UnityEngine.Vector2, UnityEngine.Vector2>),
                    null, SerializeReferences.Editors.SerializeReferenceHelpers.IsAcceptableGenericArgument)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverValueSequence<UnityEngine.Vector2>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverValueSequence<>),
                "…and its open definition must not be offered beside it.");
        }

        [Test]
        public void GetAssignableGenericDefinitions_ParameterOnlyBehindAReference_ClosesOverAnUnserializableArgument()
        {
            // Two candidates of identical shape against one field. Ray is a type Unity does not serialize, which
            // matters only for the candidate that would store it: the one holding nothing but a
            // [SerializeReference] array closes over Ray happily, and refusing it would hide a usable row.
            var offered = GenericTypeResolver
                .GetAssignableGenericDefinitions(typeof(IResolverConverter<UnityEngine.Ray, UnityEngine.Ray>),
                    null, SerializeReferences.Editors.SerializeReferenceHelpers.IsAcceptableGenericArgument)
                .ToArray();

            CollectionAssert.Contains(offered, typeof(ResolverReferenceSequence<UnityEngine.Ray>));
            CollectionAssert.DoesNotContain(offered, typeof(ResolverValueSequence<UnityEngine.Ray>),
                "The candidate that stores T must not be closed over an argument Unity would drop.");
        }
    }
}
