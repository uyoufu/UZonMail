---
title: Outlook发件
icon: at
order: 5
---

Outlook 于 2025 年 09 月全面淘汰，因此若要使用 Outlook 邮箱发件，需要使用微软提供的 Graph 方式。

UZonmail 提供两种发件方式：

1. 已知邮箱、ClientId 和 RefreshToken，可以直接添加发件
2. 已知邮箱和密码，可以通过自己申请 ClientId 进行发件

## RefreshToken 方式

直接在 smtp用户名 栏填写 ClientId，在 smtp密码 栏填 refreshToken 即可。

其它参数保持默认(其它参数没用，默认即可)

![image-20250628161219594](https://oss.223434.xyz:2234/public/files/images/image-20250628161219594.png)

添加后，请务必进行验证。由于有的 ClientId 没有发件权限，即使有 RefreshToken 也无法使用。

## 密码方式

而使用 Graph 方式，则需要注册一个应用，获取 ClientId，本章内容将一步一步地介绍如何获取 ClientId。

### 注册 Micosoft 账户

通过该链接 [https://signup.live.com/signup](https://signup.live.com/signup) 注册 Micosoft 账户。

### 注册 Microsoft Azure

可以通过以下两种试注册

1. 加入365开发者计划

   单击链接 [https://aka.ms/joinM365DeveloperProgram](https://aka.ms/joinM365DeveloperProgram) 加入 365 开发者计划。

2. 使用信用卡激活 Azure

   单击链接 [https://aka.ms/signUpForAzure](https://aka.ms/signUpForAzure) 跳转

### 注册应用

1. 进入 [Microsoft Entra ID](https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/Overview)

   进入后如图所示:

   ![image-20250628153130413](https://oss.223434.xyz:2234/public/files/images/image-20250628153130413.png)

2. 单击 [应用注册/+新注册](https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/CreateApplicationBlade/quickStartType~/null/isMSAApp~/false) 注册一个应用

   ![image-20250628153552853](https://oss.223434.xyz:2234/public/files/images/image-20250628153552853.png)

   **特别注意**：

   1. 重定向 URI 中要选择 [公共客户端/本机(移动和桌面)]
   2. 当是桌面端时，重定向的 url 为 `http://localhost:22345/api/v1/outlook-authorization/code`
   3. 当是服务器部署时，将地址改成服务器的域名地址
   4. 不支持局域网内部署

3. 设置 API 权限

   注册完成后，返回 [应用注册](https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps) 界面，切换到 [所有应用程序]，单击刚刚创建的程序进行设置界面

   ![image-20250628154403007](https://oss.223434.xyz:2234/public/files/images/image-20250628154403007.png)

   从侧边栏切换到 [API 权限] -> [+添加权限] -> [ Microsoft Graph] -> [委托的权限] 额外添加以下权限：

   - Mail.Send
   - offline_access
   - openid

   操作如下图所示：

   ![image-20250628154724024](https://oss.223434.xyz:2234/public/files/images/image-20250628154724024.png)

4. 授权管理员同意 [应该是可选，未测试]

   单击 API 权限界面中的 [授予管理员同意]

   ![image-20250628155230619](https://oss.223434.xyz:2234/public/files/images/image-20250628155230619.png)

5. 侧边栏切换到 [概述] 获取 ClientId

   ![image-20250628155444957](https://oss.223434.xyz:2234/public/files/images/image-20250628155444957.png)

### 配置后端

在后端根目录(服务器版本为程序根目录，桌面端为 ./service 目录)中添加 `appsettings.Production.json`，在其中更新如下配置：

``` json
{
  // 指定后端接口地址
  "BaseUrl": "https://uzon-mail.223434.xyz:2234",
  // 用于设置 Outlook 授权参数
  "MicrosoftEntraApp": {
    "ClientId": "xxxxx", // 你的 ClientId
    "TenantId": "", // 为空
    "ClientSecret": "" // 为空
  },
}
```

### 开始使用

新建发件箱时，只需要输入 smtp发件邮箱，smtp用户名 和 smtp密码 保持为空，点保存即可。

如下图所示：

![image-20250628161936418](https://oss.223434.xyz:2234/public/files/images/image-20250628161936418.png)

保存后，会触发委托授权流程，按提示操作后，就可以进行发件了。

除了新建编辑后会触发委托授权流程，也可以在 outlook 发件箱上右键，点 [Outlook授权] 进行手动触发。