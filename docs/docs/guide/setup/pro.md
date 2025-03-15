---
title: 企业版
icon: circle-up
order: 10
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