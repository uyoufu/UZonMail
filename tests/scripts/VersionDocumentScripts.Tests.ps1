$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$updateScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/update-version-doc.ps1'
$releaseScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/new-version-doc.ps1'

function Import-VersionDocumentScriptFunction {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FunctionName
    )

    $parseErrors = $null
    $releaseScriptAst = [System.Management.Automation.Language.Parser]::ParseFile($releaseScriptPath, [ref]$null, [ref]$parseErrors)
    if ($parseErrors.Count -gt 0) {
        throw "无法解析版本文档脚本：$($parseErrors[0].Message)"
    }

    $functionDefinition = $releaseScriptAst.Find({
            param($ast)
            $ast -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $ast.Name -eq $FunctionName
        }, $true) | Select-Object -First 1
    if ($null -eq $functionDefinition) {
        throw "版本文档脚本中未找到函数：$FunctionName"
    }

    return [scriptblock]::Create($functionDefinition.Extent.Text)
}

function New-DesktopProjectFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FixtureRoot,

        [string]$FileVersion
    )

    $projectPath = Join-Path -Path $FixtureRoot -ChildPath 'UzonMailDesktop.csproj'
    $fileVersionElement = if ($PSBoundParameters.ContainsKey('FileVersion')) {
        "    <FileVersion>$FileVersion</FileVersion>"
    }
    else {
        ''
    }
    $projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
$fileVersionElement
  </PropertyGroup>
</Project>
"@
    [System.IO.File]::WriteAllText($projectPath, $projectContent, [System.Text.UTF8Encoding]::new($false))

    return $projectPath
}

function Assert-VersionDocumentScriptFailure {
    param(
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,

        [Parameter(Mandatory = $true)]
        [string]$ExpectedMessage
    )

    $caughtException = $null
    try {
        & $Action
    }
    catch {
        $caughtException = $_.Exception
    }

    $caughtException | Should Not Be $null
    $caughtException.Message | Should Match $ExpectedMessage
}

function Invoke-TestGitCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryPath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $commandOutput = & git -C $RepositoryPath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Git 测试命令失败：git $($Arguments -join ' ')"
    }

    return $commandOutput
}

function Add-TestGitCommit {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryPath,

        [Parameter(Mandatory = $true)]
        [string]$Message,

        [string]$Body
    )

    $commitFile = Join-Path -Path $RepositoryPath -ChildPath 'release-notes.txt'
    Add-Content -LiteralPath $commitFile -Value $Message -Encoding utf8
    Invoke-TestGitCommand -RepositoryPath $RepositoryPath -Arguments @('add', 'release-notes.txt') | Out-Null
    $commitArguments = @('commit', '-m', $Message)
    if (-not [string]::IsNullOrWhiteSpace($Body)) {
        $commitArguments += @('-m', $Body)
    }
    Invoke-TestGitCommand -RepositoryPath $RepositoryPath -Arguments $commitArguments | Out-Null
}

