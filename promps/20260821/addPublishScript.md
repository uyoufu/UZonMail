# 优化scripts下自动化脚本

## 重构既有脚本

1. 将 D:\Develop\Personal\UzonMail\scripts\update-version-doc.ps1 移动到子目录中，因为该脚本一般不需要直接调用，不面向用户
2. 在 scripts/ 目录下添加 README.md, 用于说明该目录下的脚本的功能和使用方法

## 新增发布脚本

1. 在 scripts/ 目录下新增一个 publish.ps1 脚本
2. 该脚本的功能如下：
- 从 tag 中检测上一个版本号
- 让用户指定当前版本号，若不指定，默认为上一个版本号的下一个 patch 版本
- 将版本号更新到 D:\Develop\Personal\UzonMail\src\api\UZonMailService\UzonMailService.csproj 和 D:\Develop\Personal\UzonMail\src\win-desktop\UzonMailDesktop\UzonMailDesktop.csproj 中的 FileVersion 和 AssemblyVersion 属性上
- 同时将版本号更新到前端  D:\Develop\Personal\UzonMail\src\web\src\config\app.config.ts default.version 上
3. 然后调用 build.ps1 ，带有 -PushDockerImage -UploadArtifacts 参数进行构建
4. 调用 new-version-doc.ps1 发布新的版本文档