using Xunit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Aspid.FastTools.Analyzers.Descriptions;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    Aspid.FastTools.Analyzers.AspidFastToolsAnalyzer,
    Microsoft.CodeAnalysis.Testing.Verifiers.XUnitVerifier>;

namespace Aspid.FastTools.Analyzers.Tests;

public class TypeSelectorAnalyzerTests
{
    // Stand-ins for the Unity API that the package sources and the test snippets use, so the tests need no Unity
    // reference. The analyzer matches these types by full name.
    private const string UnityStubs = @"
namespace UnityEngine
{
    public class Object { }
    public sealed class SerializeField : System.Attribute { }
    public sealed class SerializeReference : System.Attribute { }

    public abstract class PropertyAttribute : System.Attribute
    {
        protected PropertyAttribute() { }
        protected PropertyAttribute(bool applyToCollection) { }
    }

    public sealed class TooltipAttribute : PropertyAttribute
    {
        public TooltipAttribute(string tooltip) { }
    }

    public interface ISerializationCallbackReceiver
    {
        void OnBeforeSerialize();
        void OnAfterDeserialize();
    }
}";

    // The package's own TypeSelectorAttribute, TypeAllow and Type wrappers, embedded by the csproj: a rename or a
    // namespace move there fails these tests instead of silently disabling the diagnostics in Unity.
    private static readonly (string FileName, string Source)[] PackageSources = typeof(TypeSelectorAnalyzerTests).Assembly
        .GetManifestResourceNames()
        .Where(name => name.StartsWith("PackageSources/", StringComparison.Ordinal))
        .Select(name => (Path.GetFileName(name), ReadResource(name)))
        .ToArray();

    // The wrappers open profiler markers, which the package compiles out under this scripting symbol.
    private const string ProfilerDisabledSymbol = "ASPID_FAST_TOOLS_UNITY_PROFILER_DISABLED";

    private static Task Verify(string code, params DiagnosticResult[] expected)
    {
        var test = CreateTest(code);
        test.ExpectedDiagnostics.AddRange(expected);

        return test.RunAsync();
    }

    private static DiagnosticResult NotSerialized(int location, string member, string reason) =>
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorNotSerializedRule).WithLocation(location).WithArguments(member, reason);

    [Fact]
    public Task StringField_TypeNamePicker_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(typeof(System.Object))] private string _type; }");

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
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector] private SerializableType _type; }");

    [Fact]
    public Task SerializableTypeGenericField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class C { [SerializeField, TypeSelector(typeof(Base))] private SerializableType<Base> _type; }");

    [Fact]
    public Task SerializableMonoScriptField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class C
{
    [SerializeField, TypeSelector] private SerializableMonoScript _type;
    [SerializeField, TypeSelector(typeof(Base))] private SerializableMonoScript<Base>[] _types;
}");

    [Fact]
    public Task SerializableTypeList_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
