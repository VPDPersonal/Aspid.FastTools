---
name: build-generator
description: Build Roslyn source generators and deploy the resulting DLL into the Unity package
user-invocable: false
---

From the repository root run:

```
dotnet build Aspid.FastTools.Generators/Aspid.FastTools.Generators/Aspid.FastTools.Generators.csproj -c Release
```

The build itself deploys: `ILRepack.targets` merges helper dependencies into a single-file DLL and `Directory.Build.targets` copies it into the Unity package — no manual copy step.

Report build output and any errors.

Arguments: $ARGUMENTS (optional: pass `Debug` to build in Debug configuration instead of Release)
