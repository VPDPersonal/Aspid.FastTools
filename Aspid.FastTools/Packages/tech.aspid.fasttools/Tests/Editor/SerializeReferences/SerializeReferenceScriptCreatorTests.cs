using System;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Aspid.FastTools.SerializeReferences.Editors.Tests
{
    internal interface IInteractable { }

    internal interface IEffect<T> { }

    // ReSharper disable once InconsistentNaming
    internal interface Idle { }

    internal class ItemBase { }

    internal class GenericBase<T> { }

    // Public: only a type that every script can see is a base type for a script in another assembly.
    public abstract class PublicFamilyOrAssembly { protected internal abstract void Run(); }

    public abstract class PublicInternalMember { internal abstract void Hidden(); }

    /// <summary>
    /// Coverage for the class name that <b>Create New Script…</b> suggests: a single interface prefix and the generic
    /// arity suffix are dropped, so accepting the default gives a valid, readable class name.
    /// </summary>
    [TestFixture]
    internal sealed class SerializeReferenceScriptCreatorTests
    {
        [TestCase(typeof(IInteractable), ExpectedResult = "NewInteractable")]
        [TestCase(typeof(IEffect<int>), ExpectedResult = "NewEffect")]
        [TestCase(typeof(IEffect<>), ExpectedResult = "NewEffect")]
        [TestCase(typeof(Idle), ExpectedResult = "NewIdle")]
        [TestCase(typeof(ItemBase), ExpectedResult = "NewItemBase")]
        [TestCase(typeof(GenericBase<int>), ExpectedResult = "NewGenericBase")]
        public string SuggestClassName_StripsOneInterfacePrefixAndArity(Type baseType) =>
            SerializeReferenceScriptCreator.SuggestClassName(baseType);
    }

    // Coverage for the stub that Create New Script… writes: it has to compile for every shape of the base type.
    // The generated text is checked here; the compile check ran against real C# output outside Unity.
    [TestFixture]
    internal sealed class SerializeReferenceScriptCreatorStubTests
    {
        private const string Throw = "throw new NotImplementedException()";

        internal interface IByRef { void Move(ref int x, out string y, in float z); }

        internal interface IGenericMethod
        {
            T Make<T>();
            void Fill<T>(T item) where T : class, new();
            bool TryGet<T>(out T value) where T : struct;
        }

        internal interface IIndexed
        {
            int this[int index] { get; set; }
            string this[string key] { get; }
        }

        internal interface IRefReturn
        {
            ref int Slot(int index);
            ref readonly int Peek(int index);
            ref int Head { get; }
        }

        internal interface IFirst { void Add<T>(T item); }

        internal interface ISecond { void Add<U>(U item); }

        internal interface IBoth : IFirst, ISecond { }

        internal interface IDefaultMember
        {
            void Required();
            void Optional() { }
        }

        internal abstract class AbstractBase
        {
            public abstract void Run();
            protected abstract int Compute(ref int value);
            public abstract T Convert<T>(T value) where T : class;
            public abstract string Name { get; protected set; }
            public abstract int this[int index] { get; }
            public abstract event Action Changed;
            public virtual void Virtual() { }
        }

        internal abstract class AbstractMiddle : AbstractBase
        {
            public override void Run() { }
            protected override int Compute(ref int value) => value;
            public override T Convert<T>(T value) => value;
        }

        internal abstract class NeedsArguments
        {
            protected NeedsArguments(int count, ref string name) { }
            public abstract void Run();
        }

        internal class OptionalConstructor { public OptionalConstructor(int count = 1) { } }

        internal class RequiredArgument { public RequiredArgument(int count) { } }

        internal sealed class SealedClass { }

        internal class HiddenConstructor { private HiddenConstructor() { } }

        internal abstract class InternalMember { internal abstract void Hidden(); }

        private abstract class PrivateBase { public abstract void Run(); }

        internal class ZeroAndOptionalConstructors
        {
            public ZeroAndOptionalConstructors() { }
            public ZeroAndOptionalConstructors(int count = 1) { }
        }

        internal class AmbiguousConstructors
        {
            public AmbiguousConstructors(int count = 0) { }
            public AmbiguousConstructors(string name = null) { }
        }

        internal abstract class AccessorsBase
        {
            public abstract int Value { get; set; }
            public abstract int this[int index] { get; set; }
        }

        internal abstract class SetterOverridden : AccessorsBase
        {
            public override int Value { set { } }
            public override int this[int index] { set { } }
        }

        internal abstract class MixedAccessors { public abstract int Value { get; protected set; } }

        internal abstract class GetterOverridden : MixedAccessors { public override int Value => 0; }

        internal sealed class SealedGeneric<T> { }

        internal interface IDictionaryLike
        {
            Dictionary<string, int>.KeyCollection Keys { get; }
            void Take(List<int>.Enumerator enumerator);
        }

        internal class GenericOuter<T>
        {
            internal interface INested { T Get(); }

            internal abstract class Nested<TItem> { public abstract TItem Pick(T value); }
        }

        private static string Generate(Type baseType, bool sameAssembly = true)
        {
            var created = SerializeReferenceScriptCreator.TryGenerateStub(
                className: "NewStub", nspace: "Game", baseType: baseType, sameAssembly: sameAssembly, out var stub, out var error);

            Assert.That(created, Is.True, message: error);
            return stub;
        }

        [Test]
        public void ByRefParameters_KeepTheirModifiers() =>
            Assert.That(Generate(typeof(IByRef)),
                Does.Contain($"public void Move(ref System.Int32 x, out System.String y, in System.Single z) => {Throw};"));

        [Test]
        public void GenericMethods_KeepTypeParametersAndConstraints()
        {
            var stub = Generate(typeof(IGenericMethod));

            Assert.That(stub, Does.Contain($"public T Make<T>() => {Throw};"));
            Assert.That(stub, Does.Contain($"public void Fill<T>(T item) where T : class, new() => {Throw};"));
            Assert.That(stub, Does.Contain($"public System.Boolean TryGet<T>(out T value) where T : struct => {Throw};"));
        }

        [Test]
        public void GenericMethodDeclaredTwice_IsImplementedOnce()
        {
            var stub = Generate(typeof(IBoth));

            Assert.That(stub, Does.Contain($"public void Add<T>(T item) => {Throw};"));
            Assert.That(stub, Does.Not.Contain("Add<U>"));
        }

        [Test]
        public void Indexers_AreNotAutoProperties()
        {
            var stub = Generate(typeof(IIndexed));

            Assert.That(stub, Does.Contain($"public System.Int32 this[System.Int32 index] {{ get => {Throw}; set => {Throw}; }}"));
            Assert.That(stub, Does.Contain($"public System.String this[System.String key] {{ get => {Throw}; }}"));
        }

        [Test]
        public void RefReturns_KeepTheirModifier()
        {
            var stub = Generate(typeof(IRefReturn));

            Assert.That(stub, Does.Contain($"public ref System.Int32 Slot(System.Int32 index) => {Throw};"));
            Assert.That(stub, Does.Contain($"public ref readonly System.Int32 Peek(System.Int32 index) => {Throw};"));
            Assert.That(stub, Does.Contain($"public ref System.Int32 Head {{ get => {Throw}; }}"));
        }

        [Test]
        public void DefaultInterfaceMember_NeedsNoStub()
        {
            var stub = Generate(typeof(IDefaultMember));

            Assert.That(stub, Does.Contain("public void Required()"));
            Assert.That(stub, Does.Not.Contain("Optional"));
        }

        [Test]
        public void AbstractClass_OverridesEveryAbstractMember()
        {
            var stub = Generate(typeof(AbstractBase));

            Assert.That(stub, Does.Contain($"public override void Run() => {Throw};"));
            Assert.That(stub, Does.Contain($"protected override System.Int32 Compute(ref System.Int32 value) => {Throw};"));
            Assert.That(stub, Does.Contain($"public override T Convert<T>(T value) => {Throw};"));
            Assert.That(stub, Does.Contain($"public override System.String Name {{ get => {Throw}; protected set => {Throw}; }}"));
            Assert.That(stub, Does.Contain($"public override System.Int32 this[System.Int32 index] {{ get => {Throw}; }}"));
            Assert.That(stub, Does.Contain("public override event System.Action Changed;"));
            Assert.That(stub, Does.Not.Contain("Virtual"));
        }

        [Test]
        public void AbstractClass_SkipsMembersThatAnIntermediateClassImplements()
        {
            var stub = Generate(typeof(AbstractMiddle));

            Assert.That(stub, Does.Not.Contain("Run()"));
            Assert.That(stub, Does.Not.Contain("Compute"));
            Assert.That(stub, Does.Not.Contain("Convert"));
            Assert.That(stub, Does.Contain("Name {"));
            Assert.That(stub, Does.Contain("event System.Action Changed;"));
        }

        [Test]
        public void ConstructorWithRequiredArguments_IsPassedThrough()
        {
            var stub = Generate(typeof(NeedsArguments));

            Assert.That(stub, Does.Contain("public NewStub(System.Int32 count, ref System.String name) : base(count, ref name) { }"));
            Assert.That(stub, Does.Contain($"public override void Run() => {Throw};"));
        }

        [Test]
        public void ConstructorOnly_LeavesNoTrailingBlankLine() =>
            Assert.That(Generate(typeof(RequiredArgument)),
                Does.Contain($"public NewStub(System.Int32 count) : base(count) {{ }}{Environment.NewLine}    }}"));

        [TestCase(typeof(OptionalConstructor))]
        [TestCase(typeof(ZeroAndOptionalConstructors))]
        [TestCase(typeof(AbstractMiddle))]
        [TestCase(typeof(object))]
        public void ConstructorCallableWithoutArguments_NeedsNoDeclaration(Type baseType) =>
            Assert.That(Generate(baseType), Does.Not.Contain("NewStub("));

        [Test]
        public void ConstructorsAmbiguousWithoutArguments_AreCalledThroughTheStubConstructor() =>
            Assert.That(Generate(typeof(AmbiguousConstructors)),
                Does.Match(@"public NewStub\(System\.(Int32|String) \w+\) : base\(\w+\) \{ \}"));

        [TestCase(typeof(SealedClass), true)]
        [TestCase(typeof(HiddenConstructor), true)]
        [TestCase(typeof(PublicInternalMember), false)]
        [TestCase(typeof(int), false)]
        public void TypeThatCannotBeDerivedFrom_IsRefusedWithAReason(Type baseType, bool sameAssembly)
        {
            var created = SerializeReferenceScriptCreator.TryGenerateStub(
                className: "NewStub", nspace: null, baseType: baseType, sameAssembly: sameAssembly, out var stub, out var error);

            Assert.That(created, Is.False);
            Assert.That(stub, Is.Null);
            Assert.That(error, Does.Contain(baseType.Name));
        }

        [Test]
        public void GenericTypeInAReason_IsNamedLikeCSharp()
        {
            SerializeReferenceScriptCreator.TryGenerateStub(
                className: "NewStub", nspace: null, baseType: typeof(SealedGeneric<int>), sameAssembly: true, out _, out var error);

            Assert.That(error, Does.Contain("SealedGeneric<System.Int32>"));
            Assert.That(error, Does.Not.Contain("`"));
        }

        [Test]
        public void InternalMember_IsOverriddenOnlyInTheSameAssembly() =>
            Assert.That(Generate(typeof(InternalMember)), Does.Contain($"internal override void Hidden() => {Throw};"));

        [TestCase(false, "protected override")]
        [TestCase(true, "protected internal override")]
        public void ProtectedInternalMember_FollowsTheAssemblyOfTheScript(bool sameAssembly, string expected) =>
            Assert.That(Generate(typeof(PublicFamilyOrAssembly), sameAssembly: sameAssembly), Does.Contain($"{expected} void Run()"));

        [Test]
        public void PublicBaseType_GivesAPublicStub() =>
            Assert.That(Generate(typeof(PublicFamilyOrAssembly), sameAssembly: false), Does.Contain("public class NewStub : "));

        [Test]
        public void InternalBaseType_GivesAnInternalStubInTheSameAssembly() =>
            Assert.That(Generate(typeof(AbstractBase)), Does.Contain("internal class NewStub : "));

        [Test]
        public void InternalBaseType_IsRefusedForAScriptInAnotherAssembly()
        {
            var created = SerializeReferenceScriptCreator.TryGenerateStub(
                className: "NewStub", nspace: null, baseType: typeof(AbstractBase), sameAssembly: false, out var stub, out var error);

            Assert.That(created, Is.False);
            Assert.That(stub, Is.Null);
            Assert.That(error, Does.Contain("is not public"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PrivateNestedBaseType_IsRefusedInEveryAssembly(bool sameAssembly)
        {
            var created = SerializeReferenceScriptCreator.TryCheckBaseType(
                typeof(PrivateBase), sameAssembly: sameAssembly, out _, out var error);

            Assert.That(created, Is.False);
            Assert.That(error, Does.Contain("private or protected"));
        }

        [Test]
        public void BaseTypeCheck_RefusesOnlyWhatNoFolderCanFix()
        {
            Assert.That(SerializeReferenceScriptCreator.TryCheckBaseType(
                typeof(SealedClass), sameAssembly: true, out _, out _), Is.False);

            Assert.That(SerializeReferenceScriptCreator.TryCheckBaseType(
                typeof(AbstractBase), sameAssembly: true, out var accessibility, out _), Is.True);
            Assert.That(accessibility, Is.EqualTo("internal"));
        }

        [Test]
        public void PropertyWithOneAccessorOverridden_GetsAnOverrideOfTheOtherOne()
        {
            var stub = Generate(typeof(SetterOverridden));

            Assert.That(stub, Does.Contain($"public override System.Int32 Value {{ get => {Throw}; }}"));
            Assert.That(stub, Does.Contain($"public override System.Int32 this[System.Int32 index] {{ get => {Throw}; }}"));
            Assert.That(stub, Does.Not.Contain("set =>"));
        }

        [Test]
        public void PropertyWithOneAccessorOverridden_KeepsTheAccessOfTheProperty() =>
            Assert.That(Generate(typeof(GetterOverridden)),
                Does.Contain($"public override System.Int32 Value {{ protected set => {Throw}; }}"));

        [Test]
        public void NestedTypeOfGenericType_KeepsTheArgumentsOfItsOuterType()
        {
            var stub = Generate(typeof(IDictionaryLike));

            Assert.That(stub, Does.Contain("System.Collections.Generic.Dictionary<System.String, System.Int32>.KeyCollection Keys"));
            Assert.That(stub, Does.Contain($"public void Take(System.Collections.Generic.List<System.Int32>.Enumerator enumerator) => {Throw};"));
        }

        [TestCase(typeof(GenericOuter<int>.INested), ExpectedResult = "GenericOuter<System.Int32>.INested")]
        [TestCase(typeof(GenericOuter<List<string>>.Nested<int>), ExpectedResult = "GenericOuter<System.Collections.Generic.List<System.String>>.Nested<System.Int32>")]
        public string NestedGenericBaseType_IsWrittenWithEveryArgument(Type baseType)
        {
            var stub = Generate(baseType);
            var line = stub.Split('\n').First(text => text.Contains(" class NewStub : "));

            return line.Substring(line.IndexOf("GenericOuter<", StringComparison.Ordinal)).Trim();
        }

        [Test]
        public void StubWithoutNamespace_IsNotIndented()
        {
            SerializeReferenceScriptCreator.TryGenerateStub(
                className: "NewStub", nspace: null, baseType: typeof(IByRef), sameAssembly: true, out var stub, out _);

            Assert.That(stub, Does.Contain($"{Environment.NewLine}    public void Move("));
            Assert.That(stub, Does.Not.Contain("namespace"));
        }

        [Test]
        public void ScriptInAnotherAssemblyFolder_IsNotTheSameAssembly()
        {
            const string testsFolder = "Packages/tech.aspid.fasttools/Tests/Editor/SerializeReferences";

            Assert.That(SerializeReferenceScriptCreator.IsSameAssembly($"{testsFolder}/NewStub.cs", typeof(SerializeReferenceScriptCreatorStubTests)), Is.True);
            Assert.That(SerializeReferenceScriptCreator.IsSameAssembly("Assets/NewStub.cs", typeof(SerializeReferenceScriptCreatorStubTests)), Is.False);
        }
    }
}
