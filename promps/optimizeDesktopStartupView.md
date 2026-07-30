# 美化桌面启动视图

1. 桌面端启动时，不够美观，进行美化， 可以参考以 D:\Develop\Personal\UzonCalc\src\web\public\welcome.html, 但是使用 wpf 实现，带阴影、带动画效果
2. 需要支持多语言，当前端将语言修改为指定语言时，桌面端也要同步切换语言并持久化到本地配置文件中，使用 HostObjects 技术实现
3. 初始化时，采用系统中的语言设置