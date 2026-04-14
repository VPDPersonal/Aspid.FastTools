// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Internal
{
    /// <summary>
    /// Defines the visual status of an Aspid UI element, controlling its color accent.
    /// </summary>
    public enum StatusStyle
    {
        /// <summary>
        /// No status applied.
        /// </summary>
        None,

        /// <summary>
        /// Indicates a successful or positive state.
        /// </summary>
        Success,

        /// <summary>
        /// Indicates a warning or cautionary state.
        /// </summary>
        Warning,

        /// <summary>
        /// Indicates an error or critical state.
        /// </summary>
        Error,

        /// <summary>
        /// Indicates an informational state.
        /// </summary>
        Info,
    }
}