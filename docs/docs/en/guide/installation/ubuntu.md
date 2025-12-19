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

## Install runtime

Follow Microsoft's documentation to install .NET on Ubuntu: https://learn.microsoft.com/dotnet/core/install/linux-ubuntu

Install with:

``` bash
sudo apt update && sudo apt install -y aspnetcore-runtime-9.0
```

## Download

Download `uzonmail-service-linux-x64-version.zip` from [Versions](/versions).

Example commands:

``` bash
# install unzip if needed
sudo apt install -y unzip

# download and unzip to ~/uzonmail
cd ~
wget --no-check-certificate https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.10.0.0.zip -O uzonmail.zip
unzip uzonmail.zip -d ./uzonmail
cd ./uzonmail

# run installer
bash ./install.sh
```

## Register service

Run the bundled `install.sh` script to register the service:

``` bash
bash ./install.sh
```

## Firewall

Allow port 22345:

``` bash
sudo ufw allow 22345/tcp
```

At this point the backend should be installed. Visit `http://your-ubuntu-ip:22345` in a browser.

## Modify configuration

For server deployments you may proxy the service to the public Internet. Make related configuration changes in the [Backend Configuration](/guide/setup/) section.

::: warning
If you expose the service publicly, make sure to change default configuration for security!
:::
