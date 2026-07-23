#!/usr/bin/env bash
set -euo pipefail

ENV_DOWNLOAD_URL="https://raw.githubusercontent.com/uyoufu/UzonMail/refs/heads/master/docker/.env"
COMPOSE_DOWNLOAD_URL="https://raw.githubusercontent.com/uyoufu/UzonMail/refs/heads/master/docker/docker-compose.yml"
DEFAULT_CONTAINER_NAME="uzon-mail"
DEFAULT_TOKEN_SECRET="B81806DA00600865988B2A305B91C47825750972A0A7159CCDC63A9838248D77"
DEFAULT_ADMIN_PASSWORD="admin1234"
DEFAULT_DATABASE_PASSWORD="uzon-mail"

# 输出普通安装日志。
write_install_log() {
  echo "[UzonMail] $*" >&2
}

# 输出错误日志。
write_error_log() {
  echo "[UzonMail] $*" >&2
}

# 检查 Docker 与 Docker Compose v2 是否可用。
ensure_docker_runtime_ready() {
  if ! command -v docker >/dev/null 2>&1; then
    write_error_log "未检测到 docker，请先安装 Docker。"
    exit 1
  fi

  if ! docker ps >/dev/null 2>&1; then
    write_error_log "无法访问 Docker，请确认 Docker 已启动且当前用户有权限。"
    exit 1
  fi

  if ! docker compose version >/dev/null 2>&1; then
    write_error_log "未检测到 Docker Compose v2，请先安装 docker compose 插件。"
    exit 1
  fi
}

# 判断指定容器是否已处于 running 状态。
is_container_running_by_name() {
  local container_name="$1"
  docker ps \
    --filter "name=^/${container_name}$" \
    --filter "status=running" \
    --format "{{.Names}}" | grep -Fxq "$container_name"
}

# 默认容器已启动时直接跳过安装。
exit_when_default_service_running() {
  if is_container_running_by_name "$DEFAULT_CONTAINER_NAME"; then
    write_install_log "检测到 $DEFAULT_CONTAINER_NAME 容器已启动，跳过安装。"
    exit 0
  fi
}

# 将用户输入的目录转换为绝对路径。
normalize_install_directory() {
  local input_path="$1"

  case "$input_path" in
    "~")
      input_path="$HOME"
      ;;
    "~/"*)
      input_path="$HOME/${input_path#~/}"
      ;;
  esac

  mkdir -p "$input_path"
  cd "$input_path"
  pwd -P
}

# 读取用户输入，空值时返回默认值。
read_value_with_default() {
  local prompt_text="$1"
  local default_value="$2"
  local input_value

  read -r -p "$prompt_text [$default_value]: " input_value
  if [[ -z "$input_value" ]]; then
    printf "%s" "$default_value"
    return
  fi

  printf "%s" "$input_value"
}

# 输出配置作用后读取用户输入。
read_described_value_with_default() {
  local prompt_text="$1"
  local default_value="$2"
  local config_usage="$3"

  write_install_log "作用：$config_usage"
  read_value_with_default "$prompt_text" "$default_value"
}

# 输出配置作用后读取必填项，避免关键连接信息为空。
read_required_described_value() {
  local prompt_text="$1"
  local default_value="$2"
  local config_usage="$3"
  local input_value

  while true; do
    input_value="$(read_described_value_with_default "$prompt_text" "$default_value" "$config_usage")"
    if [[ -n "$input_value" ]]; then
      printf "%s" "$input_value"
      return
    fi

    write_install_log "该配置不能为空。"
  done
}

# 读取用户确认，支持设置默认是否确认。
read_yes_no_with_default() {
  local prompt_text="$1"
  local default_value="$2"
  local input_value

  while true; do
    if [[ "$default_value" == "y" ]]; then
      read -r -p "$prompt_text [Y/n]: " input_value
      input_value="${input_value:-y}"
    else
      read -r -p "$prompt_text [y/N]: " input_value
      input_value="${input_value:-n}"
    fi

    case "$input_value" in
      y|Y|yes|YES)
        return 0
        ;;
      n|N|no|NO)
        return 1
        ;;
      *)
        write_install_log "请输入 y 或 n。"
        ;;
    esac
  done
}

