# UZonMail

[![Github release](https://img.shields.io/github/v/tag/uyoufu/UZonMail)](https://github.com/uyoufu/UZonMail/releases)
[![GitHub](https://img.shields.io/github/license/uyoufu/UZonMail)](https://github.com/uyoufu/UZonMail/blob/main/LICENSE)
[![GitHub last commit](https://img.shields.io/github/last-commit/uyoufu/UZonMail)](https://github.com/uyoufu/UZonMail/commits/main)
[![GitHub issues](https://img.shields.io/github/issues/uyoufu/UZonMail)](https://github.com/uyoufu/UZonMail/issues)

[English](/README.md) | [简体中文](/README.zh-CN.md)


## 🥝 Introduction

![image-20251108135045436](https://oss.uzoncloud.com:2234/public/files/images/image-20251108135045436.png)

This software is named "UZonMail", an open-source and free bulk email software that provides features such as bulk emailing, email marketing, email scraping, and arbitrary variables. It supports all types of email accounts, including Outlook's OAuth2. Native enterprise-grade quality, supports multi-platform users, compatible with Windows, Linux, macOS, and other operating systems, supports server deployment. Native multi-threaded concurrency, supports multiple accounts simultaneously, with powerful performance. Years of continuous iteration, updates, and optimizations, widely used in industries such as foreign trade marketing, education and training, finance and accounting.

Common application scenarios include:

1. Marketing email bulk sending
2. Payroll email bulk sending
3. Invoice email bulk sending
4. Integration into systems as a notification center for bulk emailing

[Online Demo](https://maildemo.uzoncloud.com:2234), account/password: admin/admin1234

> To prevent abuse, some features are disabled in the demo version. Each email task can send a maximum of 5 emails

<!--more-->

## ⭐ Featured Functions

![image-20251108135212257](https://oss.uzoncloud.com:2234/public/files/images/image-20251108135212257.png)

1. **Support for multiple senders sending simultaneously**

   You can add any number of different senders to send simultaneously, which not only improves sending efficiency but also breaks through the daily sending limit of a single mailbox. In theory, with enough mailboxes, you can send massive emails without constraints.

2. **Support for custom email content templates**

   Sending content can be entered directly or use predefined modules.

   Templates can be customized as needed, saved to the template library after definition, enabling easy reuse of templates.

   Templates are defined in HTML format, and the program provides a visual interface for editing, friendly to both beginners and experts, allowing infinite possibilities in content.

3. **Support for unlimited variables, each email unique**

   Supports data variables and random variables. You can introduce these variables in templates, and during sending, the program will automatically replace variables with real values, allowing the same template to send different content to different recipients.

4. **Multi-threaded concurrent sending, daily sending up to 100,000+**

   Each sender uses a separate thread for sending, the more senders, the faster the sending speed. With enough mailboxes, sending 100,000 per day is not a problem.

5. **Failure resend**

   When there are multiple mailboxes, if sender A fails, it will switch to sender B to continue sending.

   If there is only one mailbox, failed items can be resent from the sending history.

6. **Self-developed random algorithm, enabling intelligent random selection of senders and templates**

   When there are multiple senders and templates, the program intelligently selects randomly to improve email delivery rates.

7. **Support for daily sending limit per mailbox**

8. **Support for CC and BCC**

   Supports CC multiple emails to specific mailboxes or different CC for different emails.

9. **Support for email read tracking**

   Supports tracking email open status for more precise marketing implementation.

10. **Support for unsubscribe function**

    For marketing emails, includes unsubscribe functionality to better comply with local laws and regulations, improve user experience, protect sender reputation, reduce complaint rates, and enhance marketing effectiveness.

11. **Support for permission management**

    Enterprise organization administrators can manage and view all sub-user data and settings for centralized management.

12. **Native crawler support, assisting foreign trade market development**

13. **Support for Outlook OAuth2 sending**

    Outlook supports sending via RefreshToken or account password.

14. **Native enterprise-grade quality**

    Developed from the start with enterprise-level positioning, using MySQL and Redis databases, supporting multi-user, high concurrency. Compatible with Windows, Linux, macOS, supports server deployment.

## 👍 Main Advantages

Compared to other bulk email software on the market, UZonMail's advantages mainly include:

1. Enterprise-grade quality: Multi-user, high concurrency, low hardware requirements, powerful performance
2. Web architecture, supports private deployment, adapts to multiple platforms
3. Truly supports all email accounts, including Outlook OAuth2 sending
4. Modern UI, UI design ranks first among similar software
5. Emails support unsubscribe, protecting your reputation
6. Unlimited variables, unlimited possibilities, usability beyond imagination

## 💻 Supported Platforms

1. Windows
2. Windows Server
3. Linux
4. Docker
5. macOS

## 📖 More Documentation

For more detailed content: [https://mail.uzoncloud.com/](https://mail.uzoncloud.com/)