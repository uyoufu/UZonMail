param(
  [string]$Version,
  [string]$TargetBranch = "docs"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

function Assert-Linux {
  if (-not $IsLinux) {
    Write-Host "此脚本仅支持 Linux 执行。" -ForegroundColor Red
    exit 1
  }
}

function Assert-CommandExists {
  param(
    [Parameter(Mandatory = $true)]
    [string]$CommandName
  )

  if (-not (Get-Command $CommandName -ErrorAction SilentlyContinue)) {
    Write-Host "未检测到 $CommandName，请先安装后再执行。" -ForegroundColor Red
    exit 1
  }
}

function Read-MultiLineInput {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Prompt
  )

  Write-Host $Prompt -ForegroundColor Yellow
  Write-Host "输入空行结束。" -ForegroundColor DarkGray

  $lines = @()
  while ($true) {
    $line = Read-Host
    if ([string]::IsNullOrWhiteSpace($line)) {
      break
    }

    $lines += $line
  }

  return ($lines -join "`n").Trim()
}

function Get-NormalizedVersion {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Version
  )

  $normalized = $Version.Trim()
  if ($normalized.StartsWith("v")) {
    $normalized = $normalized.Substring(1)
  }

  if ($normalized -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "版本号格式不正确，请输入 x.y.z，例如 0.22.1。" -ForegroundColor Red
    exit 1
  }

  return $normalized
}

function Assert-GitSuccess {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Step
  )

  if ($LASTEXITCODE -ne 0) {
    Write-Host "$Step 失败，已中止。" -ForegroundColor Red
    exit $LASTEXITCODE
  }
}

# 不强制要求在 Linux 环境执行
# Assert-Linux

Assert-CommandExists -CommandName "git"
Assert-CommandExists -CommandName "opencode"

$repoRoot = & git rev-parse --show-toplevel
Assert-GitSuccess -Step "获取仓库根目录"

Set-Location -Path $repoRoot

$currentBranch = & git branch --show-current
Assert-GitSuccess -Step "获取当前分支"

if ($currentBranch -ne "master") {
  Write-Host "切换到 master 分支..." -ForegroundColor Yellow
  & git switch master
  Assert-GitSuccess -Step "切换到 master 分支"
}

Write-Host "拉取 master 最新更新..." -ForegroundColor Yellow
& git pull --ff-only origin master
Assert-GitSuccess -Step "拉取 master 最新更新"

$docsDir = Join-Path -Path $repoRoot -ChildPath "docs"
$docsDirty = & git status --porcelain -- docs
Assert-GitSuccess -Step "检查 docs 目录状态"

if ($docsDirty) {
  Write-Host "docs 目录已有未提交变更，请先处理后再运行此脚本。" -ForegroundColor Red
  exit 1
}

if ([string]::IsNullOrWhiteSpace($Version)) {
  $versionInput = Read-Host "请输入本次版本号（x.y.z，可带 v 前缀）"
  if ([string]::IsNullOrWhiteSpace($versionInput)) {
    Write-Host "版本号不能为空。" -ForegroundColor Red
    exit 1
  }

  $version = Get-NormalizedVersion -Version $versionInput
}
else {
  $version = Get-NormalizedVersion -Version $Version
  Write-Host "使用外部传入版本号: $version" -ForegroundColor Green
}

$updateContent = Read-MultiLineInput -Prompt "请输入本次更新内容"
if ([string]::IsNullOrWhiteSpace($updateContent)) {
  Write-Host "更新内容不能为空。" -ForegroundColor Red
  exit 1
}

$today = Get-Date -Format "yyyy-MM-dd"
$opencodePrompt = @"
请先阅读 .agents/skills/new-version/SKILL.md 的要求，然后根据下面的输入生成新的版本文档。

执行环境说明：
- 当前目录是 docs
- 中文文档目标：docs/downloads.md
- 英文文档目标：docs/en/downloads.md
- 必要时使用 .agents/skills/new-version/scripts/update_version_zh.js 和 update_version_en.js 更新文档
- 不要直接读取大文件内容

版本号：$version
当前日期：$today

本次更新内容：
$updateContent

要求：
1. 把内容整理到 功能新增、功能优化、Bug 修复 三类中。
2. 生成符合技能规范的新版本文档。
3. 如果内容是中文，请同步生成英文版；如果内容是英文，请同步生成中文版。
4. 只修改文档相关文件。
"@

Write-Host "在 docs 目录中调用 opencode 生成版本文档..." -ForegroundColor Yellow
Push-Location -Path $docsDir
try {
  & opencode run -m "zai-coding-plan/glm-4.7" $opencodePrompt
  Assert-GitSuccess -Step "opencode 生成版本文档"
}
finally {
  Pop-Location
}

$changedDocsFiles = @()
$changedDocsFiles += & git diff --name-only -- docs
Assert-GitSuccess -Step "获取 docs 目录已修改文件"
$changedDocsFiles += & git ls-files --others --exclude-standard -- docs
Assert-GitSuccess -Step "获取 docs 目录未跟踪文件"
$changedDocsFiles = $changedDocsFiles | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique
$allowedDocsFiles = @(
  "docs/downloads.md",
  "docs/en/downloads.md"
)
$changedDocsFiles = $changedDocsFiles | Where-Object { $allowedDocsFiles -contains $_ }

if (-not $changedDocsFiles -or $changedDocsFiles.Count -eq 0) {
  Write-Host "opencode 未生成任何可提交的文档变更。" -ForegroundColor Red
  exit 1
}

Write-Host "提交生成的文档变更..." -ForegroundColor Yellow
& git add -- @changedDocsFiles
Assert-GitSuccess -Step "暂存文档变更"

& git commit -m "docs: update version documentation"
Assert-GitSuccess -Step "提交文档变更"

$branchAfterCommit = & git branch --show-current
Assert-GitSuccess -Step "获取提交后的当前分支"
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

Write-Host "版本文档生成、合并和推送已完成。" -ForegroundColor Green
