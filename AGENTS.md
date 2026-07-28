# 仓库规范

## 目录约定

- tests : 全局的测试文件
- scripts : 脚本目录
- docs : 网站文档
- docker: Docker 脚本
- src
  - api: API 层
  - web：前端 web 应用
  - win-desktop： Windows 端对 web 应用的封装

## 语言要求

**.NET**

- 使用 .NET 10 / C# 14 语法
- 严格遵循 异步编程 (async/await) 规范
- 使用 `dotnet-csharpier path/to/file.cs` 格式化C#代码

## 项目规范

- 项目 src/api、src/web、src/win-desktop 各自维护了 codegraph, 优先在这些目录中查询

## 项目测试

在仓库根目录执行以下命令运行所有测试：

```powershell
dotnet test --project tests/UzonMailDotNET.Test/UzonMailDotNET.Test.csproj
```

## 用户安装软件及命令

- ripgrep : rg