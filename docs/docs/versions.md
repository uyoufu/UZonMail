---
title: 历史版本
editLink: false
description: 本页收录了宇正群邮所有历史发布版本。宇正群邮是一款开源免费的邮件群发软件，支持邮件群发、邮件营销、邮箱爬取、变量替换等功能，兼容所有邮箱账号（含Outlook OAuth2），支持Windows、Linux、MacOS及服务器部署。多线程并发，支持多账号，性能强劲，持续迭代优化，被外贸、教育、财务等行业广泛认可。宇正群邮致力于成为最好用的邮件群发软件，是企业和个人邮件营销的首选开源邮件群发解决方案。
---

## 0.16.2

> 更新时期：2025-08-25

### bug 修复

1. 修复定时任务时区问题导致实际发件时间不准确的bug
2. 修复 JS 变量无法解析的bug

### 下载地址

[uzonmail-desktop-win-x64-0.16.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.2.0.zip)

[uzonmail-service-win-x64-0.16.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.16.2.0.zip)

[uzonmail-service-linux-x64-0.16.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.16.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.1

> 更新时期：2025-08-16

### 新增功能

1. 强化模板和正文编辑器，支持直接拖动添加图片，支持表格、编辑图片等高级功能

### bug 修复

1. 修复模板直接复制 html 内容时，无法保存的 bug
2. 修复服务器时间与UI时间显示不一致问题

### 下载地址

[uzonmail-desktop-win-x64-0.16.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.1.0.zip)

[uzonmail-service-win-x64-0.16.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.16.1.0.zip)

[uzonmail-service-linux-x64-0.16.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.16.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)


## 0.16.0

> 更新时期：2025-08-01

### 新增功能

1. 支持 PostgreSQL 数据库
2. 提升变量解析性能

### bug 修复

1. 修复变量换行后解析错误导致服务崩溃bug

### 下载地址

[uzonmail-desktop-win-x64-0.16.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.0.0.zip)

[uzonmail-service-win-x64-0.16.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.16.0.0.zip)

[uzonmail-service-linux-x64-0.16.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.16.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.3

> 更新时期：2025-07-04

### 新增功能

1. 桌面端限制只能启动一个实例
2. 发件明细中增加 [未发送] 过滤项


### bug 修复

1. 修复桌面端退出后，后台进程未关闭的 bug
2. 修复发件明细里分类过滤数据不正确的bug

### 下载地址

[uzonmail-desktop-win-x64-0.15.3.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.3.0.zip)

[uzonmail-service-win-x64-0.15.3.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.15.3.0.zip)

[uzonmail-service-linux-x64-0.15.3.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.15.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.2

> 更新时期：2025-07-03

### bug 修复

1. 修复 outlook 输入账号密码后, 验证不通过的bug
2. 修复 hotmail 未能匹配 graph 发件的bug

### 下载地址

[uzonmail-desktop-win-x64-0.15.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.2.0.zip)

[uzonmail-service-win-x64-0.15.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.15.2.0.zip)

[uzonmail-service-linux-x64-0.15.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.15.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.1

> 更新时期：2025-06-28

### 新增功能

1. 支持配置 outlook 的 ClientId 进行统一授权发件，需要在后端 `appsettings.Production.json` 添加 `MicrosoftEntraApp`，具体格式如下：

   ``` json
   {
     "MicrosoftEntraApp": {
       "ClientId": "",
       "TenantId": "",
       "ClientSecret": ""
     }
   }
   ```

2. 优化发件箱、收件箱批量上传逻辑，支持邮箱格式异常继续上传

### bug 修复

1. 修复邮件跟踪部分数据未保存的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.15.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.1.0.zip)

[uzonmail-service-win-x64-0.15.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.15.1.0.zip)

[uzonmail-service-linux-x64-0.15.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.15.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.0

> 更新日期：2025-06-21

### 新增功能

1. 支持通过 outlook refresh_token 发件。client_id 填写到用户名栏，token 填写到密码栏。
2. 企业版本增加 api 管理功能
3. 将前端的配置统一到后端进行修改。支持在配置 `appsettings.Production.json` 中添加 `BaseUrl` 来修复前端 `app.config.json` 中的 `baseUrl` 字段。

### bug 修复

1. 修复申请 ssl 证书时，前端无法验证 .well-known 结果的bug 

### 下载地址

[uzonmail-desktop-win-x64-0.15.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.0.0.zip)

[uzonmail-service-win-x64-0.15.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.15.0.0.zip)

[uzonmail-service-linux-x64-0.15.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.15.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.14.2.2

> 更新日期：2025-05-28

### 新增功能

1. 新增 [ipfox](https://referral.ipfoxy.com/EpH8pH) 动态代理支持

### bug 修复

1. 修复专业版本没有变量管理功能的 bug
2. 修复激活码退出后，前端刷新失败的bug

### 下载地址

[uzonmail-desktop-win-x64-0.14.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.2.2.zip)

[uzonmail-service-win-x64-0.14.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.14.2.2.zip)

[uzonmail-service-linux-x64-0.14.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.14.2.2.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)


## 0.14.2

> 更新日期：2025-05-25

### 新增功能

1. 主题和正文支持 js 函数和自定义变量(专业版本)
2. 增加对  [ip2world](https://www.ip2world.com/?ref=Y2NFJBM3CP) 动态代理的支持
3. 新建发件时，支持双击打开弹窗
4. 优化手机端页面样式显示

### bug 修复

1. 修复动态代理测活优先级未生效的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.14.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.2.0.zip)

[uzonmail-service-win-x64-0.14.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.14.2.0.zip)

[uzonmail-service-linux-x64-0.14.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.14.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)


## 0.14.1

> 更新日期：2025-05-15

### 新增功能

1. 桌面端支持关闭到系统托盘区，当程序退出时，将同步退出后端进程

### bug 修复

1. 修复设置读取异常导致回信人为空

### 下载地址

[uzonmail-desktop-win-x64-0.14.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.1.0.zip)

[uzonmail-service-win-x64-0.14.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.14.1.0.zip)

[uzonmail-service-linux-x64-0.14.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.14.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.14.0

> 更新日期：2025-05-10

::: warning

由于数据库变动较大，本次更新不兼容老版本数据

:::

### 新增功能

1. 设置拆分为系统、组织、用户设置，方便多用户管理
2. 发件历史中新增【复制发件】功能，可从既有的发件任务中发起新的发件任务
3. 专业版新增新的代理测活服务，提升专业版使用体验

### bug 修复

1. 修复向多个用户发件时，附件丢失的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.14.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.0.0.zip)

[uzonmail-service-win-x64-0.14.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.14.0.0.zip)

[uzonmail-service-linux-x64-0.14.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.14.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.13.0

> 更新日期：2025-04-24

### 新增功能

1. 增加发件完成后邮件通知功能。可在【系统设置/基础设置/通知设置】中进行设置
2. 优化 smtp 连接缓存，实现更智能、准确地的缓存和释放连接

### bug 修复

1. 修复发件箱验证时由于证书验证问题导致验证失败的bug

### 下载地址

[uzonmail-desktop-win-x64-0.13.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.13.0.0.zip)

[uzonmail-service-win-x64-0.13.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.13.0.0.zip)

[uzonmail-service-linux-x64-0.13.0.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.13.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.12.4

> 更新日期：2025-04-11

### 新增功能

1. 增加通过粘贴文本导入发件箱、收件箱
2. 添加发件箱时，默认智能补全
3. 发件箱检测失败时，提示失败的发件箱号
4. 优化侧边栏显示样式

### bug 修复

1. 修复模板中包含网络图片跨域时，无法保存问题
2. 修复设置更新后，未及时更新问题

### 下载地址

[uzonmail-desktop-win-x64-0.12.4.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.4.0.zip)

[uzonmail-service-win-x64-0.12.4.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.12.4.0.zip)

[uzonmail-service-linux-x64-0.12.4.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.12.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.12.3

> 更新日期: 2025-04-02

### 新增功能

1. 增加版本更新提醒
2. 支持退出激活状态
3. 增加对个人激活码的支持

### 下载地址

[uzonmail-desktop-win-x64-0.12.3.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.3.0.zip)

[uzonmail-service-win-x64-0.12.3.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.12.3.0.zip)

[uzonmail-service-linux-x64-0.12.3.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.12.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.12.2

> 更新日期: 2025-03-23

### 新增功能

1. 支持动态代理(目前仅支持 [易代理](http://www.ydaili.cn//main/register.aspx?str_code=80TL8T6X)，后期会继续增加)

### bug 修复

1. 修复和优化若干已知问题

### 下载地址

[uzonmail-desktop-win-x64-0.12.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.2.0.zip)

[uzonmail-service-win-x64-0.12.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.12.2.0.zip)

[uzonmail-service-linux-x64-0.12.2.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.12.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.12.1

> 更新日期: 2025-03-15

### 新增功能

1. 新增多语言切换(未完全适配)
2. 部分页面手机端兼容
3. 发件箱增加更多易用功能, 比如批量验证、批量删除无效
4. 将 pro 插件完全从主体中独立

### bug 修复

1. 修复 docker 部署时异常 bug
2. 修复 pro 版本，爬虫效率低或者报错问题
3. 修复服务器部署后，导致无法发送附件
4. 修复无法通过发件箱组发送邮件

### 下载地址

[uzonmail-desktop-win-x64-0.12.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.1.0.zip)

[uzonmail-service-win-x64-0.12.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-win-x64-0.12.1.0.zip)

[uzonmail-service-linux-x64-0.12.1.0.zip](https://obs.uamazing.cn:2234/public/files/soft/uzonmail-service-linux-x64-0.12.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.11.3

### bug 修复

1. 修复修改模板后，缓存未更新的bug

### 下载地址

[uzonmail-desktop-win-x64-0.11.3.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-desktop-win-x64-0.11.3.0.zip)

[uzonmail-service-win-x64-0.11.3.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-win-x64-0.11.3.0.zip)

[uzonmail-service-linux-x64-0.11.3.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-linux-x64-0.11.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.11.2

### bug 修复

1. 修复前端邮箱验证与后端逻辑不一致bug
2. 管理员打开授权页面，自动更新当前授权

### 下载地址

[uzonmail-desktop-win-x64-0.11.2.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-desktop-win-x64-0.11.2.0.zip)

[uzonmail-service-win-x64-0.11.2.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-win-x64-0.11.2.0.zip)

[uzonmail-service-linux-x64-0.11.2.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-linux-x64-0.11.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.11.1

> 更新日期: 2025-01-29

### 功能优化

1. 增加 TikTok 邮箱爬虫

   ![image-20250129102009655](https://oss.uzoncloud.com:2234/public/files/images/image-20250129102009655.png)

### bug 修复

1. 修复发件箱对于-符号的域名验证不通过的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.11.1.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-desktop-win-x64-0.11.1.0.zip)

[uzonmail-service-win-x64-0.11.1.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-win-x64-0.11.1.0.zip)

[uzonmail-service-linux-x64-0.11.1.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-linux-x64-0.11.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

### 新春福利

值此新春佳节，祝所有同学新春快乐，万事如意！

在此为同学们提供 uzon-mail 企业版本两个月的试用码，希望 uzon-mail 可以对你有所帮助。

激活码: `happy-chinese-new-year`，该码有效期 2 个月，单次激活一个月，每台机器可重复使用 2 次。

## 0.11.0

> 更新日期: 2024-12-29

### 功能优化

1. 重构发件核心
2. 优化数据缓存与内存回收

### bug 修复

1. 修复文件管理中无法删除附件的bug
2. 修复当使用数据发件时，若发件箱重复会导致报错的bug

### 特别说明

::: warning
本版本虽然支持直接从 0.10.x 版本升级，但是由于数据格式有变化，会导致发件状态显示异常。
建议直接使用新版本。
:::

### 下载地址

[uzonmail-desktop-win-x64-0.11.0.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-desktop-win-x64-0.11.0.0.zip)

[uzonmail-service-win-x64-0.11.0.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-win-x64-0.11.0.0.zip)

[uzonmail-service-linux-x64-0.11.0.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-linux-x64-0.11.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.10.0

> 2024-10-13

### 新增功能

1. 重构用户设置模块
2. 在发件时，可以增加取消订阅按钮。企业版本可用
3. 增加 docker 安装

### 下载地址

[uzonmail-desktop-win-x64-0.10.0.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-desktop-win-x64-0.10.0.0.zip)

[uzonmail-service-win-x64-0.10.0.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-win-x64-0.10.0.0.zip)

[uzonmail-service-linux-x64-0.10.0.0.zip](https://obs.uamazing.cn:52443/public/files/soft/uzonmail-service-linux-x64-0.10.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.9.5.1

> 2024-09-14

### bug 修复与优化

1. 启用跟踪后，提示 sequence contains no elements 问题

### 下载地址

[uzonmail-desktop-win-x64-0.9.5.1.zip](https://cloud.uamazing.cn:52443/#s/-zaqbDqg)

[uzonmail-service-linux-x64-0.9.5.1.zip](https://cloud.uamazing.cn:52443/#s/-zarK9AA)

## 0.9.5

> 2024-09-11

### 新增功能

1. 新增邮件阅读跟踪。在基础设置中启用邮件跟踪后，发件时会自动进行邮件跟踪

### bug 修复与优化

1. 修复定时任务启动后，无法找到发件组从而发件失败的 bug
2. 开始发件时，在数据处理时间内，增加等待进度条
3. 优化历史发件的状态显示
4. 优化发件箱错误后，未发邮件未能重置状态的 bug
5. 修复程序崩溃后，重新发送可能导致成功项重新发送的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.9.5.0.zip](https://cloud.uamazing.cn:52443/#s/-yx5IyVA)

[uzonmail-service-linux-x64-0.9.5.0.zip](https://cloud.uamazing.cn:52443/#s/-yyMJlFw)

## 0.9.4

> 2024-09-04

### 新增功能

1. 文件管理右键支持分享功能：可以将文件通过链接的形式分享出去，可以通过这种方式将图片插入到模板中。分享功能需要服务器部署并且拥有域名。

### bug 修复

1. 修复基础设置修改失效的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.9.4.0.zip](https://cloud.uamazing.cn:52443/#s/-xXJKHsw)

[uzonmail-service-linux-x64-0.9.4.0.zip](https://cloud.uamazing.cn:52443/#s/-xXJtdbg)

## 0.9.3.2

> 2024-09-03

### bug 修复

1. 修复发件箱报错后仍用于下次发件的 bug
2. 修复一级路由互相跳转时,layout 会刷新的 bug
3. 修复删除邮箱组报错
4. 修复当发件数据中的收件邮箱不存在于系统中时，收件数为空的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.9.3.2.zip](https://cloud.uamazing.cn:52443/#s/-xKrNfbg)

[uzonmail-service-linux-x64-0.9.3.2.zip](https://cloud.uamazing.cn:52443/#s/-xKrufiA)

## 0.9.3.1

> 2024-09-02

### bug 修复

1. 修复数据库初始化报 Department 表错误

### 下载地址

[uzonmail-desktop-win-x64-0.9.3.1.zip](https://cloud.uamazing.cn:52443/#s/-w6vVVFQ)

[uzonmail-service-linux-x64-0.9.3.1.zip](https://cloud.uamazing.cn:52443/#s/-w6v6IGA)

## v0.9.3

`!!! 有严重 bug, 请勿使用`

> 2024-08-30

### 新增功能

1. 企业版本增加组织相关设置
2. 子账户将无法修改相关设置

### bug 修复

1. 修复证书过期的邮局无法发件的 bug
2. 修复无法设置子账户的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.9.3.0.zip](https://cloud.uamazing.cn:52443/#s/-wYGIksg)

[uzonmail-service-linux-x64-0.9.3.0.zip](https://cloud.uamazing.cn:52443/#s/-wYGiIDQ)

## v0.9.2

> 2024-08-28

### 新增功能

1. 增加子账户功能(企业版)：主账户可以创建子账户，主账户具有管理子账户的设置、查看发件数据的功能
2. 增加管理员禁用其它账户功能

### bug 修复

1. 修复权限管理页面权限错误的 bug
2. 修复无法删除收件箱的 bug
3. 修复管理员无法重置用户密码的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.9.2.0.zip](https://cloud.uamazing.cn:52443/#s/-v-KfpJw)

[uzonmail-service-linux-x64-0.9.2.0.zip](https://cloud.uamazing.cn:52443/#s/-v-K5ZMg)

## v0.9.1

> 2024-08-27

### 新增功能

1. 发件箱、收件箱支持批量导出
2. 发件明细支持按状态分类查看
3. 发件明细支持数据导出
4. 模板支持调整字体大小和设置颜色
5. 标签栏可拖动
6. 登陆页面增加版本显示，方便核对版本

### 下载地址

[uzonmail-desktop-win-x64-0.9.1.0.zip](https://cloud.uamazing.cn:52443/#s/-vw-iMhA)

[uzonmail-service-linux-x64-0.9.1.0.zip](https://cloud.uamazing.cn:52443/#s/-vw_NGIw)

## v0.9.0

### 新增功能

1. 增加专业版本和企业版本
2. 增加权限管理

### bug 修复

1. 修复无法修改发件箱 ssl 的 bug
2. 修复同时使用数据和发件箱时，报错 bug

### 下载地址

[uzonmail-desktop-win-x64-0.9.0.0.zip](https://cloud.uamazing.cn:52443/#s/-u66gFEw)

[uzonmail-service-linux-x64-0.9.0.0.zip](https://cloud.uamazing.cn:52443/#s/-vhdM6NA)

## v0.4.3

功能更新：

1. 支持非加密抄送
2. 主页显示到达率
3. 支持头像修改

## v0.4.2

功能更新：

1. 新增发送附件功能
   在新建附件中选择的附件是全局附件，每个收件箱都会收到，如果要针对个别收件箱发送不同的附件，可以在数据里面添加 attachments 字段不重写，多个文件用冒号分隔。

bug 修复：

1. 修复选择数据文件错误导致的发送失败问题

## v0.3.2

**新增功能：**

1. 增加默认变量 userName 和 inbox
2. 可通过数据中的 userName 自动关联收件人

## v0.3.1

**新增功能：**

1. 增加新建和编辑模板功能
2. 增加图文混发功能
3. 解除对模板数据必须输入的限制
4. 增加请求头伪装

## v0.2.1

数据重构版本
