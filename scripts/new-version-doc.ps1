<#
.SYNOPSIS
生成版本更新内容并发布文档分支
#>
[CmdletBinding()]
param(
    [string]$Version,

    [string]$TargetBranch = 'docs'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
[Console]::InputEncoding = [System.Text.Encoding]::UTF8

function Assert-CommandExists {
    param(
        [Parameter(Mandatory = $true)]
        [string]$CommandName
    )

    if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
        throw "未检测到 $CommandName，请先安装后再执行"
    }
}

function Assert-GitSuccess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Step
    )

    if ($LASTEXITCODE -ne 0) {
        throw "$Step 失败，已中止"
    }
}

function Read-MultiLineInput {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Prompt
    )

    Write-Host $Prompt -ForegroundColor Yellow
    Write-Host '输入空行结束。' -ForegroundColor DarkGray

    $inputLines = [System.Collections.Generic.List[string]]::new()
    while ($true) {
        $inputLine = Read-Host
        if ([string]::IsNullOrWhiteSpace($inputLine)) {
            break
        }

        [void]$inputLines.Add($inputLine)
    }

    return ($inputLines -join [Environment]::NewLine).Trim()
}

function Get-NormalizedVersion {
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

function Get-DesktopDocumentVersion {
    <#
    .SYNOPSIS
    从桌面项目文件读取用于发布文档的三段版本号
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath
    )

    if (-not (Test-Path -LiteralPath $ProjectPath -PathType Leaf)) {
        throw "桌面项目文件不存在：$ProjectPath"
    }

    try {
        [xml]$project = Get-Content -LiteralPath $ProjectPath -Raw -Encoding utf8
    }
    catch {
        throw "无法解析桌面项目文件 $ProjectPath：$($_.Exception.Message)"
    }

    $fileVersionNode = $project.SelectSingleNode('/Project/PropertyGroup/FileVersion')
    if ($null -eq $fileVersionNode -or [string]::IsNullOrWhiteSpace($fileVersionNode.InnerText)) {
        throw "桌面项目文件未配置 FileVersion：$ProjectPath"
    }

    $versionMatch = [regex]::Match($fileVersionNode.InnerText.Trim(), '^(\d+\.\d+\.\d+)\.\d+$')
    if (-not $versionMatch.Success) {
        throw "桌面项目 FileVersion 必须为 x.y.z.build 格式：$($fileVersionNode.InnerText.Trim())"
    }

    return $versionMatch.Groups[1].Value
}

Assert-CommandExists -CommandName 'git'
Assert-CommandExists -CommandName 'opencode'

$repositoryRoot = & git rev-parse --show-toplevel
Assert-GitSuccess -Step '获取仓库根目录'

