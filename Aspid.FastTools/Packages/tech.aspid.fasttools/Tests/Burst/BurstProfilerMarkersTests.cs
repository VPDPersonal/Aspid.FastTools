using Unity.Jobs;
using Unity.Burst;
using NUnit.Framework;
using Unity.Collections;

namespace Aspid.FastTools.Editors.Tests
{
    [BurstCompile(CompileSynchronously = true)]
    internal struct MarkerJob : IJob
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            using (this.Marker()) Result[0] += 1;
            using (this.Marker().WithName("Inner")) Result[0] += 10;

            var managed = 0;
            MarkManaged(ref managed);
            Result[1] = managed;
        }

        // Burst drops this call; the managed fallback runs it.
        [BurstDiscard]
        private static void MarkManaged(ref int managed) => managed = 1;
    }

    [BurstCompile(CompileSynchronously = true)]
    internal struct GenericMarkerJob<T> : IJob where T : unmanaged
    {
        public NativeArray<int> Result;

        public void Execute()
        {
            using (this.Marker()) Result[0] += 1;

            var managed = 0;
            MarkManaged(ref managed);
            Result[1] = managed;
        }

        [BurstDiscard]
        private static void MarkManaged(ref int managed) => managed = 1;
    }

    // Runs this.Marker() inside Burst jobs. Burst rejects managed code in a job and falls back to the managed
    // runner, so a job that still ran managed means the generated overload broke the Burst contract
    // (BC1025, BC1360). The assembly exists only where the Burst package is installed.
    [TestFixture]
    internal sealed class BurstProfilerMarkersTests
    {
        private static void AssertRunsUnderBurst(NativeArray<int> result, int expectedSum)
        {
            Assert.AreEqual(0, result[1], "The job ran as managed code: Burst rejected it. Read the Burst error in the Console.");
            Assert.AreEqual(expectedSum, result[0]);
        }

        [SetUp]
        public void SetUp() =>
            Assume.That(BurstCompiler.Options.EnableBurstCompilation, "Burst compilation is turned off.");

        [Test]
        public void StructJob_CompilesUnderBurst()
        {
            using var result = new NativeArray<int>(2, Allocator.TempJob);

            new MarkerJob { Result = result }.Run();

            AssertRunsUnderBurst(result, expectedSum: 11);
        }

        [Test]
        public void GenericStructJob_CompilesUnderBurst()
        {
            using var result = new NativeArray<int>(2, Allocator.TempJob);

            new GenericMarkerJob<int> { Result = result }.Run();

            AssertRunsUnderBurst(result, expectedSum: 1);
        }
    }
}