# 输出配置作用后读取用户确认。
read_described_yes_no_with_default() {
  local prompt_text="$1"
  local default_value="$2"
  local config_usage="$3"

  write_install_log "作用：$config_usage"
  read_yes_no_with_default "$prompt_text" "$default_value"
}

# 提示并确认安装目录。
confirm_install_directory() {
  local default_directory
  local input_directory
  local normalized_directory

  default_directory="$(pwd -P)"
  while true; do
    input_directory="$(read_value_with_default "请输入安装目录" "$default_directory")"
    normalized_directory="$(normalize_install_directory "$input_directory")"
    write_install_log "安装目录：$normalized_directory"

    if read_yes_no_with_default "确认使用该安装目录吗" "y"; then
      printf "%s" "$normalized_directory"
      return
    fi
  done
}

# 若当前没有代理环境变量，则允许用户输入本次下载代理。
configure_download_proxy() {
  local proxy_value

  if [[ -n "${http_proxy:-}${https_proxy:-}${all_proxy:-}${HTTP_PROXY:-}${HTTPS_PROXY:-}${ALL_PROXY:-}" ]]; then
    write_install_log "检测到已有代理环境变量，将直接用于下载。"
    return
  fi

  read -r -p "请输入下载代理地址，留空跳过: " proxy_value
  if [[ -z "$proxy_value" ]]; then
    write_install_log "未设置下载代理。"
    return
  fi

  export http_proxy="$proxy_value"
  export https_proxy="$proxy_value"
  export all_proxy="$proxy_value"
  export HTTP_PROXY="$proxy_value"
  export HTTPS_PROXY="$proxy_value"
  export ALL_PROXY="$proxy_value"
  write_install_log "已设置本次下载代理。"
}

# 下载文件到目标路径，若目标已存在则复用。
download_file_when_missing() {
  local target_file="$1"
  local download_url="$2"
  local download_tmp

  if [[ -f "$target_file" ]]; then
    write_install_log "检测到 $target_file 已存在，直接复用。"
    return
  fi

  write_install_log "开始下载 $target_file"
  download_tmp="${target_file}.download.$$"
  rm -f "$download_tmp"

  if command -v curl >/dev/null 2>&1; then
    curl -fL -o "$download_tmp" "$download_url"
  elif command -v wget >/dev/null 2>&1; then
    wget -O "$download_tmp" "$download_url"
  else
    write_error_log "未检测到 curl 或 wget，无法下载 $target_file。"
    exit 1
  fi

  mv "$download_tmp" "$target_file"
}

# 准备安装目录中的 compose 与 env 文件。
prepare_installation_files() {
  local install_directory="$1"

  cd "$install_directory"
  download_file_when_missing ".env" "$ENV_DOWNLOAD_URL"
  download_file_when_missing "docker-compose.yml" "$COMPOSE_DOWNLOAD_URL"
  cp ".env" ".env.tmp"
}

# 从 env 文件中读取指定键的值。
read_env_config_value() {
  local env_file="$1"
  local config_key="$2"
  local matched_line

  matched_line="$(grep -E "^${config_key}=" "$env_file" | tail -n 1 || true)"
  if [[ -z "$matched_line" ]]; then
    return
  fi

  printf "%s" "${matched_line#*=}"
}

# 更新 env 文件中的指定键，不存在时追加。
update_env_config_value() {
  local env_file="$1"
  local config_key="$2"
  local config_value="$3"
  local write_tmp

  write_tmp="${env_file}.write.$$"
  awk -v key="$config_key" -v value="$config_value" '
    BEGIN { updated = 0 }
    $0 ~ "^" key "=" {
      print key "=" value
      updated = 1
      next
    }
    { print }
    END {
      if (updated == 0) {
        print key "=" value
      }
    }
  ' "$env_file" > "$write_tmp"
  mv "$write_tmp" "$env_file"
}

# 使用系统随机源生成十六进制随机值。
generate_random_hex_value() {
  local byte_count="$1"
  od -An -N "$byte_count" -tx1 /dev/urandom | tr -d " \n"
}

# 当默认值不安全时生成新的 Token Secret。
build_default_token_secret() {
  local current_value="$1"

  if [[ -z "$current_value" || "$current_value" == "$DEFAULT_TOKEN_SECRET" ]]; then
    generate_random_hex_value 32
    return
  fi

  printf "%s" "$current_value"
}

