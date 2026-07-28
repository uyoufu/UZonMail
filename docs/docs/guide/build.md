---
title: 代码编译
icon: code
order: 10
description: 宇正群邮（UzonMail）代码编译指南，详细介绍如何在 Windows 环境下手动编译开源邮件群发软件。支持邮件群发、邮件营销，适用于企业级邮件群发场景，是最好用的开源邮件群发软件。
permalink: /guide/build
---

建议普通用户不要采用这种方式，手动编译需要有一定的编程能力。可以直接从 [历史版本](/versions) 中下载。

本文仅介绍在 Windows 环境的编译。

## 环境要求

需保证以下工具可在命令行中访问:

- Git
- 7z
- DotNET 10.0 SDK
- Bun
- Docker Desktop 或 WSL 内的 Docker（仅构建 Docker 镜像时需要）

## 编译步骤

1. 打开终端

2. 克隆仓库 `git clone https://github.com/GalensGan/UzonMail`，切换到 `master` 分支

3. 进入项目根目录下的 `scripts` 目录，使用统一入口执行构建。产物位于仓库根目录的 `build` 目录中。

   | 类型 | 命令 | 位置 |
   | --- | --- | --- |
   | 交互选择构建目标 | `./build.ps1` | 使用上下箭头移动、空格选择、Enter 确认 |
   | 桌面端 | `./build.ps1 -Target Desktop` | `build/uzonmail-desktop-win-x64-version.zip` |
   | Windows 服务端 | `./build.ps1 -Target WindowsServer` | `build/uzonmail-service-win-x64-version.zip` |
   | Linux 服务端 | `./build.ps1 -Target Linux` | `build/uzonmail-service-linux-x64-version.zip` |
   | 本地 Linux 构建镜像 | `./build.ps1 -Target Docker` | Docker 镜像 |
   | 全部安装包和镜像 | `./build.ps1 -Target All,Docker` | 全部 ZIP 和 Docker 镜像 |
   | 既有 Linux ZIP 构建镜像 | `./build.ps1 -Target Docker -LinuxPackageUrl <URL>` | Docker 镜像 |

   传入 `-PushDockerImage` 才会推送 Docker 镜像，传入 `-UploadArtifacts` 才会通过已安装的 `od` 上传 ZIP。传入 `-UpdateSource` 会在工作区干净时以 fast-forward 方式同步当前分支；脚本不会切换分支。需要指定 WSL 发行版时，添加 `-WslDistribution <名称>`。

   编译成功截图：

   ![image-20240616124656131](https://oss.uzoncloud.com:2234/public/files/images/image-20240616124656131.png)

::: tip
手动编译时会自动检测当前目标所需环境。Docker 构建优先使用本机 Docker，未检测到可用环境时自动回退至 WSL Docker。新增插件放在 `src/api/Plugins` 的直属目录并包含唯一项目文件后，会随服务端自动发布。
:::
