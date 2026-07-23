#! /bin/bash

echo "[UzonMail] 开始安装 uzon-mail 服务"

echo "[UzonMail] 检测是否存在 dotnet 环境"
# 检测是否有 dotnet 环境
if ! dotnet --version &> /dev/null; then
    echo "[UzonMail] 请先安装 dotnet 环境"
    exit 1
fi
echo "[UzonMail] dotnet 环境已安装"

echo "[UzonMail] 检测是否存在 uzon-mail 服务"
# 若不存在 www, 则创建 www 用户
if ! id www &> /dev/null; then
    sudo useradd -s /sbin/nologin -M www
fi
echo "[UzonMail] www 用户已创建"

echo "[UzonMail] 检测是否存在 /usr/local/uzon-mail 目录"
# 将当前目录下的所有文件复制到 /usr/local/uzon-mail 目录下
# 创建 /usr/local/uzon-mail 目录
if [ ! -d /usr/local/uzon-mail ]; then
  sudo mkdir /usr/local/uzon-mail
fi
echo "[UzonMail] /usr/local/uzon-mail 目录已创建"

echo "[UzonMail] 复制程序文件到 /usr/local/uzon-mail 目录下"
# 复制程序文件到 /usr/local/uzon-mail 目录下
sudo cp -raf ./service-linux-x64/. /usr/local/uzon-mail

# 设置 www 用户对 /usr/local/uzon-mail 目录的权限
sudo chown -R www:www /usr/local/uzon-mail

echo "[UzonMail] 注册 uzon-mail 服务并设置开机启动"
# 复制当前目录中的 uzonmail.service 文件到 /etc/systemd/system 目录下
sudo cp ./uzon-mail.service /etc/systemd/system

# 重载 systemd 管理的服务
sudo systemctl daemon-reload

# 设置 uzon-mail 服务开机自启动
sudo systemctl enable uzon-mail

echo -e "[UzonMail] ${GREEN}安装 uzon-mail 服务成功!${NC}"