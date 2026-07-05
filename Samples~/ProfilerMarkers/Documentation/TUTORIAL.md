# ProfilerMarkers — Step-by-Step Tutorial

`this.Marker()` gives any method a named `ProfilerMarker` without the boilerplate. The Aspid.FastTools source
generator replaces every call site with a unique marker keyed by `(type, method, line)`; wrapping work in a
`using` scope makes it show up as a named row in the Unity Profiler.

```csharp
using (this.Marker().WithName("Physics")) // Profiler: "ProfilerMarkersTutorial.Physics (<line>)"
    SimulatePhysics();
```

Each lesson maps to one `STEP` section of the `ProfilerMarkersTutorial` component and demonstrates one call form;
the fields are load knobs that change what each marker costs.

## Open the tutorial

1. Open the Welcome window (**Tools → Aspid 🐍 → FastTools → Welcome**) and import the **ProfilerMarkers** sample.
2. Open **`Scenes/ProfilerMarkersTutorial.unity`** and select the **ProfilerMarkers Tutorial** GameObject.
3. Open **Window → Analysis → Profiler** and **enter Play Mode** — the markers appear under `ProfilerMarkersTutorial.*`
   (markers only run during play).

> **The display name is `"{Type}.{name} ({line})"`.** The `(line)` suffix is always appended, so two markers in
> the same method never collide. Pass only the short suffix to `.WithName("Physics")` — the type name is added
> for you.

---

## Lesson 1 — Named block markers

**Fields:** `_physicsIterations`, `_renderIterations` · **Markers:** `ProfilerMarkersTutorial.Physics`,
`ProfilerMarkersTutorial.Render`

A block `using`-statement with an explicit name scopes the marker to the block:

```csharp
using (this.Marker().WithName("Physics"))
    Spin(_physicsIterations, SpinOp.Sqrt);
```

- `.WithName(...)` takes the **short suffix only**; the full marker reads `ProfilerMarkersTutorial.Physics (<line>)`.
- The marker begins when the `using` block is entered and ends when it exits — you never call `Begin`/`End`.
- Raising `_physicsIterations` makes the `Physics` marker cost more; `Render` is independent.

---

## Lesson 2 — Auto-named marker (no `WithName`)

**Field:** `_inputIterations` · **Marker:** `ProfilerMarkersTutorial.SimulateInput`

Drop `.WithName(...)` and use a `using`-**declaration**; the generator names the marker after the enclosing method:

```csharp
private void SimulateInput()
{
    using var _ = this.Marker();  // Profiler: "ProfilerMarkersTutorial.SimulateInput (<line>)"
    Spin(_inputIterations, SpinOp.Tan);
}
```

The marker's scope is the rest of the method body. Use auto-naming when one marker per method is enough and the
method name already says what it does.

---

## Lesson 3 — Nested & per-iteration markers

**Fields:** `_aiAgents`, `_aiStepsPerAgent` · **Markers:** `ProfilerMarkersTutorial.AI` →
`ProfilerMarkersTutorial.AI.Agent`

Markers nest exactly like the `using` scopes that produce them. `SimulateAI` opens an outer `AI` marker, then
wraps every agent in its own `AI.Agent` marker:

```csharp
using (this.Marker().WithName("AI"))
{
    for (var agent = 0; agent < _aiAgents; agent++)
        using (this.Marker().WithName("AI.Agent"))
            Spin(_aiStepsPerAgent, SpinOp.Sin);
}
```

- A marker inside a loop is **one** marker with N samples, not N markers — the `(line)` in the name is fixed per
  call site, so every iteration lands on the same entry.
- Nesting mirrors the code: `AI.Agent` sits inside `AI`, with no manual parent/child wiring.

---

## Lesson 4 — Combined form

**Field:** `_audioIterations` · **Marker:** `ProfilerMarkersTutorial.SimulateAudio` (twice)

One method can carry a method-wide marker **and** a narrower one around a hot sub-step. Both are auto-named after
the method, so only their **line numbers** tell them apart:

```csharp
private void SimulateAudio()
{
    using var _ = this.Marker();     // "ProfilerMarkersTutorial.SimulateAudio (<outer-line>)"
    Spin(_audioIterations, SpinOp.Sqrt);

    using (this.Marker())            // "ProfilerMarkersTutorial.SimulateAudio (<inner-line>)"
        Spin(_audioIterations, SpinOp.Cos);
}
```

You get **two** distinct `ProfilerMarkersTutorial.SimulateAudio` markers with different `(line)` suffixes — the
outer one covers the whole method, the inner one only the nested block. That is exactly why the generator bakes the
line number into the name: same type, same method, still distinct.

---

## Where to look in code

| File | Shows |
|---|---|
| `Scripts/Tutorial/ProfilerMarkersTutorial.cs` | All four lessons as `STEP` sections with load knobs |
| `Scripts/FrameProfiler.cs` | The same three call forms used by a plain component (the `ProfilerMarkers.unity` demo scene) |
| [README.md](README.md) | The compact reference for the three supported forms |
