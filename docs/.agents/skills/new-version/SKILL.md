---
name: new-version
description: 在文档中添加新版本
---

# Instructions

根据用户对新版本的描述，生成一个新的版本更新日志，生成的内容要求如下：

1. 版本号
格式为 x.y.z，其中 x 是主版本号，y 是次版本号，z 是修订号
2. 更新时期
更新时间为当前日期，可以使用脚本 `get_now_date` 获取当前日期，格式为 YYYY-MM-DD
3. 功能新增[可选]
4. 功能优化[可选]
5. Bug 修复[可选]
6. 下载地址
下载地址必须存在，格式如下, version 格式为 x.y.z.b, 其中 x.y.z 是版本号，b 是构建号，构建号默认为 0, 版本号要替换为实际版本号。

``` markdown
[uzonmail-desktop-win-x64-{version}.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-{version}.zip)

[uzonmail-service-win-x64-{version}.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-{version}.zip)

[uzonmail-service-linux-x64-{version}.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-{version}.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)
```

具体步骤如下:

1. 将用户的描述分为 3 类，分别为：功能新增、功能优化和 Bug 修复，并在对应的分类下添加用户提供的信息。
2. 可以使用脚本 `get_now_date` 获取当前日期，格式为 `YYYY-MM-DD`。
3. 下载地址中，根据版本号进行相应修改，具体格式见下方示例, 下载地址中的版本号格式为 x.y.z.b, 其中 x.y.z 是版本号，b 是构建号，构建号默认为 0, 版本号必须完整。
4. 当用户输入的内容非中文时，翻译为中文，然后使用 `update_version_zh` 脚本将更新的信息保存到 `docs/downloads.md` 文件中。
5. 当用户输入的内容非英文时，翻译为英文，然后使用 `update_version_en` 脚本将更新的信息保存到 `docs/en/downloads_en.md` 文件中。

# update_version_x 使用方式

```bash
# 使用示例
printf '## 0.20.1\n> 更新时期：...\n' | node update_version_zh.js
```

# Warning

一定不要读取 `docs/downloads.md` 和 `docs/en/downloads_en.md` 文件中的内容，该文件非常大，使用脚本进行更新

# Example

```markdown
## 0.20.6

> 更新时期：2026-01-27

### 功能优化

1. 更改 UzonMail 图标

### Bug 修复

1. 修复自定义域名的 Outlook 无法被正确识别问题

### 下载地址

[uzonmail-desktop-win-x64-0.20.6.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.6.0.zip)

[uzonmail-service-win-x64-0.20.6.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.6.0.zip)

[uzonmail-service-linux-x64-0.20.6.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.6.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)
```
