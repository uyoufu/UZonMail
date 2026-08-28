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

本项目采用 Monorepo 管理，在进行对应项目开发时，需要提前阅读项目下的 AGENTS.md 文件，了解项目的开发规范，然后优先使用各自项目下的 codegraph 来查询代码。目录结构如下：

| 目录            | 说明                        | AGENTS.md 文件            | codegraph |
| --------------- | --------------------------- | ------------------------- | --------- |
| src/api         | API 层                      | src/api/AGENTS.md         | 存在      |
| src/web         | 前端 web 应用               | src/web/AGENTS.md         | 存在      |
| src/win-desktop | Windows 端对 web 应用的封装 | src/win-desktop/AGENTS.md | 存在      |

## 数据库设计

- 数据库项目位于 src/api/UZonMailDB/ 目录下，在生成过程中，若发现数据库定义不合理或者不满足需求，需要对数据库定义进行优化

## 项目测试

在仓库根目录执行以下命令运行所有测试：

```powershell
dotnet test --project tests/UzonMailDotNET.Test/UzonMailDotNET.Test.csproj
```

## 用户安装软件及命令

- ripgrep : rg
