---
title: 企业版
icon: shield-heart
order: 10
description: 宇正群邮企业版邮件群发软件配置教程，详细介绍企业版专属功能与配置方法，支持开源邮件群发、邮件营销软件，助力企业高效邮件群发，体验最好用的邮件群发软件。
permalink: /guide/setup/pro
---

本文介绍企业版本相关的配置

## 专属配置

### 取消订阅

在 `appsettings.Production.json` 中添加 `Unsubscribe` 字段可以针对不同的收件域名，配置不同的退订头。默认配置如下：

``` json
{
  "Unsubscribe": {
    "Headers": [
      {
        "Domain": "gmail.com",
        "Header": "RFC8058",
        "Description": "这个是默认的退订头"
      },
      {
        "Domain": "aliyun.com",
        "Header": "AliDM",
        "Description": "阿里云的退订头"
      }
    ]
  },
}
```

- Domain 表示收件箱域名
- Header 表示使用的头协议，目前程序实现了两种
  - RFC8058
  - AliDM