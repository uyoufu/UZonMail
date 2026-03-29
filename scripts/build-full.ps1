# 脚本说明
# 本脚本先在 Windows 中编译各平台产物，然后在 WSL 中构建 Docker 镜像并上传，完成后清理 WSL 临时文件，最后使用 od 命令上传编译产物到对象存储

param(
  [bool]$upload = $true,
  [bool]$skipWindowsBuild = $false
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

# 设置代码页为 UTF-8
chcp 65001

$scriptRoot = $PSScriptRoot
Set-Location -Path $scriptRoot

# ==================== 第一步：Windows 编译 ====================

if (-not $skipWindowsBuild) {
  # 编译 windows desktop
  Write-Host "1. 开始编译 windows desktop ..." -ForegroundColor Yellow
  $winDesktop = & ./build-core.ps1 -platform win -desktop $true
  Write-Host "-------------------------windows desktop 编译完成(1/3)-------------------------" -ForegroundColor Green

  # 编译 windows server
  Write-Host "2. 开始编译 windows server ..." -ForegroundColor Yellow
  $winServer = . ./build-core.ps1 -platform win -rebuildFrontend $false
  Write-Host "-------------------------windows server 编译完成(2/3)-------------------------" -ForegroundColor Green
  Write-Host ""

  # 编译 linux server（不编译 docker，后续在 WSL 中处理）
  Write-Host "3. 开始编译 linux server ..." -ForegroundColor Yellow
  $linuxServer = . ./build-core.ps1 -platform linux -rebuildFrontend $false -docker $false
  Write-Host "-------------------------linux server 编译完成(3/3)-------------------------" -ForegroundColor Green
  Write-Host ""
}
else {
  Write-Host "跳过 Windows 编译步骤" -ForegroundColor Yellow
}

# ==================== 第二步：在 WSL 中构建 Docker 镜像 ====================

# 获取 linux 编译产物路径
$gitRoot = & git rev-parse --show-toplevel
$linuxBuildPath = Join-Path -Path $gitRoot -ChildPath "build/service-linux-x64"

if (-not (Test-Path -Path $linuxBuildPath -PathType Container)) {
  Write-Host "Linux 编译产物不存在: $linuxBuildPath" -ForegroundColor Red
  Write-Host "请先编译 Linux 版本，或移除 -skipWindowsBuild 参数" -ForegroundColor Red
  exit 1
}

# 获取版本号：从编译目录中的 dll 中提取
$serviceDll = Get-ChildItem -Path $linuxBuildPath -Filter "UZonMailService.dll" | Select-Object -First 1
if (-not $serviceDll) {
  Write-Host "未找到 UZonMailService.dll，无法获取版本号" -ForegroundColor Red
  exit 1
}
$serviceVersion = $serviceDll.VersionInfo.FileVersion
Write-Host "检测到版本号: $serviceVersion" -ForegroundColor Green

# 生成 Docker 镜像标签
$imageVersion = $serviceVersion -replace "\.0$", ""
$dockerImageName = "gmxgalens/uzon-mail"
$dockerImage = "$dockerImageName`:$imageVersion"
Write-Host "Docker 镜像: $dockerImage" -ForegroundColor Green

# 将 Windows 路径转换为 WSL 路径
# 例如: D:\Develop\Personal\UzonMail\build\service-linux-x64 -> /mnt/d/Develop/Personal/UzonMail/build/service-linux-x64
$wslBuildPath = & wsl wslpath -u "$linuxBuildPath" 2>$null
if (-not $wslBuildPath) {
  # 手动转换作为后备
  $driveLetter = $linuxBuildPath.Substring(0, 1).ToLower()
  $restPath = $linuxBuildPath.Substring(2).Replace('\', '/')
  $wslBuildPath = "/mnt/$driveLetter$restPath"
}
$wslBuildPath = $wslBuildPath.Trim()

# WSL 中的临时目录
$wslTempDir = "/tmp/uzonmail-docker-build-$(Get-Random)"

Write-Host "WSL 编译路径: $wslBuildPath" -ForegroundColor Green
Write-Host "WSL 临时目录: $wslTempDir" -ForegroundColor Green

# 检查 WSL 中是否有 docker
Write-Host "检查 WSL 中的 Docker 环境..." -ForegroundColor Yellow
$wslDockerCheck = & wsl bash -c "which docker 2>/dev/null"
if ([string]::IsNullOrWhiteSpace($wslDockerCheck)) {
  Write-Host "WSL 中未安装 Docker，请先在 WSL 中安装 Docker" -ForegroundColor Red
  exit 1
}
Write-Host "WSL Docker 环境检测通过！" -ForegroundColor Green

Write-Host ""
Write-Host "-------------------------开始在 WSL 中构建 Docker 镜像-------------------------" -ForegroundColor Yellow

# 在 WSL 中执行 Docker 构建
$dockerfilePath = "$wslBuildPath/Dockerfile"

# 检查 Dockerfile 是否存在
$wslFileCheck = & wsl bash -c "test -f '$dockerfilePath' && echo 'exists'"
if ($wslFileCheck.Trim() -ne "exists") {
  Write-Host "WSL 中未找到 Dockerfile: $dockerfilePath" -ForegroundColor Red
  exit 1
}

# 创建临时目录并复制构建上下文
Write-Host "复制构建上下文到 WSL 临时目录..." -ForegroundColor Yellow
& wsl bash -c "mkdir -p '$wslTempDir' && cp -r '$wslBuildPath/'* '$wslTempDir/'"

# 在 WSL 中构建 Docker 镜像
Write-Host "开始构建 Docker 镜像: $dockerImage ..." -ForegroundColor Yellow
& wsl bash -c "cd '$wslTempDir' && docker build -t '$dockerImage' -f Dockerfile ."
if ($LASTEXITCODE -ne 0) {
  Write-Host "Docker 镜像构建失败！" -ForegroundColor Red
  # 仍然尝试清理
  & wsl bash -c "rm -rf '$wslTempDir'" 2>$null
  exit 1
}

# 同时打 latest 标签
& wsl bash -c "docker tag '$dockerImage' '${dockerImageName}:latest'"
Write-Host "Docker 镜像构建完成！" -ForegroundColor Green

# ==================== 第三步：上传镜像 ====================

$dockerUploadSucceeded = $false

if ($upload) {
  Write-Host "开始上传 Docker 镜像..." -ForegroundColor Yellow
  & wsl bash -c "docker login"
  if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker 登录失败！" -ForegroundColor Red
  }
  else {
    Write-Host "上传 Docker 镜像: $imageVersion ..." -ForegroundColor Yellow
    & wsl bash -c "docker push '$dockerImage'"
    if ($LASTEXITCODE -ne 0) {
      Write-Host "Docker 镜像上传失败: $dockerImage" -ForegroundColor Red
      exit 1
    }

    Write-Host "上传 Docker 镜像: latest ..." -ForegroundColor Yellow
    & wsl bash -c "docker push '${dockerImageName}:latest'"
    if ($LASTEXITCODE -ne 0) {
      Write-Host "Docker 镜像上传失败: ${dockerImageName}:latest" -ForegroundColor Red
      exit 1
    }

    Write-Host "Docker 镜像上传完成！" -ForegroundColor Green
    $dockerUploadSucceeded = $true
  }
}

# ==================== 第四步：清理 WSL 临时文件 ====================

Write-Host "清理 WSL 临时文件: $wslTempDir ..." -ForegroundColor Yellow
& wsl bash -c "rm -rf '$wslTempDir'"
if ($LASTEXITCODE -eq 0) {
  Write-Host "WSL 临时文件清理完成！" -ForegroundColor Green
}
else {
  Write-Host "WSL 临时文件清理失败，请手动清理: $wslTempDir" -ForegroundColor Red
}

# ==================== 第五步：使用 od 上传编译产物到对象存储 ====================

if (Get-Command od -ErrorAction SilentlyContinue) {
  function New-Oss {
    $paths = @($winDesktop[-1], $winServer[-1], $linuxServer[-1])
    Write-Host "开始上传编译产物到对象存储 ..." -ForegroundColor Yellow
    $results = @()
    foreach ($path in $paths) {
      if (-not (Test-Path $path)) {
        Write-Host "$path 不存在！" -ForegroundColor Red
        continue
      }

      $result = od minio soft -p $path
      $results += $result[-1]
    }

    Write-Host "-------------------------编译产物上传完成-------------------------" -ForegroundColor Green
    Write-Host "上传结果：" -ForegroundColor Green
    $results
  }

  New-Oss
}
else {
  Write-Host "未检测到 od 命令，跳过编译产物上传" -ForegroundColor Yellow
}

# ==================== 第六步：更新版本说明文档 ====================
if ($dockerUploadSucceeded) {
  Write-Host "开始更新版本说明文档..." -ForegroundColor Yellow
  & "$scriptRoot/new-version-doc.ps1" -Version $imageVersion
  if ($LASTEXITCODE -ne 0) {
    Write-Host "版本说明文档更新失败！" -ForegroundColor Red
    exit $LASTEXITCODE
  }

  Write-Host "版本说明文档更新完成！" -ForegroundColor Green
}


Write-Host ""
Write-Host "=========================全部完成=========================" -ForegroundColor Green
Write-Host "Docker 镜像: $dockerImage" -ForegroundColor Green
Write-Host "Docker 镜像: ${dockerImageName}:latest" -ForegroundColor Green
