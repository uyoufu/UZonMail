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

The installer supports x64 systemd Linux. Except for version queries, it re-executes through sudo once and asks for one confirmation per operation. Linux packages are checked against the SHA-256 in the update manifest. When a required .NET runtime is missing, `gnupg` must be installed so the Microsoft install script can be verified before execution.

``` bash
cd ~
wget https://raw.githubusercontent.com/uyoufu/UzonMail/refs/heads/master/scripts/install/uzonmail_linux_install.py
python3 ./uzonmail_linux_install.py --install
```

The same installer is included at the root of the Linux release archive. For automation, validate sudo first and use quiet mode:

``` bash
sudo -v
python3 ./uzonmail_linux_install.py --quiet --install
```

The application and its runtime data remain under `/var/www/uzonmail/`. Backups default to the root-only `/var/backups/uzonmail/`, so sudo is required to inspect or copy them. The systemd unit is named `uzon-mail.service`.

After installation is confirmed, credentials are cached in `/var/lib/uzonmail-installer/pending-config.json` with mode `0600`. The cache is deleted after success and retained after a failure or interruption for the next attempt. Uninstall removes installer state but retains the `uzonmail` system account.

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
