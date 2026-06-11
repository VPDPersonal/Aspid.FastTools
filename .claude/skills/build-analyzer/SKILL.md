---
name: build-analyzer
description: Build the Roslyn analyzer submodule and deploy the resulting DLL into the Unity package
user-invocable: true
---

Build the Aspid.FastTools analyzer (git submodule) and deploy to Unity:

1. If `Aspid.FastTools.Analyzers/` is empty, run `git submodule update --init` first
2. Run `dotnet build Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.csproj -c Release` from the repository root
3. Run `dotnet test Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.sln -c Release` and stop if any test fails
4. Copy `Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/bin/Release/netstandard2.0/Aspid.FastTools.Analyzers.dll` to `Aspid.FastTools/Packages/tech.aspid.fasttools/Aspid.FastTools.Analyzers.dll`
5. Report the result: build/test output, any errors, and confirm the DLL was copied successfully

Note: diagnostic IDs use the `AFT*` prefix. After bumping the submodule commit, remember the gitlink change in the superproject (`git add Aspid.FastTools.Analyzers`).

Arguments: $ARGUMENTS (optional: pass `Debug` to build in Debug configuration instead of Release)
