---
title: 后端
icon: sliders
order: 1
dir:
  text: 配置说明
  order: 2
  icon: gears
---

本文介绍在进行服务器部署时，需要涉及到的配置修改。桌面端或者局域网内使用，可以忽略。

## 后端配置

配置后端通过在程序根目录创建 `appsettings.Production.json` 来覆盖系统默认配置。

::: warning
后端有一些必须的设置，必须要修改，否则会有安全风险

请阅读 [后端必要配置](#后端必要配置)
:::

系统配置文件为 `appsettings.json`, 但不建议直接修改默认配置，所有默认配置如下：

``` json
{
  // 设置日志输出级别
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },

  // 允许的主机
  "AllowedHosts": "*",

  // 用户登陆后，生成 token 所需参数
  // 若参数被泄露，别人可以伪造 token 用来登陆
  // 建议公网部署后，都必须修改 Secrect 值，且长度不小于 32 位
  "TokenParams": {
    "Secret": "640807f8983090349cca90b9640807f8983090349cca90b9",
    "Issuer": "127.0.0.1",
    "Audience": "UZonMail",
    "Expire": 86400000
  },

  // 系统相关信息
  // 比如系统名称、图标、版权信息、ICP 备案信息
  "System": {
    "Name": "UZonEmail",
    "Icon": "",
    "Copyright": "Copyright © 2024 - 2024 UZon Email",
    "ICPInfo": "渝ICP备20246498号-3"
  },

  // 调试相关配置
  "Debug": {
    "Description": "Debug 相关配置",
    // 若为 true, 则为演示模式, 不支持上传文件、发送邮件等操作
    "IsDemo": false
  },

  // 资源路径
  "Resource": {
    "Path": "resource"
  },

  // 后端 api 的基础 URL
  // 若是部署到公网服务器上或者需要局域网内访问，必须修改为对应的域名或者IP地址
  "BaseUrl": "http://localhost:22345",

  // http 设置
  // 目前未启用
  "Http": {
    "Port": 22345,
    "StaticName": "public",
    "BaseRoute": "/api/v1",
    "TokenSecret": "helloworld01",
    "ListenAnyIP": true
  },

  // websocket 设置
  "Websocket": {
    "Port": 22345
  },

  // 数据库设置
  // 将 Enable 设置为 true, 启用对应的数据库
  // 程序优化使用 mysql
  "Database": {
    // 免安装的数据库，系统默认使用这个
    "SqLite": {
      "Enable": true,
      "DataSource": "data/db/uzon-mail.db"
    },
    // 对于高并发场景，建议使用 mysql
    "MySql": {
      "Enable": false,
      "Version": "8.4.0.0",
      "Host": "",
      "Port": 3306,
      "Database": "uzon-mail",
      "User": "uzon-mail",
      "Password": "uzon-mail",
      "Description": "程序会优先使用 mysql"
    },
    // 缓存数据库
    // 默认使用内存缓存
    "Redis": {
      "Enable": false,
      "Host": "uzon-redis",
      "Port": 6379,
      "Password": "",
      "Database": 0
    }
  },

  // 日志保存位置
  "Logger": {
    "HttpLogPath": "logs/uzon-mail.http.log",
    "Log4netPath": "logs/uzon-mail.stdout.log"
  },

  // 初始用户设置
  "User": {
    // 每个用户在服务器的文件缓存位置
    "CachePath": "users/{0}",
    // 管理员用户名和密码, 只在第一次启动时初始化
    "AdminUser": {
      "UserId": "admin",
      "Password": "admin1234",
      "Avatar": ""
    },
    // 新建用户时的默认密码
    "DefaultPassword": "uzonmail123"
  },

  // 当前系统的地址，会同时加入到跨域中
  "Urls": "http://localhost:22345",

  // 跨域设置
  // 前后端分离、服务器部署时，都需要设置跨域
  "Cors": [ "http://localhost:9000", "https://desktop.uzonmail.com" ],

  // 文件存储设置
  "FileStorage": {
    "DefaultRootDir": "data/object-files"
  },

  // 退订设置
  "Unsubscribe": {
    // 设置退订的头
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

  // 定时任务设置
  "Quartz": {
    "document": "https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html",
    "quartz.scheduler.instanceName": "Quartz ASP.NET Core Sample Scheduler",
    "quartz.threadPool.maxConcurrency": 3,
    "quartz.jobStore.type": "Quartz.Impl.AdoJobStore.JobStoreTX, Quartz",
    "quartz.serializer.type": "json",
    "quartz.jobStore.driverDelegateType": "Quartz.Impl.AdoJobStore.StdAdoDelegate, Quartz",
    "quartz.jobStore.tablePrefix": "QRTZ_",
    "quartz.jobStore.dataSource": "sqlLite",
    "quartz.dataSource.sqlLite.connectionString": "Data Source=data/db/quartz-sqlite.sqlite3",
    "quartz.dataSource.sqlLite.provider": "SQLite-Microsoft",
    "quartz.jobStore.performSchemaValidation": false
  }
}

```

## 后端必要配置

### 修改 token 参数

在 `appsettings.Production.json` 中添加 `TokenParams` 修改 Token 参数，若该参数被泄露，其他人则可以不经过系统授权自行创建 Token 用于访问服务。

服务器部署时，此项必须要修改。

``` json
{
  "TokenParams": {
    "Secret": "640807f8983090349cca90b9640807f8983090349cca90b9",
    "Issuer": "127.0.0.1",
    "Audience": "UZonMail",
    "Expire": 86400000
  },
}
```

### 配置管理员

在 `appsettings.Production.json` 中添加 `User` 对管理员配置进行修改，此修改必须在初始化之前执行，否则无效。

::: tip
若是没有进行该项配置，就启动了后端，可以在启动后修改管理员密码
:::

``` json
{
  "User": {
    // 每个用户在服务器的文件缓存位置
    "CachePath": "users/{0}",
    // 管理员用户名和密码, 只在第一次启动时初始化
    "AdminUser": {
      "UserId": "admin",
      "Password": "admin1234",
      "Avatar": ""
    },
    // 新建用户时的默认密码
    "DefaultPassword": "uzonmail123"
  },
}
```

### 配置 BaseUrl

本软件采用前后端分离框架，前端的 baseUrl 默认为 `http://localhost:22345`, 若是进行反向代理或者共享给局域网内其他用户使用，则需要将其配置为服务器对应的域名或者IP。

在 `appsettings.Production.json` 中添加 `BaseUrl` 对管理员配置进行修改

``` json
{
  "BaseUrl": "http:/localhost:22345"
}
```

## 前端配置

若要修改前端的配置, 可以查看 `wwwwroot/app.config.json` 文件，所有配置如下：

``` json
{
  // baseUrl 要修改成自己的域名或者 IP
  // 当后端配置了 BaseUrl 时，该字段无效
  "baseUrl": "http:/localhost:22345",
  "api": "/api/v1",
  "signalRHub": "/hubs/uzonMailHub",
  "logger": {
    "level": "info"
  }
}
```