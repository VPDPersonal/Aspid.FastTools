# ProfilerMarkers

Provides source-generated `ProfilerMarker` registration. The generator creates a static marker per call-site, identified by the calling method and line number.

```csharp
using UnityEngine;

public class MyBehaviour : MonoBehaviour
{
    private void DoSomething1()
    {
        using var _ = this.Marker();
        // Some code
    }

    private void DoSomething2()
    {
        using (this.Marker())
        {
            // Some code
            using var _ = this.Marker().WithName("Calculate");
            // Some code
        }
    }
}
```

## Generated code

```csharp
using Unity.Profiling;
using System.Runtime.CompilerServices;

internal static class __MyBehaviourProfilerMarkerExtensions
{
    private static readonly ProfilerMarker DoSomething1_Marker_Line_7 = new("MyBehaviour.DoSomething1 (7)");
    private static readonly ProfilerMarker DoSomething2_Marker_Line_13 = new("MyBehaviour.DoSomething2 (13)");
    private static readonly ProfilerMarker DoSomething2_Marker_Line_16 = new("MyBehaviour.Calculate (16)");

    public static ProfilerMarker.AutoScope Marker(this MyBehaviour _, [CallerLineNumberAttribute] int line = -1)
    {
#if ENABLE_PROFILER
        if (line is 7) return DoSomething1_Marker_Line_7.Auto();
        if (line is 13) return DoSomething2_Marker_Line_13.Auto();
        if (line is 16) return DoSomething2_Marker_Line_16.Auto();
#endif
        return default;
    }
}
```

The dispatcher body is wrapped in `#if ENABLE_PROFILER`: in a build without the profiler every call returns `default` and costs nothing.

- **Marker name** — `"{TypeName}.{method} ({line})"`; `.WithName("…")` replaces the member part. For generic enclosing types the name is built with `typeof(T).Name`, so each closed type gets its own marker.
- **Call sites inside lambdas and local functions** resolve to the nearest declared method, field or property.

## Result

![Generated markers in the Unity Profiler window](Images/aspid_fasttools_profiler_markers.png)
