using System.Linq;
using NUnit.Framework;

namespace Aspid.FastTools.Types.Editors.Tests
{
    // The tests of this fixture that need the package's Editor assembly. DotnetTests/Types runs the rest without
    // Unity and leaves the *.Unity.cs parts out.
    internal sealed partial class TypeUtilityTests
    {
        [Test]
        public void GetAllTypeInfos_LeavesOutTypesNestedInAGeneratedOne()
        {
            var nested = GetGeneratedDataTypes().FirstOrDefault();
            Assert.IsNotNull(nested);

            var offered = TypeInfo.GetAllTypeInfos(new[] { typeof(object) }, TypeAllow.All);

            Assert.IsFalse(offered.Any(info => info.AssemblyQualifiedName == nested.AssemblyQualifiedName),
                "A compiler's data struct must not appear in the <Global> group.");
        }
    }
}
