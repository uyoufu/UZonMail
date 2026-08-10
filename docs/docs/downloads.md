---
title: 历史版本
editLink: false
description: 本页收录了宇正群邮所有历史发布版本。宇正群邮是一款开源免费的邮件群发软件，支持邮件群发、邮件营销、邮箱爬取、变量替换等功能，兼容所有邮箱账号（含Outlook OAuth2），支持Windows、Linux、MacOS及服务器部署。多线程并发，支持多账号，性能强劲，持续迭代优化，被外贸、教育、财务等行业广泛认可。宇正群邮致力于成为最好用的邮件群发软件，是企业和个人邮件营销的首选开源邮件群发解决方案。
permalink: /downloads
---

## 0.23.5

> 更新时期: 2026-08-07

### Bug 修复

1. 修复 Excel 数据状态计算错误：当多行 Excel 数据使用相同收件箱时，之前错误地按唯一收件箱数量统计，现已修正为按行数统计，使状态判断更加准确
2. 改进收件箱数据验证：正确处理空白字符的收件箱值，提升验证逻辑的准确性
3. 改进错误提示信息：验证失败时显示具体缺失收件箱数据的 Excel 行号，帮助用户快速定位问题
4. 改进重复收件人错误提示：引导用户使用「允许重复发送」功能解决重复收件人问题

### 下载地址

[uzonmail-desktop-win-x64-0.23.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.5.0.zip)

[uzonmail-service-win-x64-0.23.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.5.0.zip)

