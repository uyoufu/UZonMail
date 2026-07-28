$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$buildModulePath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/UzonMail.Build.psm1'
$buildScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/build.ps1'

Import-Module $buildModulePath -Force

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

    It 'does not reference pre-migration source directories' {
        $buildSources = (Get-Content -LiteralPath $buildModulePath -Raw) + (Get-Content -LiteralPath $buildScriptPath -Raw)

        $buildSources | Should Not Match 'backend-src|ui-src'
    }

    It 'does not require fixed plugin project paths' {
        $buildSources = (Get-Content -LiteralPath $buildModulePath -Raw) + (Get-Content -LiteralPath $buildScriptPath -Raw)

        $buildSources | Should Not Match 'CorePluginProject|ProPluginProject'
    }
}

InModuleScope UzonMail.Build {
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
