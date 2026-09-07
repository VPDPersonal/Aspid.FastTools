using Unity.Profiling;

// ReSharper disable once CheckNamespace
// The namespace is intentionally omitted, as this block serves only as a marker for the Source Generator.
/// <summary>
/// Provides the extension methods that mark call sites for the profiler-marker source generator.
/// </summary>
public static class ProfilerMarkerExtensionsForGenerator
{
    /// <summary>
    /// Opens a <see cref="ProfilerMarker"/> scope unique to this call site.
    /// </summary>
    /// <param name="instance">The instance the scope is opened on; its value is never read.</param>
    /// <returns>An empty scope, since this body never runs.</returns>
    /// <remarks>
    /// For every type that calls this method the generator emits a closer overload that overload
    /// resolution picks instead, holding one <see cref="ProfilerMarker"/> per enclosing type, member and line.
    /// </remarks>
    public static ProfilerMarker.AutoScope Marker(this object instance) => default;

    /// <summary>
    /// Names the <see cref="ProfilerMarker"/> that the generator creates for the preceding <see cref="Marker(object)"/> call.
    /// </summary>
    /// <param name="marker">The scope returned by <see cref="Marker(object)"/>.</param>
    /// <param name="name">
    /// The text replacing the member part of the marker name. Read from the source at compile time,
    /// so it must be a string literal or an interpolated string without holes; anything else leaves the name untouched.
    /// </param>
    /// <returns><paramref name="marker"/> unchanged — at runtime the call is a pass-through.</returns>
    public static ProfilerMarker.AutoScope WithName(this in ProfilerMarker.AutoScope marker, string name) => marker;
}
