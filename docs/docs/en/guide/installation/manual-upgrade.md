---
title: Manual Upgrade
icon: server
order: 8
description: How to manually upgrade UzonMail and preserve configuration.
permalink: /en/guide/installation/manual-upgrade
---

This section describes how to manually upgrade UzonMail.

## Checking for new versions

Two ways to find new versions:

1. An update check runs when the admin user logs in for the first time; if an update exists a prompt will appear.
2. Visit the Versions page: https://mail.uzoncloud.com/versions.html

## Upgrade steps

1. Stop UzonMail.
2. Backup `appsettings.Production.json` (server: root directory; desktop: `service` directory).
3. Download the latest release from the Versions page and extract/overwrite the installation directory.
4. Restore `appsettings.Production.json`.

> For the desktop edition, simply overwrite after closing the application. If you did not modify configuration, backup is optional.

::: info

Since v0.15.x the `baseUrl` value in `wwwroot/app.config.json` can be set via `BaseUrl` in `appsettings.Production.json`. You may keep `wwwroot/app.config.json` as default. Example:

``` json
{
  ... other config
  "BaseUrl": "https://uzon-mail.uzoncloud.com:443"
}
```

:::
