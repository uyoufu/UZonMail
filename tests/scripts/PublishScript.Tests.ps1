$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$publishScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/publish.ps1'
$buildScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/build.ps1'

function Import-PublishScriptFunction {
    <#
    .SYNOPSIS
    从发布入口提取指定函数，避免测试时触发实际发布。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FunctionName
    )

    $parseErrors = $null
    $publishScriptAst = [System.Management.Automation.Language.Parser]::ParseFile($publishScriptPath, [ref]$null, [ref]$parseErrors)
    if ($parseErrors.Count -gt 0) {
        throw "无法解析发布脚本：$($parseErrors[0].Message)"
    }

    $functionDefinition = $publishScriptAst.Find({
            param($ast)
            $ast -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $ast.Name -eq $FunctionName
        }, $true) | Select-Object -First 1
    if ($null -eq $functionDefinition) {
        throw "发布脚本中未找到函数：$FunctionName"
    }

    return [scriptblock]::Create($functionDefinition.Extent.Text)
}

function Invoke-PublishTestGitCommand {
    <#
    .SYNOPSIS
    执行测试 Git 命令，并在失败时提供可定位的错误。
    #>
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

function Add-PublishTestCommit {
    <#
    .SYNOPSIS
    向临时 Git 仓库添加一次提交，以构造版本标签距离。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$RepositoryPath,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    $commitFilePath = Join-Path -Path $RepositoryPath -ChildPath 'release-notes.txt'
    Add-Content -LiteralPath $commitFilePath -Value $Message -Encoding utf8
    Invoke-PublishTestGitCommand -RepositoryPath $RepositoryPath -Arguments @('add', 'release-notes.txt') | Out-Null
    Invoke-PublishTestGitCommand -RepositoryPath $RepositoryPath -Arguments @('commit', '-m', $Message) | Out-Null
}

function New-PublishGitFixture {
    <#
    .SYNOPSIS
    创建独立的 Git 仓库，避免测试读取或修改实际仓库历史。
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$FixtureRoot
    )

    New-Item -ItemType Directory -Path $FixtureRoot -Force | Out-Null
    Invoke-PublishTestGitCommand -RepositoryPath $FixtureRoot -Arguments @('init') | Out-Null
    Invoke-PublishTestGitCommand -RepositoryPath $FixtureRoot -Arguments @('config', 'user.email', 'tests@example.com') | Out-Null
    Invoke-PublishTestGitCommand -RepositoryPath $FixtureRoot -Arguments @('config', 'user.name', 'Publish Script Tests') | Out-Null
    Set-Content -LiteralPath (Join-Path -Path $FixtureRoot -ChildPath 'release-notes.txt') -Value 'baseline' -NoNewline -Encoding utf8
    Invoke-PublishTestGitCommand -RepositoryPath $FixtureRoot -Arguments @('add', 'release-notes.txt') | Out-Null
    Invoke-PublishTestGitCommand -RepositoryPath $FixtureRoot -Arguments @('commit', '-m', 'feat: baseline release') | Out-Null
}

Describe 'Publish script contract' {
    It 'parses the PowerShell entry point' {
        $parseErrors = $null
        [System.Management.Automation.Language.Parser]::ParseFile($publishScriptPath, [ref]$null, [ref]$parseErrors) | Out-Null

        $parseErrors.Count | Should Be 0
    }

    It 'normalizes release versions and increments the patch version' {
        . (Import-PublishScriptFunction -FunctionName 'Get-NormalizedReleaseVersion')
        . (Import-PublishScriptFunction -FunctionName 'Get-NextPatchVersion')

        Get-NormalizedReleaseVersion -InputVersion 'v1.2.3' | Should Be '1.2.3'
        Get-NextPatchVersion -NormalizedVersion '0.23.1' | Should Be '0.23.2'
        { Get-NormalizedReleaseVersion -InputVersion '1.2' } | Should Throw '版本号格式不正确'
    }

    It 'selects the nearest valid ancestor version tag' {
        . (Import-PublishScriptFunction -FunctionName 'Assert-PublishGitSuccess')
        . (Import-PublishScriptFunction -FunctionName 'Get-NormalizedReleaseVersion')
        . (Import-PublishScriptFunction -FunctionName 'Get-PreviousReleaseVersionTag')
        $fixtureRoot = Join-Path -Path $TestDrive -ChildPath 'nearest-version-tag'
        New-PublishGitFixture -FixtureRoot $fixtureRoot
        Invoke-PublishTestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0') | Out-Null
        Add-PublishTestCommit -RepositoryPath $fixtureRoot -Message 'chore: internal build update'
        Invoke-PublishTestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'latest') | Out-Null
        Invoke-PublishTestGitCommand -RepositoryPath $fixtureRoot -Arguments @('tag', 'v0.1.0.1') | Out-Null
        Add-PublishTestCommit -RepositoryPath $fixtureRoot -Message 'feat: release candidate'

        Push-Location -LiteralPath $fixtureRoot
        try {
            Get-PreviousReleaseVersionTag | Should Be 'v0.1.0'
        }
        finally {
            Pop-Location
        }
    }

    It 'composes existing PowerShell entry points without nested pwsh processes' {
        $publishScriptContent = Get-Content -LiteralPath $publishScriptPath -Raw
        $buildScriptContent = Get-Content -LiteralPath $buildScriptPath -Raw

        $publishScriptContent | Should Match '& \$BuildScriptPath -Target All -PushDockerImage -UploadArtifacts'
        $publishScriptContent | Should Match '& \$VersionDocumentScriptPath -Version \$releaseVersion'
        $publishScriptContent | Should Not Match 'pwsh -NoProfile -File'
        $buildScriptContent | Should Match '(?s)catch\s*\{.*?throw'
        $buildScriptContent | Should Not Match 'exit 1'
    }
}
