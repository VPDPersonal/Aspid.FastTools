using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Represents the method that decides whether <paramref name="argument"/> may close
    /// <paramref name="parameter"/>.
    /// </summary>
    /// <param name="openDefinition">The generic definition being closed.</param>
    /// <param name="parameter">The type parameter being closed.</param>
    /// <param name="argument">The concrete type proposed for it.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="argument"/> may close <paramref name="parameter"/>; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public delegate bool GenericArgumentFilter(Type openDefinition, Type parameter, Type argument);
}
