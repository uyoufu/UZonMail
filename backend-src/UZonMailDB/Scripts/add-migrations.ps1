# 进行数据库迁移脚本的添加
param(
  [Parameter(Mandatory = $true)]
  [string]$Name
)

# 判断是否有 $Name 参数
if (-not $Name) {
  Write-Host "请提供迁移脚本名称。"
  exit 1
}

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


# 迁移 mysql
Write-Host "正在添加 MySQL 迁移脚本：$Name"
dotnet ef migrations add $Name --context MysqlContext --output-dir Migrations/Mysql -v

# 迁移 sqlite
Write-Host "正在添加 SQLite 迁移脚本：$Name"
dotnet ef migrations add $Name --context SqLiteContext --output-dir Migrations/SqLite -v

# 迁移 postgresql
Write-Host "正在添加 PostgreSQL 迁移脚本：$Name"
dotnet ef migrations add $Name --context PostgreSqlContext --output-dir Migrations/PostgreSQL -v

Set-Location $current
Write-Host "迁移脚本添加完成。" -ForegroundColor Green