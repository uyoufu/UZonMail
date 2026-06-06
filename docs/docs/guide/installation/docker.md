---
title: Docker
icon: fab fa-docker
order: 5
description: 宇正群邮 Docker 安装教程，详细讲解如何在 Ubuntu 等环境下使用 Docker Compose 部署宇正群邮邮件群发软件。支持开源邮件群发、邮件营销软件，适用于企业和个人，助力高效邮件群发，体验最好用的邮件群发解决方案。
permalink: /guide/installation/docker
---

本文将介绍如何使用 Docker Compose 在 Ubuntu 中进行安装，若是其它宿主机或者其它安装方式，可以将本文发给 DeepSeek 让其帮你转换。

请放心，Docker Compose 不是什么新技术，没有难度，它只是一些安装配置的集合，一言概之就是，它能简化你部署流程，跟着以下教程，无脑配置即可。

## 视频教程

::: info

部署文档配置有精简，视频与当前文档不一致时，以当前文档为准

:::

<BiliBili bvid="BV1JLJqziEd5" />

## 连接到 Ubuntu

使用 ssh 连接到 Ubuntu 上，在 windows 中可以直接打开 PowerShell 使用。

命令如下：

``` powershell
# 执行后根据提示输入密码
ssh username@ip
```

## 安装步骤

### 创建数据目录

在启动 docker 前，需要提前创建需要的工作目录(若有，可忽略)，

按如下命令进行操作：

``` bash
cd ~
# 1. 创建数据目录
mkdir -p apps/uzon-mail/data
# 2. 进入数据目录
cd apps/uzon-mail
```

### 快捷安装

使用快捷安装时，可忽略本节后续下载 `.env`、下载 `docker-compose.yml` 和启动服务的手动步骤。安装脚本会从 GitHub 下载所需文件，引导填写关键配置，并在确认后启动服务。

请先确保已经安装 Docker 和 Docker Compose v2，并位于 `~/apps/uzon-mail` 目录中，然后执行：

``` bash
curl -fsSL https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/install.sh -o install.sh
bash ./install.sh
```

快捷安装完成后，建议继续阅读后续配置说明和安全提示，确认公网访问地址、管理员初始密码和 Token Secret 等配置符合实际部署环境。

### 下载 .env 配置文件

从 github 上下载 [.env](https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/.env) 到当前目录

若无法下载，请手动创建文件 `~/apps/uzon-mail/.env`，然后将上述 `.env` 内容复制进去。

``` bash
# 确保在 ~/apps/uzon-mail 目录中，然后执行下列命令
wget https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/.env
```

### 下载 docker-compose 文件

从 github 上下载 [docker-compose.yml](https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/docker-compose.yml) 到当前目录

若无法下载，请手动创建文件 ~/apps/uzon-mail/docker-compose.yml，然后将上述 docker-compose 内容复制进去

``` bash
# 确保在 ~/apps/uzon-mail 目录中，然后执行下列命令
wget https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/docker/docker-compose.yml
```

上述命令将会下载完整的 docker-compose 文件，具体的配置项说明请见文件内容。

### 修改 .env 配置

Docker 部署默认通过 `.env` 注入后端配置，无需再生成 `appsettings.Production.json`。为了使用安全，启动前至少需要检查以下配置：

- 将 `BaseUrl` 改为实际访问地址，例如 `https://mail.example.com`。
- 修改 `TokenParams__Secret`，防止其他人伪造登录状态。
- 修改 `User__AdminUser__Password`，该配置只在首次初始化管理员时生效。
- 如需修改访问端口，保持 `UZON_MAIL_HOST_PORT` 与 `BaseUrl` 中的端口一致。

`.env` 中的 `COMPOSE_PROFILES` 用于控制是否启动内置服务：

``` env
# 默认同时启动内置 PostgreSQL 和 Redis
COMPOSE_PROFILES=postgresql,redis

# 只启动内置 PostgreSQL，不启动内置 Redis
COMPOSE_PROFILES=postgresql

# 只启动 uzon-mail 主程序，PostgreSQL 和 Redis 都使用外部服务或关闭
COMPOSE_PROFILES=
```

若已经有可用的 PostgreSQL 数据库，将 `COMPOSE_PROFILES` 中的 `postgresql` 删除，并把 `.env` 中 `Database__PostgreSql__Host`、`Database__PostgreSql__Port`、`Database__PostgreSql__Database`、`Database__PostgreSql__User` 和 `Database__PostgreSql__Password` 改为已有数据库的连接信息。请确保 `uzon-mail` 容器所在网络能够访问该数据库。

