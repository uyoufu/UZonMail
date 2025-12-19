---
title: Windows 10+
icon: fab fa-windows
order: 3
description: 宇正群邮 Windows 桌面版安装教程，详细介绍如何在 Windows 10、Windows 7 等系统上部署宇正群邮邮件群发软件。支持开源邮件群发、邮件营销软件，助力企业和个人高效邮件群发，体验最好用的邮件群发软件。
permalink: /guide/installation/windows-desktop
---

Windows 桌面版本有两种安装方式，一种是像 Windows Server 那样，仅安装服务端，然后通过 Web 使用；另一种是直接使用带有 UI 的桌面版本。

这两种方式的核心都一样，效率也一样，可根据自己的喜好进行安装。

## Web 方式

Web 方式按如下步骤进行安装：

1. 安装 [.NET runtime-9.0.7-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-9.0.7-windows-x64-installer)和 [.NET runtime-aspnetcore-9.0.7-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.7-windows-x64-installer)

2. 从历史版本中下载 `uzonmail-desktop-win-x64` 版本，解压

3. 打开 `service/UZonMailService.exe` 程序

  打开后界面如下图所示，其中黄颜色的 WARN 不用理会。	![image-20241017221814354](https://oss.uzoncloud.com:2234/public/files/images/image-20241017221814354.png)

4. 打开浏览器，输入 [http://localhost:22345/](http://localhost:22345/) 进行使用


## Windows 7

由于微软已经停止对 Win7 的维护，因此本软件只是有限支持，可以上述参考 [Web](#web-方式) 的使用方式。

## Windows 10+

Windows 10 及以上系统按如下步骤进行安装

1. 安装 [.NET runtime-9.0.7-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-9.0.7-windows-x64-installer)和 [.NET runtime-aspnetcore-9.0.7-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.7-windows-x64-installer)
2. 安装 [.NET runtime-desktop-9.0.7-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-9.0.7-windows-x64-installer)(一般系统自带)
3. 安装 [Webview2](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/)(有 edge 浏览器，一般都自带)
4. 从历史版本中下载 `uzonmail-desktop-win-x64` 版本，解压
5. 打开 `UZonMailDesktop.exe` 开始使用

::: tip
第 3 步可以忽略，若打开 `UZonMailDesktop.exe` 时出现闪退或者空白，说明这个环境缺失，需要手动安装一下
:::

## 高级应用

想把自己电脑分享给局域网内的其他朋友使用，可以打开 22345 端口的防火墙，然后对方就可以使用 http://your-pc-ip:22345 来进行访问了。

打开防火墙的方式见 [Windows Server 放行端口](windows-server#放行端口)