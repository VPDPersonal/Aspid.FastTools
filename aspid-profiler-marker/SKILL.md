---
name: aspid-profiler-marker
description: "Aspid.FastTools generated profiler markers: `using var _ = this.Marker();` and `this.Marker().WithName(\"...\")`. Use when the user asks to profile a method or section, add a ProfilerMarker, or show code in the Unity Profiler in a project with tech.aspid.fasttools, or when such markers are missing, merged or misnamed."
---

# this.Marker()

`this.Marker()` returns a `ProfilerMarker.AutoScope` for a marker that a source generator creates for that exact
call site. No field, name string, attribute or `partial` is needed; works in any class or struct.

- If the user's scripts have an `.asmdef`, it must reference `Aspid.FastTools`. No `using` is needed.
- Without `ENABLE_PROFILER` (release players) every call returns `default`; do not add guards.

```csharp
public void Step()
{
    using var _ = this.Marker();                          // FlockSimulation.Step (12)

    using (this.Marker().WithName("Steering"))            // FlockSimulation.Steering (14)
        foreach (var agent in _agents)
            using (this.Marker().WithName("Steering.Agent"))   // one row, Calls = agent count
                ComputeSteering(agent);
}
```

Profiler nesting follows `using` nesting. Markers in loops, lambdas and local functions are fine.

## Names

`Type.Member (line)`, where `line` is the line of the `Marker()` call:

- constructor -> `Ctor`; property or indexer accessor -> property name / `Indexer`; event accessor -> event name;
  lambda or local function -> the enclosing member; explicit interface implementation -> the interface member name;
- nested type -> only the innermost type name (`Agent.Move (line)`);
- generic type -> one marker per closed type (`Worker<Int32>.Run (line)`).

`WithName("X")` replaces only the member part (`FlockSimulation.X (line)`). It is read from source at compile time:
only a literal `"X"`, `@"X"` or a hole-free `$"X"` chained directly on `this.Marker()` counts. Variables, `const`,
`nameof`, concatenation and interpolation holes are ignored (the member name stays). For runtime-computed names use a
hand-written `static readonly ProfilerMarker`.

## Pitfalls

- **Only `this`.** `other.Marker()` does not measure `other`'s code; static members cannot use `Marker()`.
- **Helper wrappers merge callers.** `void Profile(Action a) { using (this.Marker()) a(); }` is one call site. Put
  `this.Marker()` at each real call site.
- **One call per line.** Calls on the same line of a type (including across `partial` files) share the first call's
  marker.
- **Line numbers move** with edits; compare Profiler captures by name without the `(line)` suffix.
- **No marker, warning `AFT0010`:** a call inside a `private`/`protected` nested type (or a type nested in one), or in
  `Outer<T>.Inner<T>` where the inner type parameter shadows the outer one. Make the nested type `internal`/`public`,
  rename the parameter, or use a hand-written `ProfilerMarker`.
- `ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED` strips only the package's own markers, not user `this.Marker()` calls.
- Leave existing `Profiler.BeginSample` / `ProfilerMarker` code alone unless the user asks to migrate it.
