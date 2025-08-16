---
title: 使用代理
icon: globe
order: 3
description: 宇正群邮（UZonMail）邮件群发软件支持固定代理和动态代理，兼容 http、https、socks4、socks5 协议，助力高效邮件群发和邮件营销。开源邮件群发，适用于企业级邮件群发场景，是最好用的邮件群发软件。
---

在进行发件时，支持固定代理和动态代理。

## 视频教程

<BiliBili bvid="BV1JLJqziEd5" />

## 使用步骤

1. 在【代理管理】中添加代理

   ![image-20250323172703801](https://oss.uzoncloud.com:2234/public/files/images/image-20250323172703801.png)

2. 在基础设置中设置单个代理每次可发件总数

   ![image-20250816161516396](https://oss.uzoncloud.com:2234/public/files/images/image-20250816161516396.png)

3. 在新建发件页面，选择需要使用的代理

   ![image-20250323172154711](https://oss.uzoncloud.com:2234/public/files/images/image-20250323172154711.png)

## 代理格式

### 固态代理

代理的格式为：`schema://username:password@host:port`

- schema 为协议，支持代理协议有 http、https、socks4、socks5
- username 为代理的用户名
- password 为代理的密码

若没有用户名密码，代理格式为：

`schema://host:port`

### 动态代理

动态代理的格式因每个提供商而不同，但需要注意的时，在获取动态代理的 url 时，必须选择 **json** 格式。

::: tip

动态代理只有专业版本以上才支持

:::



**支持的动态代理：**

> 以下排名不分先后顺序

| 序号 | 代理                                                         | 支持协议    | 备注                       | 格式示例                                                     |
| ---- | ------------------------------------------------------------ | ----------- | -------------------------- | ------------------------------------------------------------ |
| 1    | [易代理](http://www.ydaili.cn//main/register.aspx?str_code=80TL8T6X) | http/socks5 | 无                         | http://api1.ydaili.cn/tools/MeasureApi.ashx?action=EAPI&secret=YourSecret&number=10&orderId=YourOrderID&format=json |
| 2    | [ip2world](https://www.ip2world.com/?ref=Y2NFJBM3CP)         | http/socks5 | 国外的代理，不支持国内使用 | http://api.proxy.ip2world.com/getProxyIp?lb=4&return_type=json&protocol=https&num=2 |
| 3    | [ipfox](https://referral.ipfoxy.com/EpH8pH)                  | http/socks5 | 无                         | https://api.ipfoxy.com/ip/dynamic-api/ips?count=1&host=gate-sg.ipfoxy.io&port=58688&type=json&token=YourToken&period=1 |
| 4    | [IPIDEA](https://share.ipidea.net/uzonmail)                  | http/socks5 | 国外代理                   | http://api.proxy.ipidea.io/getBalanceProxyIp?num=100&return_type=json&lb=4&sb=0&flow=1&regions=&protocol=socks5 |

