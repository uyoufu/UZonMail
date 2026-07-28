---
title: Building from Source
icon: code
order: 10
description: UzonMail build guide — how to manually compile the open-source bulk email software on Windows.
permalink: /en/guide/build
---

This method is not recommended for regular users. Manual compilation requires programming knowledge. You can download prebuilt releases from [Versions](/versions).

This page describes compilation on Windows.

## Requirements

Make sure the following tools are available in your command line:

- Git
- 7z
- .NET 10.0 SDK
- Bun
- Docker Desktop or Docker installed inside WSL (only for Docker images)

## Build Steps

1. Open a terminal.

2. Clone the repository: `git clone https://github.com/GalensGan/UzonMail` and switch to the `master` branch.

3. Go to the `scripts` directory and use the unified entry point. Outputs are written to the repository-root `build` directory.

   | Target | Command | Output |
   | --- | --- | --- |
   | choose targets interactively | `./build.ps1` | Use arrow keys to move, Space to select, and Enter to confirm |
   | desktop | `./build.ps1 -Target Desktop` | `build/uzonmail-desktop-win-x64-version.zip` |
   | Windows server | `./build.ps1 -Target WindowsServer` | `build/uzonmail-service-win-x64-version.zip` |
   | Linux server | `./build.ps1 -Target Linux` | `build/uzonmail-service-linux-x64-version.zip` |
   | Docker from a local Linux build | `./build.ps1 -Target Docker` | Docker image |
   | all packages and image | `./build.ps1 -Target All,Docker` | All ZIP files and Docker images |
   | Docker from an existing Linux ZIP | `./build.ps1 -Target Docker -LinuxPackageUrl <URL>` | Docker image |

   Docker images are pushed only with `-PushDockerImage`. ZIP files are uploaded through an installed `od` command only with `-UploadArtifacts`. `-UpdateSource` fast-forwards the current branch only when the working tree is clean; it never switches branches. Use `-WslDistribution <name>` to select a non-default WSL distribution.

   Build success screenshot:

   ![build screenshot](https://oss.uzoncloud.com:2234/public/files/images/image-20240616124656131.png)

::: tip
The script validates prerequisites for the selected target. Docker builds prefer a local Docker daemon and fall back to Docker in WSL. A plugin is published automatically when its direct child directory under `src/api/Plugins` contains one project file.
:::
