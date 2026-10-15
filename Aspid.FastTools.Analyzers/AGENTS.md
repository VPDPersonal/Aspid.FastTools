# Aspid.FastTools.Analyzers

## Never

- Write to `Console` from analyzer code, or reference `SourceGenerator.Foundations`.

## Rules

- After a change to analyzer source, run `dotnet build -c Release` here and commit the rebuilt DLL in the package.
  Run it before `PackagedAnalyzerTests` in `Aspid.FastTools.Generators/` too: they test the committed DLL.

## Checks

- `dotnet test`.