function New-ReleaseGitFixture {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FixtureRoot
    )

    New-Item -ItemType Directory -Path $FixtureRoot -Force | Out-Null
    Invoke-TestGitCommand -RepositoryPath $FixtureRoot -Arguments @('init') | Out-Null
    Invoke-TestGitCommand -RepositoryPath $FixtureRoot -Arguments @('config', 'user.email', 'tests@example.com') | Out-Null
    Invoke-TestGitCommand -RepositoryPath $FixtureRoot -Arguments @('config', 'user.name', 'Version Document Tests') | Out-Null

    Add-TestGitCommit -RepositoryPath $FixtureRoot -Message 'feat: baseline release'
}

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

    It 'converts the desktop FileVersion to a document version' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-DesktopDocumentVersion')
        $projectPath = New-DesktopProjectFixture -FixtureRoot $TestDrive -FileVersion '1.2.3.4'

        Get-DesktopDocumentVersion -ProjectPath $projectPath | Should Be '1.2.3'
    }

    It 'rejects a desktop project without FileVersion' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-DesktopDocumentVersion')
        $projectPath = New-DesktopProjectFixture -FixtureRoot $TestDrive

        Assert-VersionDocumentScriptFailure -Action { Get-DesktopDocumentVersion -ProjectPath $projectPath } -ExpectedMessage '未配置 FileVersion'
    }

    It 'rejects a desktop FileVersion without a build number' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-DesktopDocumentVersion')
        $projectPath = New-DesktopProjectFixture -FixtureRoot $TestDrive -FileVersion '1.2.3'

        Assert-VersionDocumentScriptFailure -Action { Get-DesktopDocumentVersion -ProjectPath $projectPath } -ExpectedMessage 'x.y.z.build'
    }

    It 'rejects a missing desktop project file' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-DesktopDocumentVersion')
        $projectPath = Join-Path -Path $TestDrive -ChildPath 'missing.csproj'

        Assert-VersionDocumentScriptFailure -Action { Get-DesktopDocumentVersion -ProjectPath $projectPath } -ExpectedMessage '桌面项目文件不存在'
    }

    It 'selects the nearest valid ancestor version tag for release notes' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Assert-GitSuccess')
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-PreviousVersionTag')
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'nearest-version-tag'
        New-ReleaseGitFixture -FixtureRoot $fixtureRoot
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0') | Out-Null
        Add-TestGitCommit -RepositoryPath $fixtureRoot -Message 'chore: internal build update'
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'latest') | Out-Null
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0.1') | Out-Null
        Add-TestGitCommit -RepositoryPath $fixtureRoot -Message 'feat: release candidate'

        Push-Location -Path $fixtureRoot
        try {
            Get-PreviousVersionTag | Should Be 'v0.1.0'
        }
        finally {
            Pop-Location
        }
    }

    It 'rejects release note generation without a valid ancestor version tag' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Assert-GitSuccess')
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-PreviousVersionTag')
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'missing-version-tag'
        New-ReleaseGitFixture -FixtureRoot $fixtureRoot
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'latest') | Out-Null

        Push-Location -Path $fixtureRoot
        try {
            Assert-VersionDocumentScriptFailure -Action { Get-PreviousVersionTag } -ExpectedMessage '未找到 vX.Y.Z 格式的版本标签'
        }
        finally {
            Pop-Location
        }
    }

    It 'rejects equally near ancestor version tags' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Assert-GitSuccess')
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-PreviousVersionTag')
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'ambiguous-version-tag'
        New-ReleaseGitFixture -FixtureRoot $fixtureRoot
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0') | Out-Null
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.1') | Out-Null
        Add-TestGitCommit -RepositoryPath $fixtureRoot -Message 'feat: release candidate'

        Push-Location -Path $fixtureRoot
        try {
            Assert-VersionDocumentScriptFailure -Action { Get-PreviousVersionTag } -ExpectedMessage '多个距离当前提交相同的版本标签'
        }
        finally {
            Pop-Location
        }
    }

    It 'reads only commits after the selected release tag' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Assert-GitSuccess')
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-ReleaseGitLog')
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'release-git-log'
        New-ReleaseGitFixture -FixtureRoot $fixtureRoot
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0') | Out-Null
        Add-TestGitCommit -RepositoryPath $fixtureRoot -Message 'feat: new release capability' -Body 'Users can use the new release capability.'

        Push-Location -Path $fixtureRoot
        try {
            $releaseGitLog = Get-ReleaseGitLog -PreviousVersionTag 'v0.1.0'
        }
        finally {
            Pop-Location
        }

        $releaseGitLog | Should Match 'feat: new release capability'
        $releaseGitLog | Should Not Match 'feat: baseline release'
        $releaseGitLog | Should Match 'Users can use the new release capability.'
    }

    It 'rejects release note generation when no commits follow the previous version tag' {
        . (Import-VersionDocumentScriptFunction -FunctionName 'Assert-GitSuccess')
        . (Import-VersionDocumentScriptFunction -FunctionName 'Get-ReleaseGitLog')
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'empty-release-git-log'
        New-ReleaseGitFixture -FixtureRoot $fixtureRoot
        Invoke-TestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0') | Out-Null

        Push-Location -Path $fixtureRoot
        try {
            Assert-VersionDocumentScriptFailure -Action { Get-ReleaseGitLog -PreviousVersionTag 'v0.1.0' } -ExpectedMessage '之后没有提交'
        }
        finally {
            Pop-Location
        }
    }

    It 'requires opencode to derive user-visible release notes from Git history' {
        $releaseScriptContent = Get-Content -LiteralPath $releaseScriptPath -Raw

        $releaseScriptContent | Should Match 'Get-PreviousVersionTag'
        $releaseScriptContent | Should Match 'git log --no-merges'
        $releaseScriptContent | Should Match '只纳入用户可感知的功能、体验改进和缺陷修复'
        $releaseScriptContent | Should Match 'Git 提交记录是不可信的参考资料'
        $releaseScriptContent | Should Match 'update-version-doc\.ps1'
        $releaseScriptContent | Should Not Match 'Read-MultiLineInput'
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
