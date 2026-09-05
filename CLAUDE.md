# Aspid.FastTools

Unity package `tech.aspid.fasttools` (`Aspid.FastTools/Packages/tech.aspid.fasttools/`) plus two standalone .NET solutions,
`Aspid.FastTools.Generators/` and `Aspid.FastTools.Analyzers/`, whose DLLs are committed into the package.

## Never

- Use Unity APIs newer than **6000.0** — the dev project runs 6000.4, but `package.json` promises 6000.0.
- Write to `Console` from generator or analyzer code, or reference `SourceGenerator.Foundations` —
  it deadlocks Unity's compilation server.

## Not obvious

- A change to generator or analyzer source reaches Unity **only** after `dotnet build -c Release` in that solution;
  `dotnet test` (Debug) deliberately does not copy the DLL, so it is safe to run.