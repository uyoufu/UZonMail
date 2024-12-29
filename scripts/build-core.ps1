# 脚本说明
# 本脚本为自动编译核心程序
# 设置参数
param(
    [string]$platform = "win",
    [bool]$desktop = $false,
    [bool]$rebuildFrontend = $true,
    [bool]$docker = $false
)

$publishPlatform = "win-x64"
if ($platform -eq "linux") {
    $publishPlatform = "linux-x64"
}


# 遇到错误即退出
$ErrorActionPreference = "Stop"
# 严格模式,
Set-StrictMode -Version Latest

# 检测环境
Write-Host "开始检测环境..." -ForegroundColor Yellow

# 检查是否有 yarn 环境
if (-not (Get-Command yarn -ErrorAction SilentlyContinue)) {
    Write-Host "请先安装 yarn 环境！" -ForegroundColor Red
    return
}
Write-Host "yarn 环境检测通过！" -ForegroundColor Green

# 检查是否有 node 环境
if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    Write-Host "请先安装 node 环境！" -ForegroundColor Red
    return
}
Write-Host "node 环境检测通过！" -ForegroundColor Green

# 检查是否有 dotnet 环境
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "请先安装 dotnet 环境！" -ForegroundColor Red
    return
}
Write-Host "dotnet 环境检测通过！" -ForegroundColor Green

# # 检测 msbuild 环境
# if (-not (Get-Command MSBuild -ErrorAction SilentlyContinue)) {
#     Write-Host "请将 msbuild 添加到环境变量！" -ForegroundColor Red
#     return
# }
# Write-Host "MSBuild 环境检测通过！" -ForegroundColor Green

# 检测 7z
if (-not (Get-Command 7z.exe -ErrorAction SilentlyContinue)) {
    Write-Host "7z 未安装，请从 https://www.7-zip.org/download.html 下载并安装" -ForegroundColor Red
    return
}
Write-Host "7z 环境检测通过！" -ForegroundColor Green

# 找到 git 的根目录
$gitRoot = $null
try {
    $gitRoot = & git rev-parse --show-toplevel    
}
catch {
    Write-Host "无法找到 Git 仓库的根目录，请确保在 Git 仓库中运行此脚本" -ForegroundColor Red
    exit 1
}

if (-not $gitRoot) {
    Write-Host "未找到 Git 仓库的根目录" -ForegroundColor Red
    exit 1
}

# 检测脚本位置是否正确：当前目录下是否有 ui-src 目录和 backend-src 目录
$sriptRoot = $PSScriptRoot
$subDirs = $("ui-src", "backend-src")
foreach ($subDir in $subDirs) {
    $dir = Join-Path -Path $gitRoot -ChildPath $subDir
    if (-not (Test-Path -Path $dir -PathType Container)) {
        Write-Host "请在项目的根目录下执行当前脚本！"
        return
    }
}
Write-Host "脚本位置检测通过！" -ForegroundColor Green

Write-Host "开始拉取更新" -ForegroundColor Green
git checkout master
git pull

# 开始编译项目
Write-Host "开始编译项目..." -ForegroundColor Yellow

# 编译前端
$uiSrc = Join-Path -Path $gitRoot -ChildPath "ui-src"
function Update-Frontend {
    if (-not $rebuildFrontend) {
        return
    }

    Write-Host "前端编译中..." -ForegroundColor Yellow    
    # 判断是否已经执行过 yarn install
    $nodeModules = Join-Path -Path $uiSrc -ChildPath "node_modules"
    if (-not (Test-Path -Path $nodeModules -PathType Container)) {
        Write-Host "开始安装依赖..." -ForegroundColor Yellow
        Set-Location -Path $uiSrc
        yarn install
        Write-Host "依赖安装完成！" -ForegroundColor Green
        Write-Host "开始编译..." -ForegroundColor Yellow
    }
    
    Set-Location -Path $uiSrc
    # yarn install
    yarn build
    Write-Host "前端编译完成！" -ForegroundColor Green
}
Update-Frontend

# 编译后端 UZonMailService
Write-Host "开始编译后端 UZonMailService ..." -ForegroundColor Yellow
$backendSrc = Join-Path -Path $gitRoot -ChildPath "backend-src"

$serviceSrc = Join-Path -Path $backendSrc -ChildPath "UZonMailService"
# 使用 dotnet 编译
Set-Location -Path $serviceSrc
$mainService = "$gitRoot/build/service-$publishPlatform"
# 先清空
if (Test-Path -Path $mainService -PathType Container) {
    Remove-Item -Path $mainService -Recurse -Force
}
New-Item -Path $mainService -ItemType Directory -Force

$serviceDist = $mainService
dotnet publish -c Release -o $serviceDist -r $publishPlatform --self-contained false
# 创建 public 目录
New-Item -Path "$serviceDist/public" -ItemType Directory -Force
# 创建 wwwwroot 目录
New-Item -Path "$serviceDist/wwwroot" -ItemType Directory -Force
# 创建 Plugins 目录
New-Item -Path "$serviceDist/Plugins" -ItemType Directory -Force
# 创建 assembly 目录
New-Item -Path "$serviceDist/Assembly" -ItemType Directory -Force

