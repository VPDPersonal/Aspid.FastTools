using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements.Editors.Internal
{
    internal static class AspidInspectorHeaderExtensions
    {
        public static AspidInspectorHeader SetText(this AspidInspectorHeader element, string value)
        {
            element.Text = value;
            return element;
        }

        public static AspidInspectorHeader SetObj(this AspidInspectorHeader element, Object value)
        {
            element.Obj = value;
            return element;
        }

        public static AspidInspectorHeader SetSubtext(this AspidInspectorHeader element, string value)
        {
            element.Subtext = value;
            return element;
        }

        public static AspidInspectorHeader SetStatus(this AspidInspectorHeader element, StatusStyle.Type value)
        {
            element.Status = value;
            return element;
        }
    }
}
