# ProfilerMarkers Sample

A flock of cubes whose every frame phase shows up in the Profiler under its own name.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **ProfilerMarkers**.
2. Open `Scenes/ProfilerMarkers.unity` and **Window → Analysis → Profiler**, enter Play Mode and select a frame in the CPU module.
3. In the **Hierarchy** view, expand `PlayerLoop` down to `Flock.Update (74)`, under Unity's own `Flock.Update() [Invoke]` row.

Needs Unity's built-in **Physics** module: without it the sample scripts are skipped and the scene shows Missing Script.

The full tutorial, with what to try in the sample and where to look in its code, is on the site: [ProfilerMarkers sample](https://vpdpersonal.github.io/Aspid.FastTools/tutorials/profiler-markers).
