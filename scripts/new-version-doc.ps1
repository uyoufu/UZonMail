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

Push-Location -Path $repoRoot

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

$opencodePrompt = @"
请只输出 JSON，不要输出任何解释、命令、代码块或文件操作。

JSON 结构：
{
  "zhMarkdown": "## x.y.z\n> 更新时期：YYYY-MM-DD\n...",
  "enMarkdown": "## x.y.z\n> Release Date: YYYY-MM-DD\n..."
}

要求：
1. 把内容整理到 功能新增、功能优化、Bug 修复 三类中。
2. 版本号：$version
3. 日期：$(Get-Date -Format "yyyy-MM-dd")
4. 如果用户输入偏中文，zhMarkdown 和 enMarkdown 都要给出；如果偏英文，也要给出。
5. 不要执行任何文件写入操作。

用户输入：
$updateContent
"@

Write-Host "在 docs 目录中调用 opencode 生成结构化内容..." -ForegroundColor Yellow
Push-Location -Path $docsDir
try {
  $opencodeOutput = & opencode run -m "zai-coding-plan/glm-4.7" $opencodePrompt
  Assert-GitSuccess -Step "opencode 生成结构化内容"
}
finally {
  Pop-Location
}

$opencodeText = ($opencodeOutput | Out-String).Trim()
if ([string]::IsNullOrWhiteSpace($opencodeText)) {
  Write-Host "未从 opencode 获取到输出。" -ForegroundColor Red
  exit 1
}

$jsonStart = $opencodeText.IndexOf('{')
$jsonEnd = $opencodeText.LastIndexOf('}')
if ($jsonStart -lt 0 -or $jsonEnd -le $jsonStart) {
  Write-Host "opencode 输出不是有效 JSON。" -ForegroundColor Red
  Write-Host $opencodeText
  exit 1
}

$jsonText = $opencodeText.Substring($jsonStart, $jsonEnd - $jsonStart + 1)
try {
  $payload = $jsonText | ConvertFrom-Json
}
catch {
  Write-Host "解析 opencode JSON 失败。" -ForegroundColor Red
  Write-Host $jsonText
  exit 1
}

if (-not $payload.zhMarkdown -or -not $payload.enMarkdown) {
  Write-Host "opencode 输出缺少 zhMarkdown 或 enMarkdown。" -ForegroundColor Red
  exit 1
}

function Convert-ToBase64Utf8 {
  param(
    [Parameter(Mandatory = $true)]
    [string]$Text
  )

  return [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Text))
}

$scriptDir = Join-Path -Path $docsDir -ChildPath ".agents/skills/new-version/scripts"
$zhPayload = Convert-ToBase64Utf8 -Text $payload.zhMarkdown
$enPayload = Convert-ToBase64Utf8 -Text $payload.enMarkdown

Write-Host "更新中文版本文档..." -ForegroundColor Yellow
& node (Join-Path $scriptDir "update_version_zh.js") --base64 $zhPayload
Assert-GitSuccess -Step "更新中文版本文档"

Write-Host "更新英文版本文档..." -ForegroundColor Yellow
& node (Join-Path $scriptDir "update_version_en.js") --base64 $enPayload
Assert-GitSuccess -Step "更新英文版本文档"

Write-Host "提交生成的文档变更..." -ForegroundColor Yellow
$allowedDocsFiles = @(
  "docs/docs/downloads.md",
  "docs/docs/en/downloads.md"
)
& git add -- @allowedDocsFiles
Assert-GitSuccess -Step "暂存文档变更"

& git commit -m "docs: update version $Version documentation"
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
Pop-Location
