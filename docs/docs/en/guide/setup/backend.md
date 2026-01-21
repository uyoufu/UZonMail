---
title: Backend
icon: sliders
order: 1
---

This document explains configuration changes needed for server deployments. Desktop or LAN-only usage can ignore most of these settings.

## Backend configuration

Override default settings by creating `appsettings.Production.json` in the application root.

::: warning
Some backend settings are required for security and must be changed for public deployments. See "Required backend configuration" below.
:::

The default system configuration is in `appsettings.json`. It's not recommended to edit defaults directly. Example defaults:

``` json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft": "Warning", "Microsoft.Hosting.Lifetime": "Information" } },
  "AllowedHosts": "*",
  "TokenParams": {
    "Secret": "640807f8983090349cca90b9640807f8983090349cca90b9",
    "Issuer": "127.0.0.1",
    "Audience": "UZonMail",
    "Expire": 86400000
  },
  "System": {
    "Name": "UZonEmail",
    "Icon": "",
    "Copyright": "Copyright © 2024 - 2024 UZon Email",
    "ICPInfo": "渝ICP备20246498号-3"
  },
  "Debug": { "Description": "Debug configuration", "IsDemo": false },
  "Resource": { "Path": "resource" },
  "BaseUrl": "http://localhost:22345",
  "Http": { "Port": 22345, "StaticName": "public", "BaseRoute": "/api/v1", "TokenSecret": "helloworld01", "ListenAnyIP": true },
  "Websocket": { "Port": 22345 },
  "Database": {
    "SqLite": { "Enable": true, "DataSource": "data/db/uzon-mail.db" },
    "MySql": { "Enable": false, "Version": "8.4.0.0", "Host": "", "Port": 3306, "Database": "uzon-mail", "User": "uzon-mail", "Password": "uzon-mail", "Description": "MySQL is preferred" },
    "Redis": { "Enable": false, "Host": "uzon-redis", "Port": 6379, "Password": "", "Database": 0 }
  },
  "Logger": { "HttpLogPath": "logs/uzon-mail.http.log", "Log4netPath": "logs/uzon-mail.stdout.log" },
  "User": { "CachePath": "users/{0}", "AdminUser": { "UserId": "admin", "Password": "admin1234", "Avatar": "" }, "DefaultPassword": "uzonmail123" },
  "Urls": "http://localhost:22345",
  "Cors": [ "http://localhost:9000", "https://desktop.uzonmail.com" ],
  "FileStorage": { "DefaultRootDir": "data/object-files" },
  "Unsubscribe": { "Headers": [ { "Domain": "gmail.com", "Header": "RFC8058", "Description": "default unsubscribe header" }, { "Domain": "aliyun.com", "Header": "AliDM", "Description": "Aliyun unsubscribe header" } ] },
  "Quartz": { /* scheduler config omitted for brevity */ }
}
```

## Required backend configuration

### Modify token parameters

Add `TokenParams` to `appsettings.Production.json` and change the values. If token parameters are leaked, attackers could forge tokens to access the service. This must be changed for public deployments.

``` json
{ "TokenParams": { "Secret": "your-secret-here", "Issuer": "127.0.0.1", "Audience": "UZonMail", "Expire": 86400000 } }
```

### Configure admin user

Add `User` to `appsettings.Production.json` before first initialization to customize the admin account. If not set before initialization, you can still change the admin password after startup.

``` json
{ "User": { "CachePath": "users/{0}", "AdminUser": { "UserId": "admin", "Password": "admin1234", "Avatar": "" }, "DefaultPassword": "uzonmail123" } }
```

### Configure BaseUrl

Since the application uses a frontend-backend separated architecture, set `BaseUrl` to the server domain or IP when exposing via reverse proxy or LAN sharing.

``` json
{ "BaseUrl": "http://your-server:22345" }
```

## Frontend configuration

To modify frontend settings, edit `wwwroot/app.config.json`.

``` json
{
  "baseUrl": "http:/localhost:22345",
  "api": "/api/v1",
  "signalRHub": "/hubs/uzonMailHub",
  "logger": { "level": "info" }
}
```
