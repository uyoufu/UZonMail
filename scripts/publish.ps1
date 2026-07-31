<#
.SYNOPSIS
更新版本、构建发布产物并发布版本文档和 Git 标签。

.DESCRIPTION
发布步骤在同一 PowerShell 进程内组合现有脚本，使版本文件回滚由此入口统一负责。
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Version,

    [switch]$Help
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8

$ReleaseBranch = 'master'
$OriginRemoteName = 'origin'
$VersionTagPrefix = 'v'
$VersionSourcePaths = @(
    'src/api/UZonMailService/UzonMailService.csproj',
    'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj',
    'src/web/src/config/app.config.ts'
)
$VersionUpdateScriptPath = Join-Path -Path $PSScriptRoot -ChildPath 'internal/update-release-version.ps1'
$BuildScriptPath = Join-Path -Path $PSScriptRoot -ChildPath 'build.ps1'
$VersionDocumentScriptPath = Join-Path -Path $PSScriptRoot -ChildPath 'new-version-doc.ps1'

function Show-PublishUsage {
    <#
    .SYNOPSIS
    输出发布脚本的命令行用法。
    #>
    Write-Host @'
用法：pwsh -File scripts/publish.ps1 [vX.Y.Z]

不传版本号时，脚本会提示输入。直接回车将使用上一个发布标签的下一 patch 版本。
'@
}

function Assert-PublishCommandExists {
    <#
    .SYNOPSIS
    确认发布流程依赖的命令可用。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$CommandName
    )

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "未检测到 $CommandName，请先安装后再执行"
    }
}

function Assert-PublishGitSuccess {
    <#
    .SYNOPSIS
    将 Git 的非零退出码转换为终止错误，避免发布流程继续执行。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$Step
    )

    if ($LASTEXITCODE -ne 0) {
        throw "$Step 失败，已中止"
    }
}

function Get-NormalizedReleaseVersion {
    <#
    .SYNOPSIS
    验证并移除发布版本号的可选 v 前缀。
    #>
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

function Get-NextPatchVersion {
    <#
    .SYNOPSIS
    计算三段式版本的下一 patch 版本。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$NormalizedVersion
    )

    $versionComponents = $NormalizedVersion.Split('.')
    $nextPatch = [System.Numerics.BigInteger]::Parse($versionComponents[2]) + [System.Numerics.BigInteger]::One

    return "$($versionComponents[0]).$($versionComponents[1]).$nextPatch"
}

function Test-IsReleaseVersionGreaterThan {
    <#
    .SYNOPSIS
    按数值比较两个三段式版本号，避免字符串排序导致错误的发布判断。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$CandidateVersion,

        [Parameter(Mandatory = $true)]
        [string]$ReferenceVersion
    )

    $candidateComponents = $CandidateVersion.Split('.')
    $referenceComponents = $ReferenceVersion.Split('.')
    for ($componentIndex = 0; $componentIndex -lt 3; $componentIndex++) {
        $candidateComponent = [System.Numerics.BigInteger]::Parse($candidateComponents[$componentIndex])
        $referenceComponent = [System.Numerics.BigInteger]::Parse($referenceComponents[$componentIndex])
        if ($candidateComponent -ne $referenceComponent) {
            return $candidateComponent -gt $referenceComponent
        }
    }

    return $false
}

