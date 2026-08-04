---
title: Release History
editLink: false
description: This page contains all historical releases of UzonMail. UzonMail is an open-source bulk email application that supports bulk sending, email marketing, contact scraping, variable replacement, and is compatible with all mailbox types (including Outlook OAuth2). It supports Windows, Linux, macOS and server deployment. High-performance multi-threading, multi-account support, continuously optimized — widely used in foreign trade, education, finance and other industries. UzonMail aims to be the most user-friendly bulk email software and the preferred open-source solution for email marketing.
permalink: /en/downloads
---

## 0.23.3

> Release Date: 2026-08-04

### New Features

1. Enhanced version update notification: Interactive notification with View History, Ignore, and Update actions
2. New Version History dialog: Displays official release notes with safe download links

### Improvements

1. Email address validation upgraded: Adopts strict MIME compliance mode for sender, recipients, CC, BCC, and reply-to address validation

### Downloads

[uzonmail-desktop-win-x64-0.23.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.3.0.zip)

[uzonmail-service-win-x64-0.23.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.3.0.zip)

[uzonmail-service-linux-x64-0.23.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.2

> Release Date: 2026-07-31

### Improvements

1. Add HTML-to-plaintext conversion for emails, generating a plain-text alternative alongside the HTML body to prevent MPART_ALT_DIFF mismatches in email clients

### Bug Fixes

1. Optimize email template caching with a per-template lease-based approach, improving reliability and timely resource release

### Downloads

[uzonmail-desktop-win-x64-0.23.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.2.0.zip)

[uzonmail-service-win-x64-0.23.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.2.0.zip)

[uzonmail-service-linux-x64-0.23.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.1

> Release Date: 2026-07-30

### New Features

1. Runtime theme switching: support switching between default theme and Cherry Blossom Romance theme
2. Desktop application localization: complete localization infrastructure for Windows desktop app with bidirectional Chinese/English sync
3. Organization-level inbox validity management: batch mark inboxes as valid or invalid across organization users
4. Batch move email boxes: support batch moving inboxes and outboxes to target groups
5. Batch soft-delete selected inboxes: multi-select soft delete with validation status display in table
6. Desktop application auto-updater: CLI tooling and WebView2 integrated automatic update capability
7. Hard bounce detection and inbox verification system: SMTP status codes 550/551/553 automatically identified as hard bounces
8. API error localization: key-based API error localization system
9. Allow duplicate sending: support sending multiple emails to same recipient in single task with duplicate detection
10. DraggableTree component: hierarchical data with drag-and-drop, context menus, and controlled tree state
11. File storage redesign: categories, content-addressable storage, and reference counting
12. Desktop app refactoring: migrated to new location with CommunityToolkit.Mvvm framework

### Improvements

1. Theme renamed from Fairy Pink to Cherry Blossom Romance
2. Inbox manager template formatting cleanup, invalid import button added red visual indicator
3. Plugin loading supports shared assembly directory
4. Unified build scripts, exclude hard bounce items from sending queries
5. i18n extended to all UI pages and components
6. Context menu visibility conditions based on selection count
7. WYSIWYG editor support for lowCode forms
8. Status labels resolved through i18n system

### Bug Fixes

1. Fix WSL path conversion with proper Bash quoting
2. Fix email verification order and add SPF/unknown-state tests
3. Fix property name disableAutogrow to disableAutoGrow
4. Fix SendCore bugs and improve async lifecycle handling

### Downloads

[uzonmail-desktop-win-x64-0.23.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.1.0.zip)

[uzonmail-service-win-x64-0.23.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.1.0.zip)

[uzonmail-service-linux-x64-0.23.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.0

> Release Date: 2026-07-28

### New Features

1. Desktop application auto-updater: New UzonMailUpdater CLI tool with package/update commands for version checking, ZIP download, file hash validation, and rollback support. Desktop app integrates WebView2 update entry, superadmins can check and apply updates from dashboard.
2. Batch move email boxes to target group: Support batch selecting and moving inboxes/outboxes to specified groups from management pages.
3. Batch soft-delete for selected inboxes: Support batch soft-deleting selected inboxes with validation status displayed in table.
4. Hard bounce detection and inbox verification: Auto-detect SMTP 550/551/553 as hard bounces during sending, mark target inbox as invalid and move to "validation failed" group. Support verifying individual inbox or entire group.
5. Allow duplicate sending option: New sending task setting to allow sending to same recipient multiple times, with warning dialog showing duplicates when detected.
6. API localization infrastructure: New key-based API error localization system with localized error messages for Chinese and English.

### Improvements

1. Full UI internationalization: Added and improved translations for crawlerTask, sendDetail, sendHistory, sendingProgress, statisticsReport, profile and other pages.
2. SendCore scheduling optimization: Non-blocking cooldown with lazy daily quota reset to avoid blocking worker slots. Cursor-based pagination for outbox loading with adaptive catalog replenishment.
3. DraggableTree component: New reusable component supporting hierarchical data with drag-and-drop and context menus.
4. Context menu visibility conditions: Control menu item visibility based on selection count (any/onlyMulti/onlySingle).
5. Strengthened TypeScript typing: useQTable and ContextMenu with generic type parameters and multi-select v-model for better type safety and batch operations.
6. File storage redesign: Introduced file categories with tree structure, content-addressable storage, and reference counting for safe cleanup.
7. Plugin loading optimization: Support loading shared dependencies from parent-level Assembly directory, sort plugins by dependency order.

### Bug Fixes

1. Fix WSL path conversion with proper Bash quoting for Windows paths with backslashes.
2. Rename disableAutogrow to disableAutoGrow for consistency across LowCodeForm property naming.

### Downloads

[uzonmail-desktop-win-x64-0.23.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.0.0.zip)

[uzonmail-service-win-x64-0.23.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.0.0.zip)

[uzonmail-service-linux-x64-0.23.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.22.1

> Release Date: 2026-06-03

### Improvements

1. Simplified Docker installation and supported overriding backend configuration through environment variables

### Bug Fixes

1. Fixed an issue where the daily sending limit reset incorrectly

### Downloads

[uzonmail-desktop-win-x64-0.22.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.22.1.0.zip)

[uzonmail-service-win-x64-0.22.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.22.1.0.zip)

[uzonmail-service-linux-x64-0.22.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.22.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.22.0

> Release Date: 2026-03-29

### Function Optimization

1. Optimize login screen animation

### Bug Fixes

1. Fix bug where email still displays after deleting email group

### Downloads

[uzonmail-desktop-win-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.22.0.0.zip)

[uzonmail-service-win-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.22.0.0.zip)

[uzonmail-service-linux-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.22.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.22.0

> Release Date: 2026-03-29

### Features Optimization

1. Optimize login interface animations

### Bug Fixes

1. Fix the bug where emails still display after deleting email groups

### Downloads

[uzonmail-desktop-win-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.22.0.0.zip)

[uzonmail-service-win-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.22.0.0.zip)

[uzonmail-service-linux-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.22.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.21.0
> Release Date: 2026-03-22

### Performance Improvements

1. Optimized performance usage
2. Upgraded the framework to .NET 10

### Bug Fixes

1. Fixed AI not working on Windows

### Downloads

[uzonmail-desktop-win-x64-0.21.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.21.0.0.zip)

[uzonmail-service-win-x64-0.21.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.21.0.0.zip)

[uzonmail-service-linux-x64-0.21.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.21.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.6

> Release Date:2026-01-27

### Improvements

1. Update UzonMail icon

### Bug Fixes

1. Fix custom domain Outlook sending failure issue

### Downloads
[uzonmail-desktop-win-x64-0.20.6.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.6.0.zip)

[uzonmail-service-win-x64-0.20.6.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.6.0.zip)

[uzonmail-service-linux-x64-0.20.6.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.6.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.5

> Release Date:2025-12-19

### Improvements

1. Split database upgrade module initialization and upgrade to reduce coupling.
2. Use outbox history to assist SMTP information completion.
3. Correct the status of sending tasks that exit due to outbox issues to "paused".

### Downloads

[uzonmail-desktop-win-x64-0.20.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.5.0.zip)

[uzonmail-service-win-x64-0.20.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.5.0.zip)

[uzonmail-service-linux-x64-0.20.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.5.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

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
