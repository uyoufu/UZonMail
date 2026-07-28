# 开发说明

## 开发环境

- .NET 10 SDK

## 运行 .NET 测试

在仓库根目录执行以下命令：

```powershell
dotnet test --project tests/UzonMailDotNET.Test/UzonMailDotNET.Test.csproj
```

根目录的 `global.json` 将 .NET 10 的 `dotnet test` runner 指定为 `Microsoft.Testing.Platform`。测试项目使用 `MSTest.Sdk`，其基于 Microsoft Testing Platform (MTP) 执行测试；若未选择该 runner，.NET 10 会默认尝试使用 VSTest，并因与该测试项目不兼容而无法执行。

该配置仅选择测试执行引擎，使从仓库根目录和子目录调用 `dotnet test` 都使用相同的 runner。它不会安装或固定 .NET SDK 版本、不会改变 MSTest 测试框架，也不会自动在 CI 中运行测试。
