<#
.SYNOPSIS
统一构建 UzonMail 安装包、Docker 镜像和可选发布产物
#>
[CmdletBinding()]
param(
    [ValidateSet('All', 'Desktop', 'WindowsServer', 'Linux', 'Docker')]
    [string[]]$Target,

    [switch]$UpdateSource,

    [string]$LinuxPackageUrl,

    [switch]$PushDockerImage,

    [switch]$UploadArtifacts,

    [string]$UpdateManifestOutputDirectory,

    [string]$WslDistribution
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'UzonMail.Build.psm1') -Force

$BuildTargetOptions = @('All', 'Desktop', 'WindowsServer', 'Linux', 'Docker')
$ServicePublishDirectories = @('public', 'wwwroot', 'Plugins', 'Assembly', 'data/db')
$PluginDirectoryName = 'Plugins'
$PluginAssemblyDirectoryName = 'Assembly'
$PluginPublishStagingDirectoryName = '.plugin-publish'

function Show-BuildTargetMenu {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$SelectedTargets,

        [Parameter(Mandatory = $true)]
        [int]$SelectedIndex
    )

    Clear-Host
    Write-Host '请选择构建目标（上下箭头移动，空格切换，Enter 确认，Esc 取消）' -ForegroundColor Cyan
    for ($optionIndex = 0; $optionIndex -lt $BuildTargetOptions.Count; $optionIndex++) {
        $targetOption = $BuildTargetOptions[$optionIndex]
        $cursor = if ($optionIndex -eq $SelectedIndex) { '>' } else { ' ' }
        $selection = if ($SelectedTargets -contains $targetOption) { '[x]' } else { '[ ]' }
        Write-Host "$cursor $selection $targetOption"
    }
}

function Select-BuildTargets {
    if ([Console]::IsInputRedirected -or [Console]::IsOutputRedirected) {
        throw '未传入 -Target 时需要交互式终端，请显式指定构建目标'
    }

    $selectedTargetSet = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    [void]$selectedTargetSet.Add('All')
    $selectedIndex = 0

    while ($true) {
        Show-BuildTargetMenu -SelectedTargets @($selectedTargetSet) -SelectedIndex $selectedIndex
        $pressedKey = [Console]::ReadKey($true).Key

        switch ($pressedKey) {
            ([ConsoleKey]::UpArrow) {
                $selectedIndex = ($selectedIndex + $BuildTargetOptions.Count - 1) % $BuildTargetOptions.Count
            }
            ([ConsoleKey]::DownArrow) {
                $selectedIndex = ($selectedIndex + 1) % $BuildTargetOptions.Count
            }
            ([ConsoleKey]::Spacebar) {
                $selectedTarget = $BuildTargetOptions[$selectedIndex]
                if ($selectedTarget -eq 'All') {
                    $selectedTargetSet.Clear()
                    [void]$selectedTargetSet.Add('All')
                }
                else {
                    [void]$selectedTargetSet.Remove('All')
                    if (-not $selectedTargetSet.Add($selectedTarget)) {
                        [void]$selectedTargetSet.Remove($selectedTarget)
                    }
                }
            }
            ([ConsoleKey]::Enter) {
                if ($selectedTargetSet.Count -gt 0) {
                    return @($BuildTargetOptions | Where-Object { $selectedTargetSet.Contains($_) })
                }
            }
            ([ConsoleKey]::Escape) {
                throw '已取消构建目标选择'
            }
        }
    }
}

function Resolve-BuildTargets {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$RequestedTargets,

        [bool]$HasRemoteLinuxPackage
    )

    $uniqueTargets = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($requestedTarget in $RequestedTargets) {
        [void]$uniqueTargets.Add($requestedTarget)
    }

    $includesAll = $uniqueTargets.Contains('All')
    if ($includesAll -and $uniqueTargets.Count -gt 2) {
        throw 'Target 为 All 时只能额外组合 Docker'
    }

    if ($HasRemoteLinuxPackage) {
        if (-not $uniqueTargets.Contains('Docker') -or $uniqueTargets.Count -ne 1) {
            throw '-LinuxPackageUrl 只能与 -Target Docker 单独使用'
        }

        return @('Docker')
    }

    $resolvedTargets = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    if ($includesAll) {
        @('Desktop', 'WindowsServer', 'Linux') | ForEach-Object { [void]$resolvedTargets.Add($_) }
    }
    else {
        foreach ($requestedTarget in $uniqueTargets) {
            [void]$resolvedTargets.Add($requestedTarget)
        }
    }

    # Docker 镜像必须基于 Linux 发布目录构建，单独指定 Docker 时自动补齐该依赖
    if ($resolvedTargets.Contains('Docker')) {
        [void]$resolvedTargets.Add('Linux')
    }

    return @($resolvedTargets)
}