function Get-PreviousReleaseVersionTag {
    <#
    .SYNOPSIS
    返回当前 HEAD 可达的最近一个有效三段式版本标签。
    #>
    $versionTagCandidates = @(& git tag --merged HEAD --list "$VersionTagPrefix*")
    Assert-PublishGitSuccess -Step '获取当前提交可达的版本标签'

    $nearestCommitDistance = $null
    $nearestVersionTags = [System.Collections.Generic.List[string]]::new()
    foreach ($candidateTag in $versionTagCandidates) {
        try {
            Get-NormalizedReleaseVersion -InputVersion $candidateTag | Out-Null
        }
        catch {
            continue
        }

        $commitDistanceOutput = @(& git rev-list --count "$candidateTag..HEAD")
        Assert-PublishGitSuccess -Step "计算版本标签 $candidateTag 到当前提交的距离"
        if ($commitDistanceOutput.Count -ne 1) {
            throw "无法解析版本标签 $candidateTag 到当前提交的距离：$($commitDistanceOutput -join ' ')"
        }

        $commitDistance = $commitDistanceOutput[0].Trim()
        $parsedCommitDistance = 0L
        if (-not [long]::TryParse($commitDistance, [ref]$parsedCommitDistance)) {
            throw "无法解析版本标签 $candidateTag 到当前提交的距离：$commitDistance"
        }
        if ($null -eq $nearestCommitDistance -or $parsedCommitDistance -lt $nearestCommitDistance) {
            $nearestCommitDistance = $parsedCommitDistance
            $nearestVersionTags.Clear()
            [void]$nearestVersionTags.Add($candidateTag)
        }
        elseif ($parsedCommitDistance -eq $nearestCommitDistance) {
            [void]$nearestVersionTags.Add($candidateTag)
        }
    }

    if ($nearestVersionTags.Count -eq 0) {
        throw '当前提交历史中未找到 vX.Y.Z 格式的版本标签，无法确定发布版本'
    }
    if ($nearestVersionTags.Count -ne 1) {
        throw "找到多个距离当前提交相同的版本标签：$($nearestVersionTags -join ' ')，无法确定上一个版本"
    }

    return $nearestVersionTags[0]
}

function Assert-PublishWorktreeClean {
    <#
    .SYNOPSIS
    阻止携带已有变更的工作区进入发布流程。
    #>
    $worktreeChanges = @(& git status --porcelain --untracked-files=all)
    Assert-PublishGitSuccess -Step '检查工作区状态'
    if ($worktreeChanges.Count -gt 0) {
        throw '工作区存在未提交变更，请先提交、暂存或清理后再发布'
    }
}

function Assert-OnlyVersionFilesAreModified {
    <#
    .SYNOPSIS
    限制构建前允许产生的工作区变更，防止提交意外文件。
    #>
    $changedPaths = @(& git diff --name-only)
    Assert-PublishGitSuccess -Step '检查未暂存文件变更'
    foreach ($changedPath in $changedPaths) {
        if ($VersionSourcePaths -notcontains $changedPath) {
            throw "构建产生了未预期的受跟踪文件变更：$changedPath"
        }
    }

    $stagedPaths = @(& git diff --cached --name-only)
    Assert-PublishGitSuccess -Step '检查暂存区变更'
    if ($stagedPaths.Count -gt 0) {
        throw '构建过程中产生了暂存区变更，请检查后再发布'
    }

    $untrackedPaths = @(& git ls-files --others --exclude-standard)
    Assert-PublishGitSuccess -Step '检查未跟踪文件'
    if ($untrackedPaths.Count -gt 0) {
        throw '构建过程中产生了未跟踪文件，请检查后再发布'
    }
}

function Restore-UncommittedVersionUpdate {
    <#
    .SYNOPSIS
    在版本提交前发布失败时恢复由版本更新脚本产生的文件变更。
    #>
    & git restore --source=HEAD -- $VersionSourcePaths 2>$null | Out-Null
}

