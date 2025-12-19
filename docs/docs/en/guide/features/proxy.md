---
title: Using Proxies
icon: globe
order: 3
description: UzonMail supports static and dynamic proxies, compatible with http, https, socks4, socks5 protocols to aid efficient bulk sending.
permalink: /en/guide/features/proxy
---

Both static and dynamic proxies are supported during sending.

## Video Tutorial

<BiliBili bvid="BV1JLJqziEd5" />

## Steps

1. Add proxies in **Proxy Management**

   ![proxy management](https://oss.uzoncloud.com:2234/public/files/images/image-20250323172703801.png)

2. In Basic Settings configure the max sends per proxy per run

   ![proxy settings](https://oss.uzoncloud.com:2234/public/files/images/image-20250816161516396.png)

3. On the New Send page, select the proxy to use

   ![select proxy](https://oss.uzoncloud.com:2234/public/files/images/image-20250323172154711.png)

## Proxy Formats

### Static Proxies

Format: `schema://username:password@host:port`

- `schema` is the protocol: http, https, socks4, socks5
- `username` is the proxy username
- `password` is the proxy password

If no username/password, use:

`schema://host:port`

### Dynamic Proxies

Dynamic proxy formats vary by provider. When obtaining a dynamic proxy URL, make sure to select **JSON** format.

::: tip

Dynamic proxies are supported only in Professional edition or above.

:::

Supported dynamic proxy providers (not ranked):

| # | Provider | Protocols | Notes | Example URL |
| - | -------- | --------- | ----- | ----------- |
| 1 | [易代理](http://www.ydaili.cn//main/register.aspx?str_code=80TL8T6X) | http/socks5 | — | `http://api1.ydaili.cn/tools/MeasureApi.ashx?action=EAPI&secret=YourSecret&number=10&orderId=YourOrderID&format=json` |
| 2 | [ip2world](https://www.ip2world.com/?ref=Y2NFJBM3CP) | http/socks5 | Foreign proxies, not for domestic China use | `http://api.proxy.ip2world.com/getProxyIp?lb=4&return_type=json&protocol=https&num=2` |
| 3 | [ipfox](https://referral.ipfoxy.com/EpH8pH) | http/socks5 | — | `https://api.ipfoxy.com/ip/dynamic-api/ips?count=1&host=gate-sg.ipfoxy.io&port=58688&type=json&token=YourToken&period=1` |
| 4 | [IPIDEA](https://share.ipidea.net/uzonmail) | http/socks5 | Foreign proxies | `http://api.proxy.ipidea.io/getBalanceProxyIp?num=100&return_type=json&lb=4&sb=0&flow=1&regions=&protocol=socks5` |