function Remove-DevelopmentSettings {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PublishDirectory
    )

    $developmentSettings = Join-Path -Path $PublishDirectory -ChildPath 'appsettings.Development.json'
    if (Test-Path -LiteralPath $developmentSettings -PathType Leaf) {
        Remove-Item -LiteralPath $developmentSettings -Force
    }
}

function Copy-ServiceDockerFiles {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Context,

        [Parameter(Mandatory = $true)]
        [string]$ServiceDirectory
    )

    @(
        @{ Source = $Context.Dockerfile; Destination = 'Dockerfile' },
        @{ Source = $Context.DockerCompose; Destination = 'docker-compose.yml' },
        @{ Source = $Context.DockerEnvironment; Destination = '.env' }
    ) | ForEach-Object {
        Copy-Item -LiteralPath $_.Source -Destination (Join-Path -Path $ServiceDirectory -ChildPath $_.Destination) -Force
    }
}

function Publish-PluginProject {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Context,

        [Parameter(Mandatory = $true)]
        [string]$ServiceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$RuntimeIdentifier,

        [Parameter(Mandatory = $true)]
        [pscustomobject]$PluginProject
    )

    $pluginPublishRoot = Join-Path -Path $ServiceDirectory -ChildPath $PluginPublishStagingDirectoryName
    $pluginPublishDirectory = Join-Path -Path $pluginPublishRoot -ChildPath $PluginProject.DirectoryName
    $pluginDirectory = Join-Path -Path (Join-Path -Path $ServiceDirectory -ChildPath $PluginDirectoryName) -ChildPath $PluginProject.DirectoryName
    New-Item -ItemType Directory -Path $pluginDirectory -Force | Out-Null

    Write-BuildMessage -Message "发布插件 $($PluginProject.DirectoryName)"
    Invoke-BuildNativeCommand -Command dotnet -Arguments @(
        'publish', $PluginProject.ProjectPath, '-c', 'Release', '-o', $pluginPublishDirectory,
        '-r', $RuntimeIdentifier, '--self-contained', 'false'
    ) -WorkingDirectory $Context.ApiRoot

    $serviceAssemblyNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    Get-ChildItem -LiteralPath $ServiceDirectory -File -Filter '*.dll' | ForEach-Object {
        [void]$serviceAssemblyNames.Add($_.Name)
    }

    Get-ChildItem -LiteralPath $pluginPublishDirectory -File -Filter '*.dll' |
        Where-Object { -not $serviceAssemblyNames.Contains($_.Name) -and $_.Name -ne "$($PluginProject.AssemblyName).dll" } |
        ForEach-Object {
            Copy-Item -LiteralPath $_.FullName -Destination (Join-Path -Path $ServiceDirectory -ChildPath $PluginAssemblyDirectoryName) -Force
        }

    $pluginAssembly = Join-Path -Path $pluginPublishDirectory -ChildPath "$($PluginProject.AssemblyName).dll"
    if (-not (Test-Path -LiteralPath $pluginAssembly -PathType Leaf)) {
        throw "插件发布结果中未找到主程序集：$pluginAssembly"
    }
    Copy-Item -LiteralPath $pluginAssembly -Destination $pluginDirectory -Force

    Get-ChildItem -LiteralPath $pluginPublishDirectory -Recurse -File |
        Where-Object { $_.Extension -ne '.dll' } |
        ForEach-Object {
            $relativePath = [System.IO.Path]::GetRelativePath($pluginPublishDirectory, $_.FullName)
            $destinationPath = Join-Path -Path $pluginDirectory -ChildPath $relativePath
            New-Item -ItemType Directory -Path (Split-Path -Path $destinationPath -Parent) -Force | Out-Null
            Copy-Item -LiteralPath $_.FullName -Destination $destinationPath -Force
        }

    if (Test-Path -LiteralPath $pluginPublishRoot -PathType Container) {
        Remove-Item -LiteralPath $pluginPublishRoot -Recurse -Force
    }
}

