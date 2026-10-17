// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.DevTests.Types
{
    // Gives the enemy hierarchy an interface, so one [TypeSelector] constraint can cover all three
    // TypeAllow categories at once: two concrete classes, one abstract base, one interface.
    public interface IEnemy
    {
        float Health { get; }
    }
}