class C { [SerializeField, TypeSelector] private System.Collections.Generic.List<SerializableType> _types; }");

    [Fact]
    public Task AllowOnSerializableTypeField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(Allow = TypeAllow.None)] private SerializableType _type; }");

    [Fact]
    public Task UnsupportedFieldType_ReportsAFT0001() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, {|AFT0001:TypeSelector|}] private int _value; }");

    // The drawer draws a [Serializable], non-abstract SerializableType subclass that has a parameterless constructor
    // (public or not), and a member reference reads any ISerializableType.

    [Fact]
    public Task SerializableTypeSubclass_NoDiagnostic() => Verify(@"
using System;
using UnityEngine;
using Aspid.FastTools.Types;
class Unrelated { }
interface IWeapon { }
[Serializable] class WeaponType : SerializableType { public WeaponType() : base(null) { } }
[Serializable] class TaggedType<T> : SerializableType { public TaggedType() : base(null) { } }
[Serializable] class HiddenType : SerializableType { private HiddenType() : base(null) { } }
class C
{
    private WeaponType _base;
    private ISerializableType _contract;
    [SerializeField, TypeSelector] private WeaponType _type;
    [SerializeField, TypeSelector] private HiddenType _hidden;
    [SerializeField, TypeSelector(typeof(Unrelated))] private TaggedType<IWeapon>[] _tagged;
    [SerializeField, TypeSelector(nameof(_base), nameof(_contract))] private string _name;
}");

    // Unity does not serialize a subclass without its own [Serializable] (the attribute is not inherited), and the
    // drawer cannot instantiate an abstract one or one without a parameterless constructor.
    [Fact]
    public Task UndrawableSerializableTypeSubclass_ReportsAFT0001() => Verify(@"
using System;
using UnityEngine;
using Aspid.FastTools.Types;
class PlainType : SerializableType { public PlainType() : base(null) { } }
[Serializable] class TypedOnlyType : SerializableType { public TypedOnlyType(Type type) : base(type) { } }
[Serializable] abstract class AbstractType : SerializableType { protected AbstractType() : base(null) { } }
class C
{
    [SerializeField, {|AFT0001:TypeSelector|}] private PlainType _plain;
    [SerializeField, {|AFT0001:TypeSelector|}] private TypedOnlyType _typedOnly;
    [SerializeField, {|AFT0001:TypeSelector|}] private AbstractType[] _abstract;
}");

    // Only a field with an attribute named like [TypeSelector] is bound, so every way to write that name must count.
    [Fact]
    public Task TypeSelectorUnderOtherNames_ReportsAFT0001() => Verify(@"
using UnityEngine;
using Pick = Aspid.FastTools.Types.TypeSelectorAttribute;
namespace Game
{
    using ChooserAttribute = Aspid.FastTools.Types.TypeSelectorAttribute;
    class C
    {
        [SerializeField, {|AFT0001:Pick|}] private int _alias;
        [SerializeField, {|AFT0001:Chooser|}] private int _suffixedAlias;
        [SerializeField, {|AFT0001:Aspid.FastTools.Types.TypeSelector|}] private int _qualified;
        [SerializeField, {|AFT0001:global::Aspid.FastTools.Types.TypeSelectorAttribute|}] private int _global;
        [field: SerializeField, {|AFT0001:Pick|}] public int Property { get; set; }
    }
}");

    [Fact]
    public Task TypeSelectorThroughGlobalAliasInOtherFile_ReportsAFT0001()
    {
        var test = CreateTest(@"
using UnityEngine;
class C { [SerializeField, {|AFT0001:Pick|}] private int _value; }");
        test.TestState.Sources.Add(("GlobalUsings.cs", "global using Pick = Aspid.FastTools.Types.TypeSelectorAttribute;"));

        return test.RunAsync();
    }

    [Fact]
    public Task AbstractWrapperBase_ReportsAFT0001() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, {|AFT0001:TypeSelector|}] private SerializableTypeBase _type; }");

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
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(Allow = TypeAllow.Interface)] private string _type; }");

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

    // AFT0009 — base types that share no type: the picker needs a candidate assignable to every one of them

    [Fact]
    public Task UnrelatedClassBases_OnManagedReference_ReportsAFT0009Only() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IWeapon { }
class Pistol : IWeapon { }
class Rifle : IWeapon { }
class C { [SerializeReference, TypeSelector(typeof(Pistol), {|AFT0009:typeof(Rifle)|})] private IWeapon _weapon; }");

    [Fact]
    public Task UnrelatedClassBases_OnStringField_ReportsAFT0009() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Pistol { }
class Rifle { }
class C { [SerializeField, TypeSelector(typeof(Pistol), {|AFT0009:typeof(Rifle)|})] private string _type; }");

    [Fact]
    public Task SealedClassAndUnimplementedInterface_OnSerializableType_ReportsAFT0009() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IMarker { }
sealed class Leaf { }
class C { [SerializeField, TypeSelector(typeof(IMarker), {|AFT0009:typeof(Leaf)|})] private SerializableType _type; }");

    [Fact]
    public Task SeveralUnrelatedBases_ReportEachConflictingArgumentOnce() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class A { }
