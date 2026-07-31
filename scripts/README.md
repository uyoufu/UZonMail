# 自动化脚本

此目录存放项目维护与发布脚本。除非另有说明，请从仓库根目录执行命令。

## 前置条件

- PowerShell 脚本需要 PowerShell 7（`pwsh`）
- `publish.sh` 需要在 Git Bash 中执行，并要求本地存在 `git` 和 `pwsh`
- 构建脚本按目标检测所需工具；完整发布还需要 `bun`、`.NET SDK`、`7z.exe`、Docker、`od` 和 OpenCode
- 发布会提交、推送分支与标签，并上传安装包和 Docker 镜像。执行前请确保工作区无未提交变更

## 公开脚本

| 脚本 | 作用 | 用法 |
| --- | --- | --- |
| `build.ps1` | 构建桌面端、服务端安装包和 Docker 镜像 | `pwsh -File ./scripts/build.ps1 -Target All` |
| `publish.sh` | 更新版本、构建并上传产物、发布版本文档和 Git 标签 | `bash ./scripts/publish.sh [vX.Y.Z]` |
| `new-version-doc.ps1` | 基于上一版本后的 Git 提交生成中英文版本说明，并发布文档分支 | `pwsh -File ./scripts/new-version-doc.ps1 -Version X.Y.Z` |
| `docker-deploy.sh` | 使用已构建的 Linux 服务目录构建并启动 Docker Compose | `(cd build && bash ../scripts/docker-deploy.sh)` |
| `index-official-site.ps1` | 检查官网收录提交配置，预留搜索引擎收录入口 | `pwsh -File ./scripts/index-official-site.ps1` |

`publish.sh` 不带版本参数时，会显示从当前可达版本标签计算出的下一 patch 版本。直接回车使用默认值，也可输入 `X.Y.Z` 或 `vX.Y.Z`。它固定调用：

```bash
pwsh -NoProfile -File scripts/build.ps1 -Target All -PushDockerImage -UploadArtifacts
```

构建成功后，脚本提交版本文件，再调用 `new-version-doc.ps1` 发布文档。仅在版本文档发布成功后才会推送 `master` 和新版本标签。

## 内部实现

- `internal/update-release-version.ps1` 由 `publish.sh` 调用，将三段式发布版本写入服务端、桌面端和前端配置的四段式版本字段
- `internal/update-version-doc.ps1` 由 `new-version-doc.ps1` 调用，写入中英文下载页和更新清单
- `UzonMail.Build.psm1` 是 `build.ps1` 的共享实现模块
- `Dockerfile` 是构建服务端镜像时使用的 Docker 文件

内部脚本与模块不面向直接调用，调用入口和参数由对应公开脚本维护。
