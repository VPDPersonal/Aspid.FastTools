using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.ComponentTypeSelector
{
    [RequireComponent(typeof(ComponentSwapAbstractRequirement))]
    public sealed class ComponentSwapNeedsAbstractRequirement : ComponentSwapBase { }
}
