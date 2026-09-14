# ProfilerMarkers

`this.Marker()` automatically creates a Unity Profiler marker.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly<br />    ProfilerMarker UpdateMarker =<br />    new("MotionSimulation.Update");<br /><br />private void Update()<br />&#123;<br />    using var _ =<br />        UpdateMarker.Auto();<br />    Simulate();<br />&#125;</code></pre> | <pre lang="csharp"><code>private void Update()<br />&#123;<br />    using var _ = this.Marker();<br />    Simulate();<br />&#125;</code></pre> |

Works in `MonoBehaviour` and ordinary C# classes. The generator ships with the package; the extension is in the global namespace — no extra `using`, attributes, or `partial` declaration needed. The marker is named `Type.Method (line)`.

## Scope and names

`Marker()` returns a `ProfilerMarker.AutoScope`: measurement starts at the call and ends when leaving `using`, including on `return` or an exception. `.WithName("Steering")` replaces the method part of the name; the type and line number remain.

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

`WithName` accepts only a string literal: `"Steering"`, `@"Steering"`, or `$"Steering"` without substitutions. Variables, `const`, `nameof`, concatenation, and `$"Agent {index}"` keep the original method name — the generator reads the source text and does not evaluate expressions. The argument is still evaluated at runtime.

> [!IMPORTANT]
> Do not call `this.Marker()` without `using`: measurement will not end automatically. Scopes must not cross `await` or `yield`; measure synchronous sections separately ([Unity limitation](https://docs.unity3d.com/6000.0/Documentation/Manual/profiler-add-markers-code.html)).

## Generation details

- **Line number.** The marker is selected by `CallerLineNumber`, so each call within a type needs its own line, including across `partial` files. Moving the call changes the name suffix.
- **Lambdas and local functions** use the nearest declared member: `Ctor` for constructors, the property name for accessors.
- **Generic types** get separate markers per closed type: `Worker<int>.Run()` → `Worker<Int32>.Run (line)`.
- **Without `ENABLE_PROFILER`** the extension returns `default`: nothing is measured, the code inside `using` still runs, `WithName` arguments are still evaluated.

<a id="result"></a>

## Result in the Profiler

Open **Window → Analysis → Profiler**, enable recording, and run the scene. Select a frame in **CPU Usage → Hierarchy** and search for the type name. Deep Profile is not required.

![Flock and FlockSimulation markers in CPU Usage: Steering.Agent has 120 calls](../Samples~/ProfilerMarkers/Documentation/Images/profiler-markers.png)

Flock and FlockSimulation markers in CPU Usage: Steering.Agent has 120 calls

To reproduce this, import the [ProfilerMarkers sample](../Samples~/ProfilerMarkers/Documentation/README.md#try), open `Scenes/ProfilerMarkers.unity`, and search for `Flock`. Timings depend on the frame and machine.
