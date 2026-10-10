# ProfilerMarkers Sample

A flock of cubes whose every frame phase shows up in the Profiler under its own name.

![The flock simulation whose phases the markers measure.](Images/demo.gif)

The flock simulation whose phases the markers measure.

## Open it

1. Import the sample: **Tools → Aspid 🐍 → FastTools → Welcome** → **Samples** → **Import** on **ProfilerMarkers**.
2. Open `Scenes/ProfilerMarkers.unity` and **Window → Analysis → Profiler**, enter Play Mode and select a frame in the CPU module.
3. In the **Hierarchy** view, expand `PlayerLoop` down to <code lang="string">Flock.Update (74)</code>, under Unity's own <code lang="string">Flock.Update() [Invoke]</code> row.

Needs Unity's built-in **Physics** module: without it the sample scripts are skipped and the scene shows Missing Script.

## Try

![The markers under Flock.Update nest like the code; FlockSimulation.Steering.Agent has 120 calls for 120 agents.](Images/profiler-markers.png)

The markers under Flock.Update nest like the code; FlockSimulation.Steering.Agent has 120 calls for 120 agents.

1. **The tree follows the <code lang="csharp">using</code> scopes.** Under <code lang="string">Flock.Update</code> sit <code lang="string">FlockSimulation.Step</code> and, next to it, <code lang="string">Flock.ApplyTransforms</code>; under <code lang="string">Step</code> sit <code lang="string">FlockSimulation.Steering</code> and <code lang="string">FlockSimulation.Integrate</code>. Nothing wires the nesting, the scopes in the code do:

   ```csharp
   public void Step(float deltaTime, float neighborRadius, float maxSpeed)
   {
       using var _ = this.Marker();

       using (this.Marker().WithName("Steering"))
           ComputeSteering(neighborRadius);

       using (this.Marker().WithName("Integrate"))
           Integrate(deltaTime, maxSpeed);
   }
   ```

2. **One marker per loop.** <code lang="string">FlockSimulation.Steering.Agent</code> sits inside the loop over the agents: the Profiler shows one row with `Calls` equal to the agent count, not a row per agent.
3. **More agents.** In Play Mode, raise **Count** on **Flock** to `400`: the next frame recreates the flock, `Calls` of <code lang="string">FlockSimulation.Steering.Agent</code> becomes `400`, and <code lang="string">FlockSimulation.Steering</code> takes longer.
4. **Not only MonoBehaviour.** <code lang="class-name">FlockSimulation</code> is a plain C# class, and its markers work the same way. The marker in the local function <code lang="function">CreateAgent</code> is named after the enclosing method, <code lang="string">Flock.InitializeAgents (53)</code>: find it in the first frame or in the frame where **Count** changes, with `Calls` equal to the number of agents created.
5. **The line number in the name.** Every name ends with the line of its call, <code lang="string">FlockSimulation.Steering (62)</code>, so markers on different lines of one method never share a name, and a moved call changes its number. Search the Profiler for <code lang="string">Flock</code> to list every marker of the sample.
6. **Release builds.** The generated marker code is wrapped in <code lang="csharp">#if ENABLE_PROFILER</code>: in a player build without **Development Build** the calls measure nothing and return <code lang="csharp">default</code>.

## Where to look

| File | Shows |
|---|---|
| `Scripts/FlockSimulation.cs` | Method-wide, block and per-iteration markers in a plain class |
| `Scripts/Flock.cs` | The frame entry point, a marker inside a local function |

Reference: [ProfilerMarkers](../../docs/09-profiler-markers.md).
