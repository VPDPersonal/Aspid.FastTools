// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal readonly struct IdRegistryEntryData
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int OriginalIndex;
        public readonly bool IsDuplicate;

        public IdRegistryEntryData(int originalIndex, string name, int id, bool isDuplicate)
        {
            OriginalIndex = originalIndex;
            Name = name;
            Id = id;
            IsDuplicate = isDuplicate;
        }
    }
}
