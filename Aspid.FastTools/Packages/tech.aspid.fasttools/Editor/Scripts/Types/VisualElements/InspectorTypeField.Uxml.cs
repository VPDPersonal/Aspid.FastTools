#if !UNITY_6000_0_OR_NEWER
using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    public sealed partial class InspectorTypeField
    {
        internal new sealed class UxmlFactory : UxmlFactory<InspectorTypeField, TypeField.UxmlTraits>
        {
            public UxmlFactory() { }
        }
    }
}
#endif
