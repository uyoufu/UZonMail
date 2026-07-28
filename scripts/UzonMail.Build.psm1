Set-StrictMode -Version Latest

$script:BuildImageName = 'gmxgalens/uzon-mail'

function Write-BuildMessage {
    <#
    .SYNOPSIS
    输出统一格式的构建状态信息
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Message,

        [ValidateSet('Info', 'Success', 'Warning', 'Error', 'Detail')]
        [string]$Level = 'Info'
    )

    $style = switch ($Level) {
        'Success' { @{ Prefix = '[OK]'; Color = 'Green' } }
        'Warning' { @{ Prefix = '[WARN]'; Color = 'Yellow' } }
        'Error' { @{ Prefix = '[ERROR]'; Color = 'Red' } }
        'Detail' { @{ Prefix = '      '; Color = 'DarkGray' } }
        default { @{ Prefix = '[INFO]'; Color = 'Cyan' } }
    }

    Write-Host "$($style.Prefix) $Message" -ForegroundColor $style.Color
}

function Start-BuildStage {
    <#
    .SYNOPSIS
    输出构建阶段标题并开始计时
    #>
    param(
        [Parameter(Mandatory = $true)]
        [int]$Index,

        [Parameter(Mandatory = $true)]
        [int]$Total,

        [Parameter(Mandatory = $true)]
        [string]$Title
    )

    Write-Host ''
    Write-Host "========== [$Index/$Total] $Title ==========" -ForegroundColor Magenta
    return [System.Diagnostics.Stopwatch]::StartNew()
}

function Complete-BuildStage {
    <#
    .SYNOPSIS
    输出构建阶段的完成状态和耗时
    #>
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Stopwatch]$Stopwatch,

        [Parameter(Mandatory = $true)]
        [string]$Title
    )

    $Stopwatch.Stop()
    Write-BuildMessage -Level Success -Message "$Title 完成，耗时 $($Stopwatch.Elapsed.ToString('mm\:ss'))"
}

function Assert-BuildCommand {
    <#
    .SYNOPSIS
    验证构建所需命令可用
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$CommandName
    )

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "未检测到 $CommandName，请安装后重新执行构建"
    }

    Write-BuildMessage -Level Success -Message "$CommandName 环境检测通过"
}

function Invoke-BuildNativeCommand {
    <#
    .SYNOPSIS
    在指定目录执行原生命令并在失败时中止构建
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command,

        [string[]]$Arguments = @(),

        [string]$WorkingDirectory
    )

    if ($WorkingDirectory) {
        Push-Location -LiteralPath $WorkingDirectory
    }

    try {
        # 构建工具的输出需要保留给操作者诊断，但不能成为发布函数的返回值
        & $Command @Arguments | Out-Host
        if ($LASTEXITCODE -ne 0) {
            throw "命令执行失败（退出码 $LASTEXITCODE）：$Command $($Arguments -join ' ')"
        }
    }
    finally {
        if ($WorkingDirectory) {
            Pop-Location
        }
    }
}

function Resolve-BuildContext {
    <#
    .SYNOPSIS
    解析并验证统一后的项目目录
    #>
    $repositoryRoot = (& git rev-parse --show-toplevel).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repositoryRoot)) {
        throw '无法定位 Git 仓库根目录，请在 UzonMail 仓库中运行脚本'
    }

    $context = [pscustomobject]@{
        RepositoryRoot          = $repositoryRoot
        ArtifactRoot            = Join-Path -Path $repositoryRoot -ChildPath 'build'
        WebRoot                 = Join-Path -Path $repositoryRoot -ChildPath 'src/web'
        ApiRoot                 = Join-Path -Path $repositoryRoot -ChildPath 'src/api'
        DesktopRoot             = Join-Path -Path $repositoryRoot -ChildPath 'src/win-desktop'
        UpdaterRoot             = Join-Path -Path $repositoryRoot -ChildPath 'src/updater'
        ServiceProject          = Join-Path -Path $repositoryRoot -ChildPath 'src/api/UZonMailService/UzonMailService.csproj'
        CorePluginProject       = Join-Path -Path $repositoryRoot -ChildPath 'src/api/Plugins/UzonMailCorePlugin/UzonMailCorePlugin.csproj'
        ProPluginProject        = Join-Path -Path $repositoryRoot -ChildPath 'src/api/Plugins/UzonMailProPlugin/UZonMailProPlugin.csproj'
        DesktopProject          = Join-Path -Path $repositoryRoot -ChildPath 'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj'
        UpdaterProject          = Join-Path -Path $repositoryRoot -ChildPath 'src/updater/UzonMailUpdater/UzonMailUpdater.csproj'
        WindowsServiceRoot      = Join-Path -Path $repositoryRoot -ChildPath 'src/api/WindowsService'
        LinuxServiceRoot        = Join-Path -Path $repositoryRoot -ChildPath 'src/api/LinuxService'
        QuartzDatabase          = Join-Path -Path $repositoryRoot -ChildPath 'src/api/UZonMailService/Quartz/quartz-sqlite.sqlite3'
        Dockerfile              = Join-Path -Path $repositoryRoot -ChildPath 'scripts/Dockerfile'
        DockerCompose           = Join-Path -Path $repositoryRoot -ChildPath 'docker/docker-compose.yml'
        DockerEnvironment       = Join-Path -Path $repositoryRoot -ChildPath 'docker/.env'
        DockerDeployScript      = Join-Path -Path $repositoryRoot -ChildPath 'scripts/docker-deploy.sh'
    }

    $requiredPaths = @(
        $context.WebRoot,
        $context.ApiRoot,
        $context.DesktopRoot,
        $context.UpdaterRoot,
        $context.ServiceProject,
        $context.CorePluginProject,
        $context.ProPluginProject,
        $context.DesktopProject,
        $context.UpdaterProject,
        $context.WindowsServiceRoot,
        $context.LinuxServiceRoot,
        $context.QuartzDatabase,
        $context.Dockerfile,
        $context.DockerCompose,
        $context.DockerEnvironment,
        $context.DockerDeployScript
    )

    foreach ($requiredPath in $requiredPaths) {
        if (-not (Test-Path -LiteralPath $requiredPath)) {
            throw "构建所需路径不存在：$requiredPath"
        }
    }

    return $context
}

