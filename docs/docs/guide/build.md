---
title: 代码编译
icon: code
order: 10
description: 宇正群邮（UZonMail）代码编译指南，详细介绍如何在 Windows 环境下手动编译开源邮件群发软件。支持邮件群发、邮件营销，适用于企业级邮件群发场景，是最好用的开源邮件群发软件。
---

建议普通用户不要采用这种方式，手动编译需要有一定的编程能力。可以直接从 [历史版本](/versions) 中下载。

本文仅介绍在 Windows 环境的编译。

## 环境要求

需保证以下工具可在命令行中访问:

- Git
- 7z
- DotNET 9.0 SDK
- Node、yarn
- Docker

## 编译步骤

1. 打开终端

2. 克隆仓库 `git clone https://github.com/GalensGan/UZonMail`，切换到 `master` 分支

3. 进入到项目根目录下的 `scripts` 目录，执行下面的命令 开始编译，编译结果在 `build` 目录中。

   | 类型           | 命令                        | 位置                                       |
   | -------------- | --------------------------- | ------------------------------------------ |
   | desktop        | ./build-desktop.ps1         | build/uzonmail-desktop-win-x64-version.zip |
   | windows server | ./build-win-server.ps1      | build/uzonmail-service-win-x64-version.zip |
   | linux server   | ./build-linux.ps1           | build/uzonmail-linux-x64-version.zip       |
   | docker         | 在进行 linux 编译时，会自动编译 | docker 镜像，镜像名为 uzon-mail:latest     |
   | 全部            | ./build-all.ps1 | 一次编译上述所有平台|

   编译成功截图：

   ![image-20240616124656131](https://oss.uzoncloud.com:2234/public/files/images/image-20240616124656131.png)

::: tip
手动编译时，会自动检测环境，若没有相关环境，请根据提示进行安装。
:::