# Security policy

## Report a vulnerability

Report a vulnerability privately. Do not put the details in a public issue, discussion or pull request.

1. Open the [new advisory form](https://github.com/VPDPersonal/Aspid.FastTools/security/advisories/new). It is the **Report a vulnerability** button on the repository's **Security** tab.
2. Describe the problem: the affected package version, the Unity version, the steps to reproduce and what an attacker gains.
3. Submit the report. Only you and the maintainer can see it, and the fix is discussed in the same private thread.

If the form does not open, start a [discussion](https://github.com/VPDPersonal/Aspid.FastTools/discussions) that says you have a security report, and leave the details out. The maintainer replies with a private way to send them.

A confirmed vulnerability is fixed in a new release and published as a GitHub security advisory that credits you, unless you prefer to stay anonymous. Please keep the details private until that release is out.

## Supported versions

Only the latest version receives security fixes: the newest stable release, or the newest release candidate while no stable release exists. Older versions are not patched: update to the latest one, as the [Installation](https://vpdpersonal.github.io/Aspid.FastTools/docs#installation) section of the documentation describes.

## What to report

In scope:

- The package code in `Runtime/` and `Editor/`, for example unsafe handling of project files (the SerializeReference YAML engine reads and rewrites scenes, prefabs and assets) or of serialized type names.
- The Roslyn generator and analyzer DLLs that the package ships, which run inside the compiler.
- The release pipeline: the workflows in `.github/workflows/`, the `upm` and `upm-preview` branches, and committed DLLs that do not match their sources.

Out of scope:

- A bug with no security impact. Open an [issue](https://github.com/VPDPersonal/Aspid.FastTools/issues) instead.
- A vulnerability in Unity or in another package. Report it to its owner.