function Update-BuildSource {
    <#
    .SYNOPSIS
    在工作区干净时同步当前 Git 分支
    #>
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Context
    )

    $workingTreeState = & git -C $Context.RepositoryRoot status --porcelain
    if ($LASTEXITCODE -ne 0) {
        throw '无法读取 Git 工作区状态'
    }

    if ($workingTreeState) {
        throw '工作区存在未提交改动，拒绝执行 -UpdateSource 以避免覆盖本地工作'
    }

    Invoke-BuildNativeCommand -Command git -Arguments @('-C', $Context.RepositoryRoot, 'pull', '--ff-only')
}

function Get-BuildFileVersion {
    <#
    .SYNOPSIS
    获取已发布程序集的文件版本
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath
    )

    if (-not (Test-Path -LiteralPath $FilePath -PathType Leaf)) {
        throw "未找到版本文件：$FilePath"
    }

    $version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($FilePath).FileVersion
    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "无法读取文件版本：$FilePath"
    }

    return $version
}

function ConvertTo-BashSingleQuotedValue {
    <#
    .SYNOPSIS
    将参数转换为 Bash 单引号字面量
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $singleQuote = [string][char]39
    $escapedSingleQuote = "$singleQuote`"$singleQuote`"$singleQuote"
    return "$singleQuote$($Value.Replace($singleQuote, $escapedSingleQuote))$singleQuote"
}

function Invoke-WslBuildCommand {
    <#
    .SYNOPSIS
    在指定 WSL 发行版中执行 Bash 命令
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$BashCommand,

        [string]$Distribution
    )

    $wslArguments = @()
    if (-not [string]::IsNullOrWhiteSpace($Distribution)) {
        $wslArguments += @('--distribution', $Distribution)
    }
    $wslArguments += @('--', 'bash', '-lc', $BashCommand)

    & wsl @wslArguments
    if ($LASTEXITCODE -ne 0) {
        throw "WSL 命令执行失败（退出码 $LASTEXITCODE）：$BashCommand"
    }
}

function ConvertTo-WslPath {
    <#
    .SYNOPSIS
    将 Windows 路径转换为 WSL 路径
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$WindowsPath,

        [string]$Distribution
    )

    $wslArguments = @()
    if (-not [string]::IsNullOrWhiteSpace($Distribution)) {
        $wslArguments += @('--distribution', $Distribution)
    }
    $wslArguments += @('--', 'wslpath', '-u', $WindowsPath)

    $wslPath = (& wsl @wslArguments | Out-String).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($wslPath)) {
        throw "无法转换 WSL 路径：$WindowsPath"
    }

    return $wslPath
}

function Assert-WslDockerEnvironment {
    <#
    .SYNOPSIS
    验证 WSL 发行版内可使用 Docker
    #>
    param(
        [string]$Distribution
    )

    Assert-BuildCommand -CommandName wsl
    Invoke-WslBuildCommand -Distribution $Distribution -BashCommand 'command -v docker >/dev/null && docker info >/dev/null'
    Write-BuildMessage -Level Success -Message 'WSL Docker 环境检测通过'
}

function Get-BuildImageName {
    <#
    .SYNOPSIS
    获取固定的 Docker 镜像仓库名称
    #>
    return $script:BuildImageName
}

Export-ModuleMember -Function @(
    'Write-BuildMessage',
    'Start-BuildStage',
    'Complete-BuildStage',
    'Assert-BuildCommand',
    'Invoke-BuildNativeCommand',
    'Resolve-BuildContext',
    'Update-BuildSource',
    'Get-BuildFileVersion',
    'ConvertTo-BashSingleQuotedValue',
    'Invoke-WslBuildCommand',
    'ConvertTo-WslPath',
    'Assert-WslDockerEnvironment',
    'Get-BuildImageName'
)