若不需要内置 Redis，将 `COMPOSE_PROFILES` 中的 `redis` 删除。若完全不使用 Redis，请把 `Database__Redis__Enable=false`；若使用外部 Redis，请继续保持 `Database__Redis__Enable=true`，并修改 `Database__Redis__Host`、`Database__Redis__Port`、`Database__Redis__Password` 和 `Database__Redis__Database`。

### 启动

使用如下命令拉取更新并启动容器：

``` bash
# 拉取 docker 镜像
# 此步骤可省略，直接调用下一个命令
docker compose pull

# 创建容器并启动
docker compose up -d

# 检查是否启动成功：出现 html 内容时，表示成功
# 否则按下一节进行问题排查
curl http://localhost:22345
```

### 问题排查

**Couldn't connect to server**

当使用 `curl http://localhost:22345` 报如下错误时：

``` text
[root@VM-16-3-opencloudos uzon-mail]# curl http://localhost:22345
curl: (7) Failed to connect to localhost port 22345 after 0 ms: Couldn't connect to server
```

表示 uzon-mail 服务启动失败了，可以通过 `docker logs uzon-mail` 查看失败日志。

当日志中包含类似日志时：

``` tex
Unhandled exception. Npgsql.NpgsqlException (0x80004005): Failed to connect to 127.0.0.1:5432
```

表示无法连接 PostgreSQL，可能是数据库还未完全启动成功，或者已有数据库的 Host、端口、账号密码配置不正确。可以先使用 `docker logs uzon-postgres` 查看内置数据库状态，再使用 `docker restart uzon-mail` 重新启动一下主服务。

当日志中显示如下内容时，表示启动成功了

