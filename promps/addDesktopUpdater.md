# 为桌面端增加软件更新器

## 调用流程

1. 管理员在首页触发新版本检测
2. 当检测到新版本后，若是桌面端，则提示用户更新
3. 用户确认更新后，桌面端通过 D:\Develop\Personal\UzonMail\src\updater\UzonMailUpdateLauncher 调用 updater.exe 更新源码, 再重启桌面端，web 端与桌面端通信采用 AddHostObjectToScript 技术
4. 在程序打包过程中，需要将更新器保存到包中的 `UpdaterTmp` 目录下，桌面端启动时，若检测到该目录非空，则将 UpdaterTmp 目录下的更新器复制到 Updater/ 目录下，这样就实现了 updater 的自动更新

## 更新器

更新器项目位于 D:\Develop\Personal\UzonMail\src\updater\ 目录下

**更新器应具备以下功能**

1. 指定项目目录，为其生成包文件 appPackage.json

包文件的格式为：

```json
{
  "name": "UzonMail",
  "version": "1.0.0",
  // 若没有文件列表，只有 zip，则下载完成后，手动计算 files
  "dependencies": {
    "hash值": "文件路径"
  },
  // 根据这个地址去请求最新版本的更新包信息
  "endpoint": "https://uzonmail.uzoncalc.com/updates/latest.json",
  // 文件来源
  "sourceType": "static",
  "zipUrl": "压缩包下载地址",
  "ignores": ["Updater/**"], // 忽略的文件路径，需要支持 glob 语法
  // 默认更新器位于 Updater/ 目录下
  // 目录应相对于 ../Updater
  "restartExecutablePath": "UzonMailDesktop.exe" // 更新完成后，需要重启的进程路径
}
```

2. 更新项目

执行完整更新逻辑

**要求**

- 该更新器要为今后的增量更新预留接口，可以抽象一个 IDependencyDownloader 接口，传入 hash 值，返回一个本机文件地址，后续的更新依赖这个地址处理
- 更新时，要并发下载依赖文件，确保更新速度
- 更新完成后，要清理临时文件

## AddHostObjectToScript 技术

桌面端所有的 HostObject 都保存到
D:\Develop\Personal\UzonMail\src\win-desktop\UzonMailDesktop\WebMessage\HostObjects\ 目录中，并在一个文件中统一注册

## 参考项目

1. D:\Develop\Work\iepc-desktop\swtoolsUpdate