# 当默认管理员密码不安全时生成新的初始密码。
build_default_admin_password() {
  local current_value="$1"

  if [[ -z "$current_value" || "$current_value" == "$DEFAULT_ADMIN_PASSWORD" ]]; then
    generate_random_hex_value 8
    return
  fi

  printf "%s" "$current_value"
}

# 当默认数据库密码不安全时生成新的数据库密码。
build_default_database_password() {
  local current_value="$1"

  if [[ -z "$current_value" || "$current_value" == "$DEFAULT_DATABASE_PASSWORD" ]]; then
    generate_random_hex_value 8
    return
  fi

  printf "%s" "$current_value"
}

# 确认自定义容器名是否已经启动。
exit_when_configured_service_running() {
  local env_file="$1"
  local configured_name

  configured_name="$(read_env_config_value "$env_file" "UZON_MAIL_CONTAINER_NAME")"
  configured_name="${configured_name:-$DEFAULT_CONTAINER_NAME}"
  if [[ "$configured_name" != "$DEFAULT_CONTAINER_NAME" ]] && is_container_running_by_name "$configured_name"; then
    rm -f "$env_file"
    write_install_log "检测到 $configured_name 容器已启动，跳过安装。"
    exit 0
  fi
}

# 写入基础安全配置与访问地址配置。
configure_basic_env_values() {
  local env_file="$1"
  local base_url
  local host_port
  local token_secret
  local admin_password

  base_url="$(read_described_value_with_default "请输入后端访问地址 BaseUrl" "$(read_env_config_value "$env_file" "BaseUrl")" "配置用户访问后端服务的完整地址，并同步写入 Cors__0。")"
  host_port="$(read_described_value_with_default "请输入宿主机访问端口" "$(read_env_config_value "$env_file" "UZON_MAIL_HOST_PORT")" "配置宿主机暴露端口，需要与 BaseUrl 中端口保持一致。")"
  token_secret="$(read_described_value_with_default "请输入 Token Secret" "$(build_default_token_secret "$(read_env_config_value "$env_file" "TokenParams__Secret")")" "用于签名登录令牌，公网部署前必须修改，避免伪造登录状态。")"
  admin_password="$(read_described_value_with_default "请输入管理员初始密码" "$(build_default_admin_password "$(read_env_config_value "$env_file" "User__AdminUser__Password")")" "首次初始化管理员账号时使用，安装完成后会输出给你。")"

  update_env_config_value "$env_file" "BaseUrl" "$base_url"
  update_env_config_value "$env_file" "Cors__0" "$base_url"
  update_env_config_value "$env_file" "UZON_MAIL_HOST_PORT" "$host_port"
  update_env_config_value "$env_file" "Http__Port" "$host_port"
  update_env_config_value "$env_file" "Websocket__Port" "$host_port"
  update_env_config_value "$env_file" "TokenParams__Secret" "$token_secret"
  update_env_config_value "$env_file" "User__AdminUser__Password" "$admin_password"
}

# 写入内置 PostgreSQL 配置。
configure_builtin_postgresql_values() {
  local env_file="$1"
  local database_name
  local database_user
  local database_password

  database_name="$(read_described_value_with_default "请输入内置 PostgreSQL 数据库名" "$(read_env_config_value "$env_file" "Database__PostgreSql__Database")" "配置内置 PostgreSQL 初始化和后端连接使用的数据库名称。")"
  database_user="$(read_described_value_with_default "请输入内置 PostgreSQL 用户名" "$(read_env_config_value "$env_file" "Database__PostgreSql__User")" "配置内置 PostgreSQL 初始化和后端连接使用的用户名。")"
  database_password="$(read_described_value_with_default "请输入内置 PostgreSQL 密码" "$(build_default_database_password "$(read_env_config_value "$env_file" "Database__PostgreSql__Password")")" "配置内置 PostgreSQL 初始化和后端连接使用的密码。")"

  update_env_config_value "$env_file" "COMPOSE_PROFILES" "postgresql"
  update_env_config_value "$env_file" "Database__SqLite__Enable" "false"
  update_env_config_value "$env_file" "Database__PostgreSql__Enable" "true"
  update_env_config_value "$env_file" "Database__PostgreSql__Host" "uzon-postgres"
  update_env_config_value "$env_file" "Database__PostgreSql__Port" "5432"
  update_env_config_value "$env_file" "Database__PostgreSql__Database" "$database_name"
  update_env_config_value "$env_file" "Database__PostgreSql__User" "$database_user"
  update_env_config_value "$env_file" "Database__PostgreSql__Password" "$database_password"
}

