// Mirrors the part of Unity.Profiling.ProfilerMarker that the package and the generated code use.
// ReSharper disable once CheckNamespace
namespace Unity.Profiling
{
    public readonly struct ProfilerMarker
    {
        public ProfilerMarker(string name) { }

        public AutoScope Auto() => default;

        public readonly struct AutoScope : System.IDisposable
        {
            public void Dispose() { }
        }
    }
}
