$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$buildModulePath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/UzonMail.Build.psm1'
$buildScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/build.ps1'

Import-Module $buildModulePath -Force

function Import-BuildScriptFunction {
    <#
    .SYNOPSIS
    从构建入口脚本提取指定函数供隔离测试使用
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FunctionName
    )

    $parseErrors = $null
    $buildScriptAst = [System.Management.Automation.Language.Parser]::ParseFile($buildScriptPath, [ref]$null, [ref]$parseErrors)
    if ($parseErrors.Count -gt 0) {
        throw "无法解析构建脚本：$($parseErrors[0].Message)"
    }

    $functionDefinition = $buildScriptAst.Find({
            param($ast)
            $ast -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $ast.Name -eq $FunctionName
        }, $true) | Select-Object -First 1
    if ($null -eq $functionDefinition) {
        throw "构建脚本中未找到函数：$FunctionName"
    }

    return [scriptblock]::Create($functionDefinition.Extent.Text)
}

Describe 'UzonMail unified build script' {
    It 'parses the shared module' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($buildModulePath, [ref]$null, [ref]$parseErrors) | Out-Null

        $parseErrors.Count | Should Be 0
    }

    It 'parses the build entry point' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($buildScriptPath, [ref]$null, [ref]$parseErrors) | Out-Null

        $parseErrors.Count | Should Be 0
    }

    It 'resolves the unified src project directories' {
        $context = Resolve-BuildContext

        $context.WebRoot | Should Be (Join-Path -Path $repositoryRoot -ChildPath 'src/web')
        $context.ApiRoot | Should Be (Join-Path -Path $repositoryRoot -ChildPath 'src/api')
        $context.DesktopRoot | Should Be (Join-Path -Path $repositoryRoot -ChildPath 'src/win-desktop')
        $context.ServiceProject | Should Be (Join-Path -Path $repositoryRoot -ChildPath 'src/api/UZonMailService/UzonMailService.csproj')
        $context.PluginsRoot | Should Be (Join-Path -Path $repositoryRoot -ChildPath 'src/api/Plugins')
        $context.LinuxInstallerPath | Should Be (Join-Path -Path $repositoryRoot -ChildPath 'scripts/install/uzonmail_linux_install.py')
    }

    It 'discovers plugin projects without fixed project paths' {
        $context = Resolve-BuildContext
        $pluginProjects = Get-BuildPluginProjects -PluginsRoot $context.PluginsRoot

        $pluginProjects.Count | Should BeGreaterThan 0
        $pluginProjects | ForEach-Object { $_.DirectoryName | Should Not BeNullOrEmpty }
        $pluginProjects | ForEach-Object { $_.ProjectPath | Should Match '\.csproj$' }
        $pluginProjects | ForEach-Object { $_.AssemblyName | Should Not BeNullOrEmpty }
    }

    It 'formats stage duration without throwing' {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $exception = $null

        try {
            Complete-BuildStage -Stopwatch $stopwatch -Title 'duration test'
        }
        catch {
            $exception = $_
        }

        $exception | Should Be $null
    }

    It 'does not return native command output as a build result' {
        $nativeOutput = Invoke-BuildNativeCommand -Command powershell -Arguments @(
            '-NoProfile', '-Command', "Write-Output 'native output'"
        )

        $nativeOutput | Should Be $null
    }

    It 'does not require fixed plugin project paths' {
        $buildSources = (Get-Content -LiteralPath $buildModulePath -Raw) + (Get-Content -LiteralPath $buildScriptPath -Raw)

        $buildSources | Should Not Match 'CorePluginProject|ProPluginProject'
    }

    It 'resolves All to include Docker' {
        . (Import-BuildScriptFunction -FunctionName 'Resolve-BuildTargets')

        $resolvedTargets = @(Resolve-BuildTargets -RequestedTargets @('All') -HasRemoteLinuxPackage $false)

        ($resolvedTargets -contains 'Desktop') | Should Be $true
        ($resolvedTargets -contains 'WindowsServer') | Should Be $true
        ($resolvedTargets -contains 'Linux') | Should Be $true
        ($resolvedTargets -contains 'Docker') | Should Be $true
    }

    It 'keeps All,Docker compatible' {
        . (Import-BuildScriptFunction -FunctionName 'Resolve-BuildTargets')

        $resolvedTargets = @(Resolve-BuildTargets -RequestedTargets @('All', 'Docker') -HasRemoteLinuxPackage $false)

        ($resolvedTargets -contains 'Docker') | Should Be $true
    }

    It 'packages the Python Linux installer without legacy service helpers' {
        . (Import-BuildScriptFunction -FunctionName 'New-ServiceArchive')
        $artifactRoot = Join-Path -Path $TestDrive -ChildPath 'linux-archive'
        $serviceRoot = Join-Path -Path $artifactRoot -ChildPath 'service-linux-x64'
        New-Item -ItemType Directory -Path $serviceRoot -Force | Out-Null
        $context = [pscustomobject]@{
            ArtifactRoot = $artifactRoot
            DockerDeployScript = Join-Path -Path $repositoryRoot -ChildPath 'scripts/docker-deploy.sh'
            DockerCompose = Join-Path -Path $repositoryRoot -ChildPath 'docker/docker-compose.yml'
            DockerEnvironment = Join-Path -Path $repositoryRoot -ChildPath 'docker/.env'
            LinuxInstallerPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/install/uzonmail_linux_install.py'
        }
        $servicePackage = [pscustomobject]@{
            RuntimeIdentifier = 'linux-x64'
            Directory = $serviceRoot
            Version = '1.2.3.4'
        }
        Mock Invoke-BuildNativeCommand {}

        New-ServiceArchive -Context $context -ServicePackage $servicePackage | Out-Null

        Assert-MockCalled Invoke-BuildNativeCommand -Times 1 -Exactly -ParameterFilter {
            $Command -eq '7z.exe' -and
            $Arguments[-1] -eq $context.LinuxInstallerPath
        }
        Assert-MockCalled Invoke-BuildNativeCommand -Times 0 -Exactly -ParameterFilter {
            ($Arguments -join ' ') -match 'install\.sh|uzon-mail\.service'
        }
    }
}

