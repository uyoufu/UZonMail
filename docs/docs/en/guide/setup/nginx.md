---
title: Nginx
icon: hexagon-nodes
order: 2
description: Example Nginx reverse proxy configuration for UzonMail.
permalink: /en/guide/setup/nginx
---

If you use Nginx as a reverse proxy, consider the following configuration:

``` nginx
upstream uzonmail_upstream {
  # replace with your backend address
  server host:22345;
}

server {
  listen 443 ssl;
  server_name uzon-mail.uzoncloud.com;
  ssl_certificate /etc/nginx/cert/uzoncloud.com.cert;
  ssl_certificate_key /etc/nginx/cert/uzoncloud.com.key;
  ssl_protocols TLSv1.1 TLSv1.2 TLSv1.3;
  ssl_ciphers EECDH+CHACHA20:EECDH+CHACHA20-draft:EECDH+AES128:RSA+AES128:EECDH+AES256:RSA+AES256:EECDH+3DES:!MD5;
  ssl_session_timeout 5m;
  ssl_prefer_server_ciphers on;
  proxy_cache off;
  proxy_buffering off;
  chunked_transfer_encoding on;
  tcp_nopush on;
  tcp_nodelay on;
  keepalive_timeout 600;
  proxy_set_header X-Forwarded-Host $host;
  proxy_set_header X-Forwarded-Server $host;
  proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
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
