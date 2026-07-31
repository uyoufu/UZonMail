<#
.SYNOPSIS
将发布版本同步到服务端、桌面端和前端配置
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-NormalizedReleaseVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$InputVersion
    )

    $versionMatch = [regex]::Match($InputVersion.Trim(), '^v?(\d+\.\d+\.\d+)$')
    if (-not $versionMatch.Success) {
        throw '版本号格式不正确，请输入 x.y.z，例如 0.23.2'
    }

    return $versionMatch.Groups[1].Value
}

function Get-ResolvedRepositoryRoot {
    param(
        [string]$InputRepositoryRoot
    )

    $resolvedRepositoryRoot = if ([string]::IsNullOrWhiteSpace($InputRepositoryRoot)) {
        Split-Path -Path (Split-Path -Path $PSScriptRoot -Parent) -Parent
    }
    else {
        [System.IO.Path]::GetFullPath($InputRepositoryRoot)
    }
    if (-not (Test-Path -LiteralPath $resolvedRepositoryRoot -PathType Container)) {
        throw "仓库根目录不存在：$resolvedRepositoryRoot"
    }

    return $resolvedRepositoryRoot
}

function Set-ProjectVersionProperties {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [Parameter(Mandatory = $true)]
        [string]$FullVersion
    )

    if (-not (Test-Path -LiteralPath $ProjectPath -PathType Leaf)) {
        throw "项目文件不存在：$ProjectPath"
    }

    $projectDocument = [System.Xml.XmlDocument]::new()
    $projectDocument.PreserveWhitespace = $true
    try {
        $projectDocument.Load($ProjectPath)
    }
    catch {
        throw "无法解析项目文件 $ProjectPath：$($_.Exception.Message)"
    }

    foreach ($propertyName in @('FileVersion', 'AssemblyVersion')) {
        $propertyNodes = @($projectDocument.SelectNodes("/Project/PropertyGroup/$propertyName"))
        if ($propertyNodes.Count -ne 1) {
            throw "项目文件必须且只能包含一个 $propertyName：$ProjectPath"
        }

        $propertyNodes[0].InnerText = $FullVersion
    }

    $writerSettings = [System.Xml.XmlWriterSettings]::new()
    $writerSettings.Encoding = [System.Text.UTF8Encoding]::new($false)
    $writerSettings.Indent = $false
    $writerSettings.NewLineHandling = [System.Xml.NewLineHandling]::None
    $writerSettings.OmitXmlDeclaration = $projectDocument.FirstChild -isnot [System.Xml.XmlDeclaration]
    $projectWriter = [System.Xml.XmlWriter]::Create($ProjectPath, $writerSettings)
    try {
        $projectDocument.Save($projectWriter)
    }
    finally {
        $projectWriter.Dispose()
    }
}

function Set-FrontendDefaultVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ConfigurationPath,

        [Parameter(Mandatory = $true)]
        [string]$FullVersion
    )

    if (-not (Test-Path -LiteralPath $ConfigurationPath -PathType Leaf)) {
        throw "前端配置文件不存在：$ConfigurationPath"
    }

    $configurationContent = Get-Content -LiteralPath $ConfigurationPath -Raw -Encoding utf8
    $defaultConfigurationMatch = [regex]::Match(
        $configurationContent,
        '(?ms)^[ \t]*default\s*:\s*\{(?<defaultConfiguration>.*?^[ \t]{2}\},)'
    )
    if (-not $defaultConfigurationMatch.Success) {
        throw "前端配置中未找到 default 配置块：$ConfigurationPath"
    }

    $versionMatches = @([regex]::Matches(
            $defaultConfigurationMatch.Groups['defaultConfiguration'].Value,
            '(?m)^[ \t]*version\s*:\s*[\''](?<version>[^\'']+)[\'']\s*,?\s*$'
        ))
    if ($versionMatches.Count -ne 1) {
        throw "default 配置块必须且只能包含一个 version：$ConfigurationPath"
    }

    $versionValue = $versionMatches[0].Groups['version']
    $versionValueStart = $defaultConfigurationMatch.Groups['defaultConfiguration'].Index + $versionValue.Index
    $updatedConfigurationContent = $configurationContent.Substring(0, $versionValueStart) +
        $FullVersion +
        $configurationContent.Substring($versionValueStart + $versionValue.Length)
    [System.IO.File]::WriteAllText(
        $ConfigurationPath,
        $updatedConfigurationContent,
        [System.Text.UTF8Encoding]::new($false)
    )
}

$normalizedVersion = Get-NormalizedReleaseVersion -InputVersion $Version
$fullVersion = "$normalizedVersion.0"
$repositoryRoot = Get-ResolvedRepositoryRoot -InputRepositoryRoot $RepositoryRoot

Set-ProjectVersionProperties -ProjectPath (Join-Path -Path $repositoryRoot -ChildPath 'src/api/UZonMailService/UzonMailService.csproj') -FullVersion $fullVersion
Set-ProjectVersionProperties -ProjectPath (Join-Path -Path $repositoryRoot -ChildPath 'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj') -FullVersion $fullVersion
Set-FrontendDefaultVersion -ConfigurationPath (Join-Path -Path $repositoryRoot -ChildPath 'src/web/src/config/app.config.ts') -FullVersion $fullVersion