# 写入 SQLite 配置。
configure_sqlite_database_values() {
  local env_file="$1"

  update_env_config_value "$env_file" "COMPOSE_PROFILES" ""
  update_env_config_value "$env_file" "Database__SqLite__Enable" "true"
  update_env_config_value "$env_file" "Database__PostgreSql__Enable" "false"
}

# 写入外部 PostgreSQL 配置。
configure_external_postgresql_values() {
  local env_file="$1"
  local external_host
  local external_port
  local external_database
  local external_user
  local external_password
  local external_host_default

  external_host_default="$(read_env_config_value "$env_file" "Database__PostgreSql__Host")"
  if [[ "$external_host_default" == "uzon-postgres" ]]; then
    external_host_default=""
  fi
  external_host="$(read_required_described_value "请输入外部 PostgreSQL Host" "$external_host_default" "配置后端连接外部 PostgreSQL 的主机名或 IP。")"
  update_env_config_value "$env_file" "COMPOSE_PROFILES" ""

  external_port="$(read_described_value_with_default "请输入外部 PostgreSQL 端口" "$(read_env_config_value "$env_file" "Database__PostgreSql__Port")" "配置后端连接外部 PostgreSQL 的端口。")"
  external_database="$(read_described_value_with_default "请输入外部 PostgreSQL 数据库名" "$(read_env_config_value "$env_file" "Database__PostgreSql__Database")" "配置后端连接外部 PostgreSQL 使用的数据库名称。")"
  external_user="$(read_described_value_with_default "请输入外部 PostgreSQL 用户名" "$(read_env_config_value "$env_file" "Database__PostgreSql__User")" "配置后端连接外部 PostgreSQL 使用的用户名。")"
  external_password="$(read_described_value_with_default "请输入外部 PostgreSQL 密码" "$(read_env_config_value "$env_file" "Database__PostgreSql__Password")" "配置后端连接外部 PostgreSQL 使用的密码。")"

  update_env_config_value "$env_file" "Database__SqLite__Enable" "false"
  update_env_config_value "$env_file" "Database__PostgreSql__Enable" "true"
  update_env_config_value "$env_file" "Database__PostgreSql__Host" "$external_host"
  update_env_config_value "$env_file" "Database__PostgreSql__Port" "$external_port"
  update_env_config_value "$env_file" "Database__PostgreSql__Database" "$external_database"
  update_env_config_value "$env_file" "Database__PostgreSql__User" "$external_user"
  update_env_config_value "$env_file" "Database__PostgreSql__Password" "$external_password"
}

# 判断逗号分隔配置中是否包含指定 profile。
contains_compose_profile() {
  local profiles_value="$1"
  local target_profile="$2"
  local profile_item

  IFS=',' read -ra profile_items <<< "$profiles_value"
  for profile_item in "${profile_items[@]}"; do
    if [[ "$profile_item" == "$target_profile" ]]; then
      return 0
    fi
  done

  return 1
}

# 向 COMPOSE_PROFILES 中追加指定 profile。
add_compose_profile() {
  local env_file="$1"
  local target_profile="$2"
  local profiles_value

  profiles_value="$(read_env_config_value "$env_file" "COMPOSE_PROFILES")"
  if contains_compose_profile "$profiles_value" "$target_profile"; then
    return
  fi

  if [[ -z "$profiles_value" ]]; then
    update_env_config_value "$env_file" "COMPOSE_PROFILES" "$target_profile"
    return
  fi

  update_env_config_value "$env_file" "COMPOSE_PROFILES" "$profiles_value,$target_profile"
}

# 从 COMPOSE_PROFILES 中移除指定 profile。
remove_compose_profile() {
  local env_file="$1"
  local target_profile="$2"
  local profiles_value
  local profile_item
  local next_profiles=""

  profiles_value="$(read_env_config_value "$env_file" "COMPOSE_PROFILES")"
  IFS=',' read -ra profile_items <<< "$profiles_value"
  for profile_item in "${profile_items[@]}"; do
    if [[ -z "$profile_item" || "$profile_item" == "$target_profile" ]]; then
      continue
    fi

    if [[ -z "$next_profiles" ]]; then
      next_profiles="$profile_item"
    else
      next_profiles="$next_profiles,$profile_item"
    fi
  done

  update_env_config_value "$env_file" "COMPOSE_PROFILES" "$next_profiles"
}

