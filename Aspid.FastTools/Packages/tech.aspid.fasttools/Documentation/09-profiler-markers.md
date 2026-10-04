# ProfilerMarkers

Profiler markers in one line, with no fields or names to keep up by hand.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _marker =&#10;    new("FlockSimulation.Step");&#10;&#10;public void Step()&#10;&#123;&#10;    using var _ = _marker.Auto();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Step()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    Integrate();&#10;&#125;</code></pre> |

## Marker()

The marker name includes the type, the member name and the call’s line number:

| Where <code lang="csharp">this.Marker()</code> is called | Marker name |
|---|---|
| <code lang="csharp">void Step()</code> | <code lang="string">FlockSimulation.Step (line)</code> |
| <code lang="csharp">FlockSimulation()</code> | <code lang="string">FlockSimulation.Ctor (line)</code> |
| <code lang="csharp">float Speed &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Speed (line)</code> |
| <code lang="csharp">Agent this[int i] &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Indexer (line)</code> |
| <code lang="csharp">event Action Changed</code> | <code lang="string">FlockSimulation.Changed (line)</code> |

The line number changes when the call moves in the source.

<details>
<summary>Nested and generic types</summary>

| Call location | Marker name |
|---|---|
| Method <code lang="csharp">Move()</code> in nested <code lang="class-name">FlockSimulation.Agent</code> | <code lang="string">Agent.Move (line)</code> |
| <code lang="csharp">Step()</code> in class <code lang="class-name">FlockSimulation&lt;Int32&gt;</code> | <code lang="string">FlockSimulation&lt;Int32&gt;.Step (line)</code> |
| <code lang="csharp">Step()</code> in struct <code lang="class-name">FlockSimulation&lt;T&gt;</code> | <code lang="string">FlockSimulation&lt;T&gt;.Step (line)</code> for any <code lang="class-name">T</code> |
| Generic method <code lang="csharp">Step&lt;T&gt;()</code> | <code lang="string">FlockSimulation.Step (line)</code> for any <code lang="class-name">T</code> |

</details>

## WithName()

<code lang="csharp">WithName("Steering")</code> replaces the member name, keeping the type and line number.

```csharp
public void Step()
{
    using var _ = this.Marker();

    using (this.Marker().WithName("Steering"))
    {
        foreach (var agent in _agents)
        {
            using (this.Marker().WithName("Steering.Agent"))
                ComputeSteering(agent);
        }
    }

    using (this.Marker().WithName("Integrate"))
        Integrate();
}
```

> [!NOTE]
> Pass the name as a string literal directly to <code lang="function">WithName</code>. Variables, <code lang="csharp">const</code>, <code lang="csharp">nameof</code> and strings with interpolation holes leave the marker name unchanged; the argument is still evaluated on every call.

## In the Profiler

Markers are created once. After initialization, repeated measurements allocate no memory.

![FlockSimulation marker diagram: Steering and Integrate nested under Step, Steering.Agent with 120 calls. Timings are illustrative.](Images/profiler-markers-hierarchy.svg)

## Limitations

- Call <code lang="csharp">this.Marker()</code> inside its own type. Calls on another type’s instance, in a static class or in a <code lang="csharp">private</code>/<code lang="csharp">protected</code> nested type are unreliable: the measurement may be missing or use another call’s marker.
- Use <code lang="csharp">using</code>. A standalone <code lang="csharp">this.Marker();</code> starts a measurement without ending it.

> [!WARNING]
> Calls within one type that share a line number share a marker, even across different <code lang="csharp">partial</code> files. Give each call site a distinct line number, or measurements will be combined under the first call’s name.

## Package sample

The markers from [WithName()](#withname) run in the [ProfilerMarkers](../Samples~/ProfilerMarkers/Documentation/README.md) scene.

![A flock of agents in the ProfilerMarkers scene](../Samples~/ProfilerMarkers/Documentation/Images/demo.gif)
