# Aspid.FastTools.Analyzers

Roslyn analyzers of the package, diagnostics `AFT*`. The DLL is committed into the Unity package; the `.claude/hooks/rebuild-analyzers-on-change.sh` hook rebuilds it on `*.cs` edits here (Tests/Sample excluded).

- **The DLL is copied into the package only in Release** (`Directory.Build.targets`): Tests and Sample reference the analyzer, so a Debug `dotnet test` run would otherwise overwrite the shipped DLL.
- **The analyzer is fed to the compiler of every assembly in the user's project** (`RoslynAnalyzer` label in `.meta`) — a diagnostic must check which assembly it is looking at, or it fires in consumer code (see `AFT0009`–`AFT0011`, `AFT0013`).
- **New diagnostic:** stable id (retired ids are never reused), an entry in `AnalyzerReleases.Unshipped.md`, two tests — one firing on violating code, one silent on correct code.
- Nothing that writes to `Console` — it deadlocks Unity's compilation server.