class B { }
class D { }
class C { [SerializeField, TypeSelector(typeof(A), {|AFT0009:typeof(B)|}, {|AFT0009:typeof(D)|})] private string _type; }");

    [Fact]
    public Task TwoInterfaceBases_NoAFT0009() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IMelee { }
interface IRanged { }
class C { [SerializeField, TypeSelector(typeof(IMelee), typeof(IRanged))] private string _type; }");

    [Fact]
    public Task ClassAndItsSubclass_NoAFT0009() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class Derived : Base { }
class C { [SerializeField, TypeSelector(typeof(Base), typeof(Derived))] private string _type; }");

    // Two closed types of one generic differ only in their type arguments, so messages keep them and the namespace.
    [Fact]
    public Task GenericTypes_MessagesNameTypeArguments() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
namespace Game
{
    sealed class Slot<T> { }
    sealed class Sword { }
    sealed class Axe { }
    interface IShield<T> { }
    class C
    {
        [SerializeReference, TypeSelector({|#0:typeof(Slot<Axe>)|})] private Slot<Sword> _slot;
        [SerializeField, TypeSelector(typeof(Slot<Sword>), {|#1:typeof(Slot<Axe>)|})] private string _type;
        [SerializeReference, {|#2:TypeSelector|}] private IShield<int> _shield;
    }
}",
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorBaseTypeRule).WithLocation(0)
            .WithArguments("Game.Slot<Game.Axe>", "Game.Slot<Game.Sword>"),
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorDisjointBaseTypesRule).WithLocation(1)
            .WithArguments("Game.Slot<Game.Sword>", "Game.Slot<Game.Axe>"),
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorNoConcreteImplementationRule).WithLocation(2)
            .WithArguments("_shield", "'Game.IShield<int>'"));

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

    // Each base has an implementation, but none implements both — the intersection the picker lists is empty.
    [Fact]
    public Task BasesImplementedOnlySeparately_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IWeapon { }
interface IMelee { }
interface IRanged { }
class Sword : IWeapon, IMelee { }
class Bow : IWeapon, IRanged { }
class C { [SerializeReference, {|AFT0005:TypeSelector(typeof(IMelee), typeof(IRanged))|}] private IWeapon _weapon; }");

    [Fact]
    public Task ClassImplementingEveryBase_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IWeapon { }
interface IMelee { }
interface IRanged { }
class Sword : IWeapon, IMelee { }
class Glaive : IWeapon, IMelee, IRanged { }
class C { [SerializeReference, TypeSelector(typeof(IMelee), typeof(IRanged))] private IWeapon _weapon; }");

    // A concrete base is its own candidate only when it also fits the field type, which the message then names too.

    [Fact]
    public Task ConcreteBaseNotFittingFieldType_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IShield { }
class Sword { }
class C { [SerializeReference, {|#0:TypeSelector(typeof(Sword))|}] private IShield _shield; }",
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorNoConcreteImplementationRule).WithLocation(0).WithArguments(
            "_shield", "'Sword' and 'IShield'"));

    // A field type every base already derives from adds no constraint, so the message leaves it out.
    [Fact]
    public Task FieldTypeImpliedByBase_AFT0005NamesOnlyBase() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IBase { }
interface IDerived : IBase { }
class C { [SerializeReference, {|#0:TypeSelector(typeof(IDerived))|}] private IBase _value; }",
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorNoConcreteImplementationRule).WithLocation(0).WithArguments(
            "_value", "'IDerived'"));

    [Fact]
    public Task ConcreteBaseOrItsSubclassFittingFieldType_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IShield { }
class Sword { }
class ShieldSword : Sword, IShield { }
class Buckler : IShield { }
class C
{
    [SerializeReference, TypeSelector(typeof(Sword))] private IShield _sword;
    [SerializeReference, TypeSelector(typeof(Buckler))] private IShield _buckler;
}");

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
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private System.Type _base;
    [SerializeField, TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_TypeArrayProperty_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private System.Type[] Bases { get; set; }
    [SerializeField, TypeSelector(""Bases"")] private string _type;
}");

    [Fact]
    public Task MemberReference_StringArrayField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private string[] _baseNames;
    [SerializeField, TypeSelector(nameof(_baseNames))] private string _type;
}");

    [Fact]
    public Task MemberReference_InheritedField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { protected System.Type _base; }
class C : Base
{
    [SerializeField, TypeSelector(nameof(_base))] private string _type;
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
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector({|AFT0006:""_missing""|})] private string _type; }");

    [Fact]
    public Task MemberIsMethod_ReportsAFT0007() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private System.Type GetBase() => null;
    [SerializeField, TypeSelector({|AFT0007:nameof(GetBase)|})] private string _type;
}");

    [Fact]
    public Task StaticMember_ReportsAFT0007() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private static System.Type _base;
    [SerializeField, TypeSelector({|AFT0007:nameof(_base)|})] private string _type;
}");

