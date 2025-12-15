---
title: 自建 postfix 邮局
icon: server
order: 1
description: 宇正群邮（UZonMail）是一款开源、免费、企业级的邮件群发软件，支持邮件营销、邮箱爬取、无限变量、Outlook OAuth2 等功能。多线程高效群发，支持 Windows、Linux、MacOS、Docker 等平台部署，适用于外贸、教育、财务等行业。宇正群邮——最好用的邮件群发软件，助力高效邮件营销！
---

受限于 VPS 性能，入门机通常只有 1 核、1G内存、SSD20G，无法通过 Docker 的方式安装 Mailcow 之类的邮局软件，因此只能安装更底层的 postfix 来实现自建邮局。

本节教程将按生产级的标准，总结了如何在主机上安装 postfix。

> 本节内容均以域名 `uzoncloud.com` 为例, 假设机器配置为 1 核 1G, 20G 硬盘

## 方案对比

目前比较流行的方案有：

1. Mailcow
2. iReadMail
3. postfix

**Mailcow** 是目前最受欢迎的开源邮件服务器之一，具有企业级的品质，它基于 postfix 构建。

前两者软件比较重，对机器要求高一些。

受限于机器性能，最终决定使用 postfix 方案

## 方案细化

受限于机器性能，为了减少性能的损耗，选择最底层的实现，往往是最经济的，推荐方案如下：

| 序号 | 软件    | 用途                                                         |
| ---- | ------- | ------------------------------------------------------------ |
| 1    | postfix | 实现了 SMTP(S) 协议，用于发件                                |
| 2    | Dovecot | 实现了 pop3 及 imap 协议，用于保存邮件及其它客户端登录连接到邮件服务 |

## 软件安装

本节为软件安装实操，可根据对应内容，将域名修改为自己的域名，基本每一个配置都有说明，方便理解。主要的步骤如下：

