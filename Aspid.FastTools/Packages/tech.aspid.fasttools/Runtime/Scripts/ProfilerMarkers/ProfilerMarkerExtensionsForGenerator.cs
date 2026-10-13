using Unity.Profiling;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
// The namespace is intentionally omitted, as this block serves only as a marker for the Source Generator.
/// <summary>
/// Provides the extension methods that mark call sites for the profiler-marker source generator.
/// </summary>
/// <remarks>
/// Declared without a namespace, so code completion offers <see cref="Marker{T}(T, int)"/> after any expression.
/// </remarks>
public static class ProfilerMarkerExtensionsForGenerator
{
    // The generated overload of a calling type has the same parameters, so it wins even for a type in the global
    // namespace. Keep this signature in sync with it.
    /// <summary>
    /// Opens the <see cref="ProfilerMarker"/> of this call site, named <c>Type.Member (line)</c>.
    /// </summary>
    /// <remarks>
    /// Only a call on an instance of the calling type gets a marker. Any other call returns an empty scope,
    /// and analyzer <c>AFT0010</c> reports it.
    /// </remarks>
    /// <typeparam name="T">The type of <paramref name="instance"/>.</typeparam>
    /// <param name="instance">The instance of the calling type, usually <see langword="this"/>.</param>
    /// <param name="line">The line of the call; filled in by the compiler.</param>
    /// <returns>The scope of the call site's marker; an empty scope when the call gets no marker.</returns>
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
    /// <returns><paramref name="marker"/>, unchanged.</returns>
    public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name) => marker;
}
