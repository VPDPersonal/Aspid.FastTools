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
2. Select **Aspid.FastTools**, click **Download**, then **Import**.
3. Keep every item selected in the **Import Unity Package** window and click **Import**. The files go to `Assets/Aspid/FastTools`.
4. Wait for Unity to compile. The Console must show no errors.

Install the package once. Do not also add it from GitHub with **Install package from git URL…**: both copies hold the same code, and Unity cannot compile a project that has two of them.

## 3. First steps

1. Open **Tools → Aspid 🐍 → FastTools → Welcome**. The window also opens by itself after the first import.
2. Find a sample in the **Samples** list and click **Show**. Unity selects the sample folder in the Project window.
3. Open the `Documentation/README.md` in that folder. It says which scene or menu item to open. Skip its first step, **Import**: the samples are already in your project.
4. Read the guide for the feature you need: https://vpdpersonal.github.io/Aspid.FastTools/docs

## 4. Troubleshooting

**The Console shows `Assembly with name 'Aspid.FastTools' already exists`.**
The project has a second copy of the package. Keep one: remove the package from **Window → Package Manager** (**In Project**), or delete `Assets/Aspid/FastTools`.

**Compile errors appear right after the import.**
Check the Unity version in **Help → About Unity**. It must be 6000.0.53f1 or newer: earlier 6000.0 patches lack UI Toolkit APIs that the package uses.

## 5. Support

Report bugs and questions in [GitHub Issues](https://github.com/VPDPersonal/Aspid.FastTools/issues). Include your Unity version, the package version and the steps to reproduce the problem.

Aspid.FastTools is distributed under the MIT License (`LICENSE.md`).