# 复制 Quartz/quartz-sqlite.sqlite3 到 data/db 目录中
New-Item -Path "$serviceDist/data/db" -ItemType Directory  -ErrorAction SilentlyContinue
Copy-Item -Path "$serviceSrc/Quartz/quartz-sqlite.sqlite3" -Destination "$serviceDist/data/db/quartz-sqlite.sqlite3" -Force
Write-Host "后端 UZonMailService 编译完成!" -ForegroundColor Green

# 复制 scripts 目录中的 Dockerfile 和 docker-compose.yml 到编译目录
$scriptFiles = @("Dockerfile", "docker-compose.yml")
foreach ($file in $scriptFiles) {
    Copy-Item -Path "$gitRoot/scripts/$file" -Destination $mainService -Force
}


# 添加 windows 服务
function Add-WindowsService {
    if ($publishPlatform -ne "win-x64") {
        return
    }

    # 复制 backendSrc/WindowsService 中所有的文件到编译目录
    $windowsService = Join-Path -Path $backendSrc -ChildPath "WindowsService"
    Copy-Item -Path "$windowsService/*" -Destination $mainService -Recurse -Force
}
Add-WindowsService

# 复制程序集函数
function Copy-Assembly {
    param(
        [string]$src,
        [string]$exclude
    )
    
    # 获取 $mainService 目录中的 dll
    $mainDlls = Get-ChildItem -Path "$mainService/*" | Where-Object { -not $_.PSIsContainer }
    # 获取目标目录中的 dll
    $srcDlls = Get-ChildItem -Path "$src/*" -Exclude $exclude | Where-Object { -not $_.PSIsContainer }
    # 比较两个目录中的 dll，去掉重复的 dll
    $dlls = Compare-Object -ReferenceObject $srcDlls -DifferenceObject $mainDlls -Property Name | Where-Object { $_.SideIndicator -eq '<=' }
    $targetDir = "$mainService/Assembly"
    foreach ($dll in $dlls) {
        Copy-Item -Path "$src/$($dll.Name)" -Destination $targetDir -Force
    }    
}

# 编译后端 UZonMailCorePlugin
$pluginsSrc = Join-Path -Path $backendSrc -ChildPath "Plugins"
$uZonMailCorePlugin = 'UZonMailCorePlugin'
Write-Host "开始编译后端 $uZonMailCorePlugin ..." -ForegroundColor Yellow
$serviceSrc = Join-Path -Path $pluginsSrc -ChildPath $uZonMailCorePlugin
# 使用 dotnet 编译
$serviceDist = "$mainService/$uZonMailCorePlugin"
Set-Location $serviceSrc
dotnet publish -c Release -o $serviceDist -r $publishPlatform --self-contained false
# 复制依赖到根目录，复制库 到 Plugins 目录
Copy-Assembly -src $serviceDist -exclude "$uZonMailCorePlugin.*"
$uzonMailCorePluginPath = Join-Path -Path $mainService -ChildPath "Plugins/$uZonMailCorePlugin"
New-Item -Path $uzonMailCorePluginPath -ItemType Directory -Force
Copy-Item -Path "$serviceDist/$uZonMailCorePlugin.*" -Destination $uzonMailCorePluginPath -Force
# 删除临时目录
Remove-Item -Path $serviceDist -Recurse -Force
Write-Host "后端 $uZonMailCorePlugin 编译完成!" -ForegroundColor Green

# 编译后端 UzonMailPro
$uZonMailProPlugin = 'UZonMailProPlugin'
Write-Host "开始编译后端 $uZonMailProPlugin ..." -ForegroundColor Yellow
# 使用 dotnet 编译
Set-Location -Path $gitRoot
$proPluginPath = "../UZonMailProPlugins/$uZonMailProPlugin"
$serviceSrc = Resolve-Path -Path $proPluginPath
if (test-path -path $serviceSrc -PathType Container) {
    $serviceDist = "$mainService/$uZonMailProPlugin"
    Set-Location $proPluginPath
    dotnet publish -c Release -o $serviceDist -r $publishPlatform --self-contained false
    # 复制依赖到根目录，复制库 到 Plugins 目录
    Copy-Assembly -src $serviceDist -exclude "$uZonMailProPlugin.*"
    $uzonMailProPluginPath = Join-Path -Path $mainService -ChildPath "Plugins/$uZonMailProPlugin"
    New-Item -Path $uzonMailProPluginPath -ItemType Directory -Force
    Copy-Item -Path "$serviceDist/$uZonMailProPlugin.*" -Destination $uzonMailProPluginPath -Force
    # 删除临时目录
    Remove-Item -Path $serviceDist -Recurse -Force
    Write-Host "后端 $uZonMailProPlugin 编译完成!" -ForegroundColor Green
}

