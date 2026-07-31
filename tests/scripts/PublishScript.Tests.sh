#!/usr/bin/env bash

set -euo pipefail

TEST_DIRECTORY="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd -P)"
REPOSITORY_ROOT="$(cd -- "$TEST_DIRECTORY/../.." && pwd -P)"
PUBLISH_SCRIPT="$REPOSITORY_ROOT/scripts/publish.sh"

assert_equal() {
    local expected_value="$1"
    local actual_value="$2"
    local assertion_name="$3"
    if [[ "$actual_value" != "$expected_value" ]]; then
        printf '断言失败：%s。期望“%s”，实际“%s”\n' "$assertion_name" "$expected_value" "$actual_value" >&2
        exit 1
    fi
}

source "$PUBLISH_SCRIPT"

assert_equal '1.2.3' "$(normalize_release_version 'v1.2.3')" '版本号规范化'
assert_equal '0.23.2' "$(increment_patch_version '0.23.1')" '默认 patch 版本'

git_fixture_root="$(mktemp -d)"
cleanup_git_fixture() {
    rm -rf "$git_fixture_root"
}
trap cleanup_git_fixture EXIT

git -C "$git_fixture_root" init --quiet
git -C "$git_fixture_root" config user.email 'tests@example.com'
git -C "$git_fixture_root" config user.name 'Publish Script Tests'
printf 'baseline\n' >"$git_fixture_root/release-notes.txt"
git -C "$git_fixture_root" add release-notes.txt
git -C "$git_fixture_root" commit --quiet -m 'feat: baseline release'
git -C "$git_fixture_root" tag v0.1.0
printf 'internal change\n' >>"$git_fixture_root/release-notes.txt"
git -C "$git_fixture_root" commit --quiet -am 'chore: internal build update'
git -C "$git_fixture_root" tag latest
git -C "$git_fixture_root" tag v0.1.0.1
printf 'release candidate\n' >>"$git_fixture_root/release-notes.txt"
git -C "$git_fixture_root" commit --quiet -am 'feat: release candidate'

pushd "$git_fixture_root" >/dev/null
previous_version_tag="$(get_previous_version_tag)"
popd >/dev/null

assert_equal 'v0.1.0' "$previous_version_tag" '最近祖先版本标签'
printf 'PublishScript.Tests.sh passed\n'
