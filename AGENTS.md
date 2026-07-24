# 仓库规范

## 目录约定

- tests : 全局的测试文件
- scripts : 脚本目录
- docs : 网站文档
- docker: Docker 脚本
- src
  - api
  - web
  - win-desktop

## 语言要求

**.NET**

- 使用 .NET 10 / C# 14 语法
- 严格遵循 异步编程 (async/await) 规范
- 使用 `dotnet-csharpier .` 格式化C#代码

## 项目规范

- 项目 api、web、win-desktop 各生成了各自的 codegraph, 优先使用