---
title: Release History
editLink: false
description: This page contains all historical releases of UzonMail. UzonMail is an open-source bulk email application that supports bulk sending, email marketing, contact scraping, variable replacement, and is compatible with all mailbox types (including Outlook OAuth2). It supports Windows, Linux, macOS and server deployment. High-performance multi-threading, multi-account support, continuously optimized — widely used in foreign trade, education, finance and other industries. UzonMail aims to be the most user-friendly bulk email software and the preferred open-source solution for email marketing.
permalink: /en/downloads
---

## 0.20.4

> Release Date: 2025-12-19

### Improvements

1. Split MsGraph from SMTP; MsGraph sending authorization no longer depends on domain suffix detection.

### Downloads

[uzonmail-desktop-win-x64-0.20.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.4.0.zip)

[uzonmail-service-win-x64-0.20.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.4.0.zip)

[uzonmail-service-linux-x64-0.20.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.3

> Release Date: 2025-12-11

### Bug Fixes

1. Fixed an issue where multilingual @ configuration errors caused download functionality in mailbox management to fail.

### Downloads

[uzonmail-desktop-win-x64-0.20.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.3.0.zip)

[uzonmail-service-win-x64-0.20.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.3.0.zip)

[uzonmail-service-linux-x64-0.20.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.2

> Release Date: 2025-12-10

### Improvements

1. Optimized status label styling to reduce visual prominence.
2. Improved backend memory management to release unused memory timely and increase runtime stability.

### Bug Fixes

1. Fixed a bug where pausing caused incorrect mail status and prevented resuming sending.
2. Fixed a bug where the frontend did not respond properly after an outbox error.

### Downloads

[uzonmail-desktop-win-x64-0.20.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.2.0.zip)

[uzonmail-service-win-x64-0.20.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.2.0.zip)

[uzonmail-service-linux-x64-0.20.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.1

> Release Date: 2025-12-10

### Bug Fixes

1. Fixed a bug that prevented creating groups due to database changes.

### Downloads

[uzonmail-desktop-win-x64-0.20.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.1.0.zip)

[uzonmail-service-win-x64-0.20.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.1.0.zip)

[uzonmail-service-linux-x64-0.20.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20

> Release Date: 2025-12-06

### New Features

1. Added QQ group member collection (Professional edition and above).

### Downloads

[uzonmail-desktop-win-x64-0.20.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.0.0.zip)

[uzonmail-service-win-x64-0.20.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.0.0.zip)

[uzonmail-service-linux-x64-0.20.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.19.1

> Release Date: 2025-12-06

### Bug Fixes

1. Fixed a concurrency issue where updating caches caused the outbox to exit under heavy concurrency.

### Downloads

[uzonmail-desktop-win-x64-0.19.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.19.1.0.zip)

[uzonmail-service-win-x64-0.19.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.19.1.0.zip)

[uzonmail-service-linux-x64-0.19.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.19.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.19.0

> Release Date: 2025-12-05

::: warning
Because the password storage method was changed, direct upgrades are not supported. You may need to delete the database and restart. For the desktop client, delete the old directory.
:::

### Updates

1. Added AI template generation and optimization features [Docs - AI Assistant](/guide/features/ai.html).
2. Added AI-generated multiple subject lines based on content.
3. Improved backend cache framework to increase cache efficiency.

### Bug Fixes

1. Fixed an issue where professional edition authorization might disappear after database deletion.
2. Fixed an issue where variables in Excel data were not replaced.

### Other

1. Tested importing 70k inbox records; could not reproduce the reported stuttering.

### Downloads

[uzonmail-desktop-win-x64-0.19.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.19.0.0.zip)

[uzonmail-service-win-x64-0.19.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.19.0.0.zip)

[uzonmail-service-linux-x64-0.19.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.19.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.18.2

> Release Date: 2025-11-08

::: warning
Due to many multilingual changes, upgrades are not recommended unless necessary.
:::

### Updates

1. Home page, Outbox management and New Outbox pages support multiple languages.
2. Added bulk deletion of inbox items that have already received mail.

### Downloads

[uzonmail-desktop-win-x64-0.18.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.18.2.0.zip)

[uzonmail-service-win-x64-0.18.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.18.2.0.zip)

[uzonmail-service-linux-x64-0.18.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.18.1

> Release Date: 2025-10-30

### Bug Fixes

1. Fixed a bug where "Copy Outbox" in send history was unresponsive.
2. Fixed a bug where templates became invalid after selection.

### Downloads

[uzonmail-desktop-win-x64-0.18.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.18.1.0.zip)

[uzonmail-service-win-x64-0.18.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.18.1.0.zip)

[uzonmail-service-linux-x64-0.18.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.18.0

> Release Date: 2025-10-25

### Improvements

1. Enterprise edition added IP warming feature.

![](https://oss.uzoncloud.com:2234/public/files/images/image-20251025143555005.png)

2. Community edition adds the following default variables:

| outboxName | Recipient name |

3. Improved randomness uniformity for sending intervals.
4. Added bulk deletion of send history.

### Downloads

[uzonmail-desktop-win-x64-0.18.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.18.0.0.zip)

[uzonmail-service-win-x64-0.18.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.18.0.0.zip)

[uzonmail-service-linux-x64-0.18.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.17.1

> Release Date: 2025-09-18

### Bug Fixes

1. Fixed an issue where some devices couldn't select files.
2. Fixed a database concurrency issue during bulk validation.
3. Fixed incorrect retrieval of recipient variable names.
4. Fixed a bug where preview wasn't available when only a recipient group was selected.

### Downloads

[uzonmail-desktop-win-x64-0.17.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.17.1.0.zip)

[uzonmail-service-win-x64-0.17.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.17.1.0.zip)

[uzonmail-service-linux-x64-0.17.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.17.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.17.0

> Release Date: 2025-09-10

### Improvements

1. Optimized the sending core to increase sending rate.
2. Added per-IP per-domain hourly sending limits to prevent IP blocking during high concurrency.
3. Dynamic variables now include Outbox, Inbox, Inboxes and other process data.

### Downloads

[uzonmail-desktop-win-x64-0.17.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.17.0.0.zip)

[uzonmail-service-win-x64-0.17.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.17.0.0.zip)

[uzonmail-service-linux-x64-0.17.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.17.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.4

> Release Date: 2025-09-04

### Bug Fixes

1. Fixed an issue where enterprise settings retrieval caused the application to crash.
2. Fixed an issue where scheduled task optimizations were not merged into updates.

### Downloads

[uzonmail-desktop-win-x64-0.16.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.4.0.zip)

[uzonmail-service-win-x64-0.16.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.4.0.zip)

[uzonmail-service-linux-x64-0.16.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.3

> Release Date: 2025-09-01

### New Features

1. Support for sending via Outlook using a proxy (effective when SMTP address is not smtp.outlook.com).

### Bug Fixes

1. Fixed scheduled tasks still executing after cancellation.
2. Fixed scheduled tasks marked as completed after restart.

### Downloads

[uzonmail-desktop-win-x64-0.16.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.3.0.zip)

[uzonmail-service-win-x64-0.16.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.3.0.zip)

[uzonmail-service-linux-x64-0.16.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.2

> Release Date: 2025-08-25

### Bug Fixes

1. Fixed timezone issues in scheduled tasks that caused actual send times to be inaccurate.
2. Fixed JS variable parsing issues.

### Downloads

[uzonmail-desktop-win-x64-0.16.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.2.0.zip)

[uzonmail-service-win-x64-0.16.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.2.0.zip)

[uzonmail-service-linux-x64-0.16.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.1

> Release Date: 2025-08-16

### New Features

1. Enhanced template and editor: drag-and-drop image support, tables, and advanced image editing.

### Bug Fixes

1. Fixed a bug preventing saving when copying raw HTML into a template.
2. Fixed inconsistent server vs UI time display.

### Downloads

[uzonmail-desktop-win-x64-0.16.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.1.0.zip)

[uzonmail-service-win-x64-0.16.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.1.0.zip)

[uzonmail-service-linux-x64-0.16.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.0

> Release Date: 2025-08-01

### New Features

1. Added PostgreSQL support.
2. Improved variable parsing performance.

### Bug Fixes

1. Fixed a crash caused by variable parsing errors after line breaks.

### Downloads

[uzonmail-desktop-win-x64-0.16.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.0.0.zip)

[uzonmail-service-win-x64-0.16.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.0.0.zip)

[uzonmail-service-linux-x64-0.16.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.3

> Release Date: 2025-07-04

### New Features

1. Desktop client now restricts to a single instance.
2. Added "Unsent" filter in send details.

### Bug Fixes

1. Fixed background process not closing after desktop client exit.
2. Fixed incorrect category filter data in send details.

### Downloads

[uzonmail-desktop-win-x64-0.15.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.3.0.zip)

[uzonmail-service-win-x64-0.15.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.3.0.zip)

[uzonmail-service-linux-x64-0.15.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.2

> Release Date: 2025-07-03

### Bug Fixes

1. Fixed Outlook verification failure after entering username/password.
2. Fixed Hotmail failing to match Graph sending.

### Downloads

[uzonmail-desktop-win-x64-0.15.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.2.0.zip)

[uzonmail-service-win-x64-0.15.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.2.0.zip)

[uzonmail-service-linux-x64-0.15.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.1

> Release Date: 2025-06-28

### New Features

1. Support configuring Outlook ClientId for unified authorization; add `MicrosoftEntraApp` to backend `appsettings.Production.json` as described in the original Chinese docs.
2. Improved bulk upload logic for outbox and inbox to continue on invalid email formats.

### Bug Fixes

1. Fixed missing mail tracking data saves.

### Downloads

[uzonmail-desktop-win-x64-0.15.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.1.0.zip)

[uzonmail-service-win-x64-0.15.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.1.0.zip)

[uzonmail-service-linux-x64-0.15.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.0

> Release Date: 2025-06-21

### New Features

1. Support Outlook sending via refresh_token. Put `client_id` in the username field and the token in the password field.
2. Enterprise edition adds API management features.
3. Moved frontend configuration to backend. Add `BaseUrl` in `appsettings.Production.json` to fix `baseUrl` in `app.config.json` if needed.

### Bug Fixes

1. Fixed an issue where frontend couldn't validate `.well-known` when applying for SSL certificates.

### Downloads
