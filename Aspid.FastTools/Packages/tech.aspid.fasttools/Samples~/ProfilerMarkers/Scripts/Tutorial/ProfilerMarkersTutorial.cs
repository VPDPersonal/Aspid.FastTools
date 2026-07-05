using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.ProfilerMarkers
{
    // Step-by-step tour of this.Marker(): each [Header("STEP N …")] is one lesson.
    // Enter Play Mode and open Window → Analysis → Profiler — every marker shows up under
    // "ProfilerMarkersTutorial.*". The fields below are load knobs that change what each marker costs.
    // Open Scenes/ProfilerMarkersTutorial.unity and follow TUTORIAL.md / TUTORIAL_RU.md.
    public sealed class ProfilerMarkersTutorial : MonoBehaviour
    {
        [Header("STEP 1 — Named block markers")]
        [SerializeField] [Min(0)]
        [Tooltip("Work inside two .WithName() block markers → ProfilerMarkersTutorial.Physics and .Render.")]
        private int _physicsIterations = 5000;

        [SerializeField] [Min(0)]
        [Tooltip("Load for the second named block marker — ProfilerMarkersTutorial.Render.")]
        private int _renderIterations = 3000;

        [Header("STEP 2 — Auto-named marker (no WithName)")]
        [SerializeField] [Min(0)]
        [Tooltip("A using-declaration without .WithName(): the generator names the marker after the " +
                 "enclosing method — ProfilerMarkersTutorial.SimulateInput.")]
        private int _inputIterations = 1500;

        [Header("STEP 3 — Nested & per-iteration markers")]
        [SerializeField] [Min(0)]
        [Tooltip("Each agent is wrapped in its own marker inside the 'AI' scope. N agents → one " +
                 "marker 'AI.Agent' with N samples, nested under 'AI'.")]
        private int _aiAgents = 20;

        [SerializeField] [Min(0)]
        [Tooltip("Work per agent inside the nested ProfilerMarkersTutorial.AI.Agent marker.")]
        private int _aiStepsPerAgent = 500;

        [Header("STEP 4 — Combined form")]
        [SerializeField] [Min(0)]
        [Tooltip("A method-wide using-declaration plus a nested using-statement — two distinct markers " +
                 "on the same method (ProfilerMarkersTutorial.SimulateAudio), told apart by line number.")]
        private int _audioIterations = 2000;

        private void Update()
        {
            // Every call site of this.Marker() becomes a unique ProfilerMarker.
            // Display name: "{TypeName}.{WithName-or-method} ({line})".
            using (this.Marker().WithName("Physics")) // Profiler: ProfilerMarkersTutorial.Physics
                Spin(_physicsIterations, SpinOp.Sqrt);

            SimulateAI();

            using (this.Marker().WithName("Render")) // Profiler: ProfilerMarkersTutorial.Render
                Spin(_renderIterations, SpinOp.Cos);

            SimulateInput();
            SimulateAudio();
        }

        private void SimulateAI()
        {
            using (this.Marker().WithName("AI")) // Profiler: ProfilerMarkersTutorial.AI
            {
                for (var agent = 0; agent < _aiAgents; agent++)
                    using (this.Marker().WithName("AI.Agent")) // Profiler: ProfilerMarkersTutorial.AI.Agent
                        Spin(_aiStepsPerAgent, SpinOp.Sin);
            }
        }

        // using-declaration without .WithName(): auto-named after the method.
        private void SimulateInput()
        {
            using var _ = this.Marker(); // Profiler: ProfilerMarkersTutorial.SimulateInput
            Spin(_inputIterations, SpinOp.Tan);
        }

        // Combined form: method-wide using-declaration + nested using-statement.
        // Both auto-named after the method; distinct because their line numbers differ.
        private void SimulateAudio()
        {
            using var _ = this.Marker(); // Profiler: ProfilerMarkersTutorial.SimulateAudio (outer)
            Spin(_audioIterations, SpinOp.Sqrt);

            using (this.Marker()) // Profiler: ProfilerMarkersTutorial.SimulateAudio (inner)
                Spin(_audioIterations, SpinOp.Cos);
        }

        private enum SpinOp { Sqrt, Sin, Cos, Tan }

        // Allocation-free busy work — no delegates, so the Profiler stays free of GC.Alloc noise.
        private static void Spin(int iterations, SpinOp op)
        {
            var sum = 0f;
            for (var i = 0; i < iterations; i++)
            {
                sum += op switch
                {
                    SpinOp.Sqrt => Mathf.Sqrt(i),
                    SpinOp.Sin => Mathf.Sin(i),
                    SpinOp.Cos => Mathf.Cos(i),
                    _ => Mathf.Tan(i),
                };
            }

            _ = sum;
        }
    }
}
