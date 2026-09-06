using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Represents the method that decides whether <paramref name="argument"/> may close
    /// <paramref name="parameter"/>.
    /// </summary>
    /// <remarks>
    /// Unlike a per-type predicate this is asked about a position: what a closed type must store depends on where its
    /// parameter lands, since a parameter reaching a by-value field constrains the argument and one reaching only a
    /// <c>[SerializeReference]</c> field does not.
    /// </remarks>
    /// <param name="openDefinition">The generic definition being closed.</param>
    /// <param name="parameter">The type parameter being closed.</param>
    /// <param name="argument">The concrete type proposed for it.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="argument"/> may close <paramref name="parameter"/>; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public delegate bool GenericArgumentFilter(Type openDefinition, Type parameter, Type argument);
}
