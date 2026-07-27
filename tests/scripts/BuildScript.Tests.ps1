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
}
