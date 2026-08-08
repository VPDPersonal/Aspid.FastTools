using System;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Decides whether <paramref name="argument"/> may close <paramref name="parameter"/> of
    /// <paramref name="openDefinition"/>. Unlike a plain per-type predicate this is asked <i>about a position</i>,
    /// because what a closed type has to store depends on where its parameter lands: a parameter reaching a field
    /// the engine writes by value constrains the argument, one reaching only a <c>[SerializeReference]</c> field
    /// does not.
    /// </summary>
    /// <param name="openDefinition">The generic definition being closed.</param>
    /// <param name="parameter">The type parameter of <paramref name="openDefinition"/> being closed.</param>
    /// <param name="argument">The concrete type proposed for it.</param>
    public delegate bool GenericArgumentFilter(Type openDefinition, Type parameter, Type argument);
}
