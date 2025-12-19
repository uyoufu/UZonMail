---
title: Software Introduction
icon: ranking-star
order: 1
description: UzonMail is an open-source, free, enterprise-grade bulk email software supporting marketing, mailbox scraping, unlimited variables, Outlook OAuth2 and more.
---

The software is called "UzonMail" (宇正群邮), an open-source and free bulk email software that provides features like mass emailing, email marketing, mailbox scraping, and unlimited variables. It supports all types of mail accounts, including Outlook OAuth2. Built with enterprise-grade quality, it supports multi-user, multi-platform deployments on Windows, Linux, macOS, and Docker, and is optimized for server environments. Natively multi-threaded and concurrent, it supports multiple accounts simultaneously with strong performance. With years of continuous iteration and optimization, it has been widely used across industries such as foreign trade marketing, education and training, and finance.

![UzonMail login page](https://oss.uzoncloud.com:2234/public/files/images/uzon-mail-login-2.png)

Common use cases include:

1. Marketing email campaigns
2. Payroll email distribution
3. Invoice emailing
4. Integration as a notification center for system-triggered emails

::: tip
Open source: [UzonMail](https://github.com/GalensGan/UZonMail)
Online demo: [https://maildemo.uzoncloud.com:2234/](https://maildemo.uzoncloud.com:2234/), username/password: admin/admin1234
:::

## Key Features

![image-20240614121857801](https://github.com/uyoufu/UZonMail/raw/master/resource/images/uzon-mail-send.png)

1. **Multiple senders can send concurrently**

   You can add any number of different sender accounts to send at the same time, which not only improves sending efficiency but also bypasses the daily sending limits of single accounts. In theory, with enough sender accounts, you can send massive volumes without restriction.

2. **Customizable email content templates**

   Email content can be entered directly or composed from predefined modules.

   Templates are customizable and can be saved to a template library for easy reuse.

   Templates are defined in HTML format, and the application provides a visual editor that is friendly to both beginners and advanced users, giving templates unlimited possibilities.

3. **Unlimited variables — each email can be unique**

   Supports data variables and random variables. These variables can be referenced in templates, and during sending the program automatically replaces them with real values so the same template can produce personalized content for different recipients.

4. **Multi-threaded concurrent sending — 100k+ daily possible**

   Each sender uses its own thread for sending. The more senders you have, the faster the sending speed. With enough sender accounts, sending 100,000 emails per day is feasible.

5. **Retry on failure**

   With multiple sender accounts, if sender A fails to send, sender B will continue sending. If only one sender is available, failed items can be resent from the send history.

6. **Custom randomization algorithms**

   When multiple senders and templates exist, the program intelligently selects them randomly to improve deliverability.

7. **Daily sending limit per mailbox supported**

8. **CC and BCC supported**

   Support for copying multiple recipients to specific addresses or copying different emails to different recipients.

9. **Email read tracking**

   Track when emails are opened to enable more accurate marketing analytics.

10. **Unsubscribe support**

    For marketing emails, include an unsubscribe option to better comply with local regulations, improve user experience, protect sender reputation, reduce complaints, and enhance marketing effectiveness.

11. **Permission management**

    Organization administrators can view and manage all sub-users' data and settings for centralized control.

12. **Built-in scraping capabilities to support market expansion**

13. **Outlook OAuth2 sending supported**

    Outlook supports sending via RefreshToken or account password.

14. **Enterprise-grade by design**

    Designed from the ground up for enterprise use, UzonMail uses MySQL and Redis, supports multi-user and high concurrency, and runs on Windows, Linux, and macOS with server deployment support.

## Main Advantages

Compared with other bulk email software on the market, UzonMail's advantages include:

1. Enterprise-grade quality: multi-user, high concurrency, low hardware requirements, strong performance
2. Web-based architecture with private deployment support across platforms
3. True support for all mail providers including Outlook OAuth2
4. Modern UI design
5. Unsubscribe support to protect your reputation
6. Unlimited variables for flexible personalization

## Supported Platforms

1. Windows
2. Windows Server
3. Linux
4. Docker
5. macOS

Thanks for using UzonMail — may it help your email campaigns soar!