![image-20251018103117215](https://oss.uzoncloud.com:2234/public/files/images/image-20251018103117215.png)

## 防火墙放行

放行端口

``` bash
sudo ufw allow 22345/tcp
```

## 修改配置

修改配置有两种方式：

1. 通过 `.env` 文件修改配置
2. 将容器中的 `appsettings.Production.json` 文件挂载到主机，修改主机上的文件，然后重启容器

### .env 文件方式

Docker Compose 会把 `.env` 注入到 `uzon-mail` 容器中，后端会按 ASP.NET Core 环境变量规则读取这些配置。需要覆盖后端设置时，先在默认配置中找到对应字段，再把 JSON 路径转换成 `.env` 变量名。

命名方式如下：

- 根级配置直接使用字段名，例如 `BaseUrl=http://localhost:22345`。
- 嵌套配置用两个下划线 `__` 连接，例如 `TokenParams.Secret` 写成 `TokenParams__Secret`。
- 多级嵌套继续按层级连接，例如 `Database.PostgreSql.Host` 写成 `Database__PostgreSql__Host`。
- 数组配置用从 `0` 开始的索引，例如 `Cors[0]` 写成 `Cors__0`，`Unsubscribe.Headers[0].Domain` 写成 `Unsubscribe__Headers__0__Domain`。

示例如下：

``` env
# 根级配置
BaseUrl=https://mail.example.com

# 嵌套配置
TokenParams__Secret=replace-with-a-long-random-secret
Database__PostgreSql__Host=uzon-postgres

# 数组配置
Cors__0=https://mail.example.com
Unsubscribe__Headers__0__Domain=gmail.com
Unsubscribe__Headers__0__Header=RFC8058
```

`.env` 中也包含一些 Docker Compose 自身使用的变量，例如 `COMPOSE_PROFILES`、`UZON_MAIL_HOST_PORT`、`UZON_MAIL_IMAGE`。这些变量用于控制容器启动、镜像和端口，不属于后端 `appsettings.json` 的配置路径。

需要查看所有可覆盖的后端设置时，可以参考默认配置文件：

<https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/backend-src/UZonMailService/appsettings.json>

修改 `.env` 后，在 `docker-compose.yml` 所在目录重新启动容器即可生效：

``` bash
docker compose up -d
```

### appsettings.Production.json 文件方式

到这一步，Docker 服务就创建完成了，由于是服务器部署，您可能会将其代理到公网，因此还有一些必要的配置进行修改，请继续阅读 [后端配置](/guide/setup/) 章节。

::: warning
若代理到公网，请务必修改默认配置，否则服务将会不安全！
:::

## 软件更新

当需要更新容器时，只需要进入到 ~/apps/uzon-mail(docker-compose.yml 所在目录)，然后执行命令 `docker compose up -d` 即可

## 网址访问

访问 `http://your-docker-host-ip:22345` 登陆使用。

## docker 相关文件

此章节仅在需要进行深度配置时阅读

### .env 文件

可以在 `.env` 文件中配置环境变量，然后在 `docker-compose.yml` 中使用形如 &#36;{变量名} 的方式引用。常用配置如下：

``` env
# 内置服务开关：默认启动 PostgreSQL 和 Redis；删除对应名称即可关闭内置服务
COMPOSE_PROFILES=postgresql,redis

# 后端访问地址与监听端口
BaseUrl=http://localhost:22345
UZON_MAIL_HOST_PORT=22345
Http__Port=22345

# Token 配置，公网部署前必须修改 Secret
TokenParams__Secret=B81806DA00600865988B2A305B91C47825750972A0A7159CCDC63A9838248D77

# PostgreSQL 配置，同时用于初始化内置 PostgreSQL 容器
Database__PostgreSql__Enable=true
Database__PostgreSql__Host=uzon-postgres
Database__PostgreSql__Port=5432
Database__PostgreSql__Database=uzon-mail
Database__PostgreSql__User=uzon-mail
Database__PostgreSql__Password=uzon-mail

# Redis 配置，内置 Redis 默认不设置密码
Database__Redis__Enable=true
Database__Redis__Host=uzon-redis
Database__Redis__Port=6379
Database__Redis__Password=
Database__Redis__Database=0
```

### docker-compose 文件

`docker-compose.yml` 文件如下，下面配置时会用到，此时无须阅读，可直接跳转到下一节：

``` yaml
# 
# 说明
# 该文件是 uzon-mail 的 docker-compose 配置文件，使用时，在当前目录执行 docker-compose up -d 命令即可启动程序
#

services:
  # PostgreSQL 服务
  uzon-postgres:
    container_name: ${POSTGRES_CONTAINER_NAME}
    image: ${POSTGRES_IMAGE}
    # 通过 .env 中的 COMPOSE_PROFILES 控制是否启动内置 PostgreSQL
    profiles:
      - postgresql
    # [可选]对外暴露端口，方便外部管理
    # 本地端口:容器端口
    # 若本机 5432 已使用，可更换成其它端口，例如 25432:5432
    # ports:
    #   - ${POSTGRES_HOST_PORT}:5432
    environment:
      POSTGRES_DB: ${Database__PostgreSql__Database} # 数据库名
      POSTGRES_USER: ${Database__PostgreSql__User} # 数据库用户名
      POSTGRES_PASSWORD: ${Database__PostgreSql__Password} # 数据库密码
    volumes:
      - ${POSTGRES_DATA_DIR}:/var/lib/postgresql # 数据库数据挂载，防止容器重构后数据丢失
    restart: always
    healthcheck:
      test: [ "CMD-SHELL", "pg_isready -U ${Database__PostgreSql__User} -d ${Database__PostgreSql__Database}" ]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 10s
    # 连接到 uzonmail 主程序网络
    networks:
      - uzon_postgres_network

  # redis 缓存
  uzon-redis:
    container_name: ${REDIS_CONTAINER_NAME}
    image: ${REDIS_IMAGE}
    # 通过 .env 中的 COMPOSE_PROFILES 控制是否启动内置 Redis
    profiles:
      - redis
    # [可选]对外暴露端口，方便外部管理
    # 本地端口:容器端口
    # 若本机 6379 已使用，可更换成其它端口，例如 26379:6379
    # ports:
    #   - ${REDIS_HOST_PORT}:6379
    volumes:
      - ${REDIS_DATA_DIR}:/data # 数据库数据挂载，防止容器重构后数据丢失
    restart: always
    healthcheck:
      test: [ "CMD", "redis-cli", "ping" ]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 5s
    networks:
      - uzon_redis_network

  # 程序主体
  uzon-mail:
    container_name: ${UZON_MAIL_CONTAINER_NAME}
    image: ${UZON_MAIL_IMAGE}
    env_file:
      - .env # 注入 ASP.NET Core 层级配置
    ports:
      - ${UZON_MAIL_HOST_PORT}:${Http__Port}
    volumes:
      - ${UZON_MAIL_DATA_DIR}:/app/data # 数据存储
    # - ./data/app.config.json:/app/wwwroot/app.config.json # 前端配置, 可选
    networks:
      - uzonmail_network
      - uzon_postgres_network
      - uzon_redis_network
    command: [ "dotnet", "UZonMailService.dll" ]
    depends_on:
      uzon-postgres:
        condition: service_healthy
        # 允许用户关闭内置 PostgreSQL 后连接外部数据库
        required: false
      uzon-redis:
        condition: service_healthy
        # 允许用户关闭内置 Redis 后使用内存缓存或外部 Redis
        required: false

networks:
  uzon_postgres_network:
  uzon_redis_network:
  uzonmail_network:
```
