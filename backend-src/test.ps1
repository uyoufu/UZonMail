
$slnRoot = $PSScriptRoot
$allProjects = dotnet sln list | 
Where-Object { $_ -and ($_ -match '\\') } | 
ForEach-Object { 
  $rel = $_.Trim()
  # 将相对路径转换为绝对路径（相对于解决方案目录）
  $abs = Join-Path $slnRoot $rel
  # 如果文件不存在，提示缺失
  if (Test-Path $abs) {
    return $abs
  }
  else {
    Write-Warning "项目文件不存在： $abs"
    return $null
  }
} |
Where-Object { $_ -ne $null }

Write-Host $allProjects

function Get-ProjectFileAndAssemblyName {
  param(
    [string] $projectName
  )

  # 在所有项目中查找指定名称的项目
  foreach ($projPath in $allProjects) {
    $projItem = Get-Item $projPath
    $projBaseName = $projItem.BaseName
    if ($projBaseName -eq $projectName) {
      # 读取 csproj 文件，获取 AssemblyName 节点的值
      # 即 Project/PropertyGroup/AssemblyName
      [xml]$csprojXml = Get-Content $projPath
      $assemblyName = $proj.Project.PropertyGroup | Where-Object AssemblyName | ForEach-Object { $_.AssemblyName }  | Select-Object -First 1
      # 不存在，使用项目名
      if (-not $assemblyName) {
        $assemblyName = $projBaseName
      }

      return @{
        ProjectPath  = $projPath
        AssemblyName = $assemblyName
      }
    }
  }
}

Get-ProjectFileAndAssemblyName UZonMailProPlugin