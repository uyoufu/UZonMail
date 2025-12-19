---
title: Ubuntu
icon: fab fa-ubuntu
order: 4
description: 宇正群邮 Ubuntu 安装教程，详细介绍如何在 Ubuntu 系统上部署宇正群邮邮件群发软件。支持开源邮件群发、邮件营销软件，助力企业和个人高效邮件群发，体验最好用的邮件群发软件。
permalink: /guide/installation/ubuntu
---

::: tip
下文以 Ubuntu 22.04 LTS 举例说明
:::

本教程默认使用 ssh 连接到 ubuntu 进行操作

## 环境安装

Ubuntu 安装 `.NET` 见微软官方文档 [.NET 和 Ubuntu 概述 - .NET | Microsoft Learn](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu#im-using-ubuntu-2204-or-later-and-i-only-need-net)

安装命令如下：

```bash
sudo apt update && \
  sudo apt install -y aspnetcore-runtime-9.0
```

## 软件下载

从 [历史版本](/versions) 中下载 `uzonmail-service-linux-x64-version.zip` 。

操作命令如下：

``` bash
# 安装 unzip (若没有)
sudo apt install unzip

# 进入到 home 目录，然后下载并解压安装包
cd ~ && \
  wget --no-check-certificate https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.10.0.0.zip -O uzonmail.zip && \
  unzip uzonmail.zip -d ./uzonmail && \
  cd ./uzonmail
  
 # 使用安装脚本进行安装
 bash ./install.sh
```

## 注册服务

运行目录中的 `install.sh` 脚本进行服务注册，命令如下：

``` bash
# 安装 uzon-mail
bash ./install.sh
```

## 放行端口

使用如下命令放行端口：

``` bash
sudo ufw allow 22345/tcp
```

到这一步，后端即安装成功了。

可以在浏览器中打开 `http://your-ubuntu-ip:22345` 查看安装效果。

## 修改配置

到这一步，服务就创建完成了，由于是服务器部署，您可能会将其代理到公网，因此还有一些必要的配置进行修改，请继续阅读 [后端配置](/guide/setup/) 章节。

::: warning
若代理到公网，请务必修改默认配置，否则服务将会不安全！
:::