# 根据用户确认写入 Redis 配置。
configure_redis_values() {
  local env_file="$1"

  if read_described_yes_no_with_default "是否启用 Redis" "n" "启用后启动内置 Redis 并让后端使用 Redis 缓存；选择否时后端使用内存缓存。"; then
    add_compose_profile "$env_file" "redis"
    update_env_config_value "$env_file" "Database__Redis__Enable" "true"
    return
  fi

  remove_compose_profile "$env_file" "redis"
  update_env_config_value "$env_file" "Database__Redis__Enable" "false"
}

# 根据用户选择写入数据库配置。
configure_database_env_values() {
  local env_file="$1"

  if ! read_described_yes_no_with_default "是否启用 PostgreSQL" "y" "控制是否使用 PostgreSQL 数据库；选择否时使用 SQLite。"; then
    configure_sqlite_database_values "$env_file"
    configure_redis_values "$env_file"
    return
  fi

  if read_described_yes_no_with_default "是否使用内置 PostgreSQL" "y" "使用 docker-compose 中的 uzon-postgres 容器，适合新安装。"; then
    configure_builtin_postgresql_values "$env_file"
    configure_redis_values "$env_file"
    return
  fi

  configure_external_postgresql_values "$env_file"
  configure_redis_values "$env_file"
}

# 输出最终配置摘要。
show_config_summary() {
  local env_file="$1"

  write_install_log "请确认以下配置："
  echo "  BaseUrl: $(read_env_config_value "$env_file" "BaseUrl")"
  echo "  Cors__0: $(read_env_config_value "$env_file" "Cors__0")"
  echo "  UZON_MAIL_HOST_PORT: $(read_env_config_value "$env_file" "UZON_MAIL_HOST_PORT")"
  echo "  COMPOSE_PROFILES: $(read_env_config_value "$env_file" "COMPOSE_PROFILES")"
  echo "  Database__SqLite__Enable: $(read_env_config_value "$env_file" "Database__SqLite__Enable")"
  echo "  Database__PostgreSql__Enable: $(read_env_config_value "$env_file" "Database__PostgreSql__Enable")"
  echo "  Database__Redis__Enable: $(read_env_config_value "$env_file" "Database__Redis__Enable")"
  echo "  User__AdminUser__Password: $(read_env_config_value "$env_file" "User__AdminUser__Password")"
}

# 输出安装完成后的访问与登录信息。
show_install_completion_info() {
  local env_file="$1"
  local access_url
  local admin_user
  local admin_password

  access_url="$(read_env_config_value "$env_file" "BaseUrl")"
  admin_user="$(read_env_config_value "$env_file" "User__AdminUser__UserId")"
  admin_password="$(read_env_config_value "$env_file" "User__AdminUser__Password")"
  admin_user="${admin_user:-admin}"

  write_install_log "安装完成，请使用以下信息访问："
  echo "  访问地址: $access_url"
  echo "  用户名: $admin_user"
  echo "  默认密码: $admin_password"
}

# 确认后应用 .env.tmp 并启动 Docker Compose。
confirm_and_start_service() {
  local env_file="$1"

  show_config_summary "$env_file"
  if ! read_yes_no_with_default "确认写入 .env 并启动服务吗" "y"; then
    write_install_log "已保留 $env_file，未更新 .env，未启动服务。"
    exit 0
  fi

  mv "$env_file" ".env"
  write_install_log "开始启动服务。"
  docker compose up -d
  show_install_completion_info ".env"
}

# 安装脚本入口。
run_installation_flow() {
  local install_directory
  local env_tmp_file=".env.tmp"

  write_install_log "开始 Docker 初始化安装"
  ensure_docker_runtime_ready
  exit_when_default_service_running
  install_directory="$(confirm_install_directory)"
  configure_download_proxy
  prepare_installation_files "$install_directory"
  exit_when_configured_service_running "$env_tmp_file"
  configure_basic_env_values "$env_tmp_file"
  configure_database_env_values "$env_tmp_file"
  confirm_and_start_service "$env_tmp_file"
}

run_installation_flow "$@"