    [Fact]
    public Task WrongTypedMember_ReportsAFT0007() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private int _base;
    [SerializeField, TypeSelector({|AFT0007:nameof(_base)|})] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableTypeField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private SerializableType _base;
    [SerializeField, TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableTypeGenericField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class C
{
    private SerializableType<Base> _base;
    [SerializeField, TypeSelector(nameof(_base))] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableTypeArrayField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private SerializableType[] _bases;
    [SerializeField, TypeSelector(nameof(_bases))] private string _type;
}");

    [Fact]
    public Task AssemblyQualifiedName_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(""My.Namespace.IWeapon, MyAssembly"")] private string _type; }");

    [Fact]
    public Task NamespaceQualifiedNameWithoutAssembly_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(""System.Collections.IList"")] private string _type; }");

    [Fact]
    public Task NestedTypeName_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(""My.Outer+Nested, MyAssembly"")] private string _type; }");

    [Fact]
    public Task GenericTypeNameWithBrackets_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector(""System.Collections.Generic.List`1[[System.Int32, mscorlib]], mscorlib"")] private string _type; }");

    [Fact]
    public Task TrailingCommaTypeName_ReportsAFT0008() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector({|AFT0008:""My.Namespace.IWeapon, ""|})] private string _type; }");

    [Fact]
    public Task NameWithSpace_ReportsAFT0008() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector({|AFT0008:""My Class, MyAssembly""|})] private string _type; }");

    // Without an assembly, Type.GetType finds only mscorlib types: any other name needs its assembly.

    [Fact]
    public Task ProjectTypeNameWithoutAssembly_ReportsAFT0008WithItsAssembly() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
namespace Game { interface IWeapon { } }
class C { [SerializeField, TypeSelector({|#0:""Game.IWeapon""|})] private string _type; }",
        VerifyCS.Diagnostic(DiagnosticRules.TypeSelectorTypeNameSyntaxRule).WithLocation(0).WithArguments(
            "_type",
            "Game.IWeapon",
            "without an assembly Type.GetType finds only mscorlib types, write \"Game.IWeapon, TestProject\""));

    [Fact]
    public Task UnknownTypeNameWithoutAssembly_ReportsAFT0008() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
namespace Game { class Outer { public interface IInner { } } }
class C
{
    [SerializeField, TypeSelector({|AFT0008:""Game.IMissing""|})] private string _missing;
    [SerializeField, TypeSelector({|AFT0008:""Game.Outer+IInner""|})] private string _nested;
}");

    [Fact]
    public Task EmptyString_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [SerializeField, TypeSelector("""")] private string _type; }");

    [Fact]
    public Task ExplicitArrayArgument_ValidatesElements() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private System.Type _base;
    [SerializeField, TypeSelector(new string[] { nameof(_base), {|AFT0006:""_missing""|} })] private string _type;
}");

    // Nullable annotations do not change which members the drawer accepts.

    [Fact]
    public Task MemberReference_NullableTypeMembers_NoDiagnostic() => Verify(@"
#nullable enable
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private System.Type? _base;
    private System.Type?[]? _bases;
    [SerializeField, TypeSelector(nameof(_base))] private string _type = """";
    [SerializeField, TypeSelector(nameof(_bases))] private string _other = """";
}");

    [Fact]
    public Task NullableObjectDerivedManagedReference_ReportsAFT0004() => Verify(@"
#nullable enable
using UnityEngine;
using Aspid.FastTools.Types;
class MyComponent : Object { }
class C
{
    [SerializeReference, {|AFT0004:TypeSelector|}] private MyComponent? _comp;
    [SerializeReference, {|AFT0004:TypeSelector|}] private Object? _object;
}");

    // The drawer reads a property through its getter.

    [Fact]
    public Task MemberReference_SetOnlyProperty_ReportsAFT0007() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private System.Type Base { set { } }
    [SerializeField, TypeSelector({|AFT0007:nameof(Base)|})] private string _type;
}");

    [Fact]
    public Task MemberReference_SerializableMonoScriptField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    private SerializableMonoScript[] _bases;
    [SerializeField, TypeSelector(nameof(_bases))] private string _type;
}");

    // Generic base and field types: the drawer lists closed implementations and closes open ones.

    [Fact]
    public Task ClosedGenericInterfaceField_ClosedImpl_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo<T> { }
class IntFoo : IFoo<int> { }
class C { [SerializeReference, TypeSelector] private IFoo<int> _foo; }");

    [Fact]
    public Task ClosedGenericClassField_ClosedImpl_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
abstract class Base<T> { }
class IntImpl : Base<int> { }
class C { [SerializeReference, TypeSelector] private Base<int> _value; }");

    [Fact]
    public Task ClosedGenericInterfaceField_OpenImpl_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
interface IFoo<T> { }
class Foo<T> : IFoo<T> { }
class C { [SerializeReference, TypeSelector] private List<IFoo<int>> _foos; }");

    [Fact]
    public Task ClosedGenericTypeofBase_ClosedImpl_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAny { }
interface IFoo<T> { }
class IntFoo : IFoo<int>, IAny { }
class C { [SerializeReference, TypeSelector(typeof(IFoo<int>))] private IAny _value; }");

    [Fact]
    public Task ClosedGenericInterfaceField_OnlyOtherArgumentImpls_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo<T> { }
class StringFoo : IFoo<string> { }
class ListFoo<T> : IFoo<System.Collections.Generic.List<T>> { }
class C { [SerializeReference, {|AFT0005:TypeSelector|}] private IFoo<int> _foo; }");

    // A variant interface accepts implementations of a more derived argument, as Type.IsAssignableFrom does.

    [Fact]
    public Task CovariantBaseType_SealedFieldType_NoAFT0003() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAnimal { }
class Dog : IAnimal { }
interface IProducer<out T> { }
sealed class DogProducer : IProducer<Dog> { }
class C { [SerializeReference, TypeSelector(typeof(IProducer<IAnimal>))] private DogProducer _producer; }");

    [Fact]
    public Task CovariantField_ImplOfDerivedArgument_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAnimal { }
class Dog : IAnimal { }
interface IProducer<out T> { }
class DogProducer : IProducer<Dog> { }
class C { [SerializeReference, TypeSelector] private IProducer<IAnimal> _producer; }");

    [Fact]
    public Task VariantField_OpenImplOfConvertibleArgument_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAnimal { }
class Dog : IAnimal { }
interface IProducer<out T> { }
interface IConsumer<in T> { }
class DogProducer<TExtra> : IProducer<Dog> { }
class AnimalConsumer<TExtra> : IConsumer<IAnimal> { }
class C
{
    [SerializeReference, TypeSelector] private IProducer<IAnimal> _producer;
    [SerializeReference, TypeSelector] private IConsumer<Dog> _consumer;
}");

    [Fact]
    public Task CovariantField_OpenImplOfOpenArgument_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
interface IProducer<out T> { }
class ListProducer<T> : IProducer<List<T>> { }
class C { [SerializeReference, TypeSelector] private IProducer<IEnumerable<int>> _producer; }");

    [Fact]
    public Task CovariantField_OpenImplOfBaseArgument_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAnimal { }
class Dog : IAnimal { }
interface IProducer<out T> { }
class AnimalProducer<TExtra> : IProducer<IAnimal> { }
class ObjectProducer<TExtra> : IProducer<object> { }
class C
{
    [SerializeReference, {|AFT0005:TypeSelector|}] private IProducer<Dog> _producer;
    [SerializeReference, {|AFT0005:TypeSelector|}] private IProducer<int> _value;
}");

    // A type-parameter field is only known once the containing generic type is closed.

    [Fact]
    public Task TypeParameterField_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
class Impl { }
class Slot<T> where T : class
{
    [SerializeReference, TypeSelector(typeof(Impl))] private T _value;
    [SerializeReference, TypeSelector] private T _other;
    [SerializeReference, TypeSelector] private List<IComparer<T>> _comparers;
}");

    [Fact]
    public Task NestedInGenericArgument_OnlyOtherOuterArgumentImpls_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
interface IBar<T> { }
class Outer<T> { public class Inner { } }
class ListImpl<T> : IBar<Outer<List<T>>.Inner> { }
class C { [SerializeReference, {|AFT0005:TypeSelector|}] private IBar<Outer<int>.Inner> _bar; }");

    [Fact]
    public Task NestedInGenericArgument_OpenImpl_NoAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IBar<T> { }
class Outer<T> { public class Inner { } }
class Impl<T> : IBar<Outer<T>.Inner> { }
class C { [SerializeReference, TypeSelector] private IBar<Outer<int>.Inner> _bar; }");

    // An unbound typeof(Foo<>) accepts no closed type (Type.IsAssignableFrom): only an open generic class that
    // has a Foo<...> of its own type parameters in its hierarchy.

    [Fact]
    public Task UnboundGenericBase_SealedTypeWithoutIt_ReportsAFT0003AndAFT0009() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo<T> { }
sealed class Plain { }
class C
{
    [SerializeReference, TypeSelector({|AFT0003:typeof(IFoo<>)|})] private Plain _plain;
    [SerializeField, TypeSelector(typeof(IFoo<>), {|AFT0009:typeof(Plain)|})] private string _type;
}");

    [Fact]
    public Task UnboundGenericBase_SealedTypeImplementingClosedForm_ReportsAFT0003AndAFT0009() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo<T> { }
sealed class IntFoo : IFoo<int> { }
class C
{
    [SerializeReference, TypeSelector({|AFT0003:typeof(IFoo<>)|})] private IntFoo _foo;
    [SerializeField, TypeSelector(typeof(IFoo<>), {|AFT0009:typeof(IntFoo)|})] private string _type;
}");

    [Fact]
    public Task UnboundGenericClassBase_TypeDerivedFromClosedForm_ReportsAFT0003() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
abstract class Base<T> { }
class Derived : Base<int> { }
class C { [SerializeReference, TypeSelector({|AFT0003:typeof(Base<>)|})] private Derived _value; }");

    [Fact]
    public Task UnboundGenericBase_OnlyClosedFormImpls_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAny { }
interface IFoo<T> { }
class IntFoo : IFoo<int>, IAny { }
class IntBar<T> : IFoo<int>, IAny { }
class C { [SerializeReference, {|AFT0005:TypeSelector(typeof(IFoo<>))|}] private IAny _value; }");

    [Fact]
    public Task UnboundGenericBase_OpenImpl_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IAny { }
interface IFoo<T> { }
abstract class Base<T> { }
class Derived<T> : Base<T> { }
sealed class SealedFoo<T> : IFoo<T> { }
class Foo<T> : IFoo<T>, IAny { }
class C
{
    [SerializeReference, TypeSelector(typeof(IFoo<>))] private IAny _any;
    [SerializeReference, TypeSelector(typeof(IFoo<>))] private SealedFoo<int> _sealed;
    [SerializeReference, TypeSelector(typeof(Base<>))] private Base<int> _base;
    [SerializeReference, TypeSelector(typeof(Base<>))] private Derived<string> _derived;
}");

    // AFT0003 on SerializableType<T> / SerializableMonoScript<T>: the picker also requires T.

    [Fact]
    public Task DisjointBaseType_OnGenericWrappers_ReportsAFT0003() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class Unrelated { }
class C
{
    [SerializeField, TypeSelector({|AFT0003:typeof(Unrelated)|})] private SerializableType<Base> _type;
    [SerializeField, TypeSelector({|AFT0003:typeof(Unrelated)|})] private SerializableMonoScript<Base>[] _scripts;
}");

    [Fact]
    public Task CompatibleBaseType_OnGenericWrappers_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class Base { }
class Derived : Base { }
class Unrelated { }
class C
{
    [SerializeField, TypeSelector(typeof(Derived))] private SerializableType<Base> _type;
    [SerializeField, TypeSelector(typeof(Unrelated))] private SerializableType<object> _any;
    [SerializeField, TypeSelector(typeof(Unrelated))] private SerializableType _plain;
}");

    // A type the compiler cannot resolve reports CS0246 alone: a check against it would only add noise.

    [Fact]
    public Task UnresolvedFieldType_OnlyCompilerError() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
using System.Collections.Generic;
interface IWeapon { }
class Sword : IWeapon { }
class C
{
    [SerializeReference, TypeSelector] private {|CS0246:IMissing|} _reference;
    [SerializeReference, TypeSelector(typeof(Sword))] private List<{|CS0246:IMissing|}> _references;
    [SerializeField, TypeSelector] private {|CS0246:IMissing|} _shape;
}");

    [Fact]
    public Task UnresolvedTypeofArgument_OnlyCompilerError() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IWeapon { }
class Sword : IWeapon { }
sealed class Leaf { }
class C
{
    [SerializeReference, TypeSelector(typeof({|CS0246:IMissing|}))] private Leaf _leaf;
    [SerializeReference, TypeSelector(typeof(Sword), typeof({|CS0246:IMissing|}))] private IWeapon _weapon;
    [SerializeField, TypeSelector(typeof({|CS0246:IMissing|}))] private SerializableType<Sword> _type;
}");

    // An unresolved base class or interface hides which types a class meets, as an unresolved class itself does.
    [Fact]
    public Task UnresolvedBaseOfFieldOrTypeofType_OnlyCompilerError() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IWeapon { }
class Weapon { }
class Sword : {|CS0246:MissingBase|} { }
class Dagger : Sword { }
sealed class Leaf : Weapon, {|CS0246:IMissing|} { }
class C
{
    [SerializeReference, TypeSelector(typeof(Weapon))] private Dagger _dagger;
    [SerializeReference, TypeSelector(typeof(IWeapon))] private Sword _sword;
    [SerializeReference, TypeSelector(typeof(IWeapon))] private Leaf _leaf;
    [SerializeReference, TypeSelector(typeof(Weapon), typeof(Sword))] private object _pair;
    [SerializeField, TypeSelector] private Sword _shape;
}");

    // [field: ...] on an auto-property targets its serialized backing field.

    [Fact]
    public Task FieldTargetedAutoProperty_UnsupportedType_ReportsAFT0001() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [field: SerializeField, {|AFT0001:TypeSelector|}] public int Value { get; set; } }");

    [Fact]
    public Task FieldTargetedAutoProperty_ManagedReference_ReportsAFT0005() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IEmpty { }
class C { [field: SerializeReference, {|AFT0005:TypeSelector|}] public IEmpty Value { get; private set; } }");

    [Fact]
    public Task FieldTargetedAutoProperty_MemberReference_ReportsAFT0006() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
class C { [field: SerializeField, TypeSelector({|AFT0006:""_missing""|})] public string Value { get; set; } }");

    [Fact]
    public Task FieldTargetedAutoProperty_ValidShapes_NoDiagnostic() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class FooImpl : IFoo { }
class C
{
    [field: SerializeField, TypeSelector] public string TypeName { get; set; }
    [field: SerializeReference, TypeSelector] public IFoo Foo { get; set; }
    [field: SerializeField, TypeSelector] public SerializableType Type { get; set; }
}");

    // AFT0012 — Unity does not serialize the field, so the Inspector never draws it.

    [Fact]
    public Task NotSerializedFields_ReportAFT0012() => Verify(@"
using System;
using UnityEngine;
using Aspid.FastTools.Types;
class C
{
    [{|#0:TypeSelector|}] private string _private;
    [{|#1:TypeSelector|}] internal string _internal;
    [SerializeField, {|#2:TypeSelector|}] private static string _static;
    [{|#3:TypeSelector|}] public const string Const = """";
    [SerializeField, {|#4:TypeSelector|}] private readonly string _readonly;
    [NonSerialized, {|#5:TypeSelector|}] public string _nonSerialized;
    [field: {|#6:TypeSelector|}] public string Property { get; set; }
    [field: SerializeField, {|#7:TypeSelector|}] public string GetOnly { get; }
    [field: SerializeField, {|#8:TypeSelector|}] public static string Static { get; set; }
}",
        NotSerialized(0, "_private", "add [SerializeField] or make it public"),
        NotSerialized(1, "_internal", "add [SerializeField] or make it public"),
        NotSerialized(2, "_static", "a static field is never serialized"),
        NotSerialized(3, "Const", "a const field is never serialized"),
        NotSerialized(4, "_readonly", "a readonly field is never serialized"),
        NotSerialized(5, "_nonSerialized", "remove [NonSerialized]"),
        NotSerialized(6, "Property", "add [field: SerializeField]"),
        NotSerialized(7, "GetOnly", "an auto-property without a set accessor has a readonly backing field, add 'private set;'"),
        NotSerialized(8, "Static", "a static auto-property is never serialized"));

    [Fact]
    public Task SerializedFields_NoAFT0012() => Verify(@"
using UnityEngine;
using Aspid.FastTools.Types;
interface IFoo { }
class FooImpl : IFoo { }
class C
{
    [TypeSelector] public string _public;
    [SerializeField, TypeSelector] protected string _protected;
    [SerializeReference, TypeSelector] private IFoo _reference;
    [field: SerializeField, TypeSelector] public string Property { get; private set; }
}");

    [Fact]
    public Task UnresolvedAttribute_NoAFT0012() => Verify(@"
using Aspid.FastTools.Types;
class C { [{|#0:SerializeField|}, TypeSelector] private string _type; }",
        DiagnosticResult.CompilerError("CS0246").WithLocation(0).WithArguments("SerializeFieldAttribute"),
        DiagnosticResult.CompilerError("CS0246").WithLocation(0).WithArguments("SerializeField"));

    private static Task VerifyWithReferencedProject(string code, string referencedProjectSource)
    {
        var test = CreateTest(code);
        test.TestState.AdditionalProjects["Contracts"].Sources.Add(referencedProjectSource);
        test.TestState.AdditionalProjectReferences.Add("Contracts");

        return test.RunAsync();
    }

    private static CSharpAnalyzerTest<AspidFastToolsAnalyzer, XUnitVerifier> CreateTest(string code)
    {
        var test = new CSharpAnalyzerTest<AspidFastToolsAnalyzer, XUnitVerifier>();

        test.TestState.Sources.Add(code);
        test.TestState.Sources.Add(("UnityStubs.cs", UnityStubs));
        foreach (var (fileName, source) in PackageSources)
            test.TestState.Sources.Add((fileName, source));

        test.SolutionTransforms.Add((solution, projectId) =>
        {
            var options = (CSharpParseOptions)solution.GetProject(projectId)!.ParseOptions!;
            var symbols = options.PreprocessorSymbolNames.Append(ProfilerDisabledSymbol);

            return solution.WithProjectParseOptions(projectId, options.WithPreprocessorSymbols(symbols));
        });

        return test;
    }

    private static string ReadResource(string name)
    {
        using var stream = typeof(TypeSelectorAnalyzerTests).Assembly.GetManifestResourceStream(name)!;
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
