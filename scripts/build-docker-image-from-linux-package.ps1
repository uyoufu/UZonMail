# 脚本说明
# 本脚本用于从网络下载 Linux 包并编译 Docker 镜像
# 设置参数
param(
  [string]$downloadUrl = "https://oss.uzoncloud.com:2234/public/files/soft/uzonmail-service-linux-x64-0.18.0.0.zip",
  [bool]$upload = $true
)

# 遇到错误即退出
$ErrorActionPreference = "Stop"
# 严格模式
Set-StrictMode -Version Latest

# 检测环境
Write-Host "开始检测环境..." -ForegroundColor Yellow

# 检查是否有 docker 环境
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
  Write-Host "请先安装 docker 环境！" -ForegroundColor Red
  exit 1
}
Write-Host "docker 环境检测通过！" -ForegroundColor Green

# 检查平台
if ($env:OS -notlike "*Linux*") {
  Write-Host "请在 Linux 平台下运行此脚本！" -ForegroundColor Red
  exit 1
}

# 创建临时目录
$tempDir = Join-Path -Path $env:TEMP -ChildPath "uzonmail-build-$(Get-Random)"
New-Item -Path $tempDir -ItemType Directory -Force | Out-Null
Write-Host "临时目录: $tempDir" -ForegroundColor Green

# 下载 ZIP 文件
$fileName = [System.IO.Path]::GetFileName($downloadUrl)
$zipFile = Join-Path -Path $tempDir -ChildPath $fileName
Write-Host "开始下载文件: $downloadUrl" -ForegroundColor Yellow
try {
  Invoke-WebRequest -Uri $downloadUrl -OutFile $zipFile -UseBasicParsing
  Write-Host "文件下载完成！" -ForegroundColor Green
}
catch {
  Write-Host "文件下载失败: $_" -ForegroundColor Red
  exit 1
}

# 解压 ZIP 文件
$extractDir = Join-Path -Path $tempDir -ChildPath "extracted"
Write-Host "开始解压文件..." -ForegroundColor Yellow
try {
  Expand-Archive -Path $zipFile -DestinationPath $extractDir -Force
  Write-Host "文件解压完成！" -ForegroundColor Green
}
catch {
  Write-Host "文件解压失败: $_" -ForegroundColor Red
  exit 1
}

# 检查解压目录
$dockerfilePath = Join-Path -Path $extractDir -ChildPath "service-linux-x64/Dockerfile"
if (-not (Test-Path -Path $dockerfilePath -PathType Leaf)) {
  Write-Host "解压目录中未找到 Dockerfile！" -ForegroundColor Red
  exit 1
}

# 生成版本号，从文件名 uzonmail-service-linux-x64-0.18.0.0.zip 提取
$version = $fileName -replace "uzonmail-service-linux-x64-", "" -replace ".zip", ""
$imageVersion = $version -replace "\.0$", ""  # 去掉最后的 .0
$dockerImageName = "gmxgalens/uzon-mail"
$dockerImage = "$($dockerImageName):$imageVersion"

# 编译 Docker 镜像
Write-Host "开始编译 Docker 镜像: $dockerImage" -ForegroundColor Yellow
try {
  docker build -t $dockerImage -f $dockerfilePath $extractDir
  docker build -t "$dockerImageName:latest" -f $dockerfilePath $extractDir
  Write-Host "Docker 镜像编译完成！" -ForegroundColor Green
}
catch {
  Write-Host "Docker 镜像编译失败: $_" -ForegroundColor Red
  exit 1
}

# 上传镜像
if ($upload) {
  Write-Host "开始上传 Docker 镜像..." -ForegroundColor Yellow
  try {
    # 登录 Docker（需要手动输入凭据或配置）
    docker login
    Write-Host "开始上传 Docker 镜像: $imageVersion ..." -ForegroundColor Yellow
    docker push $dockerImage
    Write-Host "开始上传 Docker 镜像: latest ..." -ForegroundColor Yellow
    docker push "$($dockerImageName):latest"
    Write-Host "Docker 镜像上传完成！" -ForegroundColor Green
  }
  catch {
    Write-Host "Docker 镜像上传失败: $_" -ForegroundColor Red
    exit 1
  }
}

# 清理临时目录
Write-Host "清理临时文件..." -ForegroundColor Yellow
Remove-Item -Path $tempDir -Recurse -Force
Write-Host "脚本执行完成！" -ForegroundColor Green
