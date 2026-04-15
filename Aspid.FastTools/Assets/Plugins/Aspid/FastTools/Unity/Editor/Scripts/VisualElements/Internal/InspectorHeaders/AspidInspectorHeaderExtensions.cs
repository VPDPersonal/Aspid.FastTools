// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Editors.Internal
{
    /// <summary>
    /// Fluent extension methods for <see cref="AspidInspectorHeader"/>.
    /// </summary>
    public static class AspidInspectorHeaderExtensions
    {
        /// <summary>
        /// Sets <see cref="AspidInspectorHeader.Status"/> and returns the element for chaining.
        /// </summary>
        public static AspidInspectorHeader SetStatus(this AspidInspectorHeader element, StatusStyle value)
        {
            element.Status = value;
            return element;
        }
    }
}