function Publish-ServicePackage {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Context,

        [Parameter(Mandatory = $true)]
        [ValidateSet('win-x64', 'linux-x64')]
        [string]$RuntimeIdentifier,

        [Parameter(Mandatory = $true)]
        [string]$FrontendOutputDirectory,

        [Parameter(Mandatory = $true)]
        [pscustomobject[]]$PluginProjects
    )

    $serviceDirectory = Join-Path -Path $Context.ArtifactRoot -ChildPath "service-$RuntimeIdentifier"
    if (Test-Path -LiteralPath $serviceDirectory) {
        Remove-Item -LiteralPath $serviceDirectory -Recurse -Force
    }
    New-Item -ItemType Directory -Path $serviceDirectory -Force | Out-Null

    Write-BuildMessage -Message "发布服务端 ($RuntimeIdentifier)"
    Invoke-BuildNativeCommand -Command dotnet -Arguments @(
        'publish', $Context.ServiceProject, '-c', 'Release', '-o', $serviceDirectory,
        '-r', $RuntimeIdentifier, '--self-contained', 'false'
    ) -WorkingDirectory $Context.ApiRoot

    $ServicePublishDirectories | ForEach-Object {
        New-Item -ItemType Directory -Path (Join-Path -Path $serviceDirectory -ChildPath $_) -Force | Out-Null
    }
    Copy-Item -LiteralPath $Context.QuartzDatabase -Destination (Join-Path -Path $serviceDirectory -ChildPath 'data/db/quartz-sqlite.sqlite3') -Force
    Remove-DevelopmentSettings -PublishDirectory $serviceDirectory
    Copy-ServiceDockerFiles -Context $Context -ServiceDirectory $serviceDirectory

    if ($RuntimeIdentifier -eq 'win-x64') {
        Copy-Item -Path (Join-Path -Path $Context.WindowsServiceRoot -ChildPath '*') -Destination $serviceDirectory -Recurse -Force
    }

    foreach ($pluginProject in $PluginProjects) {
        Publish-PluginProject -Context $Context -ServiceDirectory $serviceDirectory -RuntimeIdentifier $RuntimeIdentifier -PluginProject $pluginProject
    }

    Copy-Item -Path (Join-Path -Path $FrontendOutputDirectory -ChildPath '*') -Destination (Join-Path -Path $serviceDirectory -ChildPath 'wwwroot') -Recurse -Force

    return [pscustomobject]@{
        RuntimeIdentifier = $RuntimeIdentifier
        Directory         = $serviceDirectory
        Version           = Get-BuildFileVersion -FilePath (Join-Path -Path $serviceDirectory -ChildPath 'UzonMailService.dll')
    }
}

function New-ServiceArchive {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Context,

        [Parameter(Mandatory = $true)]
        [pscustomobject]$ServicePackage
    )

    $archivePath = Join-Path -Path $Context.ArtifactRoot -ChildPath "uzonmail-service-$($ServicePackage.RuntimeIdentifier)-$($ServicePackage.Version).zip"
    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }

    if ($ServicePackage.RuntimeIdentifier -eq 'linux-x64') {
        Invoke-BuildNativeCommand -Command '7z.exe' -Arguments @('a', '-tzip', $archivePath, $ServicePackage.Directory)
        @($Context.DockerDeployScript, $Context.DockerCompose, $Context.DockerEnvironment) | ForEach-Object {
            Invoke-BuildNativeCommand -Command '7z.exe' -Arguments @('a', '-tzip', $archivePath, $_)
        }
        @('install.sh', 'uzon-mail.service') | ForEach-Object {
            Invoke-BuildNativeCommand -Command '7z.exe' -Arguments @('a', '-tzip', $archivePath, (Join-Path -Path $Context.LinuxServiceRoot -ChildPath $_))
        }
    }
    else {
        Invoke-BuildNativeCommand -Command '7z.exe' -Arguments @('a', '-tzip', $archivePath, (Join-Path -Path $ServicePackage.Directory -ChildPath '*'))
    }

    return $archivePath
}

