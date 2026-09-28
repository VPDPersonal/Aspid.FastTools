// One call of each shape the generator handles: a class in a namespace, a WithName label,
// a generic class, a struct and a type in the global namespace.
namespace Aspid.FastTools.Sample
{
    public class Player
    {
        public void Move()
        {
            using var _ = this.Marker();
        }

        public void Jump()
        {
            using (this.Marker().WithName("Jump.Start")) { }
            using (this.Marker()) { }
        }
    }

    public class Pool<T>
    {
        public T? Get()
        {
            using var _ = this.Marker();
            return default;
        }
    }

    public struct MoveJob
    {
        public void Execute()
        {
            using var _ = this.Marker();
        }
    }
}

// The fallback is in the global namespace too, so here the two overloads tie: the generated one wins
// only when the fallback also takes a [CallerLineNumber] int; otherwise the call binds to the fallback
// and records nothing.
public class GlobalSpawner
{
    public void Spawn()
    {
        using var _ = this.Marker();
    }
}
