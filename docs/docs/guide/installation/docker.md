---
title: Docker
icon: fab fa-docker
order: 5
description: 宇正群邮 Docker 安装教程，详细讲解如何在 Ubuntu 等环境下使用 Docker Compose 部署宇正群邮邮件群发软件。支持开源邮件群发、邮件营销软件，适用于企业和个人，助力高效邮件群发，体验最好用的邮件群发解决方案。
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

## docker-compose

`docker-compose.yml` 文件如下，下面配置时会用到，此时无须阅读，可直接跳转到下一节：

``` yaml
# 
# 说明
# 该文件是 uzon-mail 的 docker-compose 配置文件，使用时，在当前目录执行 docker-compose up -d 命令即可启动程序
#

services:
  # mysql 服务
  uzon-mysql:
    container_name: uzon-mysql
    image: mysql:8.4.0
    # [可选]对外暴露端口，方便外部管理
    # 本地端口:容器端口
    # 若本机 3306 已使用，可更换成其它端口，例如 23306:3306
    # ports:
    #   - 3306:3306
    environment:
      MYSQL_ROOT_PASSWORD: mysqlRoot3306 # root 账号的密码
      MYSQL_DATABASE: uzon-mail # 数据库名
      MYSQL_USER: uzon-mail # 数据库用户名
      MYSQL_PASSWORD: uzon-mail # 数据库密码
    volumes:
      - ./data/mysql/data:/var/lib/mysql # 数据库数据挂载，防止容器重构后数据丢失
    restart: always
    command: mysqld --character-set-server=utf8mb4 --collation-server=utf8mb4_unicode_ci
    # 连接到 uzonmail 主程序网络
    networks:
      - uzon_mysql_network

  # redis 缓存, 若要启用 redis 服务，请取消下面的注释
  uzon-redis:
    container_name: uzon-redis
    image: redis:latest
    # [可选]对外暴露端口，方便外部管理
    # 本地端口:容器端口
    # 若本机 6379 已使用，可更换成其它端口，例如 26379:3306
    # ports:
    #   - 6379:6379
    volumes:
      - ./data/redis/data:/data # 数据库数据挂载，防止容器重构后数据丢失
    restart: always
    networks:
      - uzon_redis_network

  # 程序主体
  uzon-mail:
    container_name: uzon-mail
    image: gmxgalens/uzon-mail:latest
    ports:
      - 22345:22345
    volumes:
      - ./data/appsettings.Production.json:/app/appsettings.Production.json # 生产环境配置
      - ./data/data:/app/data # 数据存储
      - ./data/app.config.json:/app/wwwroot/app.config.json # 前端配置
    networks:
      - uzonmail_network
      - uzon_mysql_network
      - uzon_redis_network
    command: [ "dotnet", "UZonMailService.dll" ]
    depends_on:
      - uzon-mysql
      - uzon-redis

networks:
  uzon_mysql_network:
  uzon_redis_network:
  uzonmail_network:
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

### 下载 docker-compose 文件

从 github 上下载 [docker-compose.yml](https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/scripts/docker-compose.yml) 到当前目录

若无法下载，请手动创建文件 ~/apps/uzon-mail/docker-compose.yml，然后将上述 docker-compose 内容复制进去

``` bash
# 确保在 ~/apps/uzon-mail 目录中，然后执行下列命令
wget https://raw.githubusercontent.com/uyoufu/UZonMail/refs/heads/master/scripts/docker-compose.yml
```

上述命令将会下载完整的 docker-compose 文件，具体的配置项说明请见文件内容。

在文件里，你可以修改数据库的连接密码、可以将端口暴露到宿主机中进行管理。

### 生成配置

为了方便对服务器进行配置，需要在外部创建相应的挂载文件。为了使用安全，有一些参数必须在配置进行修改。

``` bash
# 此处不再需要前端配置

# 生成后端配置文件
# 该文件中有一些初始化配置项，建议跳转到 [后端配置] 章节阅读，添加必要项配置，然后继续
# 当然也可以继续配置，后续再修改
echo '{
  // 改为实际的
  "BaseUrl": "http://localhost:22345",
  // Secret 必须修改，防止被其它人伪装登陆
  "TokenParams": {
    "Secret": "640807f8983090349cca90b9640807f8983090349cca90b9",
    "Issuer": "127.0.0.1",
    "Audience": "UZonMail",
    "Expire": 86400000
  },
  "User": {
    // 每个用户在服务器的文件缓存位置，可以不修改
    "CachePath": "users/{0}",
    // 管理员用户名和密码, 只在第一次启动时初始化
    "AdminUser": {
      "UserId": "admin",
      "Password": "admin1234",
      "Avatar": ""
    },
    // 新建用户时的默认密码
    "DefaultPassword": "uzonmail123"
  },
  // 数据库设置
  // 将 Enable 设置为 true, 启用对应的数据库
  // 程序优化使用 mysql
  "Database": {
    // 免安装的数据库，系统默认使用这个
    "SqLite": {
      "Enable": false,
      "DataSource": "data/db/uzon-mail.db"
    },
    // 对于高并发场景，建议使用 mysql
    "MySql": {
      "Enable": true,
      "Version": "8.4.0.0",
      "Host": "uzon-mysql",
      "Port": 3306,
      "Database": "uzon-mail",
      "User": "uzon-mail",
      "Password": "uzon-mail",
      "Description": "程序会优先使用 mysql"
    },
    // 缓存数据库
    // 默认使用内存缓存
    "Redis": {
      "Enable": true,
      "Host": "uzon-redis",
      "Port": 6379,
      "Password": "",
      "Database": 0
    }
  },
}' > data/appsettings.Production.json
```

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
Unhandled exception. System.InvalidOperationException: An exception has been raised that is likely due to a transient failure. Consider enabling transient error resiliency by adding 'EnableRetryOnFailure()' to the 'UseMySql' call.
 ---> MySqlConnector.MySqlException (0x80004005): Unable to connect to any of the specified MySQL hosts.
```

表示无法连接 mysql，可能是 mysql 还未完全启动成功，可以使用 `docker restart uzon-mail` 重新启动一下主服务。

当日志中显示如下内容时，表示启动成功了

![image-20251018103117215](https://oss.uzoncloud.com:2234/public/files/images/image-20251018103117215.png)

## 防火墙放行

放行端口

``` bash
sudo ufw allow 22345/tcp
```

## 修改配置

到这一步，Docker 服务就创建完成了，由于是服务器部署，您可能会将其代理到公网，因此还有一些必要的配置进行修改，请继续阅读 [后端配置](/guide/setup/) 章节。

::: warning
若代理到公网，请务必修改默认配置，否则服务将会不安全！
:::

## 软件更新

当需要更新容器时，只需要进入到 ~/apps/uzon-mail(docker-compose.yml 所在目录)，然后执行命令 `docker compose up -d` 即可

## 网址访问

访问 `http://your-docker-host-ip:22345` 登陆使用。
