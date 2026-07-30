# 软件更新器

`UzonMailUpdater` 是桌面端的独立更新进程，主程序通过 `UzonMailUpdateLauncher` 启动它后退出，使更新器能够安全替换应用文件。

## 生成清单

```powershell
UzonMailUpdater.exe package --project-directory D:\UzonMail `
  --out D:\UzonMail\appPackage.json `
  --out D:\updates\latest.json `
  --out D:\updates\0.23.0.0.json
```

`--out` 可重复指定；所有输出内容相同。未指定时会写入项目目录的 `appPackage.json`。默认请求地址为 `https://uzonmail.uzoncloud.com/updates/latest.json`，ZIP 地址按桌面端文件版本生成。

清单中的 `dependencies` 使用相对文件路径到 SHA-256 的映射。默认不更新 `Updater/**`、`appsettings*.json` 和 `service/data/**`，以保护本地更新器、配置和用户数据。

## 执行更新

```powershell
UzonMailUpdater.exe update --project-directory D:\UzonMail --parent-process-id 12345
```

更新器读取本地清单的 endpoint，下载远端 `latest.json` 和 ZIP，校验全部文件后替换应用并重启 `restartExecutablePath`。静态站点需要向 Web 前端开放 `latest.json` 的跨域读取权限。
