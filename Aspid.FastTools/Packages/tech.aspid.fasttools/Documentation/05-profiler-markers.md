# ProfilerMarkers

Profiler markers in one line, with no fields or names to keep up by hand.

## Quick start

| Before — Unity API | After — FastTools |
|---|---|
| <pre lang="csharp"><code>private static readonly&#10;    ProfilerMarker _marker =&#10;    new("FlockSimulation.Step");&#10;&#10;public void Step()&#10;&#123;&#10;    using var _ = _marker.Auto();&#10;    Integrate();&#10;&#125;</code></pre> | <pre lang="csharp"><code>public void Step()&#10;&#123;&#10;    using var _ = this.Marker();&#10;    Integrate();&#10;&#125;</code></pre> |

## Marker()

The marker name is built from the type, the method and the call's line number:

| Where <code lang="csharp">this.Marker()</code> is called | Marker name |
|---|---|
| <code lang="csharp">void Step()</code> | <code lang="string">FlockSimulation.Step (line)</code> |
| <code lang="csharp">FlockSimulation()</code> | <code lang="string">FlockSimulation.Ctor (line)</code> |
| <code lang="csharp">float Speed &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Speed (line)</code> |
| <code lang="csharp">Agent this[int i] &#123; get; &#125;</code> | <code lang="string">FlockSimulation.Indexer (line)</code> |
| <code lang="csharp">event Action Changed</code> | <code lang="string">FlockSimulation.Changed (line)</code> |
| <code lang="csharp">class FlockSimulation.Agent &#123; void Move() &#125;</code> | <code lang="string">Agent.Move (line)</code> |
| <code lang="csharp">class Worker&lt;T&gt; &#123; void Run() &#125;</code> | <code lang="string">Worker&lt;Int32&gt;.Run (line)</code><br /><code lang="string">Worker&lt;Single&gt;.Run (line)</code> |
| <code lang="csharp">void Run&lt;T&gt;()</code> | <code lang="string">FlockSimulation.Run (line)</code> for any <code lang="class-name">T</code> |

## WithName()

<code lang="csharp">.WithName("Steering")</code> replaces the method in the marker name with its own text: <code lang="string">FlockSimulation.Step (5)</code> → <code lang="string">FlockSimulation.Steering (5)</code>.

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
> Only a string literal works: the generator reads the name from the source. With a variable, <code lang="csharp">const</code>, <code lang="csharp">nameof</code> or <code lang="csharp">$"Agent &#123;index&#125;"</code> the method name stays, and the argument is still evaluated on every call.

## In the Profiler

The generator creates one static field per call site, so measuring allocates nothing.

![FlockSimulation marker diagram: Steering and Integrate nested under Step, Steering.Agent with 120 calls. Timings are illustrative.](Images/profiler-markers-hierarchy.svg)

## Limitations

- **Only <code lang="csharp">this</code>.** The marker opens on an instance of its own type, so static methods cannot hold one.
- **The line number in the name** changes when the call moves: compare captures from before and after an edit by the name without it.
- **Private and protected nested types** get no marker — analyzer `AFT0010` warns about it.

> [!WARNING]
> A measurement can land in someone else's Profiler row when <code lang="csharp">this.Marker()</code> calls in different <code lang="csharp">partial</code> files of one type sit on the same line, or when the call is made on an object of another type — <code lang="csharp">other.Marker()</code>.

## Package sample

The markers from [WithName()](#withname) run in the [ProfilerMarkers](../Samples~/ProfilerMarkers/Documentation/README.md) scene.

![A flock of agents in the ProfilerMarkers scene](../Samples~/ProfilerMarkers/Documentation/Images/demo.gif)