Push-Location -Path $repositoryRoot
try {
    $currentBranch = & git branch --show-current
    Assert-GitSuccess -Step '获取当前分支'

    if ($currentBranch -ne 'master') {
        Write-Host '切换到 master 分支...' -ForegroundColor Yellow
        & git switch master
        Assert-GitSuccess -Step '切换到 master 分支'
    }

    Write-Host '拉取 master 最新更新...' -ForegroundColor Yellow
    & git pull --ff-only origin master
    Assert-GitSuccess -Step '拉取 master 最新更新'

    $docsDirectory = Join-Path -Path $repositoryRoot -ChildPath 'docs'
    $docsDirty = & git status --porcelain -- docs
    Assert-GitSuccess -Step '检查 docs 目录状态'
    if ($docsDirty) {
        throw 'docs 目录已有未提交变更，请先处理后再运行此脚本'
    }

    if ([string]::IsNullOrWhiteSpace($Version)) {
        $desktopProjectPath = Join-Path -Path $repositoryRoot -ChildPath 'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj'
        $defaultVersion = Get-DesktopDocumentVersion -ProjectPath $desktopProjectPath
        $versionInput = Read-Host "请输入本次版本号（x.y.z，可带 v 前缀，直接回车使用 $defaultVersion）"
        if ([string]::IsNullOrWhiteSpace($versionInput)) {
            $version = $defaultVersion
            Write-Host "使用桌面项目默认版本号: $version" -ForegroundColor Green
        }
        else {
            $version = Get-NormalizedVersion -InputVersion $versionInput
        }
    }
    else {
        $version = Get-NormalizedVersion -InputVersion $Version
        Write-Host "使用外部传入版本号: $version" -ForegroundColor Green
    }

    $updateDescription = Read-MultiLineInput -Prompt '请输入本次更新内容'
    if ([string]::IsNullOrWhiteSpace($updateDescription)) {
        throw '更新内容不能为空'
    }

    $opencodePrompt = @"
你正在发布 UzonMail $version 版本。请根据用户输入生成中文和英文的版本更新 Markdown 正文，并在当前 docs 目录中实际执行下面两次命令更新文档。

严格要求：
1. 每段正文只包含有内容的分类标题和编号列表。中文分类只能使用“功能新增”“功能优化”“Bug 修复”；英文分类只能使用“New Features”“Improvements”“Bug Fixes”。
2. 不要生成版本标题、发布日期、下载地址或 docker 链接；这些内容由脚本生成。
3. 先将每种语言的 Markdown 正文保存到 PowerShell here-string 变量，再通过管道调用脚本。不得直接编辑 docs/downloads.md、docs/en/downloads.md 或 updates 目录中的任何文件。
4. 必须依次调用以下命令，两个命令均成功后才完成任务：

`$chineseMarkdown = @'
<中文 Markdown 正文>
'@
`$chineseMarkdown | & pwsh -NoProfile -File ..\scripts\update-version-doc.ps1 -Version '$version' -UpdatePath 'docs/docs/downloads.md'`

`$englishMarkdown = @'
<English Markdown body>
'@
`$englishMarkdown | & pwsh -NoProfile -File ..\scripts\update-version-doc.ps1 -Version '$version' -UpdatePath 'docs/docs/en/downloads.md'`

5. 若脚本返回错误，停止执行并如实报告错误。完成后只简要报告已更新的文件。

用户输入：
$updateDescription
"@

    Write-Host '调用 opencode 生成内容并更新文档...' -ForegroundColor Yellow
    Push-Location -Path $docsDirectory
    try {
        & opencode run -m 'zai-coding-plan/glm-4.7' $opencodePrompt
        Assert-GitSuccess -Step 'opencode 更新版本文档'
    }
    finally {
        Pop-Location
    }

    $allowedDocsPaths = [System.Collections.Generic.List[string]]::new()
    [void]$allowedDocsPaths.Add('docs/docs/downloads.md')
    [void]$allowedDocsPaths.Add('docs/docs/en/downloads.md')
    $publicUpdatesDirectory = Join-Path -Path $repositoryRoot -ChildPath 'docs/docs/.vuepress/public/updates'
    if (Test-Path -LiteralPath $publicUpdatesDirectory -PathType Container) {
        [void]$allowedDocsPaths.Add('docs/docs/.vuepress/public/updates')
    }

    Write-Host '提交生成的文档变更...' -ForegroundColor Yellow
    & git add -- @($allowedDocsPaths)
    Assert-GitSuccess -Step '暂存文档变更'

    & git diff --cached --quiet -- @($allowedDocsPaths)
    if ($LASTEXITCODE -eq 0) {
        Write-Host '版本文档内容未变化，无需提交、合并或推送。' -ForegroundColor Yellow
        return
    }

    if ($LASTEXITCODE -ne 1) {
        throw '检查暂存文档变更失败，已中止'
    }

    & git commit -m "docs: update version $version documentation"
    Assert-GitSuccess -Step '提交文档变更'

    $branchAfterCommit = & git branch --show-current
    Assert-GitSuccess -Step '获取提交后的当前分支'
    if ($branchAfterCommit -ne $TargetBranch) {
        Write-Host "切换到 $TargetBranch 分支..." -ForegroundColor Yellow
        & git switch $TargetBranch
        Assert-GitSuccess -Step "切换到 $TargetBranch 分支"
    }

    Write-Host "将 master 合并到 $TargetBranch..." -ForegroundColor Yellow
    & git merge --no-ff master -m "Merge master into $TargetBranch"
    Assert-GitSuccess -Step "合并 master 到 $TargetBranch"

    Write-Host "推送 $TargetBranch 分支..." -ForegroundColor Yellow
    & git push origin $TargetBranch
    Assert-GitSuccess -Step "推送 $TargetBranch 分支"

    Write-Host '版本文档生成、合并和推送已完成。' -ForegroundColor Green
}
finally {
    Pop-Location
}
