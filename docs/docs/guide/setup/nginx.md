---
title: Nginx
icon: hexagon-nodes
order: 2
description: 宇正群邮邮件群发软件 Nginx 反向代理配置教程，详细介绍如何通过 Nginx 部署宇正群邮，实现高效邮件群发、邮件营销。支持开源邮件群发，助力企业和个人体验最好用的邮件群发软件。
---

若使用 nginx 进行反向代理，可以参考如下配置：

``` nginx
upstream uzonmail_upstream {
  # 此处修改成实际地址
  server host:22345;
}

server {
  listen 443 ssl;
  # 实际域名
  server_name uzon-mail.uzoncloud.com;
  # ssl
  ssl_certificate /etc/nginx/cert/uzoncloud.com.cert;
  ssl_certificate_key /etc/nginx/cert/uzoncloud.com.key;
  ssl_protocols TLSv1.1 TLSv1.2 TLSv1.3;
  ssl_ciphers EECDH+CHACHA20:EECDH+CHACHA20-draft:EECDH+AES128:RSA+AES128:EECDH+AES256:RSA+AES256:EECDH+3DES:!MD5;
  ssl_session_timeout 5m;
  ssl_prefer_server_ciphers on;
  # 不缓存，支持流式输出
  proxy_cache off;
  # 关闭缓存
  proxy_buffering off;
  # 关闭代理缓冲
  chunked_transfer_encoding on;
  # 开启分块传输编码
  tcp_nopush on;
  # 开启TCP NOPUSH选项，禁止Nagle算法
  tcp_nodelay on;
  # 开启TCP NODELAY选项，禁止延迟ACK算法
  keepalive_timeout 600;
  # 设定keep-alive超时时间为600秒
  # 头信息
  proxy_set_header X-Forwarded-Host $host;
  proxy_set_header X-Forwarded-Server $host;
  proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
  # location请求映射规则，/ 代表一切请求路径
  location / {
    proxy_connect_timeout 600;
    proxy_read_timeout 600;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_pass http://uzonmail_upstream;
  }
}
```