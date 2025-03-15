---
title: Docker
icon: fab fa-docker
order: 5
---

本文将介绍如何使用 Docker Compose 在 Ubuntu 中进行安装，若是其它宿主机或者其它安装方式，可以将本文发给 DeepSeek 让其帮你转换。

请放心，Docker Compose 不是什么新技术，没有难度，它只是一些安装配置的集合，一言概之就是，它能简化你部署流程，跟着以下教程，无脑配置即可。

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
    # 对外暴露端口，方便外部管理
    # 本地端口:容器端口
    # 若本机 3306 已使用，可更换成其它端口，例如 23306:3306
    ports:
      - 3306:3306
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
      - uzonmail_network

  # redis 缓存, 若要启用 redis 服务，请取消下面的注释
  uzon-redis:
    container_name: uzon-redis
    image: redis:latest
    # 对外暴露端口，方便外部管理
    # 本地端口:容器端口
    # 若本机 6379 已使用，可更换成其它端口，例如 26379:3306
    ports:
      - 6379:6379
    volumes:
      - ./data/redis/data:/data # 数据库数据挂载，防止容器重构后数据丢失
    restart: always
    networks:
      - uzonmail_network

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
    command: [ "dotnet", "UZonMailService.dll" ]
    depends_on:
      - uzon-mysql
      - uzon-redis

networks:
  uzonmail_network:
```

## 安装准备

在启动 docker 前，需要提前进行如下工作：

1. 创建 docker compose 根目录
2. 生成默认配置文件，根据实际情况进行修改

按如下命令进行操作：

``` bash
cd ~
# 创建数据目录
mkdir -p apps/uzon-mail/data
# 进入数据目录
cd apps/uzon-mail
# 生成前端配置文件, baseUrl 见配置章节
echo '{
  "baseUrl": "http:/localhost:22345",
  "api": "/api/v1",
  "signalRHub": "/hubs/uzonMailHub",
  "logger": {
    "level": "info"
  }
}' > data/app.config.json

# 生成后端配置文件
echo '{}' > data/appsettings.Production.json
# 该文件中有一些初始化配置项，建议跳转到 [后端配置] 章节阅读，添加必要项配置，然后继续
# 当然也可以继续配置，后续再修改

# 从 gitee 下载 docker-compose.yml 到当前目录
# 若无法下载，请手动创建文件 ~/apps/uzon-mail/docker-compose.yml，然后将上述 docker-compose 内容复制进去
wget https://gitee.com/uzonmail/UZonMail/raw/master/scripts/docker-compose.yml

# 拉取 docker 镜像
# 此步骤可省略，直接调用下一个命令
docker compose pull

# 创建容器并启动
docker compose up -d
```

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