[uzonmail-service-linux-x64-0.23.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.5.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.4

> 更新时期: 2026-08-07

### Bug 修复

1. 修复了收件箱状态计算逻辑：之前 ExcelDataStatus.All 要求每行都填写收件箱，但计算时错误地使用了去重后的收件箱数量，现已修正为按行计数。
2. 改进了收件箱数据校验：现在正确使用 Trim() 清理空格并通过 IsNullOrWhiteSpace 进行空值判断。

### 功能优化

1. 在 SendingGroupValidator 中新增行级错误提示，明确显示缺失收件箱数据的 Excel 行号。
2. 优化了重复收件人错误提示信息，引导用户前往「允许重复发送」设置。

### 下载地址

[uzonmail-desktop-win-x64-0.23.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.4.0.zip)

[uzonmail-service-win-x64-0.23.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.4.0.zip)

[uzonmail-service-linux-x64-0.23.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.3

> 更新时期: 2026-08-04

### 功能新增

1. 全新版本更新通知：支持查看更新历史、忽略版本、立即更新等交互操作
2. 新增版本历史对话框：展示官方更新说明及安全下载链接

### 功能优化

1. 邮箱地址验证升级：采用 MIME 标准严格模式，增强发件箱、收件人、抄送、密送、回复地址的校验能力

### 下载地址

[uzonmail-desktop-win-x64-0.23.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.3.0.zip)

[uzonmail-service-win-x64-0.23.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.3.0.zip)

[uzonmail-service-linux-x64-0.23.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.2

> 更新时期: 2026-07-31

### 功能优化

1. 添加邮件 HTML 内容转纯文本功能，生成与 HTML 正文对应的纯文本版本，避免部分邮件客户端出现 MPART_ALT_DIFF 不匹配问题

### Bug 修复

1. 优化邮件模板缓存机制，改为基于租约的 per-template 缓存方案，提升模板管理的可靠性和资源释放及时性

### 下载地址

[uzonmail-desktop-win-x64-0.23.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.2.0.zip)

[uzonmail-service-win-x64-0.23.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.2.0.zip)

[uzonmail-service-linux-x64-0.23.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.1

> 更新时期: 2026-07-30

### 功能新增

1. 运行时主题切换：支持在默认主题与樱花浪漫主题之间切换
2. 桌面应用本地化：Windows 桌面应用完整本地化基础设施，支持中英文双向同步
3. 组织级邮箱有效性管理：支持批量标记邮箱为有效或无效状态
4. 批量移动邮箱：支持将收件箱和发件箱批量移动到目标组
5. 批量软删除选中邮箱：支持多选软删除并在表格中显示验证状态
6. 桌面应用自动更新：集成 CLI 工具和 WebView2 的自动更新功能
7. 硬退回检测与邮箱验证系统：SMTP 状态码 550/551/553 自动识别为硬退回并标记邮箱
8. API 错误本地化：基于键的 API 错误本地化系统
9. 允许重复发送：支持单次任务中向同一收件人发送多封邮件并检测重复收件人
10. 可拖拽树组件：支持层级数据的拖拽排序、上下文菜单和受控状态
11. 文件存储重构：分类管理、内容寻址存储和引用计数
12. 桌面应用重构：迁移至新目录结构并采用 CommunityToolkit.Mvvm 框架

### 功能优化

1. 主题重命名 Fairy Pink 为 Cherry Blossom Romance
2. 收件箱管理界面格式化优化，无效导入按钮添加红色视觉提示
3. 插件加载支持共享程序集目录
4. 构建脚本统一，发送查询排除硬退回项目
5. i18n 国际化扩展至所有 UI 页面和组件
6. 上下文菜单根据选择数量动态显示
7. 低代码表单支持富文本编辑器
8. 状态标签通过 i18n 系统解析

### Bug 修复

1. 修复 WSL 路径转换的 Bash 引号问题
2. 修复邮件验证顺序和 SPF/未知状态测试
3. 修复 disableAutogrow 属性名正为 disableAutoGrow
4. 修复 SendCore 核心 bug 并改进异步生命周期处理

### 下载地址

[uzonmail-desktop-win-x64-0.23.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.1.0.zip)

[uzonmail-service-win-x64-0.23.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.1.0.zip)

[uzonmail-service-linux-x64-0.23.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.23.0

> 更新时期: 2026-07-28

### 功能新增

1. 桌面应用自动更新程序：新增 UzonMailUpdater CLI 工具，支持版本检查、包下载、文件校验和回滚更新。桌面应用集成 WebView2 更新入口，超级管理员可在仪表板查看并执行更新。
2. 批量移动邮箱到目标分组：支持在收件箱和发件箱管理页面批量选择邮箱并移动到指定分组。
3. 批量软删除收件箱：支持批量软删除选中的收件箱，验证状态在表格中直接显示。
4. 硬退回检测与邮箱验证：发件时自动识别 SMTP 550/551/553 状态码为硬退回，标记目标邮箱为无效并移至"验证失败"分组。支持验证单个邮箱或整个分组。
5. 允许重复发送选项：邮件任务新增"允许重复发送"设置，检测到重复收件人时显示警告弹窗。
6. API 国际化基础设施：新增基于键的 API 错误本地化系统，支持中文和英文错误信息。

### 功能优化

1. 国际化覆盖全站 UI：新增并完善 crawlerTask、sendDetail、sendHistory、sendingProgress、statisticsReport、profile 等页面的多语言翻译。
2. 发送调度性能优化：改用非阻塞冷却等待和延迟配额重置，避免阻塞工作线程。使用游标分页加载发件箱，自适应补充目录。
3. 拖拽树组件：新增 DraggableTree 组件，支持层级数据的拖拽排序和右键菜单。
4. 上下文菜单显示条件：根据选择数量控制菜单项显示时机（任意/仅多选/仅单选）。
5. TypeScript 类型强化：useQTable 和 ContextMenu 支持泛型和 multi-select v-model，提升类型安全和批量操作体验。
6. 文件存储重构：引入文件分类体系，支持分类树展示、拖拽排序、内容寻址存储和引用计数清理。
7. 插件加载优化：支持从上级 Assembly 目录加载共享依赖，按依赖关系排序插件加载顺序。

### Bug 修复

1. 修复 WSL 路径转换问题：正确处理含反斜杠的 Windows 路径字面量。
2. 修复 LowCodeForm 的 disableAutogrow 属性命名不一致问题，统一为 disableAutoGrow。

### 下载地址

[uzonmail-desktop-win-x64-0.23.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.0.0.zip)

[uzonmail-service-win-x64-0.23.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.23.0.0.zip)

[uzonmail-service-linux-x64-0.23.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.23.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.22.1

> 更新时期：2026-06-03

### 功能优化

1. 简化 Docker 安装方式，支持通过环境变量覆盖后端配置

### Bug 修复

1. 修复每日发件数量限制重置错误问题

### 下载地址

[uzonmail-desktop-win-x64-0.22.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.22.1.0.zip)

[uzonmail-service-win-x64-0.22.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.22.1.0.zip)

[uzonmail-service-linux-x64-0.22.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.22.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.22.0

> 更新时期：2026-03-29

### 功能优化

1. 优化登录界面动画

### Bug 修复

1. 修复删除邮箱组后，邮箱依然显示bug

### 下载地址

[uzonmail-desktop-win-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.22.0.0.zip)

[uzonmail-service-win-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.22.0.0.zip)

[uzonmail-service-linux-x64-0.22.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.22.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.21.0

> 更新时期：2026-03-22

### 功能优化

1. 框架升级为 .NET 10
2. 优化性能占用

### Bug 修复

1. 修复 AI 在 Windows 下无法使用问题

### 下载地址

[uzonmail-desktop-win-x64-0.21.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.21.0.0.zip)

[uzonmail-service-win-x64-0.21.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.21.0.0.zip)

[uzonmail-service-linux-x64-0.21.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.21.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.7

> 更新时期：2026-02-02

### Bug 修复

1. 修复附件带有唯一标识符前缀的bug

### 下载地址

[uzonmail-desktop-win-x64-0.20.7.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.7.0.zip)

[uzonmail-service-win-x64-0.20.7.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.7.0.zip)

[uzonmail-service-linux-x64-0.20.7.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.7.0.zip)

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

## 0.20.5

> 更新时期：2025-12-19

### 功能优化

1. 数据库升级模块将初始化与升级进行拆分，减少耦合
2. 使用发件箱历史记录来辅助 smtp 信息补全
3. 发件任务因发件箱问题导致的退出，状态修正为 “暂停”

### 下载地址

[uzonmail-desktop-win-x64-0.20.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.5.0.zip)

[uzonmail-service-win-x64-0.20.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.5.0.zip)

[uzonmail-service-linux-x64-0.20.5.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.5.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.4

> 更新时期：2025-12-19

### 功能优化

1. 将 MsGraph 与 SMTP 拆分，MsGraph 发件授权不再依赖于后缀检测

### 下载地址

[uzonmail-desktop-win-x64-0.20.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.4.0.zip)

[uzonmail-service-win-x64-0.20.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.4.0.zip)

[uzonmail-service-linux-x64-0.20.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.3

> 更新时期：2025-12-11

### Bug 修复

1. 修复多语言 @ 配置错误导致的邮箱管理中下载功能异常

### 下载地址

[uzonmail-desktop-win-x64-0.20.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.3.0.zip)

[uzonmail-service-win-x64-0.20.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.3.0.zip)

[uzonmail-service-linux-x64-0.20.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.2

> 更新时期：2025-12-10

### 功能优化

1. 优化状态标签样式，减弱标签注意力
2. 优化后端内存管理，及时释放无用内存，提升运行稳定性

### Bug 修复

1. 修复暂停后邮件状态错误导致无法继续发件的bug
2. 修复发件箱报错后，前端无正确响应的bug

### 下载地址

[uzonmail-desktop-win-x64-0.20.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.2.0.zip)

[uzonmail-service-win-x64-0.20.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.2.0.zip)

[uzonmail-service-linux-x64-0.20.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20.1

> 更新时期：2025-12-10

### Bug 修复

1. 修复因数据库变动导致无法新建组的bug

### 下载地址

[uzonmail-desktop-win-x64-0.20.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.1.0.zip)

[uzonmail-service-win-x64-0.20.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.1.0.zip)

[uzonmail-service-linux-x64-0.20.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.20

> 更新时期：2025-12-06

### 新增功能

1. 增加 QQ 群成员采集功能 [专业版本及以上]

### 下载地址

[uzonmail-desktop-win-x64-0.20.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.20.0.0.zip)

[uzonmail-service-win-x64-0.20.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.20.0.0.zip)

[uzonmail-service-linux-x64-0.20.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.20.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.19.1

> 更新时期：2025-12-06

### Bug 修复

1. 修复并发下，更新缓存发生竞争导致发件箱退出的bug

### 下载地址

[uzonmail-desktop-win-x64-0.19.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.19.1.0.zip)

[uzonmail-service-win-x64-0.19.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.19.1.0.zip)

[uzonmail-service-linux-x64-0.19.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.19.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.19.0

> 更新时期：2025-12-05

::: warning
由于修改了密码保存方式，不支持直接升级，可将数据库删除后，重新启动
桌面端删除原来的目录即可
:::

### 功能新增

1. 增加 AI 生成和优化模板功能 [文档-AI助手](/guide/features/ai.html)
2. 增加 AI 根据内容生成多个邮件主题功能
3. 优化后端缓存框架，提升缓存效率

### Bug 修复

1. 修复专业版及以上授权在数据库删除后，可能消失的问题
2. 修复 Excel 数据中，变量未进行替换的问题

### 其它

1. 验证导入 7w 收件箱数据，程序卡顿问题，未能复现

### 下载地址

[uzonmail-desktop-win-x64-0.19.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.19.0.0.zip)

[uzonmail-service-win-x64-0.19.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.19.0.0.zip)

[uzonmail-service-linux-x64-0.19.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.19.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.18.2

> 更新时期：2025-11-08

::: warning
由于多语言改动较多，若没有特殊需求，不建议升级
:::

### 功能新增

1. 主页、发件管理、新建发件页面支持多语言
2. 增加批量删除已经接收过邮件的收件箱

### 下载地址

[uzonmail-desktop-win-x64-0.18.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.18.2.0.zip)

[uzonmail-service-win-x64-0.18.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.18.2.0.zip)

[uzonmail-service-linux-x64-0.18.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.18.1

> 更新时期：2025-10-30

### Bug 修复

1. 修复发件历史中【复制发件】无响应的 bug
2. 修复选择模板后，模板无效的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.18.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.18.1.0.zip)

[uzonmail-service-win-x64-0.18.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.18.1.0.zip)

[uzonmail-service-linux-x64-0.18.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.18.0

> 更新时期：2025-10-25

### 功能优化

1. 专业版本新增 IP 预热功能

   ![image-20251025143555005](https://oss.uzoncloud.com:2234/public/files/images/image-20251025143555005.png)

2. 社区版本新增以下默认变量

   | 变量名     | 值         |
   | ---------- | ---------- |
   | inbox      | 收件人     |
   | inboxName  | 收件人姓名 |
   | outbox     | 发件人     |
   | outboxName | 收件人姓名 |

3. 增强发件间隔随机值的均匀性
4. 增加批量删除发件历史功能

### 下载地址

[uzonmail-desktop-win-x64-0.18.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.18.0.0.zip)

[uzonmail-service-win-x64-0.18.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.18.0.0.zip)

[uzonmail-service-linux-x64-0.18.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.17.1

> 更新时期：2025-09-18

### Bug 修复

1. 修复部分机型无法选择文件的bug
2. 修复批量验证时，数据库并发占用错误
3. 修复收件人变量名获取不正确
4. 修复仅选择收件组时，无法预览的bug

### 下载地址

[uzonmail-desktop-win-x64-0.17.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.17.1.0.zip)

[uzonmail-service-win-x64-0.17.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.17.1.0.zip)

[uzonmail-service-linux-x64-0.17.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.17.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.17.0

> 更新时期：2025-09-10

### 功能优化

1. 优化发件核心, 提升发件速率
2. 增加每个IP每个域名每小时最大发送数量设置, 防止大并发下被封 IP
3. 动态变量增加 Outbox、Inbox、Inboxes 等过程数据

### 下载地址

[uzonmail-desktop-win-x64-0.17.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.17.0.0.zip)

[uzonmail-service-win-x64-0.17.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.17.0.0.zip)

[uzonmail-service-linux-x64-0.17.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.17.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.4

> 更新时期：2025-09-04

### bug 修复

1. 修复企业版设置获取异常导致程序崩溃问题
2. 修复定时任务优化未合并到更新的问题

### 下载地址

[uzonmail-desktop-win-x64-0.16.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.4.0.zip)

[uzonmail-service-win-x64-0.16.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.4.0.zip)

[uzonmail-service-linux-x64-0.16.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.3

> 更新时期：2025-09-01

### 新增功能

2. 支持 outlook 使用代理发件（当 smtp 地址非 smtp.outlook.com 时有效）

### bug 修复

1. 修复定时任务取消后依然执行的 bug
2. 修复重启后，定时任务状态变更为完成的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.16.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.3.0.zip)

[uzonmail-service-win-x64-0.16.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.3.0.zip)

[uzonmail-service-linux-x64-0.16.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.2

> 更新时期：2025-08-25

### bug 修复

1. 修复定时任务时区问题导致实际发件时间不准确的bug
2. 修复 JS 变量无法解析的bug

### 下载地址

[uzonmail-desktop-win-x64-0.16.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.2.0.zip)

[uzonmail-service-win-x64-0.16.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.2.0.zip)

[uzonmail-service-linux-x64-0.16.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.1

> 更新时期：2025-08-16

### 新增功能

1. 强化模板和正文编辑器，支持直接拖动添加图片，支持表格、编辑图片等高级功能

### bug 修复

1. 修复模板直接复制 html 内容时，无法保存的 bug
2. 修复服务器时间与UI时间显示不一致问题

### 下载地址

[uzonmail-desktop-win-x64-0.16.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.1.0.zip)

[uzonmail-service-win-x64-0.16.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.1.0.zip)

[uzonmail-service-linux-x64-0.16.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.16.0

> 更新时期：2025-08-01

### 新增功能

1. 支持 PostgreSQL 数据库
2. 提升变量解析性能

### bug 修复

1. 修复变量换行后解析错误导致服务崩溃bug

### 下载地址

[uzonmail-desktop-win-x64-0.16.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.16.0.0.zip)

[uzonmail-service-win-x64-0.16.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.16.0.0.zip)

[uzonmail-service-linux-x64-0.16.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.16.0.0.zip)

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

[uzonmail-desktop-win-x64-0.15.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.3.0.zip)

[uzonmail-service-win-x64-0.15.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.3.0.zip)

[uzonmail-service-linux-x64-0.15.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.2

> 更新时期：2025-07-03

### bug 修复

1. 修复 outlook 输入账号密码后, 验证不通过的bug
2. 修复 hotmail 未能匹配 graph 发件的bug

### 下载地址

[uzonmail-desktop-win-x64-0.15.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.2.0.zip)

[uzonmail-service-win-x64-0.15.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.2.0.zip)

[uzonmail-service-linux-x64-0.15.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.15.1

> 更新时期：2025-06-28

### 新增功能

1. 支持配置 outlook 的 ClientId 进行统一授权发件，需要在后端 `appsettings.Production.json` 添加 `MicrosoftEntraApp`，具体格式如下：

   ```json
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

[uzonmail-desktop-win-x64-0.15.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.1.0.zip)

[uzonmail-service-win-x64-0.15.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.1.0.zip)

[uzonmail-service-linux-x64-0.15.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.1.0.zip)

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

[uzonmail-desktop-win-x64-0.15.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.15.0.0.zip)

[uzonmail-service-win-x64-0.15.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.15.0.0.zip)

[uzonmail-service-linux-x64-0.15.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.15.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.14.2.2

> 更新日期：2025-05-28

### 新增功能

1. 新增 [ipfox](https://referral.ipfoxy.com/EpH8pH) 动态代理支持

### bug 修复

1. 修复专业版本没有变量管理功能的 bug
2. 修复激活码退出后，前端刷新失败的bug

### 下载地址

[uzonmail-desktop-win-x64-0.14.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.2.2.zip)

[uzonmail-service-win-x64-0.14.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.14.2.2.zip)

[uzonmail-service-linux-x64-0.14.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.14.2.2.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.14.2

> 更新日期：2025-05-25

### 新增功能

1. 主题和正文支持 js 函数和自定义变量(专业版本)
2. 增加对 [ip2world](https://www.ip2world.com/?ref=Y2NFJBM3CP) 动态代理的支持
3. 新建发件时，支持双击打开弹窗
4. 优化手机端页面样式显示

### bug 修复

1. 修复动态代理测活优先级未生效的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.14.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.2.0.zip)

[uzonmail-service-win-x64-0.14.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.14.2.0.zip)

[uzonmail-service-linux-x64-0.14.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.14.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.14.1

> 更新日期：2025-05-15

### 新增功能

1. 桌面端支持关闭到系统托盘区，当程序退出时，将同步退出后端进程

### bug 修复

1. 修复设置读取异常导致回信人为空

### 下载地址

[uzonmail-desktop-win-x64-0.14.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.1.0.zip)

[uzonmail-service-win-x64-0.14.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.14.1.0.zip)

[uzonmail-service-linux-x64-0.14.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.14.1.0.zip)

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

[uzonmail-desktop-win-x64-0.14.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.14.0.0.zip)

[uzonmail-service-win-x64-0.14.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.14.0.0.zip)

[uzonmail-service-linux-x64-0.14.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.14.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.13.0

> 更新日期：2025-04-24

### 新增功能

1. 增加发件完成后邮件通知功能。可在【系统设置/基础设置/通知设置】中进行设置
2. 优化 smtp 连接缓存，实现更智能、准确地的缓存和释放连接

### bug 修复

1. 修复发件箱验证时由于证书验证问题导致验证失败的bug

### 下载地址

[uzonmail-desktop-win-x64-0.13.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.13.0.0.zip)

[uzonmail-service-win-x64-0.13.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.13.0.0.zip)

[uzonmail-service-linux-x64-0.13.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.13.0.0.zip)

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

[uzonmail-desktop-win-x64-0.12.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.4.0.zip)

[uzonmail-service-win-x64-0.12.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.12.4.0.zip)

[uzonmail-service-linux-x64-0.12.4.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.12.4.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.12.3

> 更新日期: 2025-04-02

### 新增功能

1. 增加版本更新提醒
2. 支持退出激活状态
3. 增加对个人激活码的支持

### 下载地址

[uzonmail-desktop-win-x64-0.12.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.3.0.zip)

[uzonmail-service-win-x64-0.12.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.12.3.0.zip)

[uzonmail-service-linux-x64-0.12.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.12.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.12.2

> 更新日期: 2025-03-23

### 新增功能

1. 支持动态代理(目前仅支持 [易代理](http://www.ydaili.cn//main/register.aspx?str_code=80TL8T6X)，后期会继续增加)

### bug 修复

1. 修复和优化若干已知问题

### 下载地址

[uzonmail-desktop-win-x64-0.12.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.2.0.zip)

[uzonmail-service-win-x64-0.12.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.12.2.0.zip)

[uzonmail-service-linux-x64-0.12.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.12.2.0.zip)

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

[uzonmail-desktop-win-x64-0.12.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.12.1.0.zip)

[uzonmail-service-win-x64-0.12.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.12.1.0.zip)

[uzonmail-service-linux-x64-0.12.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.12.1.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.11.3

### bug 修复

1. 修复修改模板后，缓存未更新的bug

### 下载地址

[uzonmail-desktop-win-x64-0.11.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.11.3.0.zip)

[uzonmail-service-win-x64-0.11.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.11.3.0.zip)

[uzonmail-service-linux-x64-0.11.3.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.11.3.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.11.2

### bug 修复

1. 修复前端邮箱验证与后端逻辑不一致bug
2. 管理员打开授权页面，自动更新当前授权

### 下载地址

[uzonmail-desktop-win-x64-0.11.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.11.2.0.zip)

[uzonmail-service-win-x64-0.11.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.11.2.0.zip)

[uzonmail-service-linux-x64-0.11.2.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.11.2.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.11.1

> 更新日期: 2025-01-29

### 功能优化

1. 增加 TikTok 邮箱爬虫

   ![image-20250129102009655](https://oss.uzoncloud.com:2234/public/files/images/image-20250129102009655.png)

### bug 修复

1. 修复发件箱对于-符号的域名验证不通过的 bug

### 下载地址

[uzonmail-desktop-win-x64-0.11.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.11.1.0.zip)

[uzonmail-service-win-x64-0.11.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.11.1.0.zip)

[uzonmail-service-linux-x64-0.11.1.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.11.1.0.zip)

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

[uzonmail-desktop-win-x64-0.11.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.11.0.0.zip)

[uzonmail-service-win-x64-0.11.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.11.0.0.zip)

[uzonmail-service-linux-x64-0.11.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.11.0.0.zip)

[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)

## 0.10.0

> 2024-10-13

### 新增功能

1. 重构用户设置模块
2. 在发件时，可以增加取消订阅按钮。企业版本可用
3. 增加 docker 安装

### 下载地址

[uzonmail-desktop-win-x64-0.10.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.10.0.0.zip)

[uzonmail-service-win-x64-0.10.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-win-x64-0.10.0.0.zip)

[uzonmail-service-linux-x64-0.10.0.0.zip](https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.10.0.0.zip)

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

功能新增：

1. 支持非加密抄送
2. 主页显示到达率
3. 支持头像修改

## v0.4.2

功能新增：

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