1. 在域名托管商处配置 DNS 服务

   主要配置邮件服务器名称、MX、SPF、DMARC、DKIM、SMTP域名、POP域名、IMAP 域名。这些概念现在不需要理解，在 [DNS 配置](#DNS 配置) 章节中会有介绍。

2. 安装 postfix

   主要将自己的域名信息更新到配置中，然后启动 TLS 或 STARTTLS

3. 安装 Dovecot

   配置 IMAP、POP 协议，配置 smtp 授权

4. 安装 opendkim

   用于 DKIM 加密与验证

5. 配置 RDNS，也叫 PTR

### DNS 配置

**所有 DNS 见下表**

| 类型 | 名称               | 完整域名                         | 内容                                             | 作用                                                         | 必须 |
| ---- | ------------------ | -------------------------------- | ------------------------------------------------ | ------------------------------------------------------------ | ---- |
| A    | mail1              | mail1.uzonmail.com               | IP地址                                           | 服务器名称；RDNS                                             | 是   |
| MX   | @                  | uzoncloud.com                    | mx.uzoncloud.com                                 | 告诉外部邮局，你的 mx 地址。                                 | 是   |
| TXT  | @                  | uzoncloud.com                    | v=spf1 a mx ip4:xx.xx.xx.xx ~all                 | SPF 设置，验证邮件发件人的IP地址是否被域名所有者授权发送该域名的邮件 | 是   |
| TXT  | mail1              | mail1.uzonmail.com               | v=spf1 redirect=uzoncloud.com                    | mail1 的 SPF 重定向到主域                                    | 是   |
| TXT  | _dmarc             | _dmarc.uzoncloud.com             | v=DMARC1; p=quarantine; pct=100; adkim=r; aspf=r | 基于SPF和DKIM的认证结果，指定如何处理邮件认证失败的情况（如隔离或拒绝），并提供报告帮助域名所有者监控和改进邮件安全性 | 是   |
| TXT  | default._domainkey | default._domainkey.uzoncloud.com | v=DKIM1; k=rsa; p=公钥内容                       | DKIM 用于数字签名邮件，确保内容未被篡改                      | 是   |
| A    | mx                 | mx.uzonmail.com                  | IP地址                                           | 用于别的邮局向你的服务器投递邮件 [可选]                      | 否   |
| A    | smtp               | smtp.uzoncloud.com               | IP地址                                           | 约定的 smtp 域名，客户端使用 smtp 发件 [可选]                | 否   |
| A    | pop                | pop.uzoncloud.com                | IP地址                                           | 约定的 pop 域名，客户端使用 pop 协议取件[可选]               | 否   |
| A    | imap               | imap.uzoncloud.com               | IP地址                                           | 约定的 imap 域名，客户端使用 imap 协议复制邮件[可选]         | 否   |

上表中，可选项可以不用添加 DNS 记录，使用某个指向服务器的域名即可。同时有以下几点要注意：

1. 若 `mx` 域名未填写，注意修改 SPF 记录中的域名
2. DKIM 中的私钥和公钥在 [opendkim安装](#opendkim安装) 步骤中生成
3. mail1 配置 SPF 是为了解决 `SPF: HELO does not publish an SPF Record` 警告


### postfix 安装

**安装软件**

``` bash
# 安装时，配置类型选择: 2. Internet Site
apt install postfix
```

**配置 main.cf** 

main.cf 文件提供最少的配置参数。

以下是详细配置过程：

``` bash
# 配置主机名
# 主机名的作用有：
#  1. 对外发件时，用于声明自己
#  2. RDNS 的指向域名
#  3. postfix 从主机名中提取域名
# 主机名习惯上是 mail.uzoncloud.com, 也可以设置为其它的
sudo postconf -e "myhostname = mail1.uzoncloud.com"

# 配置邮箱目录
sudo postconf -e 'home_mailbox = Maildir /'

# 使用 SASL（Dovecot SASL）配置 Postfix 的 SMTP-AUTH
# 授权验证使用 dovecot
sudo postconf -e 'smtpd_sasl_type = dovecot'
sudo postconf -e 'smtpd_sasl_path = private/auth'
sudo postconf -e 'smtpd_sasl_local_domain = $myhostname'
sudo postconf -e 'smtpd_sasl_security_options = noanonymous,noplaintext'
sudo postconf -e 'smtpd_sasl_tls_security_options = noanonymous'
sudo postconf -e 'broken_sasl_auth_clients = yes'
sudo postconf -e 'smtpd_sasl_auth_enable = yes'
sudo postconf -e 'smtpd_recipient_restrictions = permit_sasl_authenticated,permit_mynetworks,reject_unauth_destination'

# 配置 SSL 证书
# 证书可以通过 acme.sh 申请, 申请后，按以下操作
# 注意, 不要使用自签名证书, 自签名在某些安全验证环境下, 会存在问题
sudo postconf -e 'smtp_tls_security_level = may'
sudo postconf -e 'smtpd_tls_security_level = may'
sudo postconf -e 'smtp_tls_note_starttls_offer = yes'
sudo postconf -e 'smtpd_tls_key_file = /etc/nginx/conf.d/cert/uzoncloud.com.key'
sudo postconf -e 'smtpd_tls_cert_file = /etc/nginx/conf.d/cert/uzoncloud.com.cert'
sudo postconf -e 'smtpd_tls_loglevel = 1'
sudo postconf -e 'smtpd_tls_received_header = yes'
sudo postconf -e 'smtpd_tls_auth_only = yes'
```

**配置 SMTPS**

``` bash
# 在 main.cf 中添加一些 MUA 安全限制
# MUA 是 Mail User Agent（邮件用户代理）的缩写, 它指的是客户端邮件软件，如 Outlook、Thunderbird 或手机邮件应用，用于发送和接收邮件

# MUA 客户端限制: 允许认证用户，其他拒绝
sudo postconf -e 'mua_client_restrictions = permit_sasl_authenticated, reject'
# MUA HELO 限制：允许认证用户，其他拒绝
sudo postconf -e 'mua_helo_restrictions = permit_sasl_authenticated, reject'
# MUA 发送者限制：允许认证用户，其他拒绝
sudo postconf -e 'mua_sender_restrictions = permit_sasl_authenticated, reject'
# MUA 中继限制：允许认证用户，其他拒绝
sudo postconf -e 'mua_relay_restrictions = permit_sasl_authenticated, reject'
# MUA 收件人限制：允许认证用户，其他拒绝
sudo postconf -e 'mua_recipient_restrictions = permit_sasl_authenticated, reject'

# 编辑文件 master.cf 
nano /etc/postfix/master.cf 

# 找到 submission, 文件中有 submission 与 submissions, 其中，前者为 STARTTLS，后者为 TLS
# 包含 -o smtpd_tls_security_level=encrypt 表示 STARTTLS, 监听 587 端口
# 包含 -o smtpd_tls_wrappermode=yes 表示 TLS, 监听 465
# 可以启用两个 submission
# 注释掉 submission 相关的配置, 如下所示
submission inet n       -       y       -       -       smtpd
  -o syslog_name=postfix/submission
  -o smtpd_tls_security_level=encrypt
  -o smtpd_sasl_auth_enable=yes
  -o smtpd_tls_auth_only=yes
  -o local_header_rewrite_clients=static:all
  -o smtpd_reject_unlisted_recipient=no
  -o milter_macro_daemon_name=ORIGINATING
  
# 然后，额外添加如下安全配置
  -o smtpd_client_restrictions=$mua_client_restrictions
  -o smtpd_client_restrictions=$mua_helo_restrictions
  -o smtpd_sender_restriction=$mua_sender_restrictions
  -o smtpd_relay_restrictions=$mua_relay_restrictions
  -o smtpd_recipient_restrictions=$mua_recipient_restrictions

# submissions 配置也如上操作

# 保存配置
```

**重定向系统账户邮件 [可选]**

在Linux系统上，系统账户（如`root`、`postmaster`）的邮件通常通过 /etc/aliases 文件重定向到实际管理员用户账户。这可以防止邮件丢失到`/var/mail/nobody`。

``` bash
nano /etc/aliases
# 若有需要，进行以下配置
# root: your_admin_user
# postmaster: your_admin_user
```



### Dovecot 安装

**安装软件**

``` bash
# dovecot-imapd 提供 imap 协议
# dovecot-pop3d 提供 pop3 协议
sudo apt install -y dovecot-core dovecot-imapd dovecot-pop3d
```

Dovecot 所有的配置位于 `/etc/dovecot/conf.d` 中，其按配置类型，拆分为了多个配置，可以根据需要对应到每个文件去修改。

**配置 10-auth.conf**

``` bash
nano /etc/dovecot/conf.d/10-auth.conf
# 将 auth_mechanisms = plain  
# 改成 auth_mechanisms = plain login
sudo sed -i '/auth_mechanisms = plain/ { /login/! s/$/ login/; }' /etc/dovecot/conf.d/10-auth.conf
```

**配置 10-master.conf**

``` bash
nano /etc/dovecot/conf.d/10-master.conf
# 将 service auth 配置块修改为如下配置，用于 Postfix 的 SMTP 认证（smtp-auth），允许 Postfix 与 Dovecot 通信进行邮件认证

service auth {
  # auth_socket_path points to this userdb socket by default. It's typically
  # used by dovecot-lda, doveadm, possibly imap process, etc. Users that have
  # full permissions to this socket are able to get a list of all usernames and
  # get the results of everyone's userdb lookups.
  #
  # The default 0666 mode allows anyone to connect to the socket, but the
  # userdb lookups will succeed only if the userdb returns an "uid" field that
  # matches the caller process's UID. Also if caller's uid or gid matches the
  # socket's uid or gid the lookup succeeds. Anything else causes a failure.
  #
  # To give the caller full permissions to lookup all users, set the mode to
  # something else than 0666 and Dovecot lets the kernel enforce the
  # permissions (e.g. 0777 allows everyone full permissions).
  unix_listener auth-userdb {
    #mode = 0666
    #user =
    #group =
  }

  # Postfix smtp-auth
  #unix_listener /var/spool/postfix/private/auth {
  #  mode = 0666
  #}
  # 以下配置为新增
  unix_listener /var/spool/postfix/private/auth {
    mode = 0660
    user = postfix
    group = postfix
  }

  # Auth process is run as this user.
  #user = $default_internal_user
}
```

**配置 dovecot.conf**

``` bash
nano /etc/dovecot/dovecot.conf

# 指定邮件存储位置
# 添加如下配置覆盖默认
mail_location = maildir:~/Maildir
```

**配置 SSL**

``` bash
nano /etc/dovecot/conf.d/10-ssl.conf

# 将 ssl_cert, ssl_key 改成申请的证书
# ssl_cert = </etc/nginx/conf.d/cert/uzoncloud.com.cert
# ssl_key = </etc/nginx/conf.d/cert/uzoncloud.com.key
```

### opendkim 安装

它通过签名出站邮件和验证入站邮件的方式，帮助防止邮件伪造和垃圾邮件，提高邮件安全性。

安装步骤如下：

1. 安装  opendkim

   ``` bash
   sudo apt update
   sudo apt install opendkim opendkim-tools
   ```

2. 生成 DKIM 密钥

   ``` bash
   # 生成密钥对
   sudo mkdir -p /etc/opendkim/keys/uzoncloud.com
   # -s 参数指定 selector 名称，该名称为 default._domainkey.uzoncloud.com 的三级域名名称
   # 在进行配置时，也需要使用到该名称，此处使用 default
   sudo opendkim-genkey -s default -d uzoncloud.com -D /etc/opendkim/keys/uzoncloud.com
   sudo chown opendkim:opendkim /etc/opendkim/keys/uzoncloud.com/default.private
   sudo chmod 600 /etc/opendkim/keys/uzoncloud.com/default.private
   
   # 查看公钥（用于 DNS TXT 记录）
   # 使用时，只复制 () 之间的内容，同时去掉换行和引号
   cat /etc/opendkim/keys/uzoncloud.com/default.txt
   ```

3. 将公钥内容添加到 DNS TXT 记录中，域名为：default._domainkey.uzoncloud.com

4. 配置 OpenDKIM 主文件 (/etc/opendkim.conf)

   配置参考：[opendkim - Debian Wiki](https://wiki.debian.org/opendkim)

   编辑文件，添加或修改以下内容：

   ``` bash
   # 配置OpenDKIM
   nano /etc/opendkim.conf
   
   # 在文件末尾追加如下配置
   
   # 基本配置
   # 规范化算法
   Canonicalization relaxed
   # 配置域名
   Domain uzoncloud.com
   # 指定私钥
   KeyFile /etc/opendkim/keys/uzoncloud.com/default.private
   Selector default
   Mode sv
   # 启用端口监听
   Socket inet:8891@localhost
   
   # 表头文件
   # 列出不应进行 DKIM 验证的外部主机或域名，通常与 TrustedHosts 文件相同，用于忽略某些外部邮件的签名验证，以避免误报
   # refile 表示使用正则匹配
   ExternalIgnoreList refile:/etc/opendkim/TrustedHosts
   # 列出内部主机或域名，这些主机的邮件将被签名而不是验证
   InternalHosts refile:/etc/opendkim/TrustedHosts
   # 定义用于签名的密钥映射。它将选择器（default）和域名关联到私钥文件路径。Postfix 使用此表来确定签名邮件时使用哪个密钥
   KeyTable refile:/etc/opendkim/KeyTable
   # 指定哪些发件人（域名或用户）应该使用哪些密钥进行签名。它定义签名策略，确保出站邮件被正确签名
   SigningTable refile:/etc/opendkim/SigningTable
   ```

5. 创建表头文件

   ``` bash
   # 对 ExternalIgnoreList、InternalHosts、KeyTable、SigningTable 文件内容进行配置
   echo "default._domainkey.uzoncloud.com uzoncloud.com:default:/etc/opendkim/keys/uzoncloud.com/default.private" > /etc/opendkim/KeyTable
   echo "*@uzoncloud.com default._domainkey.uzoncloud.com" > /etc/opendkim/SigningTable
   echo -e "localhost\n127.0.0.1\n::1\nuzoncloud.com" > /etc/opendkim/TrustedHosts
   ```

6. 验证配置文件

   ``` bash
   sudo opendkim -n -c /etc/opendkim.conf
   ```

   若验证通过，则没有任何输出

7. 与 Postfix 结合

   **编辑 /etc/postfix/main.cf**

   ``` bash
   # 编辑 /etc/postfix/main.cf，添加 milter 配置
   # 添加到 main.cf 末尾
   milter_default_action = accept
   smtpd_milters = inet:localhost:8891  # 或 unix:/var/run/opendkim/opendkim.sock
   non_smtpd_milters = $smtpd_milters
   ```

   **编辑 /etc/postfix/master.cf [可选]**

   ``` bash
   # 编辑 /etc/postfix/master.cf，在 smtp 和 submission 服务下添加 milter（可选，用于特定端口）
   smtp      inet  n       -       y       -       -       smtpd
     -o smtpd_milters=inet:localhost:8891
   ```

### 用户管理

#### 创建用户

启动邮箱服务并新增邮箱用户

``` bash
sudo systemctl start postfix dovecot opendkim
# 重启
sudo systemctl restart postfix dovecot opendkim
# 设置开机自启动
sudo systemctl enable postfix dovecot opendkim

# 新建用户
# 用户格式建议为邮箱号, 因为有的客户端不支持自定义 smtp 用户名
useradd -m username
# 禁止用户登录
sudo usermod -s /usr/sbin/nologin username
# 设置密码
passwd username

# 验证账户
sudo doveadm auth test username
```

#### 删除用户

若用户不再需要，可以删除，删除用户主要有两个步骤：

1. 删除 dovecot 已经保存的邮件
2. 删除用户

具体操作如下：

``` bash
# 停止 Dovecot 服务（防止删除过程中出现冲突
sudo systemctl stop dovecot

# 删除系统用户及其 home 目录（这将删除用户的邮件数据）
# -r 选项会删除用户的 home 目录（包括 Maildir 中的邮件）
sudo userdel -r username

# 验证删除：
# 检查用户是否已删除：（应返回“no such user”）
id username
# 检查邮件目录：（应不存在）
ls /home/username

# 启动服务
sudo systemctl start dovecot
```

### 配置 RDNS、PTR

若 VPS 提供商允许，同时配置 PTR。

PTR（Pointer）记录是 DNS 中的反向 DNS（Reverse DNS）记录，用于将 IP 地址映射回对应的域名。它在邮件服务器中的主要作用包括：

- **身份验证**：邮件接收服务器（如 Gmail、Outlook）会检查发件 IP 的 PTR 记录是否与发件域名匹配。如果不匹配，可能被视为可疑邮件，导致邮件被标记为垃圾或拒绝。
- **反垃圾邮件**：PTR 是 SPF、DKIM、DMARC 等认证机制的补充，帮助验证发件服务器的合法性，减少伪造邮件的风险。
- **服务器配置**：在自建邮局中，PTR 记录通常指向邮件服务器的主机名（如 `mail1.uzoncloud.com`），确保 RDNS（Reverse DNS）正确解析。

配置 PTR 需要联系 VPS 提供商或域名托管商，在其 DNS 面板中添加反向记录。未正确配置 PTR 可能导致邮件投递失败或被过滤。

### 验证邮件服务器配置

可以向发送一封邮件，查看邮件得分，根据得分情况进行优化。

**原始配置发件结果：**

![未配置验证](https://oss.uzoncloud.com:2234/public/files/images/image-20251214181802651.png)

**配置完成后结果：**

![配置验证](https://oss.uzoncloud.com:2234/public/files/images/image-20251215124148996.png)

## 高级配置

### 大规模账号管理

若需要进行大规模账号管理，建议安装  `PostFixAdmin`，采用数据库的试来管理多域和多账号。由于本人目前没有这方便需求，此处不介绍。

特别说明，对于自建邮局来说，多个账号与单个账号是一样的，因为多个账号如果共用同一公网IP或同一发送域，信誉是共享的，问题会互相牵连。

- 发送速度与暖机（warm‑up）：新 IP/域需要逐步增加发送量才能建立信誉；突然大流量会触发拦截。
- 列表质量与互动：干净的白名单/双重确认订阅、低退信率、低投诉率、高打开/点击率有助提升到达。
- 内容与频率稳定性：高重复、垃圾特征、突变发送模式都会降低到达率。
- 黑名单与监控：持续监测 Spamhaus、Google Postmaster、Microsoft SNDS 等，及时处理问题。

### 反垃圾邮件

可以使用 SpamAssassin 实现垃圾邮件检测。后期若收到的垃圾邮件较多，可以再行配置。

## 开始使用

至此，邮件服务器已经搭建成功了，可以下载 [宇正群邮](http://mail.uzoncloud.com/)、Foxmail 等客户端进行使用了。