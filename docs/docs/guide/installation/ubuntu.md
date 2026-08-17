---
title: Ubuntu
icon: fab fa-ubuntu
order: 4
description: 宇正群邮 Ubuntu 安装教程，详细介绍如何在 Ubuntu 系统上部署宇正群邮邮件群发软件。支持开源邮件群发、邮件营销软件，助力企业和个人高效邮件群发，体验最好用的邮件群发软件。
permalink: /guide/installation/ubuntu
---

::: tip
下文以 Ubuntu 22.04 LTS 举例说明
:::

本教程默认使用 ssh 连接到 ubuntu 进行操作

## 安装

安装器仅支持 x64 systemd Linux。除查看版本外，它会在启动时通过 sudo 重新执行一次，并在每项操作开始前统一确认。安装包会根据更新清单中的 SHA-256 校验；缺少所需 .NET 运行时时，还会使用 `gpg` 验证微软安装脚本后再执行，因此这种情况下需要预先安装 `gnupg`。

``` bash
cd ~
wget https://raw.githubusercontent.com/uyoufu/UzonMail/refs/heads/master/scripts/install/uzonmail_linux_install.py
python3 ./uzonmail_linux_install.py --install
```

Linux 发布压缩包根目录中也包含同名安装器。自动化部署可以预先验证 sudo 后使用静默模式：

``` bash
sudo -v
python3 ./uzonmail_linux_install.py --quiet --install
```

安装目录和运行数据均位于 `/var/www/uzonmail/`。备份默认保存在 root 专用的 `/var/backups/uzonmail/`，普通用户需要通过 sudo 查看或复制。systemd 服务名为 `uzon-mail.service`。

确认安装后，输入的密钥和管理员密码会暂存在权限为 `0600` 的 `/var/lib/uzonmail-installer/pending-config.json`。安装成功后自动删除；失败或中断时保留，供下次重试使用。卸载会清理安装器状态，但保留 `uzonmail` 系统账户。

## 更新、备份和卸载

``` bash
# 查看已安装版本
python3 ./uzonmail_linux_install.py --version

# 更新到最新兼容版本
python3 ./uzonmail_linux_install.py --update

# 备份到默认目录或指定父目录
python3 ./uzonmail_linux_install.py --backup
python3 ./uzonmail_linux_install.py --backup /srv/uzonmail-backups

# 恢复指定备份；省略路径时从默认目录选择
python3 ./uzonmail_linux_install.py --restore /srv/uzonmail-backups/uzonmail-backup-version-time
python3 ./uzonmail_linux_install.py --restore

# 卸载；静默模式会先自动备份
python3 ./uzonmail_linux_install.py --uninstall
```

## 放行端口

使用如下命令放行端口：

``` bash
sudo ufw allow 22345/tcp
```

服务启动后，后端即安装成功。

可以在浏览器中打开 `http://your-ubuntu-ip:22345` 查看安装效果。

## 修改配置

安装器会生成 `appsettings.Production.json`，自动配置 Token、加密参数、管理员账号、BaseUrl 和对应的 CORS 来源。其它服务器配置请继续阅读 [后端配置](/guide/setup/) 章节。

::: warning
若代理到公网，请务必修改默认配置，否则服务将会不安全！
:::
