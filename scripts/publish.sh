#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIRECTORY="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
VERSION_UPDATE_SCRIPT="$SCRIPT_DIRECTORY/internal/update-release-version.ps1"
BUILD_SCRIPT="$SCRIPT_DIRECTORY/build.ps1"
VERSION_DOCUMENT_SCRIPT="$SCRIPT_DIRECTORY/new-version-doc.ps1"
VERSION_SOURCE_PATHS=(
    'src/api/UZonMailService/UzonMailService.csproj'
    'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj'
    'src/web/src/config/app.config.ts'
)
has_committed_version_update=false

show_usage() {
    cat <<'EOF'
用法：bash scripts/publish.sh [vX.Y.Z]

不传版本号时，脚本会提示输入。直接回车将使用上一个发布标签的下一 patch 版本。
EOF
}

fail() {
    printf '发布失败：%s\n' "$1" >&2
    exit 1
}

ensure_command_exists() {
    local command_name="$1"
    if ! command -v "$command_name" >/dev/null 2>&1; then
        fail "未检测到 $command_name，请先安装后再执行"
    fi
}

normalize_release_version() {
    local input_version="$1"
    if [[ ! "$input_version" =~ ^v?([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
        fail '版本号格式不正确，请输入 x.y.z，例如 0.23.2'
    fi

    printf '%s.%s.%s' "${BASH_REMATCH[1]}" "${BASH_REMATCH[2]}" "${BASH_REMATCH[3]}"
}

increment_patch_version() {
    local normalized_version="$1"
    local major_version
    local minor_version
    local patch_version
    IFS='.' read -r major_version minor_version patch_version <<<"$normalized_version"

    printf '%s.%s.%s' "$major_version" "$minor_version" "$((10#$patch_version + 1))"
}

is_version_greater_than() {
    local candidate_version="$1"
    local reference_version="$2"
    local candidate_major
    local candidate_minor
    local candidate_patch
    local reference_major
    local reference_minor
    local reference_patch
    IFS='.' read -r candidate_major candidate_minor candidate_patch <<<"$candidate_version"
    IFS='.' read -r reference_major reference_minor reference_patch <<<"$reference_version"

    if ((10#$candidate_major != 10#$reference_major)); then
        ((10#$candidate_major > 10#$reference_major))
        return
    fi
    if ((10#$candidate_minor != 10#$reference_minor)); then
        ((10#$candidate_minor > 10#$reference_minor))
        return
    fi

    ((10#$candidate_patch > 10#$reference_patch))
}

get_previous_version_tag() {
    local candidate_tag
    local candidate_version
    local commit_distance
    local nearest_distance=-1
    local -a nearest_tags=()
    local -a candidate_tags=()

    mapfile -t candidate_tags < <(git tag --merged HEAD --list 'v*')
    for candidate_tag in "${candidate_tags[@]}"; do
        if ! candidate_version="$(normalize_release_version "$candidate_tag" 2>/dev/null)"; then
            continue
        fi

        commit_distance="$(git rev-list --count "$candidate_tag..HEAD")"
        if [[ ! "$commit_distance" =~ ^[0-9]+$ ]]; then
            fail "无法解析版本标签 $candidate_tag 到当前提交的距离：$commit_distance"
        fi

        if ((nearest_distance < 0 || commit_distance < nearest_distance)); then
            nearest_distance=$commit_distance
            nearest_tags=("$candidate_tag")
        elif ((commit_distance == nearest_distance)); then
            nearest_tags+=("$candidate_tag")
        fi
    done

    if ((${#nearest_tags[@]} == 0)); then
        fail '当前提交历史中未找到 vX.Y.Z 格式的版本标签，无法确定发布版本'
    fi
    if ((${#nearest_tags[@]} != 1)); then
        fail "找到多个距离当前提交相同的版本标签：${nearest_tags[*]}，无法确定上一个版本"
    fi

    printf '%s' "${nearest_tags[0]}"
}

ensure_clean_worktree() {
    if [[ -n "$(git status --porcelain --untracked-files=all)" ]]; then
        fail '工作区存在未提交变更，请先提交、暂存或清理后再发布'
    fi
}

ensure_only_version_files_are_modified() {
    local changed_path
    local -a changed_paths=()

    mapfile -t changed_paths < <(git diff --name-only)
    for changed_path in "${changed_paths[@]}"; do
        case "$changed_path" in
            'src/api/UZonMailService/UzonMailService.csproj'|'src/win-desktop/UzonMailDesktop/UzonMailDesktop.csproj'|'src/web/src/config/app.config.ts')
                ;;
            *)
                fail "构建产生了未预期的受跟踪文件变更：$changed_path"
                ;;
        esac
    done

    if [[ -n "$(git diff --cached --name-only)" ]]; then
        fail '构建过程中产生了暂存区变更，请检查后再发布'
    fi
    if [[ -n "$(git ls-files --others --exclude-standard)" ]]; then
        fail '构建过程中产生了未跟踪文件，请检查后再发布'
    fi
}

rollback_uncommitted_version_update() {
    local exit_status=$?
    if ((exit_status != 0)) && [[ "$has_committed_version_update" == false ]]; then
        git restore --source=HEAD -- "${VERSION_SOURCE_PATHS[@]}" >/dev/null 2>&1 || true
    fi

    exit "$exit_status"
}

main() {
    local requested_version="${1:-}"
    local repository_root
    local previous_version_tag
    local previous_version
    local default_version
    local version_input
    local release_version
    local full_version

    if (($# > 1)); then
        show_usage >&2
        exit 1
    fi
    if [[ "$requested_version" == '--help' || "$requested_version" == '-h' ]]; then
        show_usage
        return
    fi

    ensure_command_exists git
    ensure_command_exists pwsh

    repository_root="$(git rev-parse --show-toplevel)" || fail '当前目录不在 Git 仓库中'
    cd "$repository_root"
    git remote get-url origin >/dev/null || fail '未配置 origin 远程仓库'
    git fetch --tags origin
    ensure_clean_worktree
    git switch master
    git pull --ff-only origin master
    ensure_clean_worktree

    previous_version_tag="$(get_previous_version_tag)"
    previous_version="${previous_version_tag#v}"
    default_version="$(increment_patch_version "$previous_version")"
    if [[ -n "$requested_version" ]]; then
        release_version="$(normalize_release_version "$requested_version")"
        printf '使用命令行指定的版本号：%s\n' "$release_version"
    else
        if ! IFS= read -r -p "请输入本次版本号（直接回车使用 $default_version）: " version_input; then
            fail '无法读取版本号输入'
        fi

        if [[ -z "$version_input" ]]; then
            release_version="$default_version"
            printf '使用默认版本号：%s\n' "$release_version"
        else
            release_version="$(normalize_release_version "$version_input")"
            printf '使用输入的版本号：%s\n' "$release_version"
        fi
    fi

    if ! is_version_greater_than "$release_version" "$previous_version"; then
        fail "发布版本必须高于上一个版本 $previous_version"
    fi
    if git rev-parse -q --verify "refs/tags/v$release_version" >/dev/null; then
        fail "标签 v$release_version 已存在"
    fi

    full_version="$release_version.0"
    trap rollback_uncommitted_version_update EXIT

    pwsh -NoProfile -File "$VERSION_UPDATE_SCRIPT" -Version "$release_version" -RepositoryRoot "$repository_root"
    ensure_only_version_files_are_modified

    pwsh -NoProfile -File "$BUILD_SCRIPT" -Target All -PushDockerImage -UploadArtifacts
    ensure_only_version_files_are_modified

    if ! git diff --quiet -- "${VERSION_SOURCE_PATHS[@]}"; then
        git add -- "${VERSION_SOURCE_PATHS[@]}"
        git commit -m "Build: Bump version to $full_version"
    fi
    has_committed_version_update=true

    pwsh -NoProfile -File "$VERSION_DOCUMENT_SCRIPT" -Version "$release_version"
    git push origin master
    git tag "v$release_version"
    git push origin "v$release_version"

    printf '发布完成：%s\n' "$release_version"
}

if [[ "${BASH_SOURCE[0]}" == "$0" ]]; then
    main "$@"
fi
