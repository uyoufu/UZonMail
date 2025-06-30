---
title: 使用代理
icon: globe
order: 3
---

在进行发件时，支持固定代理和动态代理。

## 使用步骤

1. 在【代理管理】中添加代理

   ![image-20250323172703801](https://oss.223434.xyz:2234/public/files/images/image-20250323172703801.png)

2. 在基础设置中设置每个发件箱使用单个代理可发件的数量

   ![image-20250323172051104](https://oss.223434.xyz:2234/public/files/images/image-20250323172051104.png)

3. 在新建发件页面，选择需要使用的代理

   ![image-20250323172154711](https://oss.223434.xyz:2234/public/files/images/image-20250323172154711.png)

## 支持的代理

软件支持所有的 http、https、socks4、socks5 协议的代理。

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

