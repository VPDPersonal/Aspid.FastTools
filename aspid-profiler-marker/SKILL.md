---
name: aspid-profiler-marker
description: "Aspid.FastTools generated profiler markers: `using var _ = this.Marker();` and `this.Marker().WithName(\"...\")`. Use when the user asks to profile a method or section, add a ProfilerMarker, or show code in the Unity Profiler in a project with tech.aspid.fasttools, or when such markers are missing, merged or misnamed."
---

# this.Marker()

`this.Marker()` returns a `ProfilerMarker.AutoScope` for a marker that a source generator creates for that exact
call site. No field, name string, attribute or `partial` is needed; works in classes, structs (including Burst jobs)
and ref structs.

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

Profiler nesting follows `using` nesting. Markers in loops, lambdas and local functions are fine. A scope held
across `yield return` or `await` measures only the part before it — put the marker inside each synchronous step.

## Names

`Type.Member (line)`, where `line` is the line of the `Marker()` call:

- constructor -> `Ctor`; static constructor -> `StaticCtor`; property or indexer accessor -> property name / `Indexer`;
  event accessor -> event name; field or auto-property initializer -> field / property name; operator -> `op_Addition`
  and the like; lambda or local function -> the enclosing member; explicit interface implementation -> the interface
  member name;
- nested type -> only the innermost type name (`Agent.Move (line)`), so same-named types in different namespaces or
  outer types show the same name;
- generic class -> one marker per closed type (`Worker<List<Int32>>.Run (line)`); generic struct -> one marker for all
  (`Job<T>.Execute (line)`), so Burst can compile it.

`WithName("X")` replaces only the member part (`FlockSimulation.X (line)`). It is read from source at compile time:
only a literal `"X"`, `@"X"` or a hole-free `$"X"` chained directly on `this.Marker()` counts. Variables, `const`,
`nameof`, concatenation and interpolation holes are ignored (the member name stays). For runtime-computed names use a
hand-written `static readonly ProfilerMarker`.

## Pitfalls

- **Only the type's own instance.** A call gets a marker only when its receiver has the type the call is written
  in — `this`, or another instance of the same type. `other.Marker()` on another type, `((Base)this).Marker()` and
  calls in static classes open nothing; so do default interface methods: call it from the implementing type.
- **Always `using`.** `this.Marker();` as a statement or `_ = this.Marker();` begins a sample that never ends — warning
  `AFT0011`.
- **No arguments, no method groups.** `this.Marker(5)` and `Func<AutoScope> f = this.Marker` do not mark a call site.
- **Helper wrappers merge callers.** `void Profile(Action a) { using (this.Marker()) a(); }` is one call site. Put
  `this.Marker()` at each real call site.
- **One call per line.** Calls on the same line of a type (including across `partial` files) share the first call's
  marker. The line is the one `[CallerLineNumber]` reports, so `#line` directives apply.
- **Line numbers move** with edits; compare Profiler captures by name without the `(line)` suffix.
- **No marker, warning `AFT0010`:** every call the generator cannot mark — the cases above, a call inside a
  `private`/`protected` nested type (or a type nested in one; typical for `private struct MyJob : IJob`), in
  `Outer<T>.Inner<T>` where the inner type parameter shadows the outer one, or inside an expression tree. Fix the call,
  make the nested type `internal`/`public`, rename the parameter, or use a hand-written `ProfilerMarker`.
- `ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED` strips only the package's own markers, not user `this.Marker()` calls.
- Leave existing `Profiler.BeginSample` / `ProfilerMarker` code alone unless the user asks to migrate it.
