<#
.SYNOPSIS
将版本更新 Markdown 正文写入指定语言的下载文档
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$UpdatePath,

    [Parameter(ValueFromPipeline = $true)]
    [AllowEmptyString()]
    [string]$MarkdownContent,

    [string]$RepositoryRoot
)

begin {
    $markdownLines = [System.Collections.Generic.List[string]]::new()
}

process {
    if ($null -ne $MarkdownContent) {
        [void]$markdownLines.Add($MarkdownContent)
    }
}

end {
    $ErrorActionPreference = 'Stop'
    Set-StrictMode -Version Latest

    function Get-NormalizedReleaseVersion {
        param(
            [Parameter(Mandatory = $true)]
            [string]$InputVersion
        )

        $versionMatch = [regex]::Match($InputVersion.Trim(), '^v?(\d+\.\d+\.\d+)$')
        if (-not $versionMatch.Success) {
            throw '版本号格式不正确，请输入 x.y.z，例如 0.22.1'
        }

        return $versionMatch.Groups[1].Value
    }

    function Get-DocumentConfiguration {
        param(
            [Parameter(Mandatory = $true)]
            [string]$NormalizedUpdatePath
        )

        $documentConfigurations = @{
            'docs/docs/downloads.md' = [pscustomobject]@{
                ReleaseDateLabel = '更新时期'
                DownloadHeading  = '下载地址'
            }
            'docs/docs/en/downloads.md' = [pscustomobject]@{
                ReleaseDateLabel = 'Release Date'
                DownloadHeading  = 'Downloads'
            }
        }

        if (-not $documentConfigurations.ContainsKey($NormalizedUpdatePath)) {
            throw "不支持的文档更新路径：$NormalizedUpdatePath"
        }

        return $documentConfigurations[$NormalizedUpdatePath]
    }

    function Get-DocumentNewLine {
        param(
            [Parameter(Mandatory = $true)]
            [string]$DocumentContent
        )

        if ($DocumentContent.Contains("`r`n")) {
            return "`r`n"
        }

        return "`n"
    }

    function Get-ReleasePackage {
        param(
            [Parameter(Mandatory = $true)]
            [string]$NormalizedVersion,

            [Parameter(Mandatory = $true)]
            [string]$BuildUpdatesDirectory
        )

        $fallbackVersion = "$NormalizedVersion.0"
        if (-not (Test-Path -LiteralPath $BuildUpdatesDirectory -PathType Container)) {
            Write-Warning "未找到更新清单目录，将使用默认构建号：$fallbackVersion"
            return [pscustomobject]@{ FullVersion = $fallbackVersion; ManifestPath = $null }
        }

        $manifestNamePattern = '^' + [regex]::Escape($NormalizedVersion) + '\.\d+\.json$'
        $matchingManifests = @(
            Get-ChildItem -LiteralPath $BuildUpdatesDirectory -File |
                Where-Object { $_.Name -match $manifestNamePattern }
        )
        if ($matchingManifests.Count -eq 0) {
            Write-Warning "未找到 $NormalizedVersion 对应的更新清单，将使用默认构建号：$fallbackVersion"
            return [pscustomobject]@{ FullVersion = $fallbackVersion; ManifestPath = $null }
        }

        if ($matchingManifests.Count -ne 1) {
            throw "找到多个 $NormalizedVersion 对应的更新清单，无法确定要发布的构建版本"
        }

        $manifest = $matchingManifests[0]
        try {
            $manifestContent = Get-Content -LiteralPath $manifest.FullName -Raw -Encoding utf8 | ConvertFrom-Json
        }
        catch {
            throw "无法解析更新清单 $($manifest.FullName)：$($_.Exception.Message)"
        }

        $fullVersion = [string]$manifestContent.version
        if ($fullVersion -notmatch ('^' + [regex]::Escape($NormalizedVersion) + '\.\d+$')) {
            throw "更新清单中的版本号 $fullVersion 与请求版本 $NormalizedVersion 不匹配"
        }

        if ($manifest.Name -ne "$fullVersion.json") {
            throw "更新清单文件名 $($manifest.Name) 与其版本号 $fullVersion 不一致"
        }

        return [pscustomobject]@{ FullVersion = $fullVersion; ManifestPath = $manifest.FullName }
    }

    function New-ReleaseEntry {
        param(
            [Parameter(Mandatory = $true)]
            [string]$NormalizedVersion,

            [Parameter(Mandatory = $true)]
            [string]$FullVersion,

            [Parameter(Mandatory = $true)]
            [string]$MarkdownBody,

            [Parameter(Mandatory = $true)]
            [pscustomobject]$DocumentConfiguration,

            [Parameter(Mandatory = $true)]
            [string]$NewLine
        )

        $normalizedBody = ($MarkdownBody.Trim() -replace "`r?`n", $NewLine)
        if ([string]::IsNullOrWhiteSpace($normalizedBody)) {
            throw '版本更新 Markdown 内容不能为空'
        }

        if ($normalizedBody -match '(?m)^##\s+' -or
            $normalizedBody -match '(?m)^>\s*(更新时期|Release Date):?' -or
            $normalizedBody -match '(?m)^###\s*(下载地址|Downloads)\s*$') {
            throw 'Markdown 内容只能包含分类正文，不能包含版本、日期或下载地址'
        }

        $downloadBaseUrl = 'https://oss.uzoncloud.com:2234/public/files/soft'
        $entryLines = @(
            "## $NormalizedVersion",
            '',
            "> $($DocumentConfiguration.ReleaseDateLabel): $(Get-Date -Format 'yyyy-MM-dd')",
            '',
            $normalizedBody,
            '',
            "### $($DocumentConfiguration.DownloadHeading)",
            '',
            "[uzonmail-desktop-win-x64-$FullVersion.zip]($downloadBaseUrl/uzonmail-desktop-win-x64-$FullVersion.zip)",
            '',
            "[uzonmail-service-win-x64-$FullVersion.zip]($downloadBaseUrl/uzonmail-service-win-x64-$FullVersion.zip)",
            '',
            "[uzonmail-service-linux-x64-$FullVersion.zip]($downloadBaseUrl/uzonmail-service-linux-x64-$FullVersion.zip)",
            '',
            '[docker](https://hub.docker.com/r/gmxgalens/uzon-mail/tags)'
        )

        return (($entryLines -join $NewLine).TrimEnd() + $NewLine)
    }

    function Update-ReleaseDocument {
        param(
            [Parameter(Mandatory = $true)]
            [string]$DocumentPath,

            [Parameter(Mandatory = $true)]
            [string]$NormalizedVersion,

            [Parameter(Mandatory = $true)]
            [string]$ReleaseEntry
        )

        if (-not (Test-Path -LiteralPath $DocumentPath -PathType Leaf)) {
            throw "目标文档不存在：$DocumentPath"
        }

        $documentContent = Get-Content -LiteralPath $DocumentPath -Raw -Encoding utf8
        $newLine = Get-DocumentNewLine -DocumentContent $documentContent
        $escapedVersion = [regex]::Escape($NormalizedVersion)
        $versionBlockPattern = "(?ms)(?<entry>^## $escapedVersion\r?\n.*?)(?<separator>\r?\n(?=^## \d+\.\d+\.\d+(?:\r?\n|$))|\z)"
        $existingEntry = [regex]::Match($documentContent, $versionBlockPattern)

        if ($existingEntry.Success) {
            $newDocumentContent = $documentContent.Substring(0, $existingEntry.Index) +
                $ReleaseEntry +
                $existingEntry.Groups['separator'].Value +
                $documentContent.Substring($existingEntry.Index + $existingEntry.Length)
            $updateAction = '更新'
        }
        else {
            $firstVersion = [regex]::Match($documentContent, '(?m)^## \d+\.\d+\.\d+(?:\r?\n|$)')
            if ($firstVersion.Success) {
                $prefix = $documentContent.Substring(0, $firstVersion.Index)
                $prefixSeparator = if ($prefix.EndsWith($newLine)) { '' } else { $newLine }
                $newDocumentContent = $prefix + $prefixSeparator + $ReleaseEntry + $newLine + $documentContent.Substring($firstVersion.Index)
            }
            else {
                $newDocumentContent = $documentContent.TrimEnd() + $newLine + $newLine + $ReleaseEntry
            }

            $updateAction = '新增'
        }

        if ($newDocumentContent -ceq $documentContent) {
            Write-Host "版本 $NormalizedVersion 的文档内容未变化。"
            return
        }

        [System.IO.File]::WriteAllText($DocumentPath, $newDocumentContent, [System.Text.UTF8Encoding]::new($false))
        Write-Host "已${updateAction}版本 $NormalizedVersion 的文档记录：$DocumentPath"
    }

    function Copy-ReleaseManifest {
        param(
            [Parameter(Mandatory = $true)]
            [pscustomobject]$ReleasePackage,

            [Parameter(Mandatory = $true)]
            [string]$PublicUpdatesDirectory
        )

        if ([string]::IsNullOrWhiteSpace($ReleasePackage.ManifestPath)) {
            return
        }

        New-Item -ItemType Directory -Path $PublicUpdatesDirectory -Force | Out-Null
        $versionManifestPath = Join-Path -Path $PublicUpdatesDirectory -ChildPath "$($ReleasePackage.FullVersion).json"
        $latestManifestPath = Join-Path -Path $PublicUpdatesDirectory -ChildPath 'latest.json'
        Copy-Item -LiteralPath $ReleasePackage.ManifestPath -Destination $versionManifestPath -Force
        Copy-Item -LiteralPath $ReleasePackage.ManifestPath -Destination $latestManifestPath -Force
        Write-Host "已同步更新清单：$versionManifestPath 和 $latestManifestPath"
    }

    $normalizedVersion = Get-NormalizedReleaseVersion -InputVersion $Version
    $resolvedRepositoryRoot = if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
        Split-Path -Path $PSScriptRoot -Parent
    }
    else {
        [System.IO.Path]::GetFullPath($RepositoryRoot)
    }
    if (-not (Test-Path -LiteralPath $resolvedRepositoryRoot -PathType Container)) {
        throw "仓库根目录不存在：$resolvedRepositoryRoot"
    }

    $normalizedUpdatePath = $UpdatePath.Replace('\', '/').TrimStart('/')
    $documentConfiguration = Get-DocumentConfiguration -NormalizedUpdatePath $normalizedUpdatePath
    $documentPath = [System.IO.Path]::GetFullPath((Join-Path -Path $resolvedRepositoryRoot -ChildPath $normalizedUpdatePath))
    $repositoryPathPrefix = $resolvedRepositoryRoot.TrimEnd('\', '/') + [System.IO.Path]::DirectorySeparatorChar
    if (-not $documentPath.StartsWith($repositoryPathPrefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "文档更新路径必须位于仓库内：$UpdatePath"
    }

    $markdownBody = ($markdownLines -join [Environment]::NewLine)
    $buildUpdatesDirectory = Join-Path -Path $resolvedRepositoryRoot -ChildPath 'build/updates'
    $releasePackage = Get-ReleasePackage -NormalizedVersion $normalizedVersion -BuildUpdatesDirectory $buildUpdatesDirectory
    $documentContent = Get-Content -LiteralPath $documentPath -Raw -Encoding utf8
    $newLine = Get-DocumentNewLine -DocumentContent $documentContent
    $releaseEntry = New-ReleaseEntry -NormalizedVersion $normalizedVersion -FullVersion $releasePackage.FullVersion -MarkdownBody $markdownBody -DocumentConfiguration $documentConfiguration -NewLine $newLine

    Update-ReleaseDocument -DocumentPath $documentPath -NormalizedVersion $normalizedVersion -ReleaseEntry $releaseEntry
    $publicUpdatesDirectory = Join-Path -Path $resolvedRepositoryRoot -ChildPath 'docs/docs/.vuepress/public/updates'
    Copy-ReleaseManifest -ReleasePackage $releasePackage -PublicUpdatesDirectory $publicUpdatesDirectory
}
