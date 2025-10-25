# 进行数据库迁移脚本的移除
function Find-ProjectRoot {
    param([string]$startPath)
    $currentPath = $startPath
    while ($currentPath -ne [System.IO.Path]::GetPathRoot($currentPath)) {
        $csprojFiles = Get-ChildItem -Path $currentPath -Filter "*.csproj" -File
        if ($csprojFiles.Count -gt 0) {
            return $currentPath
        }
        $currentPath = Split-Path -Parent $currentPath
    }
    throw "未找到 .csproj 文件"
}

$current = Get-Location
$projectRoot = Find-ProjectRoot -startPath $current
Set-Location $projectRoot

# 移除 mysql
Write-Host "正在移除 MySQL 迁移脚本"
dotnet ef migrations remove --context MysqlContext -v

# 移除 sqlite
Write-Host "正在移除 SQLite 迁移脚本"
dotnet ef migrations remove --context SqLiteContext -v

# 移除 postgresql
Write-Host "正在移除 PostgreSQL 迁移脚本"
dotnet ef migrations remove --context PostgreSqlContext -v

Set-Location $current
Write-Host "迁移脚本移除完成。" -ForegroundColor Green