function Publish-DesktopArchive {
    param(
        [Parameter(Mandatory = $true)]
        [pscustomobject]$Context,

        [Parameter(Mandatory = $true)]
        [pscustomobject]$WindowsServicePackage
    )

    $desktopDirectory = Join-Path -Path $Context.ArtifactRoot -ChildPath 'desktop'
    if (Test-Path -LiteralPath $desktopDirectory) {
        Remove-Item -LiteralPath $desktopDirectory -Recurse -Force
    }

    Write-BuildMessage -Message '发布 Windows 桌面端'
    Invoke-BuildNativeCommand -Command dotnet -Arguments @(
        'publish', $Context.DesktopProject, '-c', 'Release', '-o', $desktopDirectory,
        '-r', 'win-x64', '--self-contained', 'false'
    ) -WorkingDirectory $Context.DesktopRoot
    Remove-DevelopmentSettings -PublishDirectory $desktopDirectory

    $desktopServiceDirectory = Join-Path -Path $desktopDirectory -ChildPath 'service'
    New-Item -ItemType Directory -Path $desktopServiceDirectory -Force | Out-Null
    Copy-Item -Path (Join-Path -Path $WindowsServicePackage.Directory -ChildPath '*') -Destination $desktopServiceDirectory -Recurse -Force

    $desktopVersion = Get-BuildFileVersion -FilePath (Join-Path -Path $desktopDirectory -ChildPath 'UzonMailDesktop.exe')
    if ($desktopVersion -ne $WindowsServicePackage.Version) {
        throw "桌面端与服务端版本必须一致：$desktopVersion / $($WindowsServicePackage.Version)"
    }

    $updaterDirectory = Join-Path -Path $desktopDirectory -ChildPath 'UpdaterTmp'
    Write-BuildMessage -Message '发布桌面端更新器'
    Invoke-BuildNativeCommand -Command dotnet -Arguments @(
        'publish', $Context.UpdaterProject, '-c', 'Release', '-o', $updaterDirectory,
        '-r', 'win-x64', '--self-contained', 'true', '-p:PublishAot=true'
    ) -WorkingDirectory $Context.UpdaterRoot

    $manifestDirectory = if ($UpdateManifestOutputDirectory) {
        [System.IO.Path]::GetFullPath($UpdateManifestOutputDirectory)
    }
    else {
        Join-Path -Path $Context.ArtifactRoot -ChildPath 'updates'
    }
    New-Item -ItemType Directory -Path $manifestDirectory -Force | Out-Null
    $latestManifest = Join-Path -Path $manifestDirectory -ChildPath 'latest.json'
    $versionManifest = Join-Path -Path $manifestDirectory -ChildPath "$desktopVersion.json"
    Write-BuildMessage -Message '生成桌面端更新清单'
    Invoke-BuildNativeCommand -Command dotnet -Arguments @(
        'run', '--project', $Context.UpdaterProject, '-c', 'Release', '--', 'package',
        '--project-directory', $desktopDirectory,
        '--out', (Join-Path -Path $desktopDirectory -ChildPath 'appPackage.json'),
        '--out', $latestManifest,
        '--out', $versionManifest
    ) -WorkingDirectory $Context.UpdaterRoot

    $archivePath = Join-Path -Path $Context.ArtifactRoot -ChildPath "uzonmail-desktop-win-x64-$desktopVersion.zip"
    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
    }
    Invoke-BuildNativeCommand -Command '7z.exe' -Arguments @('a', '-tzip', $archivePath, (Join-Path -Path $desktopDirectory -ChildPath '*'))
    return $archivePath
}

function Get-RemoteLinuxPackage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$DownloadUrl
    )

    $temporaryRoot = Join-Path -Path ([System.IO.Path]::GetTempPath()) -ChildPath "uzonmail-build-$([System.Guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $temporaryRoot -Force | Out-Null

    try {
        $packagePath = Join-Path -Path $temporaryRoot -ChildPath 'linux-package.zip'
        Write-BuildMessage -Message "下载 Linux 安装包：$DownloadUrl"
        Invoke-WebRequest -Uri $DownloadUrl -OutFile $packagePath -UseBasicParsing

        $extractedRoot = Join-Path -Path $temporaryRoot -ChildPath 'extracted'
        Expand-Archive -LiteralPath $packagePath -DestinationPath $extractedRoot -Force
        $dockerfile = Get-ChildItem -LiteralPath $extractedRoot -File -Filter 'Dockerfile' -Recurse | Select-Object -First 1
        if (-not $dockerfile) {
            throw '下载的 Linux 安装包中未找到 Dockerfile'
        }

        return [pscustomobject]@{
            TemporaryRoot    = $temporaryRoot
            ServiceDirectory = $dockerfile.DirectoryName
            Version          = Get-BuildFileVersion -FilePath (Join-Path -Path $dockerfile.DirectoryName -ChildPath 'UzonMailService.dll')
        }
    }
    catch {
        Remove-Item -LiteralPath $temporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
        throw
    }
}

function Build-DockerImageInWsl {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ServiceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [string]$Distribution,

        [switch]$PushImage
    )

    $imageVersion = $Version -replace '\.0$', ''
    $imageName = Get-BuildImageName
    $versionedImage = "$imageName`:$imageVersion"
    $latestImage = "$imageName`:latest"
    $sourceDirectory = ConvertTo-WslPath -WindowsPath $ServiceDirectory -Distribution $Distribution
    $temporaryDirectory = (Invoke-WslBuildCommand -Distribution $Distribution -BashCommand 'mktemp -d /tmp/uzonmail-docker-build-XXXXXX' | Out-String).Trim()

    if ([string]::IsNullOrWhiteSpace($temporaryDirectory) -or -not $temporaryDirectory.StartsWith('/tmp/uzonmail-docker-build-')) {
        throw '无法创建安全的 WSL Docker 临时目录'
    }

    $quotedSourceDirectory = ConvertTo-BashSingleQuotedValue -Value $sourceDirectory
    $quotedTemporaryDirectory = ConvertTo-BashSingleQuotedValue -Value $temporaryDirectory
    $quotedVersionedImage = ConvertTo-BashSingleQuotedValue -Value $versionedImage
    $quotedLatestImage = ConvertTo-BashSingleQuotedValue -Value $latestImage

    try {
        Write-BuildMessage -Message "复制 Docker 构建上下文至 WSL：$temporaryDirectory"
        Invoke-WslBuildCommand -Distribution $Distribution -BashCommand "cp -a $quotedSourceDirectory/. $quotedTemporaryDirectory/"
        Invoke-WslBuildCommand -Distribution $Distribution -BashCommand "docker build -t $quotedVersionedImage -f $quotedTemporaryDirectory/Dockerfile $quotedTemporaryDirectory"
        Invoke-WslBuildCommand -Distribution $Distribution -BashCommand "docker tag $quotedVersionedImage $quotedLatestImage"

        if ($PushImage) {
            Write-BuildMessage -Message "登录 Docker Registry 并推送 $versionedImage"
            Invoke-WslBuildCommand -Distribution $Distribution -BashCommand 'docker login'
            Invoke-WslBuildCommand -Distribution $Distribution -BashCommand "docker push $quotedVersionedImage"
            Invoke-WslBuildCommand -Distribution $Distribution -BashCommand "docker push $quotedLatestImage"
        }
    }
    finally {
        try {
            Invoke-WslBuildCommand -Distribution $Distribution -BashCommand "rm -rf $quotedTemporaryDirectory"
        }
        catch {
            Write-BuildMessage -Level Warning -Message "无法清理 WSL 临时目录：$temporaryDirectory"
        }
    }

    return @($versionedImage, $latestImage)
}

function Build-DockerImageLocally {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ServiceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [switch]$PushImage
    )

    $imageVersion = $Version -replace '\.0$', ''
    $imageName = Get-BuildImageName
    $versionedImage = "$imageName`:$imageVersion"
    $latestImage = "$imageName`:latest"

    Invoke-BuildNativeCommand -Command docker -Arguments @('build', '-t', $versionedImage, '-f', 'Dockerfile', '.') -WorkingDirectory $ServiceDirectory
    Invoke-BuildNativeCommand -Command docker -Arguments @('tag', $versionedImage, $latestImage)

    if ($PushImage) {
        Write-BuildMessage -Message "登录 Docker Registry 并推送 $versionedImage"
        Invoke-BuildNativeCommand -Command docker -Arguments @('login')
        Invoke-BuildNativeCommand -Command docker -Arguments @('push', $versionedImage)
        Invoke-BuildNativeCommand -Command docker -Arguments @('push', $latestImage)
    }

    return @($versionedImage, $latestImage)
}

