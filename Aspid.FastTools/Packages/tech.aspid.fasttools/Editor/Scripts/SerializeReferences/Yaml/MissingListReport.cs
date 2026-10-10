using System.Linq;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    // The missing list elements a save dropped: the ones put back, and the ones no slot was found for.
    internal sealed class MissingListReport
    {
        public readonly List<Element> Restored = new();
        public readonly List<Element> Dropped = new();

        // "_weapons[2] (GhostPistol, rid 1002)"; markGuessed adds "slot guessed" to the elements placed by a guess.
        public static string Describe(IEnumerable<Element> elements, bool markGuessed) =>
            string.Join(", ", elements.Select(element => element.Describe(markGuessed)));

        // Index is the slot after the save for a restored element and the slot before it for a dropped one. Guessed marks
        // a restored element whose slot the save did not pin down, and a dropped one the user may not have deleted.
        internal readonly struct Element
        {
            public readonly string Field;
            public readonly int Index;
            public readonly long Rid;
            public readonly ManagedTypeName StoredType;
            public readonly bool Guessed;

            public Element(string field, int index, long rid, ManagedTypeName storedType, bool guessed)
            {
                Field = field;
                Index = index;
                Rid = rid;
                StoredType = storedType;
                Guessed = guessed;
            }

            public string Describe(bool markGuessed) =>
                $"{Field}[{Index}] ({StoredType.Class}, rid {Rid}{(markGuessed && Guessed ? ", slot guessed" : string.Empty)})";
        }
    }
}
