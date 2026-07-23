# 迁移桌面端

将桌面端 D:\Develop\Personal\UzonMail\src\api\UzonMailDesktop 迁移到 D:\Develop\Personal\UzonMail\src\win-desktop\UzonMailDesktop 中，要求如下：

1. 完整保留原功能
2. 通过配置定义后端程序入口，不在内部写死
3. 弃用 stylet 框架并重构为使用 CommunityToolkit.Mvvm 框架
4. 遵循 mvvm 最佳实践，模块要解耦
5. 启动时，需要进行环境检测，若不存在对应环境，自动安装，后端使用 asp.net, 桌面端为 dotDesktop runtime 和 webview2