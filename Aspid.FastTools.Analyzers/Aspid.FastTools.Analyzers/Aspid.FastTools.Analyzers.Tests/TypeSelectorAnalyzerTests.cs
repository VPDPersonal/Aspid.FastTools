using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Aspid.FastTools.Analyzers.AspidFastToolsAnalyzer,
    Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace Aspid.FastTools.Analyzers.Tests;

public class TypeSelectorAnalyzerTests
{
    // Minimal stand-ins for the attributes the analyzer matches by full name, so the tests need no Unity / package
    // references. Appended after the test snippet (which carries the `using` directives) to keep usings at file top.
    private const string Stubs = @"
namespace UnityEngine
{
    public class Object { }
    public sealed class SerializeReference : System.Attribute { }
}
namespace Aspid.FastTools.Types
{
    [System.Flags] public enum TypeAllow { None = 0, Abstract = 1, Interface = 2, All = 3 }

    public sealed class SerializableType { }
    public sealed class SerializableType<T> { }

    public sealed class TypeSelectorAttribute : System.Attribute
    {
        public TypeSelectorAttribute() { }
        public TypeSelectorAttribute(System.Type type) { }
        public TypeSelectorAttribute(params System.Type[] types) { }
        public TypeSelectorAttribute(string assemblyQualifiedName) { }
        public TypeSelectorAttribute(params string[] assemblyQualifiedNames) { }
        public TypeAllow Allow { get; set; }
    }
}";

    private static Task Verify(string code) => VerifyCS.VerifyAnalyzerAsync(code + "\n" + Stubs);

    [Fact]
    public Task StringField_TypeNamePicker_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(typeof(System.Object))] private string _type; }");

    [Fact]
    public Task ManagedReferenceField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class FooImpl : IFoo { }
class C { [SerializeReference, TypeSelector] private IFoo _foo; }");

    [Fact]
    public Task ManagedReferenceList_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
interface IFoo { }
class FooImpl : IFoo { }
class C { [SerializeReference, TypeSelector] private List<IFoo> _foos; }");

    [Fact]
    public Task SerializableTypeField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector] private SerializableType _type; }");

    [Fact]
    public Task SerializableTypeGenericField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class Base { }
class C { [TypeSelector(typeof(Base))] private SerializableType<Base> _type; }");

    [Fact]
    public Task SerializableTypeList_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
using System.Collections.Generic;
class C { [TypeSelector] private System.Collections.Generic.List<SerializableType> _types; }");

    [Fact]
    public Task AllowOnSerializableTypeField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(Allow = TypeAllow.None)] private SerializableType _type; }");

    [Fact]
    public Task UnsupportedFieldType_ReportsAFT0001() => Verify(@"
using Aspid.FastTools.Types;
class C { [{|AFT0001:TypeSelector|}] private int _value; }");

    [Fact]
    public Task AllowOnManagedReference_ReportsAFT0002() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class FooImpl : IFoo { }
class C { [SerializeReference, TypeSelector({|AFT0002:Allow = TypeAllow.Interface|})] private IFoo _foo; }");

    [Fact]
    public Task AllowNoneOnManagedReference_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class FooImpl : IFoo { }
class C { [SerializeReference, TypeSelector(Allow = TypeAllow.None)] private IFoo _foo; }");

    [Fact]
    public Task AllowOnStringField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(Allow = TypeAllow.Interface)] private string _type; }");

    [Fact]
    public Task DisjointBaseType_ReportsAFT0003() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class Unrelated { }
class C { [SerializeReference, TypeSelector({|AFT0003:typeof(Unrelated)|})] private Base _value; }");

    [Fact]
    public Task AssignableBaseType_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class Derived : Base { }
class C { [SerializeReference, TypeSelector(typeof(Derived))] private Base _value; }");

    [Fact]
    public Task InterfaceBaseType_ImplDerivesFromBoth_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IMarker { }
class Base { }
class MarkerImpl : Base, IMarker { }
class C { [SerializeReference, TypeSelector(typeof(IMarker))] private Base _value; }");

    // The only IMarker implementation does not derive from Base, so the intersection is empty.
    // AFT0003 must NOT fire: an interface paired with a non-sealed class is never provably disjoint.
    [Fact]
    public Task InterfaceBaseType_ImplNotDerivingFromFieldType_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IMarker { }
class Base { }
class MarkerImpl : IMarker { }
class C { [SerializeReference, {|AFT0005:TypeSelector(typeof(IMarker))|}] private Base _value; }");

    [Fact]
    public Task InterfaceBaseType_SealedFieldTypeNotImplementing_ReportsAFT0003() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IMarker { }
sealed class Leaf { }
class C { [SerializeReference, TypeSelector({|AFT0003:typeof(IMarker)|})] private Leaf _value; }");

    [Fact]
    public Task InterfaceBaseType_SealedFieldTypeImplementing_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IMarker { }
sealed class Leaf : IMarker { }
class C { [SerializeReference, TypeSelector(typeof(IMarker))] private Leaf _value; }");

    [Fact]
    public Task SealedBaseTypeNotImplementingInterfaceField_ReportsAFT0003() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IWeapon { }
sealed class Unrelated { }
class C { [SerializeReference, TypeSelector({|AFT0003:typeof(Unrelated)|})] private IWeapon _value; }");

    // AFT0004 — managed reference to a UnityEngine.Object-derived type

    [Fact]
    public Task ObjectDerivedManagedReference_ReportsAFT0004() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class MyComponent : Object { }
class C { [SerializeReference, {|AFT0004:TypeSelector|}] private MyComponent _comp; }");

    [Fact]
    public Task InterfaceManagedReference_NoAFT0004() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class Impl : IFoo { }
