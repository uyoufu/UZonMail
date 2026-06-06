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

### Quick install

With quick install, you can skip the later manual steps for downloading `.env`, downloading `docker-compose.yml`, and starting the service. The installer downloads the required files from GitHub, guides you through key configuration, and starts the service after confirmation.

Make sure Docker and Docker Compose v2 are installed, and that you are in `~/apps/uzon-mail`, then run:

``` bash
curl -fsSL https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/install.sh -o install.sh
bash ./install.sh
```

After quick install, continue reading the configuration notes and security warning to confirm the public access URL, initial admin password, Token Secret, and related settings match your deployment environment.

### Download .env file

Download [.env](https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/.env) from GitHub to the current directory.

If the download fails, manually create `~/apps/uzon-mail/.env` and paste the `.env` content from the link above.

``` bash
# Make sure you are in ~/apps/uzon-mail, then run:
wget https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/.env
```

### Download docker-compose file

Download the `docker-compose.yml` from GitHub:

``` bash
wget https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/docker-compose.yml
```

If you cannot download, create `~/apps/uzon-mail/docker-compose.yml` and paste the content from the repository.

The command downloads the complete Docker Compose file. See the file content for detailed configuration comments.

### Modify .env configuration

Docker deployment injects backend configuration through `.env` by default, so you do not need to generate `appsettings.Production.json`. For security, check at least these settings before startup:

- Change `BaseUrl` to the actual access URL, such as `https://mail.example.com`.
- Change `TokenParams__Secret` to prevent forged login tokens.
- Change `User__AdminUser__Password`. This value only takes effect when the admin user is initialized for the first time.
- If you change the exposed port, keep `UZON_MAIL_HOST_PORT` consistent with the port in `BaseUrl`.

`COMPOSE_PROFILES` in `.env` controls whether built-in services are started:

``` env
# Start both built-in PostgreSQL and Redis
COMPOSE_PROFILES=postgresql,redis

# Start built-in PostgreSQL only, without built-in Redis
COMPOSE_PROFILES=postgresql

# Start only the uzon-mail app; PostgreSQL and Redis use external services or are disabled
COMPOSE_PROFILES=
```

If you already have a PostgreSQL database, remove `postgresql` from `COMPOSE_PROFILES`, then set `Database__PostgreSql__Host`, `Database__PostgreSql__Port`, `Database__PostgreSql__Database`, `Database__PostgreSql__User`, and `Database__PostgreSql__Password` in `.env` to the existing database connection details. Make sure the `uzon-mail` container can reach that database from its Docker network.

If you do not need the built-in Redis service, remove `redis` from `COMPOSE_PROFILES`. If you do not use Redis at all, set `Database__Redis__Enable=false`. If you use an external Redis service, keep `Database__Redis__Enable=true`, then update `Database__Redis__Host`, `Database__Redis__Port`, `Database__Redis__Password`, and `Database__Redis__Database`.

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

If logs show PostgreSQL connection errors, the database may still be starting, or the Host, port, username, password, or database name may be incorrect for an existing PostgreSQL service. Check the built-in database with `docker logs uzon-postgres`, then run `docker restart uzon-mail` after PostgreSQL is ready.

Successful startup shows the web UI content.

## Firewall

Allow port 22345:

``` bash
sudo ufw allow 22345/tcp
```

## Modify configuration

There are two ways to modify configuration:

1. Modify configuration through the `.env` file.
2. Mount `appsettings.Production.json` from the container to the host, modify the host file, then restart the container.

### .env file method

Docker Compose injects `.env` into the `uzon-mail` container, and the backend reads these values using ASP.NET Core environment variable rules. To override a backend setting, find the field in the default configuration and convert the JSON path to a `.env` variable name.

The naming rules are:

- Root-level settings use the field name directly, such as `BaseUrl=http://localhost:22345`.
- Nested settings use double underscores `__`, such as `TokenParams.Secret` as `TokenParams__Secret`.
- Deeper paths continue joining each level, such as `Database.PostgreSql.Host` as `Database__PostgreSql__Host`.
- Array settings use zero-based indexes, such as `Cors[0]` as `Cors__0`, and `Unsubscribe.Headers[0].Domain` as `Unsubscribe__Headers__0__Domain`.

Examples:

``` env
# Root-level setting
BaseUrl=https://mail.example.com

# Nested settings
TokenParams__Secret=replace-with-a-long-random-secret
Database__PostgreSql__Host=uzon-postgres

# Array settings
Cors__0=https://mail.example.com
Unsubscribe__Headers__0__Domain=gmail.com
Unsubscribe__Headers__0__Header=RFC8058
```

The `.env` file also contains variables used by Docker Compose itself, such as `COMPOSE_PROFILES`, `UZON_MAIL_HOST_PORT`, and `UZON_MAIL_IMAGE`. These control container startup, image names, and exposed ports. They are not backend `appsettings.json` paths.

To view all backend settings that can be overridden, see the default configuration file:

<https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/backend-src/UZonMailService/appsettings.json>

After modifying `.env`, restart containers from the directory containing `docker-compose.yml`:

``` bash
docker compose up -d
```

### appsettings.Production.json file method

If you prefer a mounted production configuration file, mount `appsettings.Production.json` to the host, modify the host file, and restart the container. For Docker deployments, the `.env` method is the default and is usually simpler.

For server deployments you may proxy the service to the public Internet. Make related configuration changes in the [Backend Configuration](/guide/setup/) section.

::: warning
If you expose the service publicly, make sure to change default configuration for security!
:::

## Update

To update the container, run `docker compose up -d` from `~/apps/uzon-mail`.

## Access

Visit `http://your-docker-host-ip:22345` to log in.
