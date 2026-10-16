# Security policy

## Report a vulnerability

Report it privately through the [new advisory form](https://github.com/VPDPersonal/Aspid.FastTools/security/advisories/new), not in a public issue, discussion or pull request. Include the package version, the Unity version, the steps to reproduce and what an attacker gains.

A confirmed vulnerability is fixed in a new release and published as a GitHub security advisory that credits you, unless you prefer to stay anonymous. Please keep the details private until that release is out.

## Supported versions

Only the latest release receives security fixes: the newest stable one, or the newest release candidate while no stable release exists.

## Scope

In scope:

- The package code in `Runtime/` and `Editor/`, for example unsafe handling of project files (the SerializeReference YAML engine rewrites scenes, prefabs and assets) or of serialized type names.
- The Roslyn generator and analyzer DLLs that the package ships, which run inside the compiler.
- The release pipeline: the workflows in `.github/workflows/`, the `upm` and `upm-preview` branches, and committed DLLs that do not match their sources.

Out of scope: bugs with no security impact (open an [issue](https://github.com/VPDPersonal/Aspid.FastTools/issues)) and vulnerabilities in Unity or other packages (report them to their owners).