InModuleScope UzonMail.Build {
    Describe 'WSL path conversion' {
        It 'runs wslpath through Bash with a quoted Windows path' {
            Mock Invoke-WslBuildCommand { '/mnt/d/Develop/Personal/UzonMail/build/service-linux-x64' }

            $wslPath = ConvertTo-WslPath -WindowsPath 'D:\Develop\Personal\UzonMail\build\service-linux-x64' -Distribution 'Ubuntu'

            $wslPath | Should Be '/mnt/d/Develop/Personal/UzonMail/build/service-linux-x64'
            Assert-MockCalled Invoke-WslBuildCommand -Times 1 -Exactly -ParameterFilter {
                $Distribution -eq 'Ubuntu' -and $BashCommand -eq "wslpath -u -- 'D:\Develop\Personal\UzonMail\build\service-linux-x64'"
            }
        }

        It 'includes the WSL command failure when path conversion fails' {
            Mock Invoke-WslBuildCommand { throw 'WSL 命令执行失败（退出码 1）：wslpath -u' }

            $conversionException = $null
            try {
                ConvertTo-WslPath -WindowsPath 'D:\invalid-path' -Distribution 'Ubuntu'
            }
            catch {
                $conversionException = $_.Exception
            }

            $conversionException.Message | Should Match '无法转换 WSL 路径：D:\\invalid-path'
            $conversionException.Message | Should Match 'WSL 命令执行失败'
        }
    }

    Describe 'Docker build environment resolution' {
        It 'uses local Docker before checking WSL' {
            Mock Test-LocalDockerEnvironment { [pscustomobject]@{ IsAvailable = $true; FailureReason = $null } }
            Mock Test-WslDockerEnvironment { throw 'WSL should not be checked' }
            Mock Write-BuildMessage {}

            $dockerEnvironment = Resolve-DockerBuildEnvironment -WslDistribution 'Ubuntu'

            $dockerEnvironment.UseWsl | Should Be $false
            $dockerEnvironment.Distribution | Should Be $null
            Assert-MockCalled Test-WslDockerEnvironment -Times 0 -Exactly
        }

        It 'falls back to WSL when local Docker is unavailable' {
            Mock Test-LocalDockerEnvironment { [pscustomobject]@{ IsAvailable = $false; FailureReason = 'docker info failed' } }
            Mock Test-WslDockerEnvironment { [pscustomobject]@{ IsAvailable = $true; FailureReason = $null } }
            Mock Write-BuildMessage {}

            $dockerEnvironment = Resolve-DockerBuildEnvironment -WslDistribution 'Ubuntu'

            $dockerEnvironment.UseWsl | Should Be $true
            $dockerEnvironment.Distribution | Should Be 'Ubuntu'
            Assert-MockCalled Test-WslDockerEnvironment -Times 1 -Exactly -ParameterFilter { $Distribution -eq 'Ubuntu' }
        }

        It 'reports both failures when no Docker environment is available' {
            Mock Test-LocalDockerEnvironment { [pscustomobject]@{ IsAvailable = $false; FailureReason = 'local failure' } }
            Mock Test-WslDockerEnvironment { [pscustomobject]@{ IsAvailable = $false; FailureReason = 'WSL failure' } }

            { Resolve-DockerBuildEnvironment } | Should Throw '未找到可用的 Docker 构建环境。本机 Docker：local failure；WSL Docker：WSL failure'
        }
    }
}
