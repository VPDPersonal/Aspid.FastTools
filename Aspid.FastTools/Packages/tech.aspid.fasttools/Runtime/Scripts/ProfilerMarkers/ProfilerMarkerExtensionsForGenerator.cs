using Unity.Profiling;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
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
    /// Only a call on an instance of the calling type gets a marker. Any other call gets no marker of its own,
    /// and analyzer <c>AFT0010</c> reports it.
    /// </remarks>
    /// <typeparam name="T">The calling type.</typeparam>
    /// <param name="instance">The instance of the calling type, usually <see langword="this"/>.</param>
    /// <param name="line">The line of the call; filled in by the compiler.</param>
    /// <returns>The scope of the call site's marker; an empty scope when the call gets no marker.</returns>
    public static ProfilerMarker.AutoScope Marker<T>(this T instance, [CallerLineNumber] int line = -1) => default;

    /// <summary>
    /// Renames the <see cref="ProfilerMarker"/> of the preceding <see cref="Marker{T}(T, int)"/> call.
    /// </summary>
    /// <param name="marker">The scope returned by <see cref="Marker{T}(T, int)"/>.</param>
    /// <param name="name">
    /// Replaces the member part of the marker name. Must be a compile-time constant: a literal,
    /// a <see langword="const"/> string, <c>nameof(...)</c>, or their concatenation or interpolation;
    /// otherwise analyzer <c>AFT0014</c> reports it.
    /// </param>
    /// <returns><paramref name="marker"/>, unchanged.</returns>
    public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name) => marker;
}