function Build-DockerImage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ServiceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [pscustomobject]$DockerEnvironment,

        [switch]$PushImage
    )

    if ($DockerEnvironment.UseWsl) {
        return Build-DockerImageInWsl -ServiceDirectory $ServiceDirectory -Version $Version -Distribution $DockerEnvironment.Distribution -PushImage:$PushImage
    }

    return Build-DockerImageLocally -ServiceDirectory $ServiceDirectory -Version $Version -PushImage:$PushImage
}

function Upload-BuildArtifacts {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$ArchivePaths
    )

    if ($ArchivePaths.Count -eq 0) {
        throw '当前目标没有可上传的安装包'
    }

    Assert-BuildCommand -CommandName od
    foreach ($archivePath in $ArchivePaths) {
        Write-BuildMessage -Message "上传安装包：$archivePath"
        Invoke-BuildNativeCommand -Command od -Arguments @('minio', 'soft', '-p', $archivePath)
    }
}

try {
    Assert-BuildCommand -CommandName git
    if (-not $Target -or $Target.Count -eq 0) {
        $Target = Select-BuildTargets
    }

    $buildContext = Resolve-BuildContext
    $resolvedTargets = Resolve-BuildTargets -RequestedTargets $Target -HasRemoteLinuxPackage (-not [string]::IsNullOrWhiteSpace($LinuxPackageUrl))
    $includesDocker = $resolvedTargets -contains 'Docker'
    $hasLocalPublishTarget = @('Desktop', 'WindowsServer', 'Linux') | Where-Object { $resolvedTargets -contains $_ }
    $pluginProjects = if ($hasLocalPublishTarget) { Get-BuildPluginProjects -PluginsRoot $buildContext.PluginsRoot } else { @() }
    $dockerBuildEnvironment = $null

    if ($PushDockerImage -and -not $includesDocker) {
        throw '-PushDockerImage 必须与 Docker 目标组合使用'
    }
    if ($UploadArtifacts -and -not [string]::IsNullOrWhiteSpace($LinuxPackageUrl)) {
        throw '远程 Linux 安装包仅用于 Docker 构建，不能与 -UploadArtifacts 组合使用'
    }

    $stageTotal = 1
    if ($UpdateSource) {
        $stageTotal++
    }
    if ($hasLocalPublishTarget) {
        $stageTotal += 2
    }
    if ($includesDocker) {
        $stageTotal++
    }
    if ($UploadArtifacts) {
        $stageTotal++
    }
    $stageIndex = 0

    Write-Host ''
    Write-Host '========================================' -ForegroundColor DarkCyan
    Write-Host ' UzonMail 统一构建' -ForegroundColor Cyan
    Write-Host '========================================' -ForegroundColor DarkCyan
    Write-BuildMessage -Level Detail -Message "仓库：$($buildContext.RepositoryRoot)"
    Write-BuildMessage -Level Detail -Message "目标：$($resolvedTargets -join ', ')"

    if ($UpdateSource) {
        $stageIndex++
        $stage = Start-BuildStage -Index $stageIndex -Total $stageTotal -Title '同步当前源码分支'
        Update-BuildSource -Context $buildContext
        Complete-BuildStage -Stopwatch $stage -Title '同步当前源码分支'
    }

    $stageIndex++
    $stage = Start-BuildStage -Index $stageIndex -Total $stageTotal -Title '检测构建环境'
    if ($hasLocalPublishTarget) {
        @('bun', 'dotnet', '7z.exe') | ForEach-Object { Assert-BuildCommand -CommandName $_ }
    }
    if ($includesDocker) {
        $dockerBuildEnvironment = Resolve-DockerBuildEnvironment -WslDistribution $WslDistribution
    }
    Complete-BuildStage -Stopwatch $stage -Title '检测构建环境'

    $archivePaths = [System.Collections.Generic.List[string]]::new()
    $servicePackages = @{}
    $frontendOutputDirectory = $null

    if ($hasLocalPublishTarget) {
        $stageIndex++
        $stage = Start-BuildStage -Index $stageIndex -Total $stageTotal -Title '构建前端'
        Invoke-BuildNativeCommand -Command bun -Arguments @('install', '--frozen-lockfile') -WorkingDirectory $buildContext.WebRoot
        Invoke-BuildNativeCommand -Command bun -Arguments @('run', 'build') -WorkingDirectory $buildContext.WebRoot
        $frontendOutputDirectory = Join-Path -Path $buildContext.WebRoot -ChildPath 'dist/spa'
        if (-not (Test-Path -LiteralPath $frontendOutputDirectory -PathType Container)) {
            throw "前端构建结果不存在：$frontendOutputDirectory"
        }
        Complete-BuildStage -Stopwatch $stage -Title '构建前端'

        $stageIndex++
        $stage = Start-BuildStage -Index $stageIndex -Total $stageTotal -Title '发布应用与安装包'
        if ($resolvedTargets -contains 'Desktop' -or $resolvedTargets -contains 'WindowsServer') {
            $servicePackages['win-x64'] = Publish-ServicePackage -Context $buildContext -RuntimeIdentifier 'win-x64' -FrontendOutputDirectory $frontendOutputDirectory -PluginProjects $pluginProjects
        }
        if ($resolvedTargets -contains 'Desktop') {
            $archivePaths.Add((Publish-DesktopArchive -Context $buildContext -WindowsServicePackage $servicePackages['win-x64']))
        }
        if ($resolvedTargets -contains 'WindowsServer') {
            $archivePaths.Add((New-ServiceArchive -Context $buildContext -ServicePackage $servicePackages['win-x64']))
        }
        if ($resolvedTargets -contains 'Linux') {
            $servicePackages['linux-x64'] = Publish-ServicePackage -Context $buildContext -RuntimeIdentifier 'linux-x64' -FrontendOutputDirectory $frontendOutputDirectory -PluginProjects $pluginProjects
            $archivePaths.Add((New-ServiceArchive -Context $buildContext -ServicePackage $servicePackages['linux-x64']))
        }
        Complete-BuildStage -Stopwatch $stage -Title '发布应用与安装包'
    }

    $dockerImages = @()
    $remoteLinuxPackage = $null
    try {
        if ($includesDocker) {
            $stageIndex++
            $dockerStage = Start-BuildStage -Index $stageIndex -Total $stageTotal -Title '构建 Docker 镜像'
            if ($LinuxPackageUrl) {
                $remoteLinuxPackage = Get-RemoteLinuxPackage -DownloadUrl $LinuxPackageUrl
                $dockerImages = Build-DockerImage -ServiceDirectory $remoteLinuxPackage.ServiceDirectory -Version $remoteLinuxPackage.Version -DockerEnvironment $dockerBuildEnvironment -PushImage:$PushDockerImage
            }
            else {
                $linuxServicePackage = $servicePackages['linux-x64']
                $dockerImages = Build-DockerImage -ServiceDirectory $linuxServicePackage.Directory -Version $linuxServicePackage.Version -DockerEnvironment $dockerBuildEnvironment -PushImage:$PushDockerImage
            }
            Complete-BuildStage -Stopwatch $dockerStage -Title '构建 Docker 镜像'
        }
    }
    finally {
        if ($remoteLinuxPackage) {
            Remove-Item -LiteralPath $remoteLinuxPackage.TemporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    if ($UploadArtifacts) {
        $stageIndex++
        $uploadStage = Start-BuildStage -Index $stageIndex -Total $stageTotal -Title '上传安装包'
        Upload-BuildArtifacts -ArchivePaths $archivePaths.ToArray()
        Complete-BuildStage -Stopwatch $uploadStage -Title '上传安装包'
    }

    Write-Host ''
    Write-Host '========== 构建完成 ==========' -ForegroundColor Green
    if ($archivePaths.Count -gt 0) {
        Write-BuildMessage -Level Success -Message '安装包：'
        $archivePaths | ForEach-Object { Write-BuildMessage -Level Detail -Message $_ }
    }
    if ($dockerImages.Count -gt 0) {
        Write-BuildMessage -Level Success -Message 'Docker 镜像：'
        $dockerImages | ForEach-Object { Write-BuildMessage -Level Detail -Message $_ }
    }
}
catch {
    Write-Host ''
    Write-BuildMessage -Level Error -Message $_.Exception.Message
    exit 1
}
