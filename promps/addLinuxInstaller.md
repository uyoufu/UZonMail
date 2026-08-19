# 新增 linux 安装和更新脚本

在 D:\Develop\Personal\UzonMail\scripts\install 中新增 uzonmail_linux_install.py 脚本，用于在 linux 系统中安装和更新 uzonmail 软件。

- 在实现过程中，不引入额外的依赖库，只使用标准库中的模块
- 使用英文作为提示信息
- 由于需要使用 systemd，因此需要提前判断是否有 sudo 权限
- 启动脚本时，需要向用户说明该脚本的作用，对系统的修改说明，如注册服务，创建目录等，让用户对该脚本的执行有了解

## 功能需求

支持的命令：

-h, --help 查看帮助信息
--version 查看当前版本号, 通过读取安装目录下 UzonMailService.exe 中的版本信息来实现
--update 更新软件到最新版本，软件的最新版本可以通过读取 https://uzonmail.uzoncloud.com/updates/latest.json 中的 version 字段来获取，若 json 中存在 minCompatibleVersion, 则要判断是否满足升级条件，当不满足升级条件时，提示用户导出数据，再重新安装；更新时，需要让用户进行确认
--install 初始化安装
--uninstall 卸载软件

## install 逻辑

- 检测架构是否为 x64, 目前仅支持 x64 架构
- 配置通过在根目录新增 appsettings.Production.json 文件不覆盖默认值
- 初始安装时，需要对 D:\Develop\Personal\UzonMail\src\api\UZonMailService\appsettings.json 中的必要设置进行修改，包含：
  - BaseUrl 指定后，需要同步修改跨域配置
  - AdminUser 指定初始的管理员用户名，用于登录后进行配置
- EncryptParams、TokenParams 需要自动配置
- 若检测到 /var/uzonmail/ 中存在备份，提示用户是否迁入数据

用户的临时输入保存到 /tmp/uzonmail/config.json 文件中, 当中断后，重新执行时，可从这个设置中读取用户输入的配置作为默认值，只有当用户确认后，才正式执行安装流程。

正式安装流程如下：

- 读取 https://uzonmail.uzoncloud.com/updates/latest.json 中的值，格式如下：

```json
{
  "name": "UzonMail",
  "version": "0.23.5.0",
  "env": {
    "Microsoft.AspNetCore.App": "10.0.0",
    "Microsoft.NETCore.App": "10.0.0"
  },
  "dependencies": {},
  "endpoint": "https://uzonmail.uzoncloud.com/updates/latest.json",
  "zipUrl": "https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-desktop-win-x64-0.23.5.0.zip",
  "ignores": ["Updater/**", "appsettings*.json", "service/data/**"],
  "restartExecutablePath": "UzonMailDesktop.exe"
}
```
- 若环境缺失，则安装 dotnet 环境
- 通过 zipUrl 中的下载地址，推断出 linux 的安装包，格式为 uzonmail-service-linux-x64-0.23.5.0.zip，目前只支持 x64 架构
- 下载安装包到 /tmp/uzonmail/ 目录下
- 解压到 /var/www/uzonmail/ 目录下
- 使用 systemd 注册为服务
- 启动服务

## uninstall 卸载

- 卸载时，需要进行提示，是否备份数据
- 若用户要备份数据，则将目录中的 appsettings.Production.json、data/ 和 public/ 目录备份到用户指定的目录，默认为 /var/uzonmail/backup/
- 备份时，需要在备份中指明备份的版本号，用于恢复数据时校验
- 卸载后，要清理相关目录中的文件，包括临时目录、安装目录、移除服务注册文件等

## 安全性管控

**普通模式**

由于涉及到 sudo 权限，因此在执行每一步命令时，都需要用户进行确认，让其心中有数

**静默模式**

若用户输入 -q 或 --quiet, 则在执行每一步命令时，不会提示用户确认，直接执行，需要的值则采用默认值