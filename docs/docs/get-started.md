---
uuid: 7deea49a-fd1e-11eb-ad13-597bfc858c85
title: 开始使用
tags: 
  - UzonMail
updated: 2022-01-19
abbrlink: 2QMK677

editLink: false
---

## 🥝简介

![uzon-mail-login-2](https://obs.uamazing.cn:52443/public/files/images/uzon-mail-login-2.png)

本软件名为 “宇正群邮 (UZonMail)"，是一款开源免费的邮件群发软件。它具有非常强大的邮件正文定制能力，可快速实现 "千人千面" 式地批量群发，使每一封邮件都让人觉得很温暖。

原生多线程处理能力，极尽压榨每一核 CPU 性能，让发件体验纵享丝滑，盏茶间，群发任务灰飞烟灭，独怅惘，无件可发何时休。

常见的应用场景有：

2. 批量群发营销邮件
2. 批量发送资条
3. 批量发送发票

> 开源地址：[UZonMail](https://github.com/GalensGan/UZonMail)

<!--more-->

## 🍎功能介绍

![image-20240614121857800](https://obs.uamazing.cn:52443/public/files/images/image-20240614121857800.png)

1. 支持多个发件人同时发件

   可以添加任意多个不同的发件人，同时发件，不仅提高发件效率，而且突破了单个发件箱每日发件数量的限制，理论上，若有足够多的发件箱，可以不受约束地发送海量的邮件。

2. 支持邮件内容模板自定义

   发件内容可以直接输入，也可以采用预定义模块。

   模板可根据需要进行自定义，定义完成后保存到模板库，可以轻易实现模板的复用。

   模板板采用 HTML 格式定义，程序同时提供可视化界面进行编辑，对于新手和大神都很友好，模板虽然使用下限低，但是上限很高。

3. 支持无限变量，邮件封封不同

   可以在模板中引入变量，在发送的过程中，程序会自动将变量替换成真实的值进行发送，可以实现同一套模板，不同的收件人，接收的具体内容不同。

4. 多线程并发发送，日发可达 10 万+

   每个发件人采用单独的线程进行发送，当一个发件箱出问题之后并不会使发件停止，会由其它发件箱的所在线程继续发件。

   若有足够多的发件箱，日发 10 万不是问题。

   由于不同的发件箱对发件频率有限制，若想提升发件速度，建议在一次发件任务中选择多个发件箱

5. 失败重发

   当有多个发件箱时，若 A 发件箱发件失败后，会转由 B 发件箱继续进行发送。

   如果仅有一个发件箱，当发件失败后，可以在发件历史中重新对失败项进行重发。

6. 自研随机算法，可实现发件人，发件模板智能随机

   当有多个发件人、发件模板时，程序会智能随机选择，提升邮件的到达率。

7. 支持发件箱每日发件总量限制

8. 支持抄送、密送

   支持多封邮件抄送到特定邮箱或者不同邮件抄送到不同的邮箱。

9. 支持邮件阅读跟踪

10. 支持取消订阅功能

11. 支持权限管理

    企业组织管理员可以管理查看所有子用户的数据、设置。方便集中管理

12. 原生爬虫支持，助力外贸市场开拓

## 🍇环境要求

1. Windows 7 及以上
3. Webview2 环境，下载地址：[microsoft-edge/webview2/](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/)
4. ASP.NET Core 环境，下载地址：[runtime-aspnetcore-9.0.2-windows-x64-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.2-windows-x64-installer)

## 🌱软件安装

### 软件下载

可以从 [历史版本](/versions) 中下载指定版本

### 默认参数

默认用户名：admin

默认密码：admin1234

### 桌面版本安装

#### Web 方式

1. 安装 [ASP.NET Core Runtime 9，单击下载](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.2-windows-x64-installer)

2. 从历史版本中下载 `uzonmail-desktop-win-x64` 版本，解压

3. 打开 `service/UZonMailService.exe` 程序

   打开后界面如下图所示，其中黄颜色的 WARN 不用理会。	![image-20241017221814354](https://obs.uamazing.cn:52443/public/files/images/image-20241017221814354.png)

4. 打开浏览器，输入 [http://localhost:22345/](http://localhost:22345/) 进行使用


#### Windows 7

由于微软已经停止对 Win7 的维护，因此本软件只是有限支持，可以参考 Web 的使用方式。

#### Windows 10+

1. 安装 [ASP.NET Core Runtime 9，单击下载](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-9.0.2-windows-x64-installer)
2. 安装 [.NET Desktop Runtime 9](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-9.0.2-windows-x64-installer)(一般系统自带)
3. 安装 [Webview2](https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/)(有 edge 浏览器，一般都自带)
4. 从历史版本中下载 `uzonmail-desktop-win-x64` 版本，解压
5. 打开 `UZonMailDesktop.exe` 开始使用

> 第 2、3 步可以忽略，若打开 `UZonMailDesktop.exe` 闪退或者空白，说明这两个环境缺失，手动安装一下即可

若不想使用客户端版本，也可以参照上面 Web 的配置方式进行安装使用

### 服务器版本安装

本节将介绍服务器的安装方式，主要分为 3 个部分：

1. 每个环境下安装方式介绍
2. 配置 UZonMail
3. 配置代理

#### Docker 安装

**启动容器**

*方式一、docker run*

`docker run --name uzon-mail -p 22345:22345 -d gmxgalens/uzon-mail:latest`

*方式二、docker compose*

linux 版本的压缩包中自带 docker-compose.yml，压缩包的内容如下：

``` bash
service-linux-x64
docker-compose.yml
docker-deploy.sh
```

解压后进入到上述文件所在目录，执行 `docker compose up` 即可。

这种方式启动后，默认设置如下

- 端口映射：22345
- 文件挂载：`./data/uzon-mail/data:/app/data`

*方式三、本地镜像*

若服务器无法连接网络，可以编译本地镜像进行使用，执行 `bash ./docker-deploy.sh` 编译镜像和启动容器

**配置文件挂载**

若要修改 UZonMail 的配置，请先在 `./data/uzon-mail` 中创建一个空文件 `appsettings.Production.json`，然后挂载到容器 `/app/appsettings.Production.json`。

**防火墙放行**

放行端口

``` bash
sudo ufw allow 22345/tcp
```

**网址访问**

访问 `http://your-docker-host-ip:22345` 登陆使用。

#### Windows Server 安装

**环境安装**

安装 [ASP.NET Core，单击下载](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.6-windows-hosting-bundle-installer)

**软件下载**

从 [历史版本](/versions) 中下载 `uzonmail-service-win-x64-version.zip` 解压后，单击 `UZonMailService.exe` 启动。

若截图如下所示，则说明安装成功：

![image-20241017221814354](https://obs.uamazing.cn:52443/public/files/images/image-20241017221814354.png)

此时，可以在浏览器中输入[http://localhost:22345/](http://localhost:22345/) 进行测试

测试成功后，关闭上述终端，进行服务注册

**服务注册**

使用管理员打开 cmd 或 powershell（建议使用 powershell），进入到解压后的目录中，执行下列命令将 `UZonMailService.exe` 注册为服务。

``` powershell
# 注册服务
uzon-mail install
# 启动服务
uzon-mail start
```

其它命令：

- uzon-mail stop 停止服务
- uzon-mail uninstall 卸载服务
- uzon-mail status 查看服务状态
- uzon-mail restart 重启服务

**放行端口**

``` powershell
# 放行
netsh advfirewall firewall add rule name="UZonMail" dir=in action=allow protocol=TCP localport=22345

# 查看防火墙规则
netsh advfirewall firewall show rule name="UZonMail"

# 若要删除，执行以下命令
netsh advfirewall firewall delete rule name="UZonMail"
```

**修改配置**

到这一步，服务就创建完成了，由于是服务器部署，还要处理跨域问题，请继续阅读 [UZonMail 配置](#UZonMail 配置) 章节。

#### Linux 安装

> 下文以 Ubuntu 22.04 LTS 举例说明

**环境安装**

Ubuntu 安装 `.NET` 见微软官方文档 [.NET 和 Ubuntu 概述 - .NET | Microsoft Learn](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux-ubuntu#im-using-ubuntu-2204-or-later-and-i-only-need-net)

命令如下：

```bash
sudo apt update && \
  sudo apt install -y aspnetcore-runtime-8.0
```

**软件下载**

从 [历史版本](/versions) 中下载 `uzonmail-service-linux-x64-version.zip` 。

具体操作如下：

``` bash
# 安装 unzip
sudo apt install unzip

# 下载并解压安装包
cd ~ && \
  wget --no-check-certificate https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-linux-x64-0.10.0.0.zip -O uzonmail.zip && \
  unzip uzonmail.zip -d ./uzonmail && \
  cd ./uzonmail
  
 # 使用安装脚本进行安装
 bash ./install.sh
```



**注册服务**

运行目录中的 `install.sh` 进行安装。

``` bash
# 安装 uzon-mail
bash ./install.sh
```

**放行端口**

使用如下命令放行端口：

``` bash
sudo ufw allow 22345/tcp
```

**修改配置**

到这一步，服务就创建完成了，由于是服务器部署，还要处理跨域问题，请继续阅读 [UZonMail 配置](#UZonMail 配置) 章节。

#### UZonMail 配置

使用程序根目录 `appsettings.Production.json` 对系统进行配置。

默认的配置文件为 `appsettings.json` ，里面包含程序用到的所有配置。`appsettings.Production.json` 中的设置会覆盖 `appsettings.json`。

> 请不要修改 `appsettings.json` 中的内容

**配置基本信息**

添加 `System` 字段配置系统信息

``` json
{
  "System": {
    "Name": "UZonEmail",
    "Icon": "",
    "Copyright": "Copyright © 2024 - 2024 UZon Email",
    "ICPInfo": "渝ICP备20246498号-3"
  },
}
```



**配置跨域**

添加 `Cors` 配置跨域

``` json
{
  "Cors": [ "http://localhost:9000", "https://desktop.uzonmail.com" ],
}
```



**配置 Token 参数**

添加 `TokenParams` 修改 Token 参数

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



**配置数据库**

添加 `Database` 修改数据库配置

``` json
{
  "Database": {
    "SqLite": {
      "Enable": true,
      "DataSource": "data/db/uzon-mail.db"
    },
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
    "Redis": {
      "Enable": false,
      "Host": "localhost",
      "Port": 6379,
      "Password": "",
      "Database": 0,
      "Description": "暂不可用"
    }
  },
}
```

系统默认使用 `SqLite` 数据库，在用户数量较多的情况下，建议使用 `MySql` 和启用 `Redis` 缓存。

启用时，将 `Enable` 设置为 `true` 即可。

**配置管理员**

添加 `User` 对管理员配置进行修改，此修改必须在初始化之前执行，否则无效。

``` json
{
  "User": {
    "CachePath": "users/{0}",
    "AdminUser": {
      "UserId": "admin",
      "Password": "admin1234",
      "Avatar": ""
    },
    "DefaultPassword": "uzonmail123"
  },
}
```

- CachePath 配置每个用户缓存的默认保存位置
- AdminUser 配置初始化管理信息
- DefaultPassword 配置新建用户时的默认密码

**配置取消订阅**

通过添加 `Unsubscribe` 字段可以针对不同的收件域名，配置不同的退订头。默认配置如下：

``` json
{
  "Unsubscribe": {
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
}
```

- Domain 表示收件箱域名
- Header 表示使用的头协议，目前程序实现了两种
  - RFC8058
  - AliDM

#### Nginx 反向代理

系统已将前端使用 kestrel 进行代理，可以使用下面的配置将前后端使用 nginx 进行反向代理。

将下面的文件保存为 `uzonmail.conf` 文件并保存到 nginx 安装目录下的 `conf.d` 或者 `conf` 目录中，然后 `nginx -s reload` 应用配置。

```nginx
upstream uzonmail {
    server localhost:22345;
}

# 前端
server {
    listen 443 ssl;
    # 实际域名
    server_name uzonmail.yourdomain.com;
    # 证书地址
    ssl_certificate _uzonmail.yourdomain.com.pem;
    ssl_certificate_key _uzonmail.yourdomain.com-key.pem;
    ssl_session_cache shared:SSL:1m;
    ssl_session_timeout 5m;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    location / {
        root /path/to/uzonmail/wwwroot
        index index.html;
        try_files $uri $uri/ /index.html;
    }
    add_header Access-Control-Allow-Origin "*";
    default_type 'text/html';
    charset utf-8;
}

# 后端
server {
    listen 443 ssl;
    # 实际域名
    server_name api.yourdomain.com;
    # 证书地址
    ssl_certificate api.yourdomain.com.pem;
    ssl_certificate_key api.yourdomain.com-key.pem;
    ssl_session_cache shared:SSL:1m;
    ssl_session_timeout 5m;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;
    # 不缓存，支持流式输出
    proxy_cache off;
    # 关闭缓存
    proxy_buffering off;
    # 关闭代理缓冲
    chunked_transfer_encoding on;
    # 开启分块传输编码
    tcp_nopush on;
    # 开启TCP NOPUSH选项，禁止Nagle算法
    tcp_nodelay on;
    # 开启TCP NODELAY选项，禁止延迟ACK算法
    keepalive_timeout 600;
    # 设定keep-alive超时时间为600秒
    # 头信息
    proxy_set_header X-Forwarded-Host $host;
    proxy_set_header X-Forwarded-Server $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    # location请求映射规则，/ 代表一切请求路径
    location / {
        proxy_connect_timeout 600;
        proxy_read_timeout 600;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_pass http://uzonmail;
    }
}
```

 

### 手动编译

建议普通用户不要采用这种方式，手动编译需要有一定的编程能力。

此处仅介绍在 Windows 环境的编译。

#### 环境要求

- Git
- 7z
- DotNET 8.0 SDK
- Node
- Docker

#### 编译步骤

1. 打开终端

2. 克隆仓库 `git clone https://github.com/GalensGan/UZonMail`，切换到 `master` 分支；

3. 进入到项目根目录下的 `scripts` 目录，执行下面的命令 开始编译，编译结果在 `build` 目录中。

   | 类型           | 命令                        | 位置                                       |
   | -------------- | --------------------------- | ------------------------------------------ |
   | desktop        | ./build-desktop.ps1         | build/uzonmail-desktop-win-x64-version.zip |
   | windows server | ./build-win-server.ps1      | build/uzonmail-service-win-x64-version.zip |
   | linux server   | ./build-linux.ps1           | build/uzonmail-linux-x64-version.zip       |
   | docker         | 在进行 linux 编译，自动编译 | docker 镜像，镜像名为 uzon-mail:latest     |

   编译成功截图：

   ![image-20240616124656131](https://obs.uamazing.cn:52443/public/files/images/image-20240616124656131.png)

> 手动编译时，会自动检测环境，若没有相关环境，请根据提示进行安装。

## 🍒发件步骤

1. 添加发件箱（已添加请忽略）
2. 添加收件箱（已添加请忽略）
3. 导入所需模板（已添加请忽略）
4. 打开【新建发件】，输入主题 --> 选择收件人 --> 选择模板 --> 选择数据 -->预览确认发件数量和模板正确性 --> 退出预览 -->点击【发送】
5. 如果提示发送失败，转到【发件历史】，进行重发

### 邮件经常进垃圾箱怎么办

可以使用 [Newsletters spam test by mail-tester.com](https://www.mail-tester.com/) 检查待发邮件的内容。

![image-20220428140629151](https://cdn.jsdelivr.net/gh/GalensGan/objects-storage/images/image-20220428140629151.png)

## 🥑功能介绍

### 功能概览

![image-20240614121857800](https://obs.uamazing.cn:52443/public/files/images/image-20240614121857800.png)

软件主要由以下功能模块组成：

- 首页

  数据统计展示

- 邮箱管理

  管理发件箱、收件箱

- 模板管理

  管理正文模板

- 发件管理

  新建发件和历史发件查询

- 系统设置

  - 用户管理

    增加其它使用用户

  - 基础设置

    发件相关的全局设置

  - 代理管理

    配置发件代理

- 支持作者

  赞助页面

- 使用说明

  帮助页

下面开始按模块进行详细介绍。

### 登陆界面

![uzon-mail-login-2](https://obs.uamazing.cn:52443/public/files/images/uzon-mail-login-2.png)

默认用户名和密码为：admin/admin1234

### 首页

![image-20240614231957130](https://obs.uamazing.cn:52443/public/files/images/image-20240614231957130.png)

首页主要展示的内容有：

- 发件箱数量直方图
- 收件箱数量直方图
- 每月发件量折线图

### 系统设置

#### 用户管理

![image-20240612122713293](https://obs.uamazing.cn:52443/public/files/images/image-20240612122713293.png)

系统默认的用户名为 admin，默认密码为 admin1234，这是一个管理员账号，该账号具有【管理用户】的权限。

【用户管理】模块用于增加不同的用户。桌面版本的多用户功能仅限本机使用，若要多人同时使用，则需要使用服务器版本。

服务器版本可联系作者获取。

**新增用户：**

![image-20240612123329057](https://obs.uamazing.cn:52443/public/files/images/image-20240612123329057.png)

单击左上角的新增，即可新增用户。

用户新增完成后，可对用户进行操作，比如重置密码，删除等

重置后的密码为：`uzonmail123`，在重置时会有提示。

![image-20240612123429178](https://obs.uamazing.cn:52443/public/files/images/image-20240612123429178.png)

**修改密码和头像：**

可以通过右上角的个人信息界面对头像和密码进行修改。

![image-20240612125131168](https://obs.uamazing.cn:52443/public/files/images/image-20240612125131168.png)

#### 基础设置

基础设置中，主要设置全局的发件间隔，最大发件量等。

![image-20240612125859579](https://obs.uamazing.cn:52443/public/files/images/image-20240612125859579.png)

- 单个邮箱每日最大发件量

  控制单个邮箱每日发件的总数，避免因发件数量超过每个邮件服务提供商规定的每日发件量，从而导致发件失败。

  为 0 时表示不限制

- 单个发件箱最小(最大)发件间隔

  单位：秒

  为了避免因频繁发送邮件而导致被服务器认为是垃圾邮箱，所以，发送两封邮件之间需要有一定的时间间隔，为了使得发送时间间隔具有不规律性，用间隔范围来进行控制：

  实际发件间隔值 = 最小值 + （0，1）之间的随机数*（最大值-最小值）

  最大值小于等于最小值时，表示不限制

- 合并发件最大数量

  当同时向多个收件箱发送相同内容时，可以将收件箱合并成一封邮件发送，这个参数即控制合并的最大数量。

  每个邮件服务商允许的最大数量不一样，最大数包含抄送和密送的数量

  为 0 时，表示不合并

#### 代理管理

![image-20240612131312091](https://obs.uamazing.cn:52443/public/files/images/image-20240612131312091.png)

代理管理模块主要针对使用国外邮箱的情况，允许针对某一类或者某个发件箱指定代理。

该功能一般用于服务器部署的情况，本机使用时，可以打开全局代理。

**新增代理**：

![image-20240612130111084](https://obs.uamazing.cn:52443/public/files/images/image-20240612130111084.png)

新增代理参数说明：

- 名称

  必填项。在发件箱界面，可通过名称选择指定代理

- 代理地址

  代理的地址、用户名和密码。格式为：`协议:\\username:password@host`，示例：

  1. 完整格式：`http:\\admin:admin1234@127.0.0.1:7890`
  2. 无密码格式: `http:\\127.0.0.1:7890`
  3. 其它协议：`socket5:\\127.0.0.1:7890`

  目前支持的协议有：`http、https、socks4、socks4a、socket5`

- 匹配规则

  若发件箱没有指定代理，则会从代理管理的列表中根据规则自动匹配，若匹配到，则使用。

  该规则的语法为正则表达式

  `.*` 表示所有的邮件都匹配

- 优先级

  规则匹配的优先级

- 是否共享

  共享后，系统内所有人都可使用这个代理

> 代理安全提示
>
> 代理是明文存储在服务器中的，因此管理员可以查看代理的信息，可能会造成代理泄露风险，请谨慎添加个人代理

### 邮箱管理

#### 发件箱

【发件箱】模块用于管理发件人信息。下面列出在使用中需要注意的功能进行说明：

**组管理：**

增加发件箱时，必须先建立发件组。

![image-20240612131552355](https://obs.uamazing.cn:52443/public/files/images/image-20240612131552355.png)

在 "发件箱" 上右键，可新增发件组。

![image-20240612131711338](https://obs.uamazing.cn:52443/public/files/images/image-20240612131711338.png)

新增时，"序号" 表示发件组的排序号，仅用于排序。

当新建组完成后，可以在组名上右键，对组进行管理

![image-20240612131916319](https://obs.uamazing.cn:52443/public/files/images/image-20240612131916319.png)

**新增发件箱**：

![image-20240612132056895](https://obs.uamazing.cn:52443/public/files/images/image-20240612132056895.png)

本软件采用的是 SMTP 协议发件，因此发件时，需要将自己的邮箱开通 SMTP 服务，可自行查阅资料。以下对一些重要参数说明：

- 发件人名称

  若有，当发给对方后，不显示邮箱，而是显示名称

- smtp 密码

  发件采用的 SMTP 服务器，所以，它的密码并不是邮箱的密码，而是登陆邮箱后，自己申请的 SMTP 密码。

  比如，163邮箱 SMTP 密码获取方式如下：https://www.yisu.com/zixun/97973.html

- smtp 地址

  smtp 的地址，每种类型的地址不一样，可百度查找

- smtp 端口

  该端口与是否【启用 SSL】有关，默认为 25，若启动 SSL，一般为 465，需要自动查找确认。

- 启用 SSL

  是在发件时，采用 SSL 加密，打开这个可以提升发件的安全性。

- 代理

  若有需要，可以为其指定代理，可用代理候选项在【代理管理】中定义。

> 密码安全提示
>
> 服务器没有直接存储 smtp 密码，而是存储了通过密钥加密后的密文，密钥由前端生成，当有需要时，由前端传递给后端解密。
>
> 因此，即使数据库泄露了，也不会造成 smtp 的密码被盗的问题

**从EXCEL导入：**

通过【导入】功能可批量从 Excel 中导入发件箱。

可以通过【模板】按钮下载导入模板。

在使用批量添加发件人功能时，Excel 表中第一行为表头，必须包含 `smtp邮箱`、`smtp密码`、`smtp地址`、`smtp端口`  列。

![image-20240614123302248](https://obs.uamazing.cn:52443/public/files/images/image-20240614123302248.png)

#### 收件箱

![image-20240614124814856](https://obs.uamazing.cn:52443/public/files/images/image-20240614124814856.png)

该模块主要用于对收件箱的分组和管理，使用方式、注意要点与发件人一致。

收件箱只需要姓名和邮箱即可，姓名是可选的。

### 模板管理

![image-20240614125056768](https://obs.uamazing.cn:52443/public/files/images/image-20240614125056768.png)

在【正文模板】用于管理用户下的所有模板，它是 html 格式。

#### 新增模板

使用两种方式进行添加：

- 导入 HTML
  
  先在外面用 html 定义好模板，然后通过上述中的【导入模板】功能将定义的模板导入到系统。对于自定义的 html 模板，要求其中的 css 必须为行内 css。可以通过 http://automattic.github.io/juice/ 自动将 html 文件中 css 转换成行内的 css。

- 直接编辑
  
  通过单击【新增】按钮新增模板。
  
  ![image-20240614125417120](https://obs.uamazing.cn:52443/public/files/images/image-20240614125417120.png)

#### 模板编辑

通过单击模板名称或者在模板上右键，然后选择【编译】跳转到模板修改界面。

![image-20240614125609679](https://obs.uamazing.cn:52443/public/files/images/image-20240614125609679.png)

#### 模板变量

在模板的编写过程中，可以使用双花括号（`{{变量名}}`）来标记为变量，在发件的过程中，程序会在数据中查找该变量，如果查找到，就会使用实际的数据将变量替换掉。

变量定义的格式是为：&#123;&#123;变量名&#125;&#125;。

> 在发件中，模板也可以因发件人而异，需要在数据中增加 templateId 列来覆盖通用的模板。具体参考发件篇。

### 发件管理

#### 新建发件

![image-20240614125845479](https://obs.uamazing.cn:52443/public/files/images/image-20240614125845479.png)

新建发件用于添加发件任务，通过不同的参数组合，它可以实现以下功能：

1. 一对一发件

   一个发件箱，一个收件箱

2. 一对多发件

   一个发件箱，多个收件箱

3. 多对多发件

   多个发件箱，多个收件箱

4. 主题变化、正文变化发件

   同时支持主题和正文根据收件人不同而变化

##### 主题

发件的主题是必须的，主有两个作用：一是为邮件的主题，二是同一次发件将会归到一个发件历史记录中，该主题为历史记录组的名称。

多个主题使用英文分号（`;`）或者换行进行分隔。

![image-20240614130827129](https://obs.uamazing.cn:52443/public/files/images/image-20240614130827129.png)

若有多个主题，系统在发件时，会随机使用一个主题（若在数据中指定了主题，则会固定使用数据的主题）。

主题也支持变量声明，比如：&#123;&#123;日期&#125;&#125;-工资明细，`日期` 即为定义的变量，在发送邮件时，它将被替换成 Excel 表中的实际数据。

##### 模板

模板相当于是正文的一个草稿，它可以让你快速发送正文，而不需要每次都在正文处输入。

![image-20240614131034812](https://obs.uamazing.cn:52443/public/files/images/image-20240614131034812.png)

可以选择多个模板。若有多个模板时，系统将随机选择一个模板来发件（若在数据中指定了模板，则会固定使用数据中的模板）

##### 正文

![image-20240614131326894](https://obs.uamazing.cn:52443/public/files/images/image-20240614131326894.png)

![image-20240614225842011](https://obs.uamazing.cn:52443/public/files/images/image-20240614225842011.png)

软件支持用户手动输入正文。

若用户指定了正文，则不会使用模板作为邮件正文。

正文的格式与模板一样，同样支持变量。

##### 发件人

![image-20240614131606189](https://obs.uamazing.cn:52443/public/files/images/image-20240614131606189.png)

单击发件人右侧的 + 号，选择发件人。

发件人允许有多个，若有多个发件人，发件时，将会把邮件随机给其中一个发件人发件。

在一次任务中，一封邮件只会被其中一个发件箱成功发送，不会多次发送。

##### 收件人

![image-20240614131851665](https://obs.uamazing.cn:52443/public/files/images/image-20240614131851665.png)

单击收件人右侧的 + 号，选择收件人。

单次发件任务中，允许添加多个收件人，每个收件都会收到一款邮件。

##### 抄送人

若选择抄送人，每一封邮件都会抄送到每一个抄送人处。

##### 附件

![image-20240614214517919](https://obs.uamazing.cn:52443/public/files/images/image-20240614214517919.png)

若邮件中需要添加附件，可以在此添加附件。

允许添加多个附件，但是请注意，每封邮件都会携带相同的附件。

##### 数据

支持数据发件功能是该软件的灵魂。通过导入数据，可以实现一次发件中，为不同的收件箱发送不同的内容。

当将鼠标聚焦在数据栏，右侧会出现下载模板的图标，单击该图标即可下载模板。

![image-20240614215520531](https://obs.uamazing.cn:52443/public/files/images/image-20240614215520531.png)

数据格式大致如下：

![image-20240614220243117](https://obs.uamazing.cn:52443/public/files/images/image-20240614220243117.png)

**数据的效果**：

模板内容:

![image-20240614222544153](https://obs.uamazing.cn:52443/public/files/images/image-20240614222544153.png)

模板赋予数据后的正文预览：

![image-20240614222617857](https://obs.uamazing.cn:52443/public/files/images/image-20240614222617857.png)

**数据的作用**：

1. 为模板提供变量
2. 快速实现精准的批量发送

**数据中系统保留变量**：

| 变量名       | 必须 | 描述                                                         |
| ------------ | ---- | ------------------------------------------------------------ |
| inbox        | 是   | 指定收件邮箱。该字段必须存在，程序依靠该变量进行发件匹配。若为空，则该行数据无效。 |
| inboxName    | 否   | 设置收件人名称。                                             |
| subject      | 否   | 指定主题。若指定，则会忽略界面中输入的主题。                 |
| outbox       | 否   | 指定发件箱。若不指定，则使用用户在界面中选择的发件箱。<br />该发件箱必须是在【邮箱管理/发件箱】中添加的邮箱地址。其它邮箱则视为无效。 |
| outboxName   | 否   | 设置发件箱名称。若不指定，则使用发件箱管理中的名称。         |
| cc           | 否   | 指定抄送人。多个抄送人使用逗号分隔。                         |
| templateId   | 否   | 指定邮件的模板 Id。该模板 Id 可在模板管理中查看，是一个数字。<br />若不指定，则从用户选择的模板中随机取一个使用。 |
| templateName | 否   | 指定邮件的模板名称。该名称可在模板管理中查看。templateName 的优先级低于 templateId。当两者同时指定时，以 templateId 为主。<br />若不指定，则从用户选择的模板中随机取一个使用。 |
| body         | 否   | 指定邮件的正文内容。该优先级大于 templateId 和 templateName。 |

**数据的优先级**：

- 主题

  【Excel数据/subject】  > 【界面/主题】

- 正文

  【Excel数据/body】 > 【Excel数据/templateId】 > 【Excel数据/templateName】 > 【界面/正文】 >【 界面/模板 】

- 发件人

  【Excel数据/outbox 】>【 界面/发件人】

- 收件人

  【Excel数据/inbox 】> 【界面/收件人】

- 抄送人

  【Excel数据/cc】 > 【界面/抄送人】

- 附件

  附件目前无法通过数据指定

#### 发件历史

发件历史显示历次所发的所有邮件记录，一次发送任务为一条历史。

单击 ID 或者右键【详细】可查看具体发件项。

![image-20240614225346191](https://obs.uamazing.cn:52443/public/files/images/image-20240614225346191.png)

发件明细：

![image-20240614231145776](https://obs.uamazing.cn:52443/public/files/images/image-20240614231145776.png)

## 🪺技术栈

本系统采用前后端分离的模式进行开发。

**前端：**

typescript+vue3+quasar

**后端：**

C# + WPF +ASP.NET Core+webview2

使用 ASP.NET Core 实现服务端，桌面端使用 WPF 开发

## 🍄服务器部署

### 简易版

1. 安装 ASP.NET Core 环境，下载地址：[runtime-aspnetcore-8.0.6-windows-hosting-bundle-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.6-windows-hosting-bundle-installer)
2. 启动 `service/UZonMailService.exe`

至此，即可通过 `http://you-ip:22345` 进行访问了

### 前后端分离部署

到这一步，我相信您已经具备专业的知识，可以自己处理安装，此处只简述下前后端各部分内容：

1. 后端位于 `service` 目录
2. 前端位于 `service/wwwroot` 目录

您可以配置 nginx 进行反向代理，配置负载均衡

### 前后端配置说明

前端的配置位于 `service/wwwroot/app.config.ts` 文件中，若是对外提供服务，则需要修改后端接口地址，即 `baseUrl`：

``` typescript
export default {
  // 默认配置
  default: {
    baseUrl: 'http://localhost:22345',
    api: '/api/v1',
    signalRHub: '/hubs/uzonMailHub'
  },

  // 生产配置
  prod: {},

  // 开发配置
  dev: {}
} as Record<'default' | 'prod' | 'dev', any>
```

后端的配置位于 `service/wwwroot/appsettings.json`，可以在这里配置：

- 日志
- 授权密钥
- 系统参数
- 接口 Http 参数
- Websocket 参数
- 数据库参数
- 初始用户参数
- 跨域
- 文件存储
- 定时任务

请查看配置文件进行理解。

