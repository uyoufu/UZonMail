$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$versionUpdateScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/internal/update-release-version.ps1'

function New-ReleaseVersionFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FixtureRoot,

        [switch]$WithoutAssemblyVersion
    )

    $serviceProjectPath = Join-Path -Path $FixtureRoot -ChildPath 'src/api/UZonMailService/UzonMailService.csproj'
    $desktopProjectPath = Join-Path -Path $FixtureRoot -ChildPath 'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj'
    $frontendConfigurationPath = Join-Path -Path $FixtureRoot -ChildPath 'src/web/src/config/app.config.ts'
    New-Item -ItemType Directory -Path (Split-Path -Path $serviceProjectPath -Parent) -Force | Out-Null
    New-Item -ItemType Directory -Path (Split-Path -Path $desktopProjectPath -Parent) -Force | Out-Null
    New-Item -ItemType Directory -Path (Split-Path -Path $frontendConfigurationPath -Parent) -Force | Out-Null

    $assemblyVersionElement = if ($WithoutAssemblyVersion) {
        ''
    }
    else {
        '    <AssemblyVersion>0.1.0.0</AssemblyVersion>'
    }
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <FileVersion>0.1.0.0</FileVersion>
$assemblyVersionElement
  </PropertyGroup>
</Project>
"@
    [System.IO.File]::WriteAllText($serviceProjectPath, $projectContent, [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText($desktopProjectPath, $projectContent, [System.Text.UTF8Encoding]::new($false))

    $frontendConfiguration = @"
export default {
  default: {
    logger: {
      level: 'info'
    },
    version: '0.1.0.0'
  },
  production: {}
}
"@
    [System.IO.File]::WriteAllText($frontendConfigurationPath, $frontendConfiguration, [System.Text.UTF8Encoding]::new($false))

    return [pscustomobject]@{
        ServiceProjectPath        = $serviceProjectPath
        DesktopProjectPath        = $desktopProjectPath
        FrontendConfigurationPath = $frontendConfigurationPath
    }
}

Describe 'Release version update script' {
    It 'parses the internal version update script' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($versionUpdateScriptPath, [ref]$null, [ref]$parseErrors) | Out-Null

        $parseErrors.Count | Should Be 0
    }

    It 'updates all release version fields with a four-part version' {
        $fixture = New-ReleaseVersionFixture -FixtureRoot $TestDrive

        & pwsh -NoProfile -File $versionUpdateScriptPath -Version 'v1.2.3' -RepositoryRoot $TestDrive
        $LASTEXITCODE | Should Be 0

        foreach ($projectPath in @($fixture.ServiceProjectPath, $fixture.DesktopProjectPath)) {
            $projectContent = Get-Content -LiteralPath $projectPath -Raw
            $projectContent | Should Match '<FileVersion>1\.2\.3\.0</FileVersion>'
            $projectContent | Should Match '<AssemblyVersion>1\.2\.3\.0</AssemblyVersion>'
        }
        Get-Content -LiteralPath $fixture.FrontendConfigurationPath -Raw | Should Match "version: '1\.2\.3\.0'"
    }

    It 'rejects project files without exactly one assembly version' {
        New-ReleaseVersionFixture -FixtureRoot $TestDrive -WithoutAssemblyVersion | Out-Null

        & pwsh -NoProfile -File $versionUpdateScriptPath -Version '1.2.3' -RepositoryRoot $TestDrive 2>$null

        $LASTEXITCODE | Should Not Be 0
    }

    It 'rejects invalid release versions' {
        New-ReleaseVersionFixture -FixtureRoot $TestDrive | Out-Null

        & pwsh -NoProfile -File $versionUpdateScriptPath -Version '1.2' -RepositoryRoot $TestDrive 2>$null

        $LASTEXITCODE | Should Not Be 0
    }
}
