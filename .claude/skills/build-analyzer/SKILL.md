---
name: build-analyzer
description: Build the Roslyn analyzer submodule and deploy the resulting DLL into the Unity package
user-invocable: false
---

From the repository root:

1. If `Aspid.FastTools.Analyzers/` is empty, run `git submodule update --init` first
2. `dotnet build Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.csproj -c Release`
3. `dotnet test Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.sln -c Release` — stop if any test fails
4. Copy `Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/bin/Release/netstandard2.0/Aspid.FastTools.Analyzers.dll` to `Aspid.FastTools/Packages/tech.aspid.fasttools/Aspid.FastTools.Analyzers.dll` (the submodule has no auto-copy targets)

Report build/test output and any errors. After bumping the submodule commit, stage the gitlink in the superproject (`git add Aspid.FastTools.Analyzers`).

Arguments: $ARGUMENTS (optional: pass `Debug` to build in Debug configuration instead of Release)
