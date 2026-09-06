using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types
{
    /// <summary>
    /// Specifies which special type categories the type picker offers in addition to concrete classes.
    /// </summary>
    /// <seealso cref="TypeSelectorAttribute"/>
    [Flags]
    public enum TypeAllow
    {
        /// <summary>
        /// Only concrete types are offered.
        /// </summary>
        None = 0,

        /// <summary>
        /// Abstract classes are offered too. Static classes never are.
        /// </summary>
        Abstract = 1,

        /// <summary>
        /// Interfaces are offered too.
        /// </summary>
        Interface = 2,

        /// <summary>
        /// Both abstract classes and interfaces are offered.
        /// </summary>
        All = Abstract | Interface
    }
}