class C { [SerializeReference, TypeSelector] private IFoo _foo; }");

    // AFT0005 — no visible concrete implementation

    [Fact]
    public Task InterfaceWithNoImplementations_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IEmpty { }
class C { [SerializeReference, {|AFT0005:TypeSelector|}] private IEmpty _empty; }");

    [Fact]
    public Task AbstractBaseWithOnlyAbstractSubclasses_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
abstract class Animal { }
abstract class Mammal : Animal { }
class C { [SerializeReference, {|AFT0005:TypeSelector|}] private Animal _animal; }");

    [Fact]
    public Task ConcreteElementType_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Concrete { }
class C { [SerializeReference, TypeSelector] private Concrete _value; }");

    [Fact]
    public Task TypeofArgumentWithNoImplementations_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IBase { }
interface IDerived : IBase { }
class C { [SerializeReference, {|AFT0005:TypeSelector(typeof(IDerived))|}] private IBase _value; }");

    [Fact]
    public Task TypeofArgumentWithConcreteImpl_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IBase { }
interface IDerived : IBase { }
class DerivedImpl : IDerived { }
class C { [SerializeReference, TypeSelector(typeof(IDerived))] private IBase _value; }");

    // The candidate search only scans assemblies that can see the constraint types (perf: a Unity compilation
    // references hundreds of assemblies). These tests pin the reference-assembly path: a candidate living in a
    // referenced project must still be found, and its absence must still be reported.

    [Fact]
    public Task ImplInReferencedAssembly_NoAFT0005() => VerifyWithReferencedProject(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeReference, TypeSelector] private Contracts.IShared _shared; }",
        referencedProjectSource: @"
namespace Contracts
{
    public interface IShared { }
    public class SharedImpl : IShared { }
}");

    [Fact]
    public Task NoImplAnywhere_CrossAssembly_ReportsAFT0005() => VerifyWithReferencedProject(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeReference, {|AFT0005:TypeSelector|}] private Contracts.IShared _shared; }",
        referencedProjectSource: @"
namespace Contracts
{
    public interface IShared { }
}");

    // AFT0006/AFT0007/AFT0008 — string arguments: member references (identifiers) and assembly-qualified names

    [Fact]
    public Task MemberReference_TypeField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private System.Type _base;
    [TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_TypeArrayProperty_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private System.Type[] Bases { get; set; }
    [TypeSelector(""Bases"")] private string _type;
}");

    [Fact]
    public Task MemberReference_StringArrayField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private string[] _baseNames;
    [TypeSelector(nameof(_baseNames))] private string _type;
}");

    [Fact]
    public Task MemberReference_InheritedField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class Base { protected System.Type _base; }
class C : Base
{
    [TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_OnManagedReference_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class FooImpl : IFoo { }
class C
{
    private System.Type _base;
    [SerializeReference, TypeSelector(nameof(_base))] private IFoo _foo;
}");

    [Fact]
    public Task UnknownIdentifier_ReportsAFT0006() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector({|AFT0006:""_missing""|})] private string _type; }");

    [Fact]
    public Task MemberIsMethod_ReportsAFT0007() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private System.Type GetBase() => null;
    [TypeSelector({|AFT0007:nameof(GetBase)|})] private string _type;
}");

    [Fact]
    public Task StaticMember_ReportsAFT0007() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private static System.Type _base;
    [TypeSelector({|AFT0007:nameof(_base)|})] private string _type;
}");

    [Fact]
    public Task WrongTypedMember_ReportsAFT0007() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private int _base;
    [TypeSelector({|AFT0007:nameof(_base)|})] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableTypeField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private SerializableType _base;
    [TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableTypeGenericField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class Base { }
class C
{
    private SerializableType<Base> _base;
    [TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableTypeArrayField_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private SerializableType[] _bases;
    [TypeSelector(nameof(_bases))] private string _type;
}");

    [Fact]
    public Task AssemblyQualifiedName_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(""My.Namespace.IWeapon, MyAssembly"")] private string _type; }");

    [Fact]
    public Task NamespaceQualifiedNameWithoutAssembly_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(""System.Collections.IList"")] private string _type; }");

    [Fact]
    public Task NestedTypeName_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(""My.Outer+Nested, MyAssembly"")] private string _type; }");

    [Fact]
    public Task GenericTypeNameWithBrackets_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector(""System.Collections.Generic.List`1[[System.Int32, mscorlib]], mscorlib"")] private string _type; }");

    [Fact]
    public Task TrailingCommaTypeName_ReportsAFT0008() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector({|AFT0008:""My.Namespace.IWeapon, ""|})] private string _type; }");

    [Fact]
    public Task NameWithSpace_ReportsAFT0008() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector({|AFT0008:""My Class, MyAssembly""|})] private string _type; }");

    [Fact]
    public Task EmptyString_NoDiagnostic() => Verify(@"
using Aspid.FastTools.Types;
class C { [TypeSelector("""")] private string _type; }");

    [Fact]
    public Task ExplicitArrayArgument_ValidatesElements() => Verify(@"
using Aspid.FastTools.Types;
class C
{
    private System.Type _base;
    [TypeSelector(new string[] { nameof(_base), {|AFT0006:""_missing""|} })] private string _type;
}");

    private static Task VerifyWithReferencedProject(string code, string referencedProjectSource)
    {
        var test = new Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerTest<
            AspidFastToolsAnalyzer, Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>();

        test.TestState.Sources.Add(code + "\n" + Stubs);
        test.TestState.AdditionalProjects["Contracts"].Sources.Add(referencedProjectSource);
        test.TestState.AdditionalProjectReferences.Add("Contracts");

        return test.RunAsync();
    }
}
