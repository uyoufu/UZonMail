$repositoryRoot = Split-Path -Path $PSScriptRoot -Parent
$repositoryRoot = Split-Path -Path $repositoryRoot -Parent
$publishScriptPath = Join-Path -Path $repositoryRoot -ChildPath 'scripts/publish.sh'

Describe 'Publish script contract' {
    It 'uses a testable Bash entry point' {
        $publishScriptContent = Get-Content -LiteralPath $publishScriptPath -Raw

        $publishScriptContent | Should Match '^#!/usr/bin/env bash'
        $publishScriptContent | Should Match 'if \[\[ "\$\{BASH_SOURCE\[0\]\}" == "\$0" \]\]; then'
    }

    It 'prompts for the default patch version when no argument is supplied' {
        $publishScriptContent = Get-Content -LiteralPath $publishScriptPath -Raw

        $publishScriptContent | Should Match 'get_previous_version_tag'
        $publishScriptContent | Should Match 'increment_patch_version'
        $publishScriptContent | Should Match 'read -r -p "请输入本次版本号（直接回车使用 \$default_version）'
        $publishScriptContent | Should Match '使用默认版本号'
    }

    It 'uses the existing build and version document entry points' {
        $publishScriptContent = Get-Content -LiteralPath $publishScriptPath -Raw

        $publishScriptContent | Should Match 'BUILD_SCRIPT="\$SCRIPT_DIRECTORY/build\.ps1"'
        $publishScriptContent | Should Match 'pwsh -NoProfile -File "\$BUILD_SCRIPT" -Target All -PushDockerImage -UploadArtifacts'
        $publishScriptContent | Should Match 'VERSION_DOCUMENT_SCRIPT="\$SCRIPT_DIRECTORY/new-version-doc\.ps1"'
        $publishScriptContent | Should Match 'pwsh -NoProfile -File "\$VERSION_DOCUMENT_SCRIPT" -Version "\$release_version"'
        $publishScriptContent | Should Match 'git tag "v\$release_version"'
    }
}