# 复制前端编译结果到服务端指定位置
$serviceWwwroot = Join-Path -Path $mainService -ChildPath "wwwroot"
# 目录不存在时，创建
if (-not (Test-Path -Path $serviceWwwroot -PathType Container)) {
    New-Item -Path $serviceWwwroot -ItemType Directory -Force
}
Copy-Item -Path $uiSrc/dist/spa/* -Destination $serviceWwwroot -Recurse -Force

$buildVersion = "error"
$zipSrc = "$mainService/*"

# 读取服务端的版本号
$UZonMailServiceDll = Join-Path -Path $mainService -ChildPath "UZonMailService.dll"
$serviceVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($UZonMailServiceDll).FileVersion
# 生成文件路径
$zipDist = Join-Path -Path $gitRoot -ChildPath "build\uzonmail-service-$publishPlatform-$serviceVersion.zip"

# 编译桌面端
function Add-Desktop {
    # $desktop 为 false，直接返回
    if (-not $desktop) {
        return
    }

    # 编译桌面端
    Write-Host "桌面端编译中..." -ForegroundColor Yellow
    $desktopSrc = Join-Path -Path $backendSrc -ChildPath "UzonMailDesktop"
    Set-Location -Path $desktopSrc
    $desktopDist = "$gitRoot/build/desktop"
    # 若存在，则删除
    if (Test-Path -Path $desktopDist -PathType Container) {
        Remove-Item -Path $desktopDist -Recurse -Force
    }
    dotnet publish -c Release -o $desktopDist -r $publishPlatform --self-contained false

    Write-Host "桌面端编译完成！" -ForegroundColor Green

    # 整合环境
    Write-Host "整理编译结果..." -ForegroundColor Yellow

    # 复制服务端
    $svrDis = Join-Path -Path $desktopDist -ChildPath "service"
    New-Item -Path $svrDis -ItemType Directory -ErrorAction SilentlyContinue
    Copy-Item -Path $mainService/* -Destination $svrDis -Recurse -Force

    Write-Host "编译整理完成！" -ForegroundColor Green

    # 读取 desktop.exe 的版本号
    $desktopExePath = Join-Path -Path $desktopDist -ChildPath "UzonMailDesktop.exe"
    $buildVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($desktopExePath).FileVersion
    # 生成文件路径
    $script:zipSrc = "$desktopDist/*"
    $script:zipDist = Join-Path -Path $gitRoot -ChildPath "build\uzonmail-desktop-$publishPlatform-$buildVersion.zip"
}
Add-Desktop

# 打包文件
if ($platform -eq "linux") {
    # 去掉 $zipSrc 中的 /*
    $zipSrc = $zipSrc.Replace("/*", "")
}

7z a -tzip $zipDist $zipSrc

# linux 增加 docker 启动
if ($platform -eq "linux") {
    # 向压缩包添加启动文件: docker-deploy.sh
    $deployFiles = @('docker-deploy.sh', 'docker-compose.yml')
    foreach ($file in $deployFiles) {
        $dockerDeploy = Join-Path -Path $gitRoot -ChildPath "scripts/$file"
        7z a -tzip $zipDist $dockerDeploy
    }

    # 向压缩包中增加安装脚本
    $linuxInstall = Join-Path -Path $backendSrc -ChildPath "LinuxService"
    $installFiles = @('install.sh', 'uzon-mail.service')
    foreach ($file in $installFiles) {
        $installFile = Join-Path -Path $linuxInstall -ChildPath $file
        7z a -tzip $zipDist $installFile
    }
}


function Add-Docker {
    # $desktop 为 false，直接返回
    if ($platform -ne "linux") {
        return
    }

    # 编译 docker 镜像    
    Write-Host "开始编译 Docker 镜像..." -ForegroundColor Yellow
    # 生成版本号，去掉最后的 .0
    $imageVersion = $serviceVersion -replace "\.0$", ""
    $dockerImage = "gmxgalens/uzon-mail:$imageVersion"
    $dockerBuild = Join-Path -Path $mainService -ChildPath "Dockerfile"
    docker build -t $dockerImage -f $dockerBuild $mainService
    docker build -t gmxgalens/uzon-mail:latest -f $dockerBuild $mainService
    Write-Host "Docker 镜像编译完成！" -ForegroundColor Green

    # 上传镜像
    # 登陆 docker
    docker login

    Write-Host "开始上传 Docker 镜像: $imageVersion ..." -ForegroundColor Yellow
    docker push $dockerImage
    Write-Host "开始上传 Docker 镜像: latest ..." -ForegroundColor Yellow
    docker push gmxgalens/uzon-mail:latest
    Write-Host "Docker 镜像上传完成！" -ForegroundColor Green
}

if ($docker) {
    Add-Docker
}

# 回到根目录
Set-Location -Path $sriptRoot
Write-Host "编译完成：$zipDist" -ForegroundColor Green

# 返回编译后的文件路径
return $zipDist