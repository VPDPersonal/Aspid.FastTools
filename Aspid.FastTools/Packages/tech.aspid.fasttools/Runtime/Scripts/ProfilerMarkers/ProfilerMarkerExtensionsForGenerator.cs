using Unity.Profiling;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
// The namespace is intentionally omitted, as this block serves only as a marker for the Source Generator.
/// <summary>
/// Provides the extension methods that mark call sites for the profiler-marker source generator.
/// </summary>
public static class ProfilerMarkerExtensionsForGenerator
{
    /// <summary>
    /// Opens the <see cref="ProfilerMarker"/> of this call site, named <c>Type.Member (line)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For every type that calls this method on its own instance the generator emits a closer overload that
    /// overload resolution picks instead, holding one <see cref="ProfilerMarker"/> per line of that type.
    /// This overload runs only for calls the generator cannot support; analyzer <c>AFT0010</c> reports them.
    /// It takes the same parameters as the generated overload, so the generated one also wins for a type
    /// in the global namespace; that overload takes a struct by <see langword="in"/>, so the call does not copy it.
    /// </para>
    /// <para>
    /// The class has no namespace, so the method needs no <see langword="using"/> directive and code completion
    /// offers it after any expression; only a call on an instance of the calling type gets a marker.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of <paramref name="instance"/>; generic, so a struct is not boxed.</typeparam>
    /// <param name="instance">The instance the scope is opened on; its value is never read.</param>
    /// <param name="line">The line of the call, filled in by the compiler; the value is never read.</param>
    /// <returns>An empty scope: this overload runs only when the call gets no marker.</returns>
    public static ProfilerMarker.AutoScope Marker<T>(this T instance, [CallerLineNumber] int line = -1) => default;

    /// <summary>
    /// Names the <see cref="ProfilerMarker"/> that the generator creates for the preceding <see cref="Marker{T}(T, int)"/> call.
    /// </summary>
    /// <param name="marker">The scope returned by <see cref="Marker{T}(T, int)"/>.</param>
    /// <param name="name">
    /// The text replacing the member part of the marker name. Read from the source at compile time, so it must be
    /// a constant: a string literal, a <see langword="const"/> string, <c>nameof(...)</c>, or a concatenation or
    /// interpolation of them. Anything else leaves the name untouched, and analyzer <c>AFT0014</c> reports it.
    /// </param>
    /// <returns><paramref name="marker"/> unchanged — at runtime the call is a pass-through.</returns>
    public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name) => marker;
}
