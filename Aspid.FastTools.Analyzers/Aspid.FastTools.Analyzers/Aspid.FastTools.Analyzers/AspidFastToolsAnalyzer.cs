using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Aspid.FastTools.Analyzers.Descriptions;
using UnityAttributes = Aspid.FastTools.Analyzers.Descriptions.UnityEngine.AttributesDescription;
using UnityClasses = Aspid.FastTools.Analyzers.Descriptions.UnityEngine.ClassesDescription;
using AspidAttributes = Aspid.FastTools.Analyzers.Descriptions.AspidFastTools.AttributesDescription;
using AspidClasses = Aspid.FastTools.Analyzers.Descriptions.AspidFastTools.ClassesDescription;
using AspidEnums = Aspid.FastTools.Analyzers.Descriptions.AspidFastTools.EnumsDescription;

namespace Aspid.FastTools.Analyzers;

/// <summary>
/// Validates <c>[TypeSelector]</c> usage. The attribute drives two field shapes — a <c>string</c> holding an
/// assembly-qualified type name, and a <c>[SerializeReference]</c> managed reference that is instantiated — so some
/// of its knobs apply only in one context. These diagnostics surface a misuse at compile time instead of at runtime.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AspidFastToolsAnalyzer : DiagnosticAnalyzer
{
    private const string ListDefinition = "System.Collections.Generic.List<T>";

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticRules.TypeSelectorFieldTypeRule,
            DiagnosticRules.TypeSelectorAllowRule,
            DiagnosticRules.TypeSelectorBaseTypeRule,
            DiagnosticRules.TypeSelectorObjectDerivedRule,
            DiagnosticRules.TypeSelectorNoConcreteImplementationRule,
            DiagnosticRules.TypeSelectorMemberNotFoundRule,
            DiagnosticRules.TypeSelectorMemberUnsuitableRule,
            DiagnosticRules.TypeSelectorTypeNameSyntaxRule,
            DiagnosticRules.TypeSelectorDisjointBaseTypesRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(compilationContext =>
        {
            // One cache per compilation: the AFT0005 candidate search walks assembly metadata, so its results are
            // memoised per (base type, field element type) pair and the walk itself is limited to assemblies that
            // can actually contain a candidate (see CandidateSearch).
            var candidateSearch = new CandidateSearch(compilationContext.Compilation);

            compilationContext.RegisterSyntaxNodeAction(
                ctx => AnalyzeField(ctx, candidateSearch),
                SyntaxKind.FieldDeclaration);
        });
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context, CandidateSearch candidateSearch)
    {
        var field = (FieldDeclarationSyntax)context.Node;

        var typeSelector = FindAttribute(field, context.SemanticModel, AspidAttributes.TypeSelectorFull);
        if (typeSelector is null) return;

        if (field.Declaration.Variables.Count == 0) return;
        if (context.SemanticModel.GetDeclaredSymbol(field.Declaration.Variables[0]) is not IFieldSymbol fieldSymbol) return;

        // Unwrap arrays / List<T> so the checks see the element type a [SerializeReference] entry actually holds.
        var elementType = GetElementType(fieldSymbol.Type);
        var isString = elementType.SpecialType == SpecialType.System_String;
        var isSerializableType = IsSerializableType(elementType);
        var isManagedReference = FindAttribute(field, context.SemanticModel, UnityAttributes.SerializeReferenceFull) is not null;

        // AFT0001 — none of the valid shapes (a string type-name field, a SerializableType / SerializableMonoScript
        // wrapper, or a [SerializeReference] managed reference): the drawer renders an error box instead of the field.
        if (!isString && !isSerializableType && !isManagedReference)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticRules.TypeSelectorFieldTypeRule, typeSelector.GetLocation(), fieldSymbol.Name));
            return;
        }

        // AFT0006/AFT0007/AFT0008 — the drawer resolves each string argument member-first: a valid identifier is
        // looked up as a field/property on the target object, anything else falls back to Type.GetType. Both
        // failures are silent at runtime (the picker just loses its constraint), so they are surfaced here.
        ReportStringArguments(context, typeSelector, fieldSymbol);

        // AFT0009 — every field shape intersects the base types, so two of them that share no type empty the picker.
        var hasDisjointPair = ReportDisjointBaseTypePairs(context, typeSelector);

        // On a string or SerializableType field both Allow and the base types are meaningful (a Type is named, not
        // instantiated), and none of the managed-reference-only checks below apply.
        if (!isManagedReference) return;

        ReportAllowOnManagedReference(context, typeSelector, fieldSymbol.Name);

        // AFT0003 — a base provably disjoint from the field type empties the picker on its own.
        var hasDisjointBase = ReportDisjointBaseTypes(context, typeSelector, elementType);

        // AFT0004 — element type derives from UnityEngine.Object: Unity silently skips it for managed references.
        if (ReportObjectDerivedManagedReference(context, typeSelector, fieldSymbol.Name, elementType)) return;

        // AFT0003 or AFT0009 already says the selector is empty — AFT0005 on top would be redundant noise.
        if (hasDisjointPair || hasDisjointBase) return;

        // AFT0005 — no visible concrete implementation exists for the effective base set.
        ReportNoConcreteImplementation(context, typeSelector, fieldSymbol.Name, elementType, candidateSearch);
    }

    // System.Type — the member value shape the drawer reads reflectively (besides string); matched by display name
    // so the tests need no reference resolution tricks.
    private const string SystemTypeFull = "System.Type";

    // AFT0006/AFT0007/AFT0008 — validate every constant-string positional argument of [TypeSelector(...)]. The
    // drawer contract: a valid C# identifier names an instance field/property on the target object whose value
    // (Type / Type[] / string / string[]) supplies the base types; any other string must be an assembly-qualified
    // type name for Type.GetType.
    private static void ReportStringArguments(SyntaxNodeAnalysisContext context, AttributeSyntax typeSelector, IFieldSymbol fieldSymbol)
    {
        if (typeSelector.ArgumentList is null) return;

        foreach (var argument in typeSelector.ArgumentList.Arguments)
        {
            if (argument.NameEquals is not null) continue; // skip Allow = ... / Required = ...
            ValidateStringExpression(context, argument.Expression, fieldSymbol);
        }
    }

    private static void ValidateStringExpression(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, IFieldSymbol fieldSymbol)
    {
        // The params string[] overload can be called with an explicit array — validate each element.
        var initializer = expression switch
        {
            ArrayCreationExpressionSyntax array => array.Initializer,
            ImplicitArrayCreationExpressionSyntax implicitArray => implicitArray.Initializer,
            _ => null
        };

        if (initializer is not null)
        {
            foreach (var element in initializer.Expressions)
                ValidateStringExpression(context, element, fieldSymbol);
            return;
        }

        var constant = context.SemanticModel.GetConstantValue(expression);
        if (!constant.HasValue || constant.Value is not string name) return;

        // The drawer filters out blank names before resolving — no constraint, but nothing to diagnose.
        if (string.IsNullOrWhiteSpace(name)) return;

        if (SyntaxFacts.IsValidIdentifier(name))
        {
            // Identifier → member reference. The drawer walks the runtime type's hierarchy with instance-only
            // binding flags, so the member must be an instance field/property visible from the declaring type.
            var member = FindMemberFromHierarchy(fieldSymbol.ContainingType, name);

            if (member is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.TypeSelectorMemberNotFoundRule, expression.GetLocation(),
                    fieldSymbol.Name, name, fieldSymbol.ContainingType.Name));
            }
            else if (!IsSuitableConstraintSource(member))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.TypeSelectorMemberUnsuitableRule, expression.GetLocation(),
                    fieldSymbol.Name, name));
            }
        }
        else if (!IsPlausibleTypeName(name))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticRules.TypeSelectorTypeNameSyntaxRule, expression.GetLocation(),
                fieldSymbol.Name, name));
        }
    }

    // Mirrors the drawer's GetMemberFromHierarchy: nearest declaration wins. When several members share the name
    // at one level, a suitable field/property is preferred so an overload/shadow doesn't produce a false positive.
    private static ISymbol? FindMemberFromHierarchy(INamedTypeSymbol type, string name)
    {
        for (var current = type; current is not null; current = current.BaseType)
        {
            var members = current.GetMembers(name);
            if (members.Length == 0) continue;

            foreach (var member in members)
                if (IsSuitableConstraintSource(member)) return member;

            return members[0];
        }

        return null;
    }

    // A member the drawer can read as a base-type source: an instance field or property whose (element) type is
    // System.Type, string, or a SerializableType / SerializableType<T> wrapper. Static members are invisible to the
    // drawer's instance-only lookup.
    private static bool IsSuitableConstraintSource(ISymbol member)
    {
        var memberType = member switch
        {
            IFieldSymbol { IsStatic: false } field => field.Type,
            IPropertySymbol { IsStatic: false } property => property.Type,
            _ => null
        };

        if (memberType is null) return false;
        if (memberType is IArrayTypeSymbol array) memberType = array.ElementType;

        return memberType.SpecialType == SpecialType.System_String ||
            memberType.ToDisplayString() == SystemTypeFull ||
            IsSerializableType(memberType);
    }

    // Light syntax check for an assembly-qualified name: the type part is dot/plus-separated identifiers (each
    // optionally arity-suffixed with `N), the comma-separated tail parts are non-empty. Generic/array forms with
    // brackets are left to the editor — their grammar is not worth reimplementing here.
    private static bool IsPlausibleTypeName(string name)
    {
        if (name.IndexOf('[') >= 0) return true;

        var parts = name.Split(',');
        foreach (var part in parts)
            if (string.IsNullOrWhiteSpace(part)) return false;

        foreach (var segment in parts[0].Split('.', '+'))
        {
            var identifier = segment.Trim();

            var arityIndex = identifier.IndexOf('`');
            if (arityIndex >= 0)
            {
                var arity = identifier.Substring(arityIndex + 1);
                if (arity.Length == 0 || !arity.All(char.IsDigit)) return false;

                identifier = identifier.Substring(0, arityIndex);
            }

            if (!SyntaxFacts.IsValidIdentifier(identifier)) return false;
        }

        return true;
    }

    // AFT0002 — Allow opts abstract classes / interfaces into the candidate list, which cannot be instantiated for a
    // managed reference, so the flag is silently ignored on this path.
    private static void ReportAllowOnManagedReference(SyntaxNodeAnalysisContext context, AttributeSyntax typeSelector, string fieldName)
    {
        if (typeSelector.ArgumentList is null) return;

        foreach (var argument in typeSelector.ArgumentList.Arguments)
        {
            if (argument.NameEquals?.Name.Identifier.ValueText != AspidEnums.AllowArgument) continue;

            var constant = context.SemanticModel.GetConstantValue(argument.Expression);
            if (!constant.HasValue || constant.Value is null) return;

            try
            {
                if (Convert.ToInt64(constant.Value) != AspidEnums.TypeAllowNone)
                    context.ReportDiagnostic(Diagnostic.Create(
                        DiagnosticRules.TypeSelectorAllowRule, argument.GetLocation(), fieldName));
            }
            catch (Exception)
            {
                // A non-integral constant cannot be a TypeAllow flag — nothing to report.
            }

            return;
        }
    }

    // AFT0003 — a typeof(...) base type unrelated to the field's element type narrows the candidate list to nothing.
    // Returns true when any base was reported, so AFT0005 can skip the already-empty selector.
    private static bool ReportDisjointBaseTypes(
        SyntaxNodeAnalysisContext context, AttributeSyntax typeSelector, ITypeSymbol fieldElementType)
    {
        var reported = false;

        foreach (var (typeOf, baseType) in CollectTypeofBases(typeSelector, context.SemanticModel))
        {
            if (!AreProvablyDisjoint(baseType, fieldElementType)) continue;

            context.ReportDiagnostic(Diagnostic.Create(
                DiagnosticRules.TypeSelectorBaseTypeRule, typeOf.GetLocation(), baseType.Name, fieldElementType.Name));
            reported = true;
        }

        return reported;
    }

    // AFT0009 — the picker offers only types assignable to every base, so two typeof(...) bases that share no type
    // (two unrelated classes, or a sealed class and an interface it does not implement) leave it empty. Reported once
    // per argument, on the later one of its first such pair; returns true when any pair was reported.
    private static bool ReportDisjointBaseTypePairs(SyntaxNodeAnalysisContext context, AttributeSyntax typeSelector)
    {
        var bases = CollectTypeofBases(typeSelector, context.SemanticModel);
        var reported = false;

        for (var later = 1; later < bases.Length; later++)
        {
            for (var earlier = 0; earlier < later; earlier++)
            {
                if (!AreProvablyDisjoint(bases[earlier].Type, bases[later].Type)) continue;

                context.ReportDiagnostic(Diagnostic.Create(
                    DiagnosticRules.TypeSelectorDisjointBaseTypesRule,
                    bases[later].TypeOf.GetLocation(),
                    bases[earlier].Type.Name,
                    bases[later].Type.Name));
                reported = true;
                break;
            }
        }

        return reported;
    }

    // The statically checkable positional arguments of [TypeSelector(...)]: typeof(...) expressions other than the
    // unconstrained typeof(object). String arguments are resolved at edit time and are not part of this set.
    private static ImmutableArray<(TypeOfExpressionSyntax TypeOf, ITypeSymbol Type)> CollectTypeofBases(
        AttributeSyntax typeSelector, SemanticModel model)
    {
        if (typeSelector.ArgumentList is null) return ImmutableArray<(TypeOfExpressionSyntax, ITypeSymbol)>.Empty;

        var builder = ImmutableArray.CreateBuilder<(TypeOfExpressionSyntax, ITypeSymbol)>();
        foreach (var argument in typeSelector.ArgumentList.Arguments)
        {
            if (argument.NameEquals is not null) continue;                          // skip Allow = ...
            if (argument.Expression is not TypeOfExpressionSyntax typeOf) continue;

            if (model.GetTypeInfo(typeOf.Type).Type is not { } type) continue;
            if (type.SpecialType == SpecialType.System_Object) continue;             // the unconstrained default narrows nothing

            builder.Add((typeOf, type));
        }

        return builder.ToImmutable();
    }

    // AFT0004 — element type derives from UnityEngine.Object: Unity silently does not serialize such types as
    // managed references. The user should drop [SerializeReference] and use a plain object reference.
    // Returns true when the diagnostic was reported (so the caller can skip further managed-reference checks).
    private static bool ReportObjectDerivedManagedReference(
        SyntaxNodeAnalysisContext context, AttributeSyntax typeSelector, string fieldName, ITypeSymbol elementType)
    {
        if (!IsUnityObjectDerived(elementType)) return false;

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticRules.TypeSelectorObjectDerivedRule,
            typeSelector.GetLocation(),
            fieldName,
            elementType.Name));

        return true;
    }

    // AFT0005 — no visible concrete (non-abstract, non-UnityEngine.Object, non-delegate, non-string) class exists
    // that implements every effective base type. Severity is Warning only because implementations may live in
    // downstream assemblies the compilation cannot see.
    private static void ReportNoConcreteImplementation(
        SyntaxNodeAnalysisContext context,
        AttributeSyntax typeSelector,
        string fieldName,
        ITypeSymbol elementType,
        CandidateSearch candidateSearch)
    {
        // The effective base set: typeof(...) arguments when present, otherwise the element type itself.
        var typeofBases = CollectTypeofBases(typeSelector, context.SemanticModel);
        var bases = typeofBases.Length > 0
            ? typeofBases.Select(entry => entry.Type).ToImmutableArray()
            : ImmutableArray.Create(elementType);

        // Skip the search when a base is a concrete instantiable class meeting the other bases — it is its own
        // candidate.
        foreach (var baseType in bases)
        {
            if (!IsConcreteInstantiable(baseType) || IsUnityObjectDerived(baseType)) continue;
            if (bases.All(other => IsAssignableTo(baseType, other))) return;
        }

        if (candidateSearch.HasVisibleCandidate(bases, elementType, context.CancellationToken)) return;

        context.ReportDiagnostic(Diagnostic.Create(
            DiagnosticRules.TypeSelectorNoConcreteImplementationRule,
            typeSelector.GetLocation(),
            fieldName,
            string.Join(" and ", bases.Select(baseType => $"'{baseType.Name}'"))));
    }

    /// <summary>
    /// The AFT0005 candidate search: does any visible concrete class satisfy every base type and the field's
    /// element type? A naive walk of <see cref="Compilation.GlobalNamespace"/> materialises every symbol of every
    /// referenced assembly (all of the BCL and UnityEngine — minutes of csc time in a Unity compilation), so the
    /// search instead only walks assemblies that could contain a candidate: a type assignable to a base must live
    /// in an assembly that declares or references the base's assembly (metadata cannot derive from an unreferenced
    /// type), and likewise for the field's element type. The walk stops at the first match, and results are memoised
    /// per (base set, field element type) pair for the lifetime of the compilation.
    /// </summary>
    private sealed class CandidateSearch
    {
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<SearchKey, bool> _results;

        public CandidateSearch(Compilation compilation)
        {
            _compilation = compilation;
            _results = new ConcurrentDictionary<SearchKey, bool>();
        }

        // Returns true when at least one candidate in the compilation is assignable to EVERY base type and to
        // fieldElementType. The picker intersects the typeof(...) base set with the field's declared element type,
        // so a candidate must satisfy all constraints to be reachable. When fieldElementType is System.Object the
        // field constraint is trivially true for any candidate and is skipped.
        public bool HasVisibleCandidate(
            ImmutableArray<ITypeSymbol> baseTypes, ITypeSymbol fieldElementType, CancellationToken cancellationToken)
        {
            var key = new SearchKey(baseTypes, fieldElementType);
            if (_results.TryGetValue(key, out var cached)) return cached;

            var constraints = new Constraints(baseTypes, fieldElementType);
            var result = Scan(constraints, cancellationToken);
            return _results.GetOrAdd(key, result);
        }

        private bool Scan(Constraints constraints, CancellationToken cancellationToken)
        {
            var baseAssemblies = constraints.BaseTypes
                .Select(baseType => baseType.OriginalDefinition.ContainingAssembly)
                .ToImmutableArray();
            var fieldAssembly = constraints.FieldIsObject
                ? null
                : constraints.FieldElementType.OriginalDefinition.ContainingAssembly;

            // The source assembly first — candidates most often live next to the field — then only the references
            // that can see every constraint type. A null constraint assembly (error type) filters nothing.
            if (ScanAssembly(_compilation.Assembly, constraints, cancellationToken))
                return true;

            foreach (var reference in _compilation.SourceModule.ReferencedAssemblySymbols)
            {
                if (!baseAssemblies.All(baseAssembly => Sees(reference, baseAssembly)) || !Sees(reference, fieldAssembly))
                    continue;
                if (ScanAssembly(reference, constraints, cancellationToken))
                    return true;
            }

            return false;
        }

        private static bool ScanAssembly(IAssemblySymbol assembly, Constraints constraints, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return ScanNamespace(assembly.GlobalNamespace, constraints);
        }

        private static bool ScanNamespace(INamespaceSymbol ns, Constraints constraints)
        {
            foreach (var type in ns.GetTypeMembers())
                if (ScanType(type, constraints)) return true;

            foreach (var nested in ns.GetNamespaceMembers())
                if (ScanNamespace(nested, constraints)) return true;

            return false;
        }

        private static bool ScanType(INamedTypeSymbol type, Constraints constraints)
        {
            if (IsCandidate(type, constraints)) return true;

            // Recurse into nested types.
            foreach (var nested in type.GetTypeMembers())
                if (ScanType(nested, constraints)) return true;

            return false;
        }

        // A candidate is a concrete, non-abstract, non-static class, not derived from UnityEngine.Object, not string,
        // not a delegate, assignable to every constraint type. For open generic candidates assignability is tested
        // against original definitions to avoid needing concrete type arguments (any closed form would still be
        // assignable to the bases). Cheapest checks first: the UnityEngine.Object walk only runs on a type that
        // already matched every constraint.
        private static bool IsCandidate(INamedTypeSymbol type, Constraints constraints)
        {
            if (!IsConcreteInstantiable(type)) return false;

            var testFrom = type.IsGenericType ? type.OriginalDefinition : type;

            foreach (var baseType in constraints.BaseTypes)
                if (!IsAssignableTo(testFrom, AsDefinition(baseType))) return false;

            if (!constraints.FieldIsObject && !IsAssignableTo(testFrom, AsDefinition(constraints.FieldElementType)))
                return false;

            return !IsUnityObjectDerived(type);
        }

        private static ITypeSymbol AsDefinition(ITypeSymbol type) =>
            type.IsDefinition ? type : (type as INamedTypeSymbol)?.OriginalDefinition ?? type;

        // True when the assembly is (or references) the target, i.e. its metadata can declare a type derived from a
        // type of the target. A null target (unresolved constraint type) filters nothing.
        private static bool Sees(IAssemblySymbol assembly, IAssemblySymbol? target)
        {
            if (target is null) return true;
            if (SymbolEqualityComparer.Default.Equals(assembly, target)) return true;

            foreach (var module in assembly.Modules)
                foreach (var referenced in module.ReferencedAssemblySymbols)
                    if (SymbolEqualityComparer.Default.Equals(referenced, target)) return true;

            return false;
        }

        private sealed class Constraints
        {
            public Constraints(ImmutableArray<ITypeSymbol> baseTypes, ITypeSymbol fieldElementType)
            {
                BaseTypes = baseTypes;
                FieldElementType = fieldElementType;
                FieldIsObject = fieldElementType.SpecialType == SpecialType.System_Object;
            }

            public ImmutableArray<ITypeSymbol> BaseTypes { get; }

            public ITypeSymbol FieldElementType { get; }

            public bool FieldIsObject { get; }
        }

        private readonly struct SearchKey : IEquatable<SearchKey>
        {
            private readonly ImmutableArray<ITypeSymbol> _baseTypes;
            private readonly ITypeSymbol _fieldElementType;

            public SearchKey(ImmutableArray<ITypeSymbol> baseTypes, ITypeSymbol fieldElementType)
            {
                _baseTypes = baseTypes;
                _fieldElementType = fieldElementType;
            }

            public bool Equals(SearchKey other) =>
                SymbolEqualityComparer.Default.Equals(_fieldElementType, other._fieldElementType) &&
                _baseTypes.SequenceEqual<ITypeSymbol, ITypeSymbol>(other._baseTypes, SymbolEqualityComparer.Default);

            public override bool Equals(object? obj) => obj is SearchKey other && Equals(other);

            public override int GetHashCode()
            {
                var hash = SymbolEqualityComparer.Default.GetHashCode(_fieldElementType);
                foreach (var baseType in _baseTypes)
                    hash = hash * 397 ^ SymbolEqualityComparer.Default.GetHashCode(baseType);

                return hash;
            }
        }
    }

    private static bool IsConcreteInstantiable(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named) return false;
        if (named.TypeKind != TypeKind.Class) return false;
        if (named.IsAbstract || named.IsStatic) return false;
        if (named.SpecialType == SpecialType.System_String) return false;
        if (named.TypeKind == TypeKind.Delegate) return false;

        return true;
    }

    // Walks the base chain to find UnityEngine.Object by full display name. Intentionally matches both the real
    // Unity type and the test stub (both share the same namespace + class name).
    private static bool IsUnityObjectDerived(ITypeSymbol type)
    {
        for (var t = type as INamedTypeSymbol; t is not null; t = t.BaseType)
            if (t.ToDisplayString() == UnityClasses.ObjectFull) return true;

        return false;
    }

    private static AttributeSyntax? FindAttribute(FieldDeclarationSyntax field, SemanticModel model, string fullName)
    {
        foreach (var attribute in field.AttributeLists.SelectMany(list => list.Attributes))
        {
            if (model.GetSymbolInfo(attribute).Symbol is not IMethodSymbol constructor) continue;
            if (constructor.ContainingType.ToDisplayString() == fullName) return attribute;
        }

        return null;
    }

    private static ITypeSymbol GetElementType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array) return array.ElementType;

        if (type is INamedTypeSymbol { IsGenericType: true } named &&
            named.OriginalDefinition.ToDisplayString() == ListDefinition)
            return named.TypeArguments[0];

        return type;
    }

    // A SerializableType / SerializableType<T> or SerializableMonoScript / SerializableMonoScript<T> field names a Type
    // (like a string) rather than instantiating one, so [TypeSelector] is valid on it. Matched by the wrapper's original
    // definition so both the non-generic and the open-generic form are recognized; List<>/array are already unwrapped
    // into the element type by the caller.
    private static bool IsSerializableType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named) return false;

        var definition = named.OriginalDefinition.ToDisplayString();
        return definition == AspidClasses.SerializableTypeFull ||
            definition == AspidClasses.SerializableTypeGenericFull ||
            definition == AspidClasses.SerializableMonoScriptFull ||
            definition == AspidClasses.SerializableMonoScriptGenericFull;
    }

    // Two non-interface types with no inheritance relationship can share no concrete instance (single inheritance),
    // so the selector would be empty. An interface paired with a class is only provably disjoint when the class is
    // sealed and does not implement it — no further subtype can add the interface. Two interfaces are never provably
    // disjoint (one class can implement both), so they are left alone to avoid false positives.
    private static bool AreProvablyDisjoint(ITypeSymbol baseType, ITypeSymbol fieldType)
    {
        var baseIsInterface = baseType.TypeKind == TypeKind.Interface;
        var fieldIsInterface = fieldType.TypeKind == TypeKind.Interface;

        if (baseIsInterface && fieldIsInterface) return false;

        if (baseIsInterface || fieldIsInterface)
        {
            var contract = baseIsInterface ? baseType : fieldType;
            var implementation = baseIsInterface ? fieldType : baseType;

            return implementation.IsSealed && !IsAssignableTo(implementation, contract);
        }

        return !IsAssignableTo(baseType, fieldType) && !IsAssignableTo(fieldType, baseType);
    }

    private static bool IsAssignableTo(ITypeSymbol from, ITypeSymbol to)
    {
        if (SymbolEqualityComparer.Default.Equals(from, to)) return true;

        for (var baseType = from.BaseType; baseType is not null; baseType = baseType.BaseType)
            if (SymbolEqualityComparer.Default.Equals(baseType, to)) return true;

        if (to.TypeKind == TypeKind.Interface)
            foreach (var contract in from.AllInterfaces)
                if (SymbolEqualityComparer.Default.Equals(contract, to)) return true;

        return false;
    }
}
