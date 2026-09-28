using UnityEngine;

namespace Aspid.FastTools.Types.Tests
{
    [RequireComponent(typeof(ComponentSwapRequirement))]
    public sealed class ComponentSwapNeedsRequirement : ComponentSwapBase
    {
        [SerializeField] private ComponentSwapRequirement _requirement;

        public ComponentSwapRequirement Requirement => _requirement;

        private void OnValidate() => _requirement = GetComponent<ComponentSwapRequirement>();
    }
}
