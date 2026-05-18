---
name: build-generator
description: Build Roslyn source generators and deploy the resulting DLL into the Unity package
user-invocable: true
---

Build the Aspid.FastTools source generators and deploy to Unity:

1. Run `dotnet build Aspid.FastTools.Generators/Aspid.FastTools.Generators/Aspid.FastTools.Generators.csproj -c Release` from the repository root
2. Copy `Aspid.FastTools.Generators/Aspid.FastTools.Generators/bin/Release/netstandard2.0/Aspid.FastTools.Generators.dll` to `Aspid.FastTools/Assets/Aspid/FastTools/Aspid.FastTools.Generators.dll`
3. Report the result: build output, any errors, and confirm the DLL was copied successfully

Arguments: $ARGUMENTS (optional: pass `Debug` to build in Debug configuration instead of Release)