function Invoke-PublishRelease {
    <#
    .SYNOPSIS
    执行完整发布流程，并在版本提交前发生失败时恢复版本文件。
    #>
    param(
        [string]$RequestedVersion
    )

    $hasCommittedVersionUpdate = $false
    $hasCompletedPublish = $false
    $hasEnteredRepositoryRoot = $false
    try {
        Assert-PublishCommandExists -CommandName 'git'

        $repositoryRootOutput = @(& git rev-parse --show-toplevel)
        Assert-PublishGitSuccess -Step '获取 Git 仓库根目录'
        if ($repositoryRootOutput.Count -ne 1 -or [string]::IsNullOrWhiteSpace($repositoryRootOutput[0])) {
            throw '无法解析 Git 仓库根目录'
        }
        $repositoryRoot = $repositoryRootOutput[0].Trim()
        Push-Location -LiteralPath $repositoryRoot
        $hasEnteredRepositoryRoot = $true

        & git remote get-url $OriginRemoteName | Out-Null
        Assert-PublishGitSuccess -Step "检查 $OriginRemoteName 远程仓库"
        & git fetch --tags $OriginRemoteName
        Assert-PublishGitSuccess -Step '同步远程版本标签'
        Assert-PublishWorktreeClean
        & git switch $ReleaseBranch
        Assert-PublishGitSuccess -Step "切换到 $ReleaseBranch 分支"
        & git pull --ff-only $OriginRemoteName $ReleaseBranch
        Assert-PublishGitSuccess -Step "同步 $ReleaseBranch 分支"
        Assert-PublishWorktreeClean

        $previousVersionTag = Get-PreviousReleaseVersionTag
        $previousVersion = $previousVersionTag.Substring($VersionTagPrefix.Length)
        $defaultVersion = Get-NextPatchVersion -NormalizedVersion $previousVersion
        if ([string]::IsNullOrWhiteSpace($RequestedVersion)) {
            $versionInput = Read-Host "请输入本次版本号（直接回车使用 $defaultVersion）"
            if ([string]::IsNullOrWhiteSpace($versionInput)) {
                $releaseVersion = $defaultVersion
                Write-Host "使用默认版本号：$releaseVersion"
            }
            else {
                $releaseVersion = Get-NormalizedReleaseVersion -InputVersion $versionInput
                Write-Host "使用输入的版本号：$releaseVersion"
            }
        }
        else {
            $releaseVersion = Get-NormalizedReleaseVersion -InputVersion $RequestedVersion
            Write-Host "使用命令行指定的版本号：$releaseVersion"
        }

        if (-not (Test-IsReleaseVersionGreaterThan -CandidateVersion $releaseVersion -ReferenceVersion $previousVersion)) {
            throw "发布版本必须高于上一个版本 $previousVersion"
        }

        & git rev-parse -q --verify "refs/tags/$VersionTagPrefix$releaseVersion" *> $null
        if ($LASTEXITCODE -eq 0) {
            throw "标签 $VersionTagPrefix$releaseVersion 已存在"
        }
        if ($LASTEXITCODE -ne 1) {
            throw "检查标签 $VersionTagPrefix$releaseVersion 是否存在失败，已中止"
        }

        & $VersionUpdateScriptPath -Version $releaseVersion -RepositoryRoot $repositoryRoot
        Assert-OnlyVersionFilesAreModified
        & $BuildScriptPath -Target All -PushDockerImage -UploadArtifacts
        Assert-OnlyVersionFilesAreModified

        & git diff --quiet -- $VersionSourcePaths
        if ($LASTEXITCODE -eq 1) {
            & git add -- $VersionSourcePaths
            Assert-PublishGitSuccess -Step '暂存版本文件'
            $fullVersion = "$releaseVersion.0"
            & git commit -m "Build: Bump version to $fullVersion"
            Assert-PublishGitSuccess -Step '提交版本文件'
        }
        elseif ($LASTEXITCODE -ne 0) {
            throw '检查版本文件变更失败，已中止'
        }
        $hasCommittedVersionUpdate = $true

        & $VersionDocumentScriptPath -Version $releaseVersion
        & git push $OriginRemoteName $ReleaseBranch
        Assert-PublishGitSuccess -Step "推送 $ReleaseBranch 分支"
        & git tag "$VersionTagPrefix$releaseVersion"
        Assert-PublishGitSuccess -Step '创建发布版本标签'
        & git push $OriginRemoteName "$VersionTagPrefix$releaseVersion"
        Assert-PublishGitSuccess -Step '推送发布版本标签'

        $hasCompletedPublish = $true
        Write-Host "发布完成：$releaseVersion"
    }
    finally {
        if (-not $hasCompletedPublish -and -not $hasCommittedVersionUpdate -and $hasEnteredRepositoryRoot) {
            Restore-UncommittedVersionUpdate
        }
        if ($hasEnteredRepositoryRoot) {
            Pop-Location
        }
    }
}

if ($Help) {
    Show-PublishUsage
    return
}

try {
    Invoke-PublishRelease -RequestedVersion $Version
}
catch {
    throw [System.InvalidOperationException]::new("发布失败：$($_.Exception.Message)", $_.Exception)
}
