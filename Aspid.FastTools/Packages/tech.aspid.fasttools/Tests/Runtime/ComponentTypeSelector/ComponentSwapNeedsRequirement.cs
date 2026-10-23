using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Tests.ComponentTypeSelector
{
    [RequireComponent(typeof(ComponentSwapRequirement))]
    public sealed class ComponentSwapNeedsRequirement : ComponentSwapBase
    {
        [SerializeField] private ComponentSwapRequirement _requirement;

        public ComponentSwapRequirement Requirement => _requirement;

        private void OnValidate() => _requirement = GetComponent<ComponentSwapRequirement>();
    }
}
