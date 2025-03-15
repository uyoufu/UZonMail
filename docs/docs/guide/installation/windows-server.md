---
title: Windows Server
icon: server
order: 2
---

本节将介绍如何在 Windows Server 上安装宇正群邮。服务器的安装方式，主要分为 3 个部分：

## 环境安装

安装 [runtime-aspnetcore-9.0.2-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.2-windows-x64-installer)

## 软件下载

从 [历史版本](/versions) 中下载 `uzonmail-service-win-x64-version.zip` 解压后，单击 `UZonMailService.exe` 启动。

若截图如下所示，则说明安装成功：

![image-20241017221814354](https://obs.uamazing.cn:52443/public/files/images/image-20241017221814354.png)

此时，可以在浏览器中输入[http://localhost:22345/](http://localhost:22345/) 进行测试

测试成功后，关闭上述终端，进行服务注册

## 服务注册
为了实现后台启动宇正群邮，可以将启动程序 `UZonMailService.exe` 注册为 Windows 服务。具体操作如下：

使用管理员打开 cmd 或 powershell（建议使用 powershell），进入到解压后的目录中，执行下列命令:

``` powershell
# 注册服务
uzon-mail install
# 启动服务
uzon-mail start
```

其它命令：

- uzon-mail stop 停止服务
- uzon-mail uninstall 卸载服务
- uzon-mail status 查看服务状态
- uzon-mail restart 重启服务

## 修改配置

到这一步，服务就创建完成了，由于是服务器部署，您可能会将其代理到公网，因此还有一些必要的配置进行修改，请继续阅读 [后端配置](/guide/setup/) 章节。

::: warning
若代理到公网，请务必修改默认配置，否则服务将会不安全！
:::

## 放行端口

若是需要局域网内访问，需要放行对应的端口，使用管理员身份打开 PowerShell，然后执行下列命令：

``` powershell
# 放行端口 22345
netsh advfirewall firewall add rule name="UZonMail" dir=in action=allow protocol=TCP localport=22345

# 查看防火墙规则
netsh advfirewall firewall show rule name="UZonMail"

# 若要删除，执行以下命令
netsh advfirewall firewall delete rule name="UZonMail"
```