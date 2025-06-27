---
title: 手动升级
icon: server
order: 8
---

本节将介绍如何进行升级

## 发现新版本

有以下两个方式可以获取新版本信息：

1. 当使用管理员第一次登录时，会触发更新检查，若有更新，则会弹窗
2. 可以打开 [历史版本 | 宇正群邮](https://uzonmail.pages.dev/versions.html) 查看最新版本

## 更新步骤

1. 关闭宇正群邮
2. 备份 `appsettings.Production.json` 文件。服务器版本位于根目录，桌面端位于 `service` 目录
3. 从[历史版本 | 宇正群邮](https://uzonmail.pages.dev/versions.html) 中下载最新版本并解压到原安装目录进行覆盖
4. 还原 `appsettings.Production.json` 文件



::: info

从 0.15.x 版本开始，`wwwroot/app.config.json` 中的 `baseUrl` 可以通过在 `appsettings.Production.json` 文件中添加 `BaseUrl` 进行设置。示例如下：

``` json
{
  ... 其它配置
  "BaseUrl": "https://uzon-mail.223434.xyz:443"
}
```

:::