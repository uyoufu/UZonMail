---
title: Enterprise Edition
icon: circle-up
order: 10
description: Enterprise edition configuration notes for UzonMail, including unsubscribe header customization per domain.
permalink: /en/guide/setup/pro
---

This document describes enterprise edition specific configuration.

## Enterprise settings

### Unsubscribe headers

Add an `Unsubscribe` section in `appsettings.Production.json` to configure different unsubscribe headers for recipient domains. Default example:

``` json
{
  "Unsubscribe": {
    "Headers": [
      { "Domain": "gmail.com", "Header": "RFC8058", "Description": "default unsubscribe header" },
      { "Domain": "aliyun.com", "Header": "AliDM", "Description": "Aliyun unsubscribe header" }
    ]
  }
}
```

- `Domain`: recipient domain
- `Header`: header protocol. Currently supported:
  - `RFC8058`
  - `AliDM`
