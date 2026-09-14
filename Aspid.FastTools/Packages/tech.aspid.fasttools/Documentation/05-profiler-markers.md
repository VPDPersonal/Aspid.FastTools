# ProfilerMarkers

`this.Marker()` automatically creates a Unity Profiler marker.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly<br />    ProfilerMarker UpdateMarker =<br />    new("MotionSimulation.Update");<br /><br />private void Update()<br />&#123;<br />    using var _ =<br />        UpdateMarker.Auto();<br />    Simulate();<br />&#125;</code></pre> | <pre lang="csharp"><code>private void Update()<br />&#123;<br />    using var _ = this.Marker();<br />    Simulate();<br />&#125;</code></pre> |

Works in `MonoBehaviour` and ordinary C# classes. The generator ships with the package; the extension is in the global namespace — no extra `using`, attributes, or `partial` declaration needed.

<details>
<summary>Generated code example</summary>

For `MotionSimulation` with `this.Marker()` on line 10. Fully qualified type names are shortened; `GeneratedCode` attributes are omitted.

```csharp
using Unity.Profiling;
using System.Runtime.CompilerServices;

internal static class __MotionSimulationProfilerMarkerExtensions
{
    private static readonly ProfilerMarker Update_Marker_Line_10 =
        new("MotionSimulation.Update (10)");

    public static ProfilerMarker.AutoScope Marker(
        this MotionSimulation _, [CallerLineNumber] int line = -1)
    {
#if ENABLE_PROFILER
        if (line is 10) return Update_Marker_Line_10.Auto();
#endif
        return default;
    }
}
```

</details>

## Measurement scope

`Marker()` returns a `ProfilerMarker.AutoScope`: measurement starts at the call and ends when leaving `using`, including on `return` or an exception.

One example combining nested stages, custom names, and a loop inside `FlockSimulation`:

```csharp
public void Step()
{
    using var _ = this.Marker();

    using (this.Marker().WithName("Steering"))
    {
        foreach (var agent in _agents)
        {
            using var agentScope = this.Marker().WithName("Steering.Agent");
            ComputeSteering(agent);
        }
    }

    using (this.Marker().WithName("Integrate"))
    {
        Integrate();
    }
}
```

In the Profiler with 120 loop iterations:

```text
FlockSimulation.Step (…)
├── FlockSimulation.Steering (…)
│   └── FlockSimulation.Steering.Agent (…) — 120 calls to one marker
└── FlockSimulation.Integrate (…)
```

> [!IMPORTANT]
> Do not call `this.Marker()` without `using`: measurement will not end automatically. Scopes must not cross `await` or `yield`; measure synchronous sections separately ([Unity limitation](https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-add-markers-code.html)).

## Custom names

Append `.WithName("Steering")` directly to `this.Marker()`: `FlockSimulation.Step (line)` becomes `FlockSimulation.Steering (line)`. Only the method name changes; the type and line number remain.

- **Supported:** string literals `"Steering"`, `@"Steering"`, and interpolation without substitutions, `$"Steering"`.
- **Unsupported:** variables, `const`, `nameof`, concatenation, and interpolation with substitutions (`$"Agent {index}"`). These leave the original method name unchanged.

The generator reads literal text from source but does not evaluate expressions, even constant ones. The `WithName` argument is still evaluated at runtime, so a dynamic string may add unnecessary work to the measurement.

## Generation details

- **Line number.** The generated extension selects a static marker by `CallerLineNumber`. Each call within a type must have a distinct line number, including across files of a `partial` type. Moving the call changes the name suffix.
- **Lambdas and local functions.** Names come from the nearest declared method, field, or property. Constructors use `Ctor`; accessors use the property name.
- **Generic types.** Each closed type has its own static markers. For example, `Worker<int>.Run()` → `Worker<Int32>.Run (line)`; arguments are named using `typeof(T).Name`.
- **Without `ENABLE_PROFILER`.** The extension returns `default`, no measurement starts, and the code inside `using` still runs. Static fields remain in the generated source, and `WithName` arguments are still evaluated.

<a id="result"></a>

## Result in the Profiler

Open **Window → Analysis → Profiler**, enable recording, and run the scene. Select a frame in **CPU Usage → Hierarchy** and search for the type name. Deep Profile is not required.

![Flock and FlockSimulation markers in CPU Usage: Steering.Agent has 120 calls](../Samples~/ProfilerMarkers/Documentation/Images/profiler-markers.png)

Flock and FlockSimulation markers in CPU Usage: Steering.Agent has 120 calls

To reproduce this, import the [ProfilerMarkers sample](../Samples~/ProfilerMarkers/Documentation/README.md), open `Scenes/ProfilerMarkers.unity`, and search for `Flock` in the Profiler. Timings depend on the frame and machine; agent-count experiments are in the [sample documentation](../Samples~/ProfilerMarkers/Documentation/README.md#try).
