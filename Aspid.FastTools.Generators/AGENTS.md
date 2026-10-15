# Aspid.FastTools.Generators

## Never

- Write to `Console` from generator code, or reference `SourceGenerator.Foundations`.

## Rules

- After a change to generator source, run `dotnet build -c Release` here and commit the rebuilt DLL in the package.
  Run it before `PackagedAnalyzerTests` too: they test the committed DLL.

## Checks

- `dotnet test`.
