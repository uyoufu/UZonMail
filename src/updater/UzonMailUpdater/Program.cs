using System.CommandLine;
using Spectre.Console;
using UzonMailUpdater.Services;

var projectDirectoryOption = new Option<DirectoryInfo>("--project-directory")
{
    Required = true,
    Description = "需要生成或更新的应用目录"
};
var endpointOption = new Option<string?>("--endpoint") { Description = "更新清单地址" };
var zipUrlOption = new Option<string?>("--zip-url") { Description = "更新 ZIP 下载地址" };
var linuxPackagePathOption = new Option<FileInfo?>("--linux-package-path")
{
    Description = "Linux x64 安装包路径"
};
var linuxPackageUrlOption = new Option<string?>("--linux-package-url")
{
    Description = "Linux x64 安装包下载地址"
};
var outputOption = new Option<FileInfo[]>("--out")
{
    Description = "清单输出文件，可重复指定",
    AllowMultipleArgumentsPerToken = true
};
var parentProcessIdOption = new Option<int?>("--parent-process-id")
{
    Description = "需要等待退出的桌面端进程 ID"
};

var packageCommand = new Command("package", "生成应用更新清单");
packageCommand.Options.Add(projectDirectoryOption);
packageCommand.Options.Add(endpointOption);
packageCommand.Options.Add(zipUrlOption);
packageCommand.Options.Add(linuxPackagePathOption);
packageCommand.Options.Add(linuxPackageUrlOption);
packageCommand.Options.Add(outputOption);
packageCommand.SetAction(async parseResult =>
{
    var projectDirectory = parseResult.GetValue(projectDirectoryOption)!;
    var outputs = parseResult.GetValue(outputOption) ?? [];
    var packageService = new AppPackageService();
    var manifest = await packageService.CreateAsync(
        projectDirectory.FullName,
        parseResult.GetValue(endpointOption),
        parseResult.GetValue(zipUrlOption),
        parseResult.GetValue(linuxPackagePathOption)?.FullName,
        parseResult.GetValue(linuxPackageUrlOption)
    );
    await packageService.WriteAsync(
        manifest,
        projectDirectory.FullName,
        outputs.Select(x => x.FullName)
    );
    AnsiConsole.MarkupLine($"[green]已生成更新清单：[/]{Markup.Escape(manifest.Version)}");
    return 0;
});

var updateCommand = new Command("update", "下载并安装最新版本");
updateCommand.Options.Add(projectDirectoryOption);
updateCommand.Options.Add(parentProcessIdOption);
updateCommand.SetAction(async parseResult =>
{
    var projectDirectory = parseResult.GetValue(projectDirectoryOption)!;
    var parentProcessId = parseResult.GetValue(parentProcessIdOption);
    var updater = new AppUpdateService();
    await AnsiConsole
        .Status()
        .StartAsync(
            "[cyan]正在检查更新[/]",
            async context =>
            {
                await updater.UpdateAsync(
                    projectDirectory.FullName,
                    parentProcessId,
                    message => context.Status(message)
                );
            }
        );
    AnsiConsole.MarkupLine("[green]更新完成，正在启动宇正群邮[/]");
    return 0;
});

var rootCommand = new RootCommand("UzonMail 桌面端更新器");
rootCommand.Subcommands.Add(packageCommand);
rootCommand.Subcommands.Add(updateCommand);

try
{
    return await rootCommand.Parse(args).InvokeAsync();
}
catch (Exception exception)
{
    AnsiConsole.MarkupLine($"[red]更新失败：{Markup.Escape(exception.Message)}[/]");
    if (!Console.IsInputRedirected)
    {
        AnsiConsole.MarkupLine("[yellow]按任意键关闭更新器[/]");
        Console.ReadKey(intercept: true);
    }
    return 1;
}
