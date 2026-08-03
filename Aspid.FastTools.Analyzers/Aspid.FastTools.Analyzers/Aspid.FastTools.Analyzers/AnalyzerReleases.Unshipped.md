; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
AFT0001 | Usage | Error | [TypeSelector] applied to an unsupported field
AFT0002 | Usage | Warning | [TypeSelector] Allow has no effect on a managed reference
AFT0003 | Usage | Warning | [TypeSelector] base type shares no concrete type with the field
AFT0004 | Usage | Error | [TypeSelector] managed reference targets a UnityEngine.Object-derived type
AFT0005 | Usage | Warning | [TypeSelector] base type has no visible concrete implementation
AFT0006 | Usage | Error | [TypeSelector] string argument resolves to nothing
AFT0007 | Usage | Error | [TypeSelector] member reference cannot supply base types
AFT0008 | Usage | Warning | [TypeSelector] string argument is not a valid type name
