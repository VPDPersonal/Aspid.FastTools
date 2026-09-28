using UnityEngine;

namespace Aspid.FastTools.Types.Tests
{
    /// <summary>
    /// Root of the fixtures for the <c>ComponentTypeSelector</c> swap tests.
    /// </summary>
    /// <remarks>
    /// The fixtures live in a runtime assembly because Unity refuses to add a component declared in an editor one, and
    /// each in a file named after it because the swap writes the class's own <c>MonoScript</c>.
    /// </remarks>
    public abstract class ComponentSwapBase : MonoBehaviour { }
}
