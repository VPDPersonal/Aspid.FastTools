#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidSwitch
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidSwitch, BaseFieldTraits<bool, UxmlBoolAttributeDescription>>
        {
            public UxmlFactory() { }
        }
    }
}
#endif
