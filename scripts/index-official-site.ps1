# 向各大网站提交 url 索引

# 读取 ~/.uzon/config.json 配置
# 判断文件是否存在
$configPath = "$HOME/.uzon/config.seo.json"
if (-Not (Test-Path $configPath)) {
  Write-Host "配置文件不存在: $configPath"
  exit 1
}

# 读取配置文件内容
$config = Get-Content -Path $configPath -Raw | ConvertFrom-Json

# google


# biying

# baidu

# 360

# sougou