---
title: Ubuntu
icon: fab fa-ubuntu
order: 4
description: Guide to deploy UzonMail on Ubuntu (example uses Ubuntu 22.04 LTS).
permalink: /en/guide/installation/ubuntu
---

::: tip
The examples below use Ubuntu 22.04 LTS.
:::

This guide assumes SSH access to the Ubuntu host.

## Install

The installer supports x64 systemd Linux. It explains the directories, system account, and service it will create, validates sudo access, downloads the latest Linux package, and installs the .NET runtimes declared by the update manifest when necessary.

``` bash
cd ~
wget https://raw.githubusercontent.com/uyoufu/UzonMail/refs/heads/master/scripts/install/uzonmail_linux_install.py
python3 ./uzonmail_linux_install.py --install
```

The same installer is included at the root of the Linux release archive. Normal mode confirms every system-changing step. For automation, validate sudo first and use quiet mode:

``` bash
sudo -v
python3 ./uzonmail_linux_install.py --quiet --install
```

The application is installed in `/var/www/uzonmail/`, backups default to `/var/uzonmail/backup/`, and the systemd unit is named `uzon-mail.service`.

## Update, backup, and uninstall

``` bash
# Show the installed version
python3 ./uzonmail_linux_install.py --version

# Update to the latest compatible release
python3 ./uzonmail_linux_install.py --update

# Back up to the default or a custom parent directory
python3 ./uzonmail_linux_install.py --backup
python3 ./uzonmail_linux_install.py --backup /srv/uzonmail-backups

# Restore a specific backup, or select one from the default directory
python3 ./uzonmail_linux_install.py --restore /srv/uzonmail-backups/uzonmail-backup-version-time
python3 ./uzonmail_linux_install.py --restore

# Uninstall; quiet mode creates a backup first
python3 ./uzonmail_linux_install.py --uninstall
```

## Firewall

Allow port 22345:

``` bash
sudo ufw allow 22345/tcp
```

After the service starts, visit `http://your-ubuntu-ip:22345` in a browser.

## Modify configuration

The installer generates `appsettings.Production.json` and configures token and encryption secrets, the administrator account, BaseUrl, and its CORS origin. See [Backend Configuration](/guide/setup/) for additional server settings.

::: warning
If you expose the service publicly, make sure to change default configuration for security!
:::
