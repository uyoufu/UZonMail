#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd -P)"

# 加载安装脚本函数，并跳过脚本入口。
source_install_functions() {
  # shellcheck source=/dev/null
  source <(sed '/^run_installation_flow "\$@"/d' "$ROOT_DIR/docker/install.sh")
}

# 创建 Redis 测试使用的临时 env 文件。
create_test_env_file() {
  local env_file="$1"
  local compose_profiles="${2-postgresql}"

  cat > "$env_file" <<'ENV'
Database__Redis__Enable=true
ENV
  update_env_config_value "$env_file" "COMPOSE_PROFILES" "$compose_profiles"
}

# 断言 env 文件中的配置值符合预期。
assert_env_value() {
  local env_file="$1"
  local config_key="$2"
  local expected_value="$3"
  local actual_value

  actual_value="$(read_env_config_value "$env_file" "$config_key")"
  if [[ "$actual_value" != "$expected_value" ]]; then
    echo "期望 $config_key=$expected_value，实际为 $actual_value" >&2
    return 1
  fi
}

# 测试默认回车时禁用 Redis。
test_default_disables_redis() {
  local tmp_dir
  local env_file

  tmp_dir="$(mktemp -d)"
  env_file="$tmp_dir/.env"
  create_test_env_file "$env_file"

  printf '\n' | configure_redis_values "$env_file"

  assert_env_value "$env_file" "COMPOSE_PROFILES" "postgresql"
  assert_env_value "$env_file" "Database__Redis__Enable" "false"
}

# 测试确认启用时追加 Redis profile。
test_yes_enables_redis_profile() {
  local tmp_dir
  local env_file

  tmp_dir="$(mktemp -d)"
  env_file="$tmp_dir/.env"
  create_test_env_file "$env_file"

  printf 'y\n' | configure_redis_values "$env_file"

  assert_env_value "$env_file" "COMPOSE_PROFILES" "postgresql,redis"
  assert_env_value "$env_file" "Database__Redis__Enable" "true"
}

# 测试空 profile 启用 Redis 时不生成多余逗号。
test_yes_enables_redis_from_empty_profiles() {
  local tmp_dir
  local env_file

  tmp_dir="$(mktemp -d)"
  env_file="$tmp_dir/.env"
  create_test_env_file "$env_file" ""

  printf 'y\n' | configure_redis_values "$env_file"

  assert_env_value "$env_file" "COMPOSE_PROFILES" "redis"
  assert_env_value "$env_file" "Database__Redis__Enable" "true"
}

# 测试已有 Redis profile 时不会重复追加。
test_yes_keeps_existing_redis_profile() {
  local tmp_dir
  local env_file

  tmp_dir="$(mktemp -d)"
  env_file="$tmp_dir/.env"
  create_test_env_file "$env_file" "postgresql,redis"

  printf 'y\n' | configure_redis_values "$env_file"

  assert_env_value "$env_file" "COMPOSE_PROFILES" "postgresql,redis"
  assert_env_value "$env_file" "Database__Redis__Enable" "true"
}

# 测试禁用 Redis 时只移除 Redis profile。
test_no_removes_only_redis_profile() {
  local tmp_dir
  local env_file

  tmp_dir="$(mktemp -d)"
  env_file="$tmp_dir/.env"
  create_test_env_file "$env_file" "postgresql,redis"

  printf 'n\n' | configure_redis_values "$env_file"

  assert_env_value "$env_file" "COMPOSE_PROFILES" "postgresql"
  assert_env_value "$env_file" "Database__Redis__Enable" "false"
}

source_install_functions
test_default_disables_redis
test_yes_enables_redis_profile
test_yes_enables_redis_from_empty_profiles
test_yes_keeps_existing_redis_profile
test_no_removes_only_redis_profile
echo "docker_install_redis_test passed"
