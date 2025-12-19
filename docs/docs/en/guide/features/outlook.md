---
title: Outlook Sending
icon: at
order: 5
description: Guide for configuring Outlook sending via Microsoft Graph in UzonMail.
permalink: /en/guide/features/outlook
---

As of September 2025 Outlook's legacy authentication is deprecated, so to send via Outlook you must use Microsoft's Graph API.

UzonMail provides two sending methods:

1. If you already have the email, ClientId and RefreshToken, you can add the sender directly.
2. If you have the email and password, you can register your own ClientId and use it for sending.

## RefreshToken method

Fill `ClientId` in the SMTP username field and put the `refreshToken` in the SMTP password field. Other parameters can remain as default.

![outlook refresh token](https://oss.uzoncloud.com:2234/public/files/images/image-20250628161219594.png)

After adding, be sure to validate the account. Some ClientIds may not have send permissions, so even with a RefreshToken they may not work.

## Password method

Using Graph with a password requires registering an app to obtain a ClientId. This section walks through obtaining a ClientId.

### Register a Microsoft account

Sign up at: https://signup.live.com/signup

### Register for Microsoft Azure

Two options to get started:

1. Join the Microsoft 365 Developer Program: https://aka.ms/joinM365DeveloperProgram
2. Activate Azure with a credit card: https://aka.ms/signUpForAzure

### Register an application

1. Open Microsoft Entra ID: https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/Overview

   Example view:

   ![entra overview](https://oss.uzoncloud.com:2234/public/files/images/image-20250628153130413.png)

2. Click "App registrations" → "+ New registration" to register an app.

   ![new app](https://oss.uzoncloud.com:2234/public/files/images/image-20250628153552853.png)

   Notes:

   1. For Redirect URI choose "Public client/native (mobile & desktop)".
   2. For desktop, the redirect URL should be `http://localhost:22345/api/v1/outlook-authorization/code`.
   3. For server deployments, use your server domain as the redirect URL.
   4. LAN-only deployments are not supported.
   5. If using port 443, the port can be omitted.

3. Configure API permissions

   After registration, go to App registrations → All applications and open your app's settings.

   ![app settings](https://oss.uzoncloud.com:2234/public/files/images/image-20250628154403007.png)

   Add the following delegated Microsoft Graph permissions:

   - Mail.Send
   - offline_access
   - openid

   ![add permissions](https://oss.uzoncloud.com:2234/public/files/images/image-20250628154724024.png)

4. Grant admin consent (optional, not tested)

   Click "Grant admin consent" on the API permissions page if required.

   ![grant consent](https://oss.uzoncloud.com:2234/public/files/images/image-20250628155230619.png)

5. Get ClientId from the Overview page

   ![client id](https://oss.uzoncloud.com:2234/public/files/images/image-20250628155444957.png)

### Backend configuration

In the backend root (server edition: program root; desktop: ./service) add `appsettings.Production.json` and update:

``` json
{
  // other config ...

  // Base API URL
  // For desktop remove this field or leave empty
  // For server, set to your domain like https://your-domain:port
  "BaseUrl": "https://maildemo.uzoncloud.com:22345",
  // Outlook / Hotmail authorization settings
  "MicrosoftEntraApp": {
    "ClientId": "xxxxx",
    "TenantId": "",
    "ClientSecret": ""
  }
}
```

### Start using

When creating a new sender mailbox, enter the SMTP email address and leave the SMTP username and password empty, then save.

![add outlook sender](https://oss.uzoncloud.com:2234/public/files/images/image-20250628161936418.png)

Saving will trigger the delegated authorization flow; follow the prompts to complete authorization and then sending will be enabled.

You can also right-click an Outlook sender mailbox and choose "Outlook Authorization" to manually trigger the flow.
