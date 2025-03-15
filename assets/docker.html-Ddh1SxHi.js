import{_ as l}from"./plugin-vue_export-helper-DlAUqK2U.js";import{c as e,a,b as t,d as i,e as h,g as k,r as p,o as d}from"./app-CJsgKil_.js";const r={};function c(o,s){const n=p("RouteLink");return d(),e("div",null,[s[3]||(s[3]=a(`<p>本文将介绍如何使用 Docker Compose 在 Ubuntu 中进行安装，若是其它宿主机或者其它安装方式，可以将本文发给 DeepSeek 让其帮你转换。</p><p>请放心，Docker Compose 不是什么新技术，没有难度，它只是一些安装配置的集合，一言概之就是，它能简化你部署流程，跟着以下教程，无脑配置即可。</p><h2 id="连接到-ubuntu" tabindex="-1"><a class="header-anchor" href="#连接到-ubuntu"><span>连接到 Ubuntu</span></a></h2><p>使用 ssh 连接到 Ubuntu 上，在 windows 中可以直接打开 PowerShell 使用。</p><p>命令如下：</p><div class="language-powershell line-numbers-mode" data-highlighter="shiki" data-ext="powershell" data-title="powershell" style="--shiki-light:#383A42;--shiki-dark:#abb2bf;--shiki-light-bg:#FAFAFA;--shiki-dark-bg:#282c34;"><pre class="shiki shiki-themes one-light one-dark-pro vp-code"><code><span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 执行后根据提示输入密码</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">ssh username</span><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">@ip</span></span></code></pre><div class="line-numbers" aria-hidden="true" style="counter-reset:line-number 0;"><div class="line-number"></div><div class="line-number"></div></div></div><h2 id="docker-compose" tabindex="-1"><a class="header-anchor" href="#docker-compose"><span>docker-compose</span></a></h2><p><code>docker-compose.yml</code> 文件如下，下面配置时会用到，此时无须阅读，可直接跳转到下一节：</p><div class="language-yaml line-numbers-mode" data-highlighter="shiki" data-ext="yaml" data-title="yaml" style="--shiki-light:#383A42;--shiki-dark:#abb2bf;--shiki-light-bg:#FAFAFA;--shiki-dark-bg:#282c34;"><pre class="shiki shiki-themes one-light one-dark-pro vp-code"><code><span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># </span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 说明</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 该文件是 uzon-mail 的 docker-compose 配置文件，使用时，在当前目录执行 docker-compose up -d 命令即可启动程序</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">#</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">services</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">  # mysql 服务</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">  uzon-mysql</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    container_name</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-mysql</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    image</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">mysql:8.4.0</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 对外暴露端口，方便外部管理</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 本地端口:容器端口</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 若本机 3306 已使用，可更换成其它端口，例如 23306:3306</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    ports</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">3306:3306</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    environment</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">      MYSQL_ROOT_PASSWORD</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">mysqlRoot3306</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # root 账号的密码</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">      MYSQL_DATABASE</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-mail</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 数据库名</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">      MYSQL_USER</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-mail</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 数据库用户名</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">      MYSQL_PASSWORD</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-mail</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 数据库密码</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    volumes</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">./data/mysql/data:/var/lib/mysql</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 数据库数据挂载，防止容器重构后数据丢失</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    restart</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">always</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    command</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">mysqld --character-set-server=utf8mb4 --collation-server=utf8mb4_unicode_ci</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 连接到 uzonmail 主程序网络</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    networks</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzonmail_network</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">  # redis 缓存, 若要启用 redis 服务，请取消下面的注释</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">  uzon-redis</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    container_name</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-redis</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    image</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">redis:latest</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 对外暴露端口，方便外部管理</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 本地端口:容器端口</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">    # 若本机 6379 已使用，可更换成其它端口，例如 26379:3306</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    ports</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">6379:6379</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    volumes</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">./data/redis/data:/data</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 数据库数据挂载，防止容器重构后数据丢失</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    restart</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">always</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    networks</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzonmail_network</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;">  # 程序主体</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">  uzon-mail</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    container_name</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-mail</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    image</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">gmxgalens/uzon-mail:latest</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    ports</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">22345:22345</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    volumes</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">./data/appsettings.Production.json:/app/appsettings.Production.json</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 生产环境配置</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">./data/data:/app/data</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 数据存储</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">./data/app.config.json:/app/wwwroot/app.config.json</span><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"> # 前端配置</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    networks</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzonmail_network</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    command</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">: [ </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;dotnet&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">, </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">&quot;UZonMailService.dll&quot;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;"> ]</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">    depends_on</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-mysql</span></span>
<span class="line"><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">      - </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">uzon-redis</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">networks</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span>
<span class="line"><span style="--shiki-light:#E45649;--shiki-dark:#E06C75;">  uzonmail_network</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;">:</span></span></code></pre><div class="line-numbers" aria-hidden="true" style="counter-reset:line-number 0;"><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div></div></div><h2 id="安装准备" tabindex="-1"><a class="header-anchor" href="#安装准备"><span>安装准备</span></a></h2><p>在启动 docker 前，需要提前进行如下工作：</p><ol><li>创建 docker compose 根目录</li><li>生成默认配置文件，根据实际情况进行修改</li></ol><p>按如下命令进行操作：</p><div class="language-bash line-numbers-mode" data-highlighter="shiki" data-ext="bash" data-title="bash" style="--shiki-light:#383A42;--shiki-dark:#abb2bf;--shiki-light-bg:#FAFAFA;--shiki-dark-bg:#282c34;"><pre class="shiki shiki-themes one-light one-dark-pro vp-code"><code><span class="line"><span style="--shiki-light:#0184BC;--shiki-dark:#56B6C2;">cd</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> ~</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 创建数据目录</span></span>
<span class="line"><span style="--shiki-light:#4078F2;--shiki-dark:#61AFEF;">mkdir</span><span style="--shiki-light:#986801;--shiki-dark:#D19A66;"> -p</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> apps/uzon-mail/data</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 进入数据目录</span></span>
<span class="line"><span style="--shiki-light:#0184BC;--shiki-dark:#56B6C2;">cd</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> apps/uzon-mail</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 生成前端配置文件, baseUrl 见配置章节</span></span>
<span class="line"><span style="--shiki-light:#0184BC;--shiki-dark:#56B6C2;">echo</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> &#39;{</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">  &quot;baseUrl&quot;: &quot;http:/localhost:22345&quot;,</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">  &quot;api&quot;: &quot;/api/v1&quot;,</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">  &quot;signalRHub&quot;: &quot;/hubs/uzonMailHub&quot;,</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">  &quot;logger&quot;: {</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">    &quot;level&quot;: &quot;info&quot;</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">  }</span></span>
<span class="line"><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">}&#39;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;"> &gt; </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">data/app.config.json</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 生成后端配置文件</span></span>
<span class="line"><span style="--shiki-light:#0184BC;--shiki-dark:#56B6C2;">echo</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> &#39;{}&#39;</span><span style="--shiki-light:#383A42;--shiki-dark:#ABB2BF;"> &gt; </span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;">data/appsettings.Production.json</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 该文件中有一些初始化配置项，建议跳转到 [后端配置] 章节阅读，添加必要项配置，然后继续</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 当然也可以继续配置，后续再修改</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 从 gitee 下载 docker-compose.yml 到当前目录</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 若无法下载，请手动创建文件 ~/apps/uzon-mail/docker-compose.yml，然后将上述 docker-compose 内容复制进去</span></span>
<span class="line"><span style="--shiki-light:#4078F2;--shiki-dark:#61AFEF;">wget</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> https://gitee.com/uzonmail/UZonMail/raw/master/scripts/docker-compose.yml</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 拉取 docker 镜像</span></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 此步骤可省略，直接调用下一个命令</span></span>
<span class="line"><span style="--shiki-light:#4078F2;--shiki-dark:#61AFEF;">docker</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> compose</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> pull</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#A0A1A7;--shiki-light-font-style:italic;--shiki-dark:#7F848E;--shiki-dark-font-style:italic;"># 创建容器并启动</span></span>
<span class="line"><span style="--shiki-light:#4078F2;--shiki-dark:#61AFEF;">docker</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> compose</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> up</span><span style="--shiki-light:#986801;--shiki-dark:#D19A66;"> -d</span></span></code></pre><div class="line-numbers" aria-hidden="true" style="counter-reset:line-number 0;"><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div><div class="line-number"></div></div></div><h2 id="防火墙放行" tabindex="-1"><a class="header-anchor" href="#防火墙放行"><span>防火墙放行</span></a></h2><p>放行端口</p><div class="language-bash line-numbers-mode" data-highlighter="shiki" data-ext="bash" data-title="bash" style="--shiki-light:#383A42;--shiki-dark:#abb2bf;--shiki-light-bg:#FAFAFA;--shiki-dark-bg:#282c34;"><pre class="shiki shiki-themes one-light one-dark-pro vp-code"><code><span class="line"><span style="--shiki-light:#4078F2;--shiki-dark:#61AFEF;">sudo</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> ufw</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> allow</span><span style="--shiki-light:#50A14F;--shiki-dark:#98C379;"> 22345/tcp</span></span></code></pre><div class="line-numbers" aria-hidden="true" style="counter-reset:line-number 0;"><div class="line-number"></div></div></div><h2 id="修改配置" tabindex="-1"><a class="header-anchor" href="#修改配置"><span>修改配置</span></a></h2>`,18)),t("p",null,[s[1]||(s[1]=i("到这一步，Docker 服务就创建完成了，由于是服务器部署，您可能会将其代理到公网，因此还有一些必要的配置进行修改，请继续阅读 ")),h(n,{to:"/guide/setup/"},{default:k(()=>s[0]||(s[0]=[i("后端配置")])),_:1}),s[2]||(s[2]=i(" 章节。"))]),s[4]||(s[4]=a('<div class="hint-container warning"><p class="hint-container-title">注意</p><p>若代理到公网，请务必修改默认配置，否则服务将会不安全！</p></div><h2 id="软件更新" tabindex="-1"><a class="header-anchor" href="#软件更新"><span>软件更新</span></a></h2><p>当需要更新容器时，只需要进入到 ~/apps/uzon-mail(docker-compose.yml 所在目录)，然后执行命令 <code>docker compose up -d</code> 即可</p><h2 id="网址访问" tabindex="-1"><a class="header-anchor" href="#网址访问"><span>网址访问</span></a></h2><p>访问 <code>http://your-docker-host-ip:22345</code> 登陆使用。</p>',5))])}const A=l(r,[["render",c],["__file","docker.html.vue"]]),u=JSON.parse('{"path":"/guide/installation/docker.html","title":"Docker","lang":"zh-CN","frontmatter":{"title":"Docker","icon":"fab fa-docker","order":5,"description":"本文将介绍如何使用 Docker Compose 在 Ubuntu 中进行安装，若是其它宿主机或者其它安装方式，可以将本文发给 DeepSeek 让其帮你转换。 请放心，Docker Compose 不是什么新技术，没有难度，它只是一些安装配置的集合，一言概之就是，它能简化你部署流程，跟着以下教程，无脑配置即可。 连接到 Ubuntu 使用 ssh 连...","head":[["meta",{"property":"og:url","content":"https://uzonmail.pages.dev/guide/installation/docker.html"}],["meta",{"property":"og:site_name","content":"宇正群邮"}],["meta",{"property":"og:title","content":"Docker"}],["meta",{"property":"og:description","content":"本文将介绍如何使用 Docker Compose 在 Ubuntu 中进行安装，若是其它宿主机或者其它安装方式，可以将本文发给 DeepSeek 让其帮你转换。 请放心，Docker Compose 不是什么新技术，没有难度，它只是一些安装配置的集合，一言概之就是，它能简化你部署流程，跟着以下教程，无脑配置即可。 连接到 Ubuntu 使用 ssh 连..."}],["meta",{"property":"og:type","content":"article"}],["meta",{"property":"og:locale","content":"zh-CN"}],["meta",{"property":"og:updated_time","content":"2025-03-15T14:43:09.000Z"}],["meta",{"property":"article:modified_time","content":"2025-03-15T14:43:09.000Z"}],["script",{"type":"application/ld+json"},"{\\"@context\\":\\"https://schema.org\\",\\"@type\\":\\"Article\\",\\"headline\\":\\"Docker\\",\\"image\\":[\\"\\"],\\"dateModified\\":\\"2025-03-15T14:43:09.000Z\\",\\"author\\":[{\\"@type\\":\\"Person\\",\\"name\\":\\"galens\\",\\"url\\":\\"galens.uamazing.cn\\"}]}"]]},"headers":[{"level":2,"title":"连接到 Ubuntu","slug":"连接到-ubuntu","link":"#连接到-ubuntu","children":[]},{"level":2,"title":"docker-compose","slug":"docker-compose","link":"#docker-compose","children":[]},{"level":2,"title":"安装准备","slug":"安装准备","link":"#安装准备","children":[]},{"level":2,"title":"防火墙放行","slug":"防火墙放行","link":"#防火墙放行","children":[]},{"level":2,"title":"修改配置","slug":"修改配置","link":"#修改配置","children":[]},{"level":2,"title":"软件更新","slug":"软件更新","link":"#软件更新","children":[]},{"level":2,"title":"网址访问","slug":"网址访问","link":"#网址访问","children":[]}],"git":{"createdTime":1742049789000,"updatedTime":1742049789000,"contributors":[{"name":"galens","username":"galens","email":"gmx_galens@163.com","commits":1,"url":"https://github.com/galens"}]},"readingTime":{"minutes":2.97,"words":890},"filePathRelative":"guide/installation/docker.md","localizedDate":"2025年3月15日","autoDesc":true}');export{A as comp,u as data};
