$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$updateScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/update-version-doc.ps1'
$releaseScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/new-version-doc.ps1'

function New-VersionDocumentFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FixtureRoot,

        [string[]]$ManifestVersions = @('0.23.0.5')
    )

    $chineseDocumentPath = Join-Path -Path $FixtureRoot -ChildPath 'docs/docs/downloads.md'
    $englishDocumentPath = Join-Path -Path $FixtureRoot -ChildPath 'docs/docs/en/downloads.md'
    $updatesDirectory = Join-Path -Path $FixtureRoot -ChildPath 'build/updates'
    New-Item -ItemType Directory -Path (Split-Path -Path $chineseDocumentPath -Parent) -Force | Out-Null
    New-Item -ItemType Directory -Path (Split-Path -Path $englishDocumentPath -Parent) -Force | Out-Null
    New-Item -ItemType Directory -Path $updatesDirectory -Force | Out-Null

    $documentHeader = "---`npermalink: /downloads`n---`n`n"
    $existingEntry = "## 0.22.1`n`n> 更新时期: 2026-06-03`n`n### 功能优化`n`n1. Existing note`n"
    [System.IO.File]::WriteAllText($chineseDocumentPath, $documentHeader + $existingEntry, [System.Text.UTF8Encoding]::new($false))
    [System.IO.File]::WriteAllText($englishDocumentPath, $documentHeader + $existingEntry, [System.Text.UTF8Encoding]::new($false))

    foreach ($manifestVersion in $ManifestVersions) {
        $manifestPath = Join-Path -Path $updatesDirectory -ChildPath "$manifestVersion.json"
        [System.IO.File]::WriteAllText($manifestPath, "{`"version`":`"$manifestVersion`"}", [System.Text.UTF8Encoding]::new($false))
    }
}

function Invoke-VersionDocumentUpdate {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FixtureRoot,

        [Parameter(Mandatory = $true)]
        [string]$UpdatePath,

        [Parameter(Mandatory = $true)]
        [string]$MarkdownBody
    )

    & $updateScriptPath -Version '0.23.0' -UpdatePath $UpdatePath -MarkdownContent $MarkdownBody -RepositoryRoot $FixtureRoot
}

Describe 'Version document scripts' {
    It 'parses the release and direct update scripts' {
        foreach ($scriptPath in @($releaseScriptPath, $updateScriptPath)) {
            $parseErrors = $null
            [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$null, [ref]$parseErrors) | Out-Null

            $parseErrors.Count | Should Be 0
        }
    }

    It 'adds a Chinese entry and copies the matching manifest' {
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'chinese-release'
        New-VersionDocumentFixture -FixtureRoot $fixtureRoot
        $markdownBody = "### 功能新增`n`n1. 支持更新清单同步"

        Invoke-VersionDocumentUpdate -FixtureRoot $fixtureRoot -UpdatePath 'docs/docs/downloads.md' -MarkdownBody $markdownBody

        $documentContent = Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/downloads.md') -Raw
        $documentContent | Should Match '(?m)^## 0\.23\.0$'
        $documentContent | Should Match '更新时期: \d{4}-\d{2}-\d{2}'
        $documentContent | Should Match 'uzonmail-desktop-win-x64-0\.23\.0\.5\.zip'
        Test-Path -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/.vuepress/public/updates/0.23.0.5.json') | Should Be $true
        Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/.vuepress/public/updates/latest.json') -Raw | Should Match '0\.23\.0\.5'
    }

    It 'accepts Markdown from the pipeline used by opencode' {
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'pipeline-release'
        New-VersionDocumentFixture -FixtureRoot $fixtureRoot
        $markdownBody = "### Bug 修复`n`n1. 修复版本文档发布流程"

        $markdownBody | & $updateScriptPath -Version '0.23.0' -UpdatePath 'docs/docs/downloads.md' -RepositoryRoot $fixtureRoot

        $documentContent = Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/downloads.md') -Raw
        $documentContent | Should Match '修复版本文档发布流程'
    }

    It 'updates the existing version block without creating a duplicate entry' {
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'idempotent-release'
        New-VersionDocumentFixture -FixtureRoot $fixtureRoot
        $markdownBody = "### 功能优化`n`n1. 优化版本文档流程"

        Invoke-VersionDocumentUpdate -FixtureRoot $fixtureRoot -UpdatePath 'docs/docs/downloads.md' -MarkdownBody $markdownBody
        $firstDocumentContent = Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/downloads.md') -Raw
        Invoke-VersionDocumentUpdate -FixtureRoot $fixtureRoot -UpdatePath 'docs/docs/downloads.md' -MarkdownBody $markdownBody
        $secondDocumentContent = Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/downloads.md') -Raw

        ([regex]::Matches($secondDocumentContent, '(?m)^## 0\.23\.0$')).Count | Should Be 1
        $secondDocumentContent | Should Be $firstDocumentContent
    }

    It 'uses English document labels based on the update path' {
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'english-release'
        New-VersionDocumentFixture -FixtureRoot $fixtureRoot

        Invoke-VersionDocumentUpdate -FixtureRoot $fixtureRoot -UpdatePath 'docs/docs/en/downloads.md' -MarkdownBody "### Improvements`n`n1. Improve release automation"

        $documentContent = Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/en/downloads.md') -Raw
        $documentContent | Should Match 'Release Date: \d{4}-\d{2}-\d{2}'
        $documentContent | Should Match '(?m)^### Downloads$'
    }

    It 'falls back to build number zero when no matching manifest exists' {
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'missing-manifest'
        New-VersionDocumentFixture -FixtureRoot $fixtureRoot -ManifestVersions @()

        Invoke-VersionDocumentUpdate -FixtureRoot $fixtureRoot -UpdatePath 'docs/docs/downloads.md' -MarkdownBody "### Bug 修复`n`n1. 修复发布脚本"

        $documentContent = Get-Content -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/downloads.md') -Raw
        $documentContent | Should Match 'uzonmail-desktop-win-x64-0\.23\.0\.0\.zip'
        Test-Path -LiteralPath (Join-Path -Path $fixtureRoot -ChildPath 'docs/docs/.vuepress/public/updates') | Should Be $false
    }

    It 'rejects multiple manifests for the same release version' {
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'ambiguous-manifest'
        New-VersionDocumentFixture -FixtureRoot $fixtureRoot -ManifestVersions '0.23.0.0'
        $updatesDirectory = Join-Path -Path $fixtureRoot -ChildPath 'build/updates'
        [System.IO.File]::WriteAllText(
            (Join-Path -Path $updatesDirectory -ChildPath '0.23.0.1.json'),
            '{"version":"0.23.0.1"}',
            [System.Text.UTF8Encoding]::new($false)
        )
        @(Get-ChildItem -LiteralPath $updatesDirectory -File).Count | Should Be 2

        & pwsh -NoProfile -File $updateScriptPath -Version '0.23.0' -UpdatePath 'docs/docs/downloads.md' -MarkdownContent "### Bug 修复`n`n1. 修复发布脚本" -RepositoryRoot $fixtureRoot 2>$null
        $LASTEXITCODE | Should Not Be 0
    }
}
