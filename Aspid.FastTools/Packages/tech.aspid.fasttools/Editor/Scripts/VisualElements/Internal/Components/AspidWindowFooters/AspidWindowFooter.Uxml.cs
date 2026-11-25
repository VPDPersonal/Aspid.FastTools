#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal sealed partial class AspidWindowFooter
    {
        internal new sealed class UxmlFactory : UxmlFactory<AspidWindowFooter, VisualElement.UxmlTraits>
        {
            public UxmlFactory() { }
        }
    }
}
#endif
