---
title: Feature List
icon: list-check
order: 2
description: UzonMail feature list — comprehensive introduction to bulk emailing, template management, variables, and other capabilities.
permalink: /en/guide/features/detail
---

## Login Screen

![uzon-mail-login-2](https://oss.uzoncloud.com:2234/public/files/images/uzon-mail-login-2.png)

Default username/password: `admin` / `admin1234`

## Home

![dashboard](https://oss.uzoncloud.com:2234/public/files/images/image-20240614231957130.png)

The home page mainly shows:

- Sender mailbox count histogram
- Recipient mailbox count histogram
- Monthly sending volume line chart

## System Settings

### User Management

![user management](https://oss.uzoncloud.com:2234/public/files/images/image-20240612122713293.png)

The default username is `admin` and the default password is `admin1234`. This account has the "Manage Users" permission.

The User Management module is used to add different users. The desktop version's multi-user feature is limited to the local machine; for multiple simultaneous users, use the server version. Contact the author for server edition access.

**Add user:**

![add user](https://oss.uzoncloud.com:2234/public/files/images/image-20240612123329057.png)

Click the top-left "Add" to create a new user. After creating a user you can perform actions such as reset password or delete the user.

Reset password will be `uzonmail123` and a prompt will appear when resetting.

![reset password](https://oss.uzoncloud.com:2234/public/files/images/image-20240612123429178.png)

**Change password and avatar:**

You can update your avatar and password from the user profile menu at the top-right.

![profile](https://oss.uzoncloud.com:2234/public/files/images/image-20240612125131168.png)

### Basic Settings

Basic settings control global sending intervals, maximum send amounts, etc.

![basic settings](https://oss.uzoncloud.com:2234/public/files/images/image-20240612125859579.png)

- Daily max sends per mailbox

  Controls the total number of sends per mailbox per day to avoid exceeding provider limits which could cause send failures. A value of 0 means unlimited.

- Min/Max send interval per mailbox

  Unit: seconds

  To avoid being flagged as spam for sending too frequently, there should be a time interval between sends. The actual interval is computed as:

  actual_interval = min + random(0,1) * (max - min)

  If max <= min, the interval is considered unlimited.

- Maximum merge count

  When sending the same content to multiple recipients, you can merge recipients into a single email. This parameter controls the maximum number to merge. Each provider allows different maxima; the count includes CC and BCC recipients. A value of 0 disables merging.

### Proxy Management

![proxy management](https://oss.uzoncloud.com:2234/public/files/images/image-20240612131312091.png)

Proxy management is mainly for using foreign mail providers and allows assigning proxies to specific sender types or accounts. This feature is typically used on server deployments; for local use you can enable a global proxy.

**Add proxy:**

![add proxy](https://oss.uzoncloud.com:2234/public/files/images/image-20240612130111084.png)

Proxy fields:

- Name (required)

  Used to select the proxy in the sender mailbox UI.

- Proxy address

  Address, username and password. Format: `protocol:\username:password@host`. Examples:

  1. Full: `http:\admin:admin1234@127.0.0.1:7890`
  2. No password: `http:\127.0.0.1:7890`
  3. Other protocol: `socket5:\127.0.0.1:7890`

  Supported protocols: `http`, `https`, `socks4`, `socks4a`, `socket5`.

- Matching rule

  If a sender does not specify a proxy, the system will match one from the proxy list using this rule (a regular expression). `.*` matches all.

- Priority

  Matching rule priority.

- Shared

  If shared, all users in the system can use this proxy.

> Proxy security note
>
> Proxies are stored in plaintext on the server so administrators can view them. Be cautious when adding personal proxies as they may be exposed.

## Mailbox Management

### Sender Mailboxes

The Sender Mailbox module manages sender account information. Important notes:

**Group management:**

When adding a sender mailbox you must first create a sender group.

![groups](https://oss.uzoncloud.com:2234/public/files/images/image-20240612131552355.png)

Right-click on "Sender Mailboxes" to add a group.

![add group](https://oss.uzoncloud.com:2234/public/files/images/image-20240612131711338.png)

The "Order" field controls group sorting.

After creating a group you can manage it via right-click on the group name.

![manage group](https://oss.uzoncloud.com:2234/public/files/images/image-20240612131916319.png)

**Add sender mailbox:**

![add sender](https://oss.uzoncloud.com:2234/public/files/images/image-20240612132056895.png)

UzonMail uses SMTP for sending, so you must enable SMTP for your mail account and obtain the SMTP credentials. Important parameters:

- Sender name

  If provided, recipients will see the name instead of the email address.

- SMTP password

  The SMTP password is not your email login password but the SMTP-specific password (e.g., provider-generated SMTP/password or app password). For example, the method to obtain SMTP password for 163.com can be found online.

- SMTP host

  The SMTP server address varies by provider.

- SMTP port

  The port depends on whether SSL is enabled. Default 25; with SSL often 465.

- Enable SSL

  Enables SSL/TLS for sending to improve security.

- Proxy

  Optionally assign a proxy defined in Proxy Management.

> Password security note
>
> The server does not store SMTP passwords in plaintext; it stores an encrypted ciphertext generated using a key produced by the frontend. The frontend provides the key to the backend when needed for decryption. Thus, even if the database is leaked, SMTP passwords are not directly exposed.

**Import from Excel:**

Use the Import feature to bulk import sender mailboxes from Excel. Download the import template via the "Template" button. The Excel must include headers: `smtp邮箱`, `smtp密码`, `smtp地址`, `smtp端口`.

![import template](https://oss.uzoncloud.com:2234/public/files/images/image-20240614123302248.png)

### Recipient Mailboxes

![inboxes](https://oss.uzoncloud.com:2234/public/files/images/image-20240614124814856.png)

This module manages recipient grouping and follows the same usage and caveats as senders. Recipient entries need only a name and email address; name is optional.

## Template Management

![templates](https://oss.uzoncloud.com:2234/public/files/images/image-20240614125056768.png)

The Body Templates section manages all templates for a user and uses HTML format.

### Add Template

Two ways to add templates:

- Import HTML

  Prepare an HTML template externally and import it. For custom HTML templates, CSS must be inline. Use a tool like http://automattic.github.io/juice/ to inline CSS.

- Edit directly

  Click "Add" to create a new template.

  ![new template](https://oss.uzoncloud.com:2234/public/files/images/image-20240614125417120.png)

### Edit Template

Click a template name or right-click and choose "Compile" to edit the template.

![edit template](https://oss.uzoncloud.com:2234/public/files/images/image-20240614125609679.png)

### Template Variables

Within templates you can mark variables using double curly braces (`{{variable}}`). During sending the program will look up these variables in the data and replace them with actual values if found.

Variable format: {{variableName}}.

> Templates can also vary by sender. To override the default template for a specific data row, include a `templateId` column in the data. See the Sending section for details.

## Sending Management

### New Send

![new send](https://oss.uzoncloud.com:2234/public/files/images/image-20240614125845479.png)

Create sending tasks here. Depending on parameter combinations, you can perform:

1. One-to-one sends (one sender, one recipient)
2. One-to-many sends (one sender, multiple recipients)
3. Many-to-many sends (multiple senders, multiple recipients)
4. Subject/body variation per recipient

#### Subject

Subject is required. It serves two purposes: the email subject and the name of the send history group.

Separate multiple subjects with semicolons (`;`) or newlines.

![subjects](https://oss.uzoncloud.com:2234/public/files/images/image-20240614130827129.png)

If multiple subjects are provided the system randomly selects one for each send (unless a subject is specified in the data).

Subjects support variables, e.g., {{日期}}-Payroll Details — `日期` will be replaced by data from Excel when sending.

#### Template

Templates serve as drafts for the email body and allow quick sending without typing the body each time.

![choose template](https://oss.uzoncloud.com:2234/public/files/images/image-20240614131034812.png)

You can select multiple templates; if multiple are selected the system randomly chooses one (unless `templateId` is specified in the data).

#### Body

![body editor](https://oss.uzoncloud.com:2234/public/files/images/image-20240614131326894.png)

![body preview](https://oss.uzoncloud.com:2234/public/files/images/image-20240614225842011.png)

Users can manually input the body. If a body is specified it overrides template usage. The body supports variables similar to templates.

#### Sender

![sender selection](https://oss.uzoncloud.com:2234/public/files/images/image-20240614131606189.png)

Click the + next to Sender to choose senders. Multiple senders are allowed; when multiple are present the system randomly selects one to send each email. Each email will be sent by only one sender and not duplicated.

#### Recipients

![recipient selection](https://oss.uzoncloud.com:2234/public/files/images/image-20240614131851665.png)

Click the + next to Recipients to choose recipient lists. A single send task can include multiple recipients; each recipient receives one email.

#### CC

If CC recipients are selected, each email will be CC'd to those addresses.

#### Attachments

![attachments](https://oss.uzoncloud.com:2234/public/files/images/image-20240614214517919.png)

Add attachments here if needed. Multiple attachments are allowed, but note that every email will include the same attachments.

#### Data

Data-driven sending is the core of the software. By importing data you can send personalized content to different recipients in one batch.

Hover over the Data field to reveal a template download icon on the right; click it to download the template.

![data template](https://oss.uzoncloud.com:2234/public/files/images/image-20240614215520531.png)

Data format example:

![data example](https://oss.uzoncloud.com:2234/public/files/images/image-20240614220243117.png)

**Data effect:**

Template content:

![template example](https://oss.uzoncloud.com:2234/public/files/images/image-20240614222544153.png)

Body preview after applying data:

![preview with data](https://oss.uzoncloud.com:2234/public/files/images/image-20240614222617857.png)

**Data purpose:**

1. Provide variables for templates
2. Enable accurate bulk personalized sending

**System-reserved variables in data:**

| Name         | Required | Description |
| ------------ | -------- | ----------- |
| inbox        | Yes      | Specify recipient email. This field is required; the program uses it to match sends. If empty, the row is invalid. |
| inboxName    | No       | Recipient name. |
| subject      | No       | Specify subject. If set, it overrides the subject entered in the UI. |
| outbox       | No       | Specify sender mailbox. If not set, the sender selected in the UI is used. The outbox must be an address added in Mailbox Management; other addresses are invalid. |
| outboxName   | No       | Sender name. If not set, the name from sender management is used. |
| cc           | No       | CC recipients, comma-separated. |
| templateId   | No       | Template ID (numeric) from Template Management. If not set, a template is randomly selected from those chosen in the UI. |
| templateName | No       | Template name. Has lower priority than templateId. If both specified, templateId takes precedence. If not set, a template is randomly selected from the UI. |
| body         | No       | Specify the email body; has higher priority than templateId and templateName. |

**Data priority rules:**

- Subject: Excel `subject` > UI subject

- Body: Excel `body` > Excel `templateId` > Excel `templateName` > UI body > UI template

- Sender: Excel `outbox` > UI sender

- Recipient: Excel `inbox` > UI recipient

- CC: Excel `cc` > UI CC

- Attachments: cannot currently be specified via data

### Send History

Send History shows all past sent emails. Each send task is one history entry.
