---
name: build-analyzer
description: Build the Roslyn analyzer and deploy the resulting DLL into the Unity package
user-invocable: false
---

From the repository root run:

```
dotnet build Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.csproj -c Release
dotnet test Aspid.FastTools.Analyzers/Aspid.FastTools.Analyzers.sln -c Release
```

Stop if any test fails.

The build itself deploys: `Directory.Build.targets` copies the DLL into the Unity package — no manual copy step. The copy is Release-only, so a Debug build leaves the shipped DLL untouched.

Report build/test output and any errors.

Arguments: $ARGUMENTS (optional: pass `Debug` to build in Debug configuration instead of Release — note that a Debug build does not deploy)
