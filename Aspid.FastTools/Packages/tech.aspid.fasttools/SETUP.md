# Aspid.FastTools: Setup

Offline setup notes for the Asset Store package. The documentation (a guide for each feature), the sample tutorials and the API reference are on the site: https://vpdpersonal.github.io/Aspid.FastTools/

1. [Requirements](#1-requirements)
2. [Install](#2-install)
3. [First steps](#3-first-steps)
4. [Troubleshooting](#4-troubleshooting)
5. [Support](#5-support)

## 1. Requirements

- Unity 6000.0.53f1 or newer.
- No other packages: Aspid.FastTools uses only the built-in UI Toolkit module.
- The EnumValues and ProfilerMarkers samples also use the built-in Physics module, which new projects enable.

## 2. Install

1. Open **Window → Package Manager** and switch to **My Assets**.
2. Select **Aspid.FastTools** and click **Install**. The package goes to the **Packages** list of the project, not to `Assets`.
3. Wait for Unity to compile. The Console must show no errors.

Later versions arrive as **Update** on the package in the Package Manager.

The Asset Store and the git URL on GitHub install the same package, `tech.aspid.fasttools`, so the project holds one of them. To switch the source, remove the package in **Window → Package Manager** (**In Project**) first.

## 3. First steps

1. Open **Tools → Aspid 🐍 → FastTools → Welcome**. The window also opens by itself after the first import.
2. Find a sample in the **Samples** list and click **Import**. Unity copies it into `Assets/Samples/Aspid.FastTools`. The **Samples** tab of the package in the Package Manager imports the same samples.
3. Open the `Documentation/README.md` of the sample. It says which scene or menu item to open.
4. Read the guide for the feature you need: https://vpdpersonal.github.io/Aspid.FastTools/docs

## 4. Troubleshooting

**The Console shows `Assembly with name 'Aspid.FastTools' already exists`.**
The project has a second copy of the package, for example a copy of its folder under `Assets`. Delete that copy and keep the package in the Package Manager.

**Compile errors appear right after the install.**
Check the Unity version in Unity Hub or in **About Unity** (**Help → About Unity** on Windows and Linux, **Unity → About Unity** on macOS). It must be 6000.0.53f1 or newer: earlier 6000.0 patches lack UI Toolkit APIs that the package uses.

## 5. Support

Report bugs and questions in [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Include your Unity version, the package version and the steps to reproduce the problem.

Aspid.FastTools is distributed under the MIT License (`LICENSE.md`).
