using System;
using UnityEngine;
using Aspid.FastTools.Types;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.SerializeReferences
{
    public interface IRequiredDevTestPayload { }

    [Serializable]
    public sealed class RequiredDevTestPayload : IRequiredDevTestPayload
    {
        [SerializeField] private int _value;
    }

    // Dev-only fixture for manually verifying the Project References "Required violations" group and the Asset
    // References REQUIRED badge: both fields are left unset on Prefabs/RequiredViolationsDevTest.prefab on purpose.
    public sealed class RequiredViolationsDevTest : MonoBehaviour
    {
        [SerializeReference, TypeSelector(Required = true)]
        private IRequiredDevTestPayload _requiredReference;

        [TypeSelector(typeof(Component), Required = true)]
        [SerializeField] private string _requiredTypeName;
    }
}
