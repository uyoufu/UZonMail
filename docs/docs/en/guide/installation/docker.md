---
title: Docker
icon: fab fa-docker
order: 5
description: Guide to deploy UzonMail using Docker Compose (example uses Ubuntu). Covers setup, configuration and troubleshooting.
permalink: /en/guide/installation/docker
---

This guide shows how to install UzonMail on Ubuntu using Docker Compose. For other hosts or deployment methods, you can adapt the steps accordingly.

Docker Compose simplifies deployment by bundling configuration. Follow the steps below for an easy setup.

## Video Tutorial

::: info

The docs have been streamlined; if video and text differ, follow the text here.

:::

<BiliBili bvid="BV1JLJqziEd5" />

## Connect to Ubuntu

SSH to your Ubuntu host. On Windows you can use PowerShell:

``` powershell
# Run and enter password when prompted
ssh username@ip
```

## docker-compose

The example `docker-compose.yml` is shown below. You don't need to read it now; it will be used in the steps.

``` yaml
<...snipped example docker-compose.yml in original...>
```

## Installation Steps

### Create data directory

Before starting Docker create the required directories (skip if already present):

``` bash
cd ~
# 1. create data directory
mkdir -p apps/uzon-mail/data
# 2. enter the data directory
cd apps/uzon-mail
```

### Download docker-compose file

Download the `docker-compose.yml` from GitHub:

``` bash
wget https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/scripts/docker-compose.yml
```

If you cannot download, create `~/apps/uzon-mail/docker-compose.yml` and paste the content from the repository.

You can modify database credentials and exposed ports in the compose file as needed.

### Generate configuration

Create mounted configuration files externally. Some parameters must be customized for security.

``` bash
echo '{
  // replace with your values
  "BaseUrl": "http://localhost:22345",
  "TokenParams": {
    "Secret": "640807f8983090349cca90b9640807f8983090349cca90b9",
    "Issuer": "127.0.0.1",
    "Audience": "UZonMail",
    "Expire": 86400000
  },
  "User": {
    "CachePath": "users/{0}",
    "AdminUser": {
      "UserId": "admin",
      "Password": "admin1234",
      "Avatar": ""
    },
    "DefaultPassword": "uzonmail123"
  },
  "Database": {
    "SqLite": {
      "Enable": false,
      "DataSource": "data/db/uzon-mail.db"
    },
    "MySql": {
      "Enable": true,
      "Version": "8.4.0.0",
      "Host": "uzon-mysql",
      "Port": 3306,
      "Database": "uzon-mail",
      "User": "uzon-mail",
      "Password": "uzon-mail",
      "Description": "MySQL is preferred for production"
    },
    "Redis": {
      "Enable": true,
      "Host": "uzon-redis",
      "Port": 6379,
      "Password": "",
      "Database": 0
    }
  }
}' > data/appsettings.Production.json
```

Refer to the Backend Configuration section for additional settings.

### Start

Pull images and start containers:

``` bash
# option: pull images first
docker compose pull

# create and start containers
docker compose up -d

# verify: should return HTML when ready
curl http://localhost:22345
```

### Troubleshooting

**Couldn't connect to server**

If `curl http://localhost:22345` fails, check logs:

``` bash
docker logs uzon-mail
```

If logs show MySQL connection errors (e.g. Unable to connect to any of the specified MySQL hosts) then MySQL may not have finished starting. Try `docker restart uzon-mail` after MySQL is ready.

Successful startup shows the web UI content.

## Firewall

Allow port 22345:

``` bash
sudo ufw allow 22345/tcp
```

## Modify configuration

For server deployments you may proxy the service to the public Internet. Make related configuration changes in the [Backend Configuration](/guide/setup/) section.

::: warning
If you expose the service publicly, make sure to change default configuration for security!
:::

## Update

To update the container, run `docker compose up -d` from `~/apps/uzon-mail`.

## Access

Visit `http://your-docker-host-ip:22345` to log in.
