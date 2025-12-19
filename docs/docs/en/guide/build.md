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
- .NET 9.0 SDK
- Node, yarn
- Docker

## Build Steps

1. Open a terminal.

2. Clone the repository: `git clone https://github.com/GalensGan/UZonMail` and switch to the `master` branch.

3. Go to the `scripts` directory in the project root and run the commands below to start the build. Build outputs are placed in the `build` directory.

   | Target         | Command                     | Output location                              |
   | -------------- | --------------------------- | -------------------------------------------- |
   | desktop        | ./build-desktop.ps1         | build/uzonmail-desktop-win-x64-version.zip   |
   | windows server | ./build-win-server.ps1      | build/uzonmail-service-win-x64-version.zip   |
   | linux server   | ./build-linux.ps1           | build/uzonmail-linux-x64-version.zip         |
   | docker         | Built automatically during linux build | Docker image named `uzon-mail:latest` |
   | all            | ./build-all.ps1             | Builds all targets above                     |

   Build success screenshot:

   ![build screenshot](https://oss.uzoncloud.com:2234/public/files/images/image-20240616124656131.png)

::: tip
During manual build the script will detect the environment and prompt to install missing prerequisites if needed.
:::
