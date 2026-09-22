---
title: Linux
icon: fab fa-linux
order: 5
description: Installation Guide for Linux-Based systems
permalink: /en/guide/installation/linux-installation
---

## For new users

> Download the latest version from the official website [UzonCloud](https://uzonmail.uzoncloud.com/en/)

<img src="https://uzonmail.uzoncloud.com/public/screenshot/download-list.png" alt="Download" width="400"/>

> Select the latest Linux build and download the official Linux build

> Mostly **uzonmail-service-linux-x64**

## Once Downloaded

- Go to the Downloaded ZIP in the Downloads Folder
  > on Terminal:

```bash
cd Downloads
```

- extract the ZIP and save in a folder
  > using Terminal (Use the folder name which can be seen by running "ls"):

```bash
tar -xzf filename.tar.gz
```

### Open terminal shell at the the directory roots

- Head to the downloads directory

```bash
cd Downloads
```

- move to the uzonmail folder

```bash
cd uzonmail-service-linux-x64-0.23.6.0
```

- List all the active files

```bash
ls
```

- run this to install system dependencies

```bash
sudo python3 uzonmail_linux_install.py --install
```

## Installation

- Once you have already ran the command and see this. **Click Enter**

```bash
UzonMail Linux installer - install
This tool can download UzonMail and the required .NET runtimes over HTTPS.
It manages /var/www/uzonmail and /var/lib/uzonmail-installer.
It manages system account 'uzonmail' and systemd unit /etc/systemd/system/uzon-mail.service.
Backups are preserved under /var/backups/uzonmail unless another path is selected.
It does not change firewall rules or remove shared .NET runtimes during uninstall.

Base URL [http://localhost:22345]:
```

> click enter

- You would be prompted to add an admin username and password

```bash
Administrator username [admin]:
Administrator password [press Enter to reuse the saved/default password]:
```

> click enter when done

- Allow the application to install Locally

```bash
nstallation settings:
  Base URL: http://localhost:22345
  Administrator: Contractor
  Administrator password: [hidden]
  Encryption and token secrets: [generated and hidden]
Reading the latest release metadata...
   Downloaded 11%
   Downloaded 23%
   Downloaded 31%
   Downloaded 42%
   Downloaded 50%
   Downloaded 62%
   Downloaded 73%
   Downloaded 81%
   Downloaded 93%
   Downloaded 100%
```

- **Allow** it to be installed locally

```bash
Install UzonMail 0.23.6.0 at /var/www/uzonmail using these settings [y/N]: y
```

- Let it fully install (Preview of how it looks on my terminal)

```text
 Downloaded 100%
   Downloaded 100%

Import the Microsoft dotnet-install signing key
   gpg --homedir /tmp/uzonmail-installer-x4e68o2t/gnupg --batch --import /tmp/uzonmail-installer-x4e68o2t/dotnet-install.asc
gpg: keybox '/tmp/uzonmail-installer-x4e68o2t/gnupg/pubring.kbx' created
gpg: /tmp/uzonmail-installer-x4e68o2t/gnupg/trustdb.gpg: trustdb created
gpg: key B9CF1A51FC7D3ACF: public key "Microsoft DevUXTeamPrague <devuxteamprague@microsoft.com>" imported
gpg: Total number processed: 1
gpg:               imported: 1
-> Read the imported Microsoft signing key fingerprint
   gpg --homedir /tmp/uzonmail-installer-x4e68o2t/gnupg --batch --with-colons --fingerprint
-> Verify the dotnet-install.sh signature
   gpg --homedir /tmp/uzonmail-installer-x4e68o2t/gnupg --batch --status-fd=1 --verify /tmp/uzonmail-installer-x4e68o2t/dotnet-install.sig /tmp/uzonmail-installer-x4e68o2t/dotnet-install.sh
-> Install Microsoft.AspNetCore.App 10.0.0.0
   bash /tmp/uzonmail-installer-x4e68o2t/dotnet-install.sh --runtime aspnetcore --version 10.0.0 --install-dir /usr/share/dotnet --no-path
dotnet-install: Attempting to download using primary link https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.0/aspnetcore-runtime-10.0.0-linux-x64.tar.gz
dotnet-install: Remote file https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.0/aspnetcore-runtime-10.0.0-linux-x64.tar.gz size is 49515713 bytes.
dotnet-install: Extracting archive from https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.0/aspnetcore-runtime-10.0.0-linux-x64.tar.gz
dotnet-install: Downloaded file size is 49515713 bytes.
dotnet-install: The remote and local file sizes are equal.
dotnet-install: Installed version is 10.0.0
dotnet-install: Binaries of dotnet can be found in /usr/share/dotnet
dotnet-install: Note that the script does not resolve dependencies during installation.
dotnet-install: To check the list of dependencies, go to https://learn.microsoft.com/dotnet/core/install, select your operating system and check the "Dependencies" section.
dotnet-install: Installation finished successfully.
-> Create system account 'uzonmail' with runtime home /var/www/uzonmail/data
   useradd --system --user-group --home-dir /var/www/uzonmail/data --no-create-home --shell /usr/sbin/nologin uzonmail
-> Create installation staging directory /var/www/uzonmail.installing
   install -d -m 0755 -o root -g root /var/www/uzonmail.installing
-> Stage UzonMail 0.23.6.0
   cp -a /tmp/uzonmail-installer-x4e68o2t/extracted/0.23.6.0/service-linux-x64/. /var/www/uzonmail.installing
-> Stage production configuration at /var/www/uzonmail.installing/appsettings.Production.json
   install -m 0640 -o root -g uzonmail /tmp/uzonmail-installer-x4e68o2t/appsettings.Production.json /var/www/uzonmail.installing/appsettings.Production.json
-> Validate persistent tree /var/www/uzonmail.installing/data
   find /var/www/uzonmail.installing/data -xdev -printf '%D:%y\0'
-> Assign release files under /var/www/uzonmail.installing to root
   chown -R --no-dereference root:root /var/www/uzonmail.installing
-> Remove non-root write access under /var/www/uzonmail.installing
   chmod -R u=rwX,go=rX /var/www/uzonmail.installing
-> Prepare writable directory /var/www/uzonmail.installing/data
   install -d -m 0750 -o uzonmail -g uzonmail /var/www/uzonmail.installing/data
-> Assign /var/www/uzonmail.installing/data to the service account
   find /var/www/uzonmail.installing/data -xdev -exec chown --no-dereference uzonmail:uzonmail '{}' +
-> Restrict runtime data permissions under /var/www/uzonmail.installing/data
   find /var/www/uzonmail.installing/data -xdev -exec chmod u=rwX,g=rX,o= '{}' +
-> Prepare writable directory /var/www/uzonmail.installing/logs
   install -d -m 0750 -o uzonmail -g uzonmail /var/www/uzonmail.installing/logs
-> Assign /var/www/uzonmail.installing/logs to the service account
   find /var/www/uzonmail.installing/logs -xdev -exec chown --no-dereference uzonmail:uzonmail '{}' +
-> Restrict runtime data permissions under /var/www/uzonmail.installing/logs
   find /var/www/uzonmail.installing/logs -xdev -exec chmod u=rwX,g=rX,o= '{}' +
-> Allow the service to update /var/www/uzonmail.installing/wwwroot/app.config.json
   chown uzonmail:uzonmail /var/www/uzonmail.installing/wwwroot/app.config.json
-> Restrict permissions on /var/www/uzonmail.installing/wwwroot/app.config.json
   chmod 0640 /var/www/uzonmail.installing/wwwroot/app.config.json
-> Assign secure ownership to /var/www/uzonmail.installing/appsettings.Production.json
   chown root:uzonmail /var/www/uzonmail.installing/appsettings.Production.json
-> Restrict permissions on /var/www/uzonmail.installing/appsettings.Production.json
   chmod 0640 /var/www/uzonmail.installing/appsettings.Production.json
-> Activate installation at /var/www/uzonmail
   mv /var/www/uzonmail.installing /var/www/uzonmail
-> Register systemd unit /etc/systemd/system/uzon-mail.service
   install -m 0644 -o root -g root /tmp/uzonmail-installer-x4e68o2t/uzon-mail.service /etc/systemd/system/uzon-mail.service
-> Reload systemd units
   systemctl daemon-reload
-> Enable uzon-mail.service at boot
   systemctl enable uzon-mail.service
Created symlink '/etc/systemd/system/multi-user.target.wants/uzon-mail.service' → '/etc/systemd/system/uzon-mail.service'.
-> Start uzon-mail.service
   systemctl start uzon-mail.service
-> Create installer state directory /var/lib/uzonmail-installer
   install -d -m 0750 -o root -g root /var/lib/uzonmail-installer
-> Record installation state at /var/lib/uzonmail-installer/install-state.json
   install -m 0640 -o root -g root /tmp/uzonmail-installer-x4e68o2t/install-state.json /var/lib/uzonmail-installer/install-state.json
```

- Installation complete dialog
  ```bash
  UzonMail 0.23.6.0 was installed successfully.
  ```
- Open Uzonmail on your browser

```bash
Open http://localhost:22345 to continue setup.
```

> or ctrl + click to open on your browser

This document was contributed by [Contractor-x](https://github.com/Contractor-x) for the great work!
