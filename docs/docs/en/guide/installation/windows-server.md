---
title: Windows Server
icon: server
order: 2
description: Guide to install UzonMail on Windows Server, including runtime installation, download and service registration.
permalink: /en/guide/installation/windows-server
---

This section explains how to install UzonMail on Windows Server. The server installation has three main parts.

## Install runtime

1. Install `.NET runtime-9.0.7-windows-x64-installer`
2. Install `.NET runtime-aspnetcore-9.0.7-windows-x64-installer`

## Download

Download `uzonmail-service-win-x64-version.zip` from [Versions], unzip and run `UZonMailService.exe`.

If you see the following UI screenshot, the service started successfully:

![service UI](https://oss.uzoncloud.com:2234/public/files/images/image-20241017221814354.png)

You can test by visiting `http://localhost:22345/` in a browser. After confirming it works, close the terminal and proceed to register the service.

## Register service

To run UzonMail as a background service, register `UZonMailService.exe` as a Windows service. Open an elevated CMD or PowerShell, navigate to the extracted folder, and run:

``` powershell
# Register service
uzon-mail install
# Start service
uzon-mail start
```

Other useful commands:

- `uzon-mail stop` — stop service
- `uzon-mail uninstall` — uninstall service
- `uzon-mail status` — view service status
- `uzon-mail restart` — restart service

## Modify configuration

For server deployments you may proxy the service to the public Internet. Make related configuration changes in the [Backend Configuration](/guide/setup/) section.

::: warning
If you expose the service publicly, make sure to change default configuration for security!
:::

## Open firewall port

To allow LAN access, open port 22345. Run PowerShell as administrator and execute:

``` powershell
# Allow port 22345
netsh advfirewall firewall add rule name="UZonMail" dir=in action=allow protocol=TCP localport=22345

# Show rule
netsh advfirewall firewall show rule name="UZonMail"

# Delete rule
netsh advfirewall firewall delete rule name="UZonMail"
```
