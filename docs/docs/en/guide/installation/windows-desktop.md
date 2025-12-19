---
title: Windows 10+
icon: fab fa-windows
order: 3
description: Installation guide for UzonMail desktop on Windows 10/7 and later.
permalink: /en/guide/installation/windows-desktop
---

There are two ways to run the Windows desktop edition: run the backend service and access via web (like the server setup), or use the desktop UI version. Both provide the same core functionality — choose based on preference.

## Web mode

Steps:

1. Install the .NET runtimes: `runtime-9.0.7-windows-x64-installer` and `runtime-aspnetcore-9.0.7-windows-x64-installer` from Microsoft.
2. Download `uzonmail-desktop-win-x64` from Versions and unzip.
3. Run `service/UZonMailService.exe`.

   The UI may show yellow WARN messages which can be ignored.

   ![service run](https://oss.uzoncloud.com:2234/public/files/images/image-20241017221814354.png)

4. Open a browser and visit `http://localhost:22345/`.

## Windows 7

Windows 7 is no longer officially supported by Microsoft; limited support is provided. Follow the Web mode steps above as a reference.

## Windows 10+

Steps for Windows 10 and later:

1. Install `.NET runtime-9.0.7-windows-x64` and `.NET runtime-aspnetcore-9.0.7-windows-x64`.
2. Install `.NET runtime-desktop-9.0.7-windows-x64` (usually included by the OS).
3. Install WebView2 (usually bundled with Edge).
4. Download and unzip `uzonmail-desktop-win-x64` from Versions.
5. Run `UZonMailDesktop.exe`.

::: tip
Step 3 can be skipped in many cases. If `UZonMailDesktop.exe` crashes or shows a blank screen, install WebView2 manually.
:::

## Advanced

To share your machine within a LAN, open port 22345 in the firewall and others can access `http://your-pc-ip:22345`.

See [Windows Server — Allow Port](windows-server#放行端口) for firewall instructions.
