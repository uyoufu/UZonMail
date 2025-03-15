import{_ as n}from"./plugin-vue_export-helper-DlAUqK2U.js";import{c as l,a,b as e,t,d as g,o as s}from"./app-CJsgKil_.js";const m={};function c(p,i){return s(),l("div",null,[i[2]||(i[2]=a('<h2 id="登陆界面" tabindex="-1"><a class="header-anchor" href="#登陆界面"><span>登陆界面</span></a></h2><figure><img src="https://obs.uamazing.cn:52443/public/files/images/uzon-mail-login-2.png" alt="uzon-mail-login-2" tabindex="0" loading="lazy"><figcaption>uzon-mail-login-2</figcaption></figure><p>默认用户名和密码为：admin/admin1234</p><h2 id="首页" tabindex="-1"><a class="header-anchor" href="#首页"><span>首页</span></a></h2><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614231957130.png" alt="image-20240614231957130" tabindex="0" loading="lazy"><figcaption>image-20240614231957130</figcaption></figure><p>首页主要展示的内容有：</p><ul><li>发件箱数量直方图</li><li>收件箱数量直方图</li><li>每月发件量折线图</li></ul><h2 id="系统设置" tabindex="-1"><a class="header-anchor" href="#系统设置"><span>系统设置</span></a></h2><h3 id="用户管理" tabindex="-1"><a class="header-anchor" href="#用户管理"><span>用户管理</span></a></h3><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612122713293.png" alt="image-20240612122713293" tabindex="0" loading="lazy"><figcaption>image-20240612122713293</figcaption></figure><p>系统默认的用户名为 admin，默认密码为 admin1234，这是一个管理员账号，该账号具有【管理用户】的权限。</p><p>【用户管理】模块用于增加不同的用户。桌面版本的多用户功能仅限本机使用，若要多人同时使用，则需要使用服务器版本。</p><p>服务器版本可联系作者获取。</p><p><strong>新增用户：</strong></p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612123329057.png" alt="image-20240612123329057" tabindex="0" loading="lazy"><figcaption>image-20240612123329057</figcaption></figure><p>单击左上角的新增，即可新增用户。</p><p>用户新增完成后，可对用户进行操作，比如重置密码，删除等</p><p>重置后的密码为：<code>uzonmail123</code>，在重置时会有提示。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612123429178.png" alt="image-20240612123429178" tabindex="0" loading="lazy"><figcaption>image-20240612123429178</figcaption></figure><p><strong>修改密码和头像：</strong></p><p>可以通过右上角的个人信息界面对头像和密码进行修改。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612125131168.png" alt="image-20240612125131168" tabindex="0" loading="lazy"><figcaption>image-20240612125131168</figcaption></figure><h3 id="基础设置" tabindex="-1"><a class="header-anchor" href="#基础设置"><span>基础设置</span></a></h3><p>基础设置中，主要设置全局的发件间隔，最大发件量等。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612125859579.png" alt="image-20240612125859579" tabindex="0" loading="lazy"><figcaption>image-20240612125859579</figcaption></figure><ul><li><p>单个邮箱每日最大发件量</p><p>控制单个邮箱每日发件的总数，避免因发件数量超过每个邮件服务提供商规定的每日发件量，从而导致发件失败。</p><p>为 0 时表示不限制</p></li><li><p>单个发件箱最小(最大)发件间隔</p><p>单位：秒</p><p>为了避免因频繁发送邮件而导致被服务器认为是垃圾邮箱，所以，发送两封邮件之间需要有一定的时间间隔，为了使得发送时间间隔具有不规律性，用间隔范围来进行控制：</p><p>实际发件间隔值 = 最小值 + （0，1）之间的随机数*（最大值-最小值）</p><p>最大值小于等于最小值时，表示不限制</p></li><li><p>合并发件最大数量</p><p>当同时向多个收件箱发送相同内容时，可以将收件箱合并成一封邮件发送，这个参数即控制合并的最大数量。</p><p>每个邮件服务商允许的最大数量不一样，最大数包含抄送和密送的数量</p><p>为 0 时，表示不合并</p></li></ul><h3 id="代理管理" tabindex="-1"><a class="header-anchor" href="#代理管理"><span>代理管理</span></a></h3><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612131312091.png" alt="image-20240612131312091" tabindex="0" loading="lazy"><figcaption>image-20240612131312091</figcaption></figure><p>代理管理模块主要针对使用国外邮箱的情况，允许针对某一类或者某个发件箱指定代理。</p><p>该功能一般用于服务器部署的情况，本机使用时，可以打开全局代理。</p><p><strong>新增代理</strong>：</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612130111084.png" alt="image-20240612130111084" tabindex="0" loading="lazy"><figcaption>image-20240612130111084</figcaption></figure><p>新增代理参数说明：</p><ul><li><p>名称</p><p>必填项。在发件箱界面，可通过名称选择指定代理</p></li><li><p>代理地址</p><p>代理的地址、用户名和密码。格式为：<code>协议:\\\\username:password@host</code>，示例：</p><ol><li>完整格式：<code>http:\\\\admin:admin1234@127.0.0.1:7890</code></li><li>无密码格式: <code>http:\\\\127.0.0.1:7890</code></li><li>其它协议：<code>socket5:\\\\127.0.0.1:7890</code></li></ol><p>目前支持的协议有：<code>http、https、socks4、socks4a、socket5</code></p></li><li><p>匹配规则</p><p>若发件箱没有指定代理，则会从代理管理的列表中根据规则自动匹配，若匹配到，则使用。</p><p>该规则的语法为正则表达式</p><p><code>.*</code> 表示所有的邮件都匹配</p></li><li><p>优先级</p><p>规则匹配的优先级</p></li><li><p>是否共享</p><p>共享后，系统内所有人都可使用这个代理</p></li></ul><blockquote><p>代理安全提示</p><p>代理是明文存储在服务器中的，因此管理员可以查看代理的信息，可能会造成代理泄露风险，请谨慎添加个人代理</p></blockquote><h2 id="邮箱管理" tabindex="-1"><a class="header-anchor" href="#邮箱管理"><span>邮箱管理</span></a></h2><h3 id="发件箱" tabindex="-1"><a class="header-anchor" href="#发件箱"><span>发件箱</span></a></h3><p>【发件箱】模块用于管理发件人信息。下面列出在使用中需要注意的功能进行说明：</p><p><strong>组管理：</strong></p><p>增加发件箱时，必须先建立发件组。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612131552355.png" alt="image-20240612131552355" tabindex="0" loading="lazy"><figcaption>image-20240612131552355</figcaption></figure><p>在 &quot;发件箱&quot; 上右键，可新增发件组。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612131711338.png" alt="image-20240612131711338" tabindex="0" loading="lazy"><figcaption>image-20240612131711338</figcaption></figure><p>新增时，&quot;序号&quot; 表示发件组的排序号，仅用于排序。</p><p>当新建组完成后，可以在组名上右键，对组进行管理</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612131916319.png" alt="image-20240612131916319" tabindex="0" loading="lazy"><figcaption>image-20240612131916319</figcaption></figure><p><strong>新增发件箱</strong>：</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240612132056895.png" alt="image-20240612132056895" tabindex="0" loading="lazy"><figcaption>image-20240612132056895</figcaption></figure><p>本软件采用的是 SMTP 协议发件，因此发件时，需要将自己的邮箱开通 SMTP 服务，可自行查阅资料。以下对一些重要参数说明：</p><ul><li><p>发件人名称</p><p>若有，当发给对方后，不显示邮箱，而是显示名称</p></li><li><p>smtp 密码</p><p>发件采用的 SMTP 服务器，所以，它的密码并不是邮箱的密码，而是登陆邮箱后，自己申请的 SMTP 密码。</p><p>比如，163邮箱 SMTP 密码获取方式如下：<a href="https://www.yisu.com/zixun/97973.html" target="_blank" rel="noopener noreferrer">https://www.yisu.com/zixun/97973.html</a></p></li><li><p>smtp 地址</p><p>smtp 的地址，每种类型的地址不一样，可百度查找</p></li><li><p>smtp 端口</p><p>该端口与是否【启用 SSL】有关，默认为 25，若启动 SSL，一般为 465，需要自动查找确认。</p></li><li><p>启用 SSL</p><p>是在发件时，采用 SSL 加密，打开这个可以提升发件的安全性。</p></li><li><p>代理</p><p>若有需要，可以为其指定代理，可用代理候选项在【代理管理】中定义。</p></li></ul><blockquote><p>密码安全提示</p><p>服务器没有直接存储 smtp 密码，而是存储了通过密钥加密后的密文，密钥由前端生成，当有需要时，由前端传递给后端解密。</p><p>因此，即使数据库泄露了，也不会造成 smtp 的密码被盗的问题</p></blockquote><p><strong>从EXCEL导入：</strong></p><p>通过【导入】功能可批量从 Excel 中导入发件箱。</p><p>可以通过【模板】按钮下载导入模板。</p><p>在使用批量添加发件人功能时，Excel 表中第一行为表头，必须包含 <code>smtp邮箱</code>、<code>smtp密码</code>、<code>smtp地址</code>、<code>smtp端口</code> 列。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614123302248.png" alt="image-20240614123302248" tabindex="0" loading="lazy"><figcaption>image-20240614123302248</figcaption></figure><h3 id="收件箱" tabindex="-1"><a class="header-anchor" href="#收件箱"><span>收件箱</span></a></h3><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614124814856.png" alt="image-20240614124814856" tabindex="0" loading="lazy"><figcaption>image-20240614124814856</figcaption></figure><p>该模块主要用于对收件箱的分组和管理，使用方式、注意要点与发件人一致。</p><p>收件箱只需要姓名和邮箱即可，姓名是可选的。</p><h2 id="模板管理" tabindex="-1"><a class="header-anchor" href="#模板管理"><span>模板管理</span></a></h2><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614125056768.png" alt="image-20240614125056768" tabindex="0" loading="lazy"><figcaption>image-20240614125056768</figcaption></figure><p>在【正文模板】用于管理用户下的所有模板，它是 html 格式。</p><h3 id="新增模板" tabindex="-1"><a class="header-anchor" href="#新增模板"><span>新增模板</span></a></h3><p>使用两种方式进行添加：</p><ul><li><p>导入 HTML</p><p>先在外面用 html 定义好模板，然后通过上述中的【导入模板】功能将定义的模板导入到系统。对于自定义的 html 模板，要求其中的 css 必须为行内 css。可以通过 <a href="http://automattic.github.io/juice/" target="_blank" rel="noopener noreferrer">http://automattic.github.io/juice/</a> 自动将 html 文件中 css 转换成行内的 css。</p></li><li><p>直接编辑</p><p>通过单击【新增】按钮新增模板。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614125417120.png" alt="image-20240614125417120" tabindex="0" loading="lazy"><figcaption>image-20240614125417120</figcaption></figure></li></ul><h3 id="模板编辑" tabindex="-1"><a class="header-anchor" href="#模板编辑"><span>模板编辑</span></a></h3><p>通过单击模板名称或者在模板上右键，然后选择【编译】跳转到模板修改界面。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614125609679.png" alt="image-20240614125609679" tabindex="0" loading="lazy"><figcaption>image-20240614125609679</figcaption></figure><h3 id="模板变量" tabindex="-1"><a class="header-anchor" href="#模板变量"><span>模板变量</span></a></h3><p>在模板的编写过程中，可以使用双花括号（<code>{{变量名}}</code>）来标记为变量，在发件的过程中，程序会在数据中查找该变量，如果查找到，就会使用实际的数据将变量替换掉。</p>',71)),e("p",null,"变量定义的格式是为："+t(p.变量名)+"。",1),i[3]||(i[3]=a('<blockquote><p>在发件中，模板也可以因发件人而异，需要在数据中增加 templateId 列来覆盖通用的模板。具体参考发件篇。</p></blockquote><h2 id="发件管理" tabindex="-1"><a class="header-anchor" href="#发件管理"><span>发件管理</span></a></h2><h3 id="新建发件" tabindex="-1"><a class="header-anchor" href="#新建发件"><span>新建发件</span></a></h3><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614125845479.png" alt="image-20240614125845479" tabindex="0" loading="lazy"><figcaption>image-20240614125845479</figcaption></figure><p>新建发件用于添加发件任务，通过不同的参数组合，它可以实现以下功能：</p><ol><li><p>一对一发件</p><p>一个发件箱，一个收件箱</p></li><li><p>一对多发件</p><p>一个发件箱，多个收件箱</p></li><li><p>多对多发件</p><p>多个发件箱，多个收件箱</p></li><li><p>主题变化、正文变化发件</p><p>同时支持主题和正文根据收件人不同而变化</p></li></ol><h4 id="主题" tabindex="-1"><a class="header-anchor" href="#主题"><span>主题</span></a></h4><p>发件的主题是必须的，主有两个作用：一是为邮件的主题，二是同一次发件将会归到一个发件历史记录中，该主题为历史记录组的名称。</p><p>多个主题使用英文分号（<code>;</code>）或者换行进行分隔。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614130827129.png" alt="image-20240614130827129" tabindex="0" loading="lazy"><figcaption>image-20240614130827129</figcaption></figure><p>若有多个主题，系统在发件时，会随机使用一个主题（若在数据中指定了主题，则会固定使用数据的主题）。</p>',11)),e("p",null,[g("主题也支持变量声明，比如："+t(p.日期)+"-工资明细，",1),i[0]||(i[0]=e("code",null,"日期",-1)),i[1]||(i[1]=g(" 即为定义的变量，在发送邮件时，它将被替换成 Excel 表中的实际数据。"))]),i[4]||(i[4]=a('<h4 id="模板" tabindex="-1"><a class="header-anchor" href="#模板"><span>模板</span></a></h4><p>模板相当于是正文的一个草稿，它可以让你快速发送正文，而不需要每次都在正文处输入。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614131034812.png" alt="image-20240614131034812" tabindex="0" loading="lazy"><figcaption>image-20240614131034812</figcaption></figure><p>可以选择多个模板。若有多个模板时，系统将随机选择一个模板来发件（若在数据中指定了模板，则会固定使用数据中的模板）</p><h4 id="正文" tabindex="-1"><a class="header-anchor" href="#正文"><span>正文</span></a></h4><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614131326894.png" alt="image-20240614131326894" tabindex="0" loading="lazy"><figcaption>image-20240614131326894</figcaption></figure><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614225842011.png" alt="image-20240614225842011" tabindex="0" loading="lazy"><figcaption>image-20240614225842011</figcaption></figure><p>软件支持用户手动输入正文。</p><p>若用户指定了正文，则不会使用模板作为邮件正文。</p><p>正文的格式与模板一样，同样支持变量。</p><h4 id="发件人" tabindex="-1"><a class="header-anchor" href="#发件人"><span>发件人</span></a></h4><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614131606189.png" alt="image-20240614131606189" tabindex="0" loading="lazy"><figcaption>image-20240614131606189</figcaption></figure><p>单击发件人右侧的 + 号，选择发件人。</p><p>发件人允许有多个，若有多个发件人，发件时，将会把邮件随机给其中一个发件人发件。</p><p>在一次任务中，一封邮件只会被其中一个发件箱成功发送，不会多次发送。</p><h4 id="收件人" tabindex="-1"><a class="header-anchor" href="#收件人"><span>收件人</span></a></h4><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614131851665.png" alt="image-20240614131851665" tabindex="0" loading="lazy"><figcaption>image-20240614131851665</figcaption></figure><p>单击收件人右侧的 + 号，选择收件人。</p><p>单次发件任务中，允许添加多个收件人，每个收件都会收到一款邮件。</p><h4 id="抄送人" tabindex="-1"><a class="header-anchor" href="#抄送人"><span>抄送人</span></a></h4><p>若选择抄送人，每一封邮件都会抄送到每一个抄送人处。</p><h4 id="附件" tabindex="-1"><a class="header-anchor" href="#附件"><span>附件</span></a></h4><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614214517919.png" alt="image-20240614214517919" tabindex="0" loading="lazy"><figcaption>image-20240614214517919</figcaption></figure><p>若邮件中需要添加附件，可以在此添加附件。</p><p>允许添加多个附件，但是请注意，每封邮件都会携带相同的附件。</p><h4 id="数据" tabindex="-1"><a class="header-anchor" href="#数据"><span>数据</span></a></h4><p>支持数据发件功能是该软件的灵魂。通过导入数据，可以实现一次发件中，为不同的收件箱发送不同的内容。</p><p>当将鼠标聚焦在数据栏，右侧会出现下载模板的图标，单击该图标即可下载模板。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614215520531.png" alt="image-20240614215520531" tabindex="0" loading="lazy"><figcaption>image-20240614215520531</figcaption></figure><p>数据格式大致如下：</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614220243117.png" alt="image-20240614220243117" tabindex="0" loading="lazy"><figcaption>image-20240614220243117</figcaption></figure><p><strong>数据的效果</strong>：</p><p>模板内容:</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614222544153.png" alt="image-20240614222544153" tabindex="0" loading="lazy"><figcaption>image-20240614222544153</figcaption></figure><p>模板赋予数据后的正文预览：</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614222617857.png" alt="image-20240614222617857" tabindex="0" loading="lazy"><figcaption>image-20240614222617857</figcaption></figure><p><strong>数据的作用</strong>：</p><ol><li>为模板提供变量</li><li>快速实现精准的批量发送</li></ol><p><strong>数据中系统保留变量</strong>：</p><table><thead><tr><th>变量名</th><th>必须</th><th>描述</th></tr></thead><tbody><tr><td>inbox</td><td>是</td><td>指定收件邮箱。该字段必须存在，程序依靠该变量进行发件匹配。若为空，则该行数据无效。</td></tr><tr><td>inboxName</td><td>否</td><td>设置收件人名称。</td></tr><tr><td>subject</td><td>否</td><td>指定主题。若指定，则会忽略界面中输入的主题。</td></tr><tr><td>outbox</td><td>否</td><td>指定发件箱。若不指定，则使用用户在界面中选择的发件箱。<br>该发件箱必须是在【邮箱管理/发件箱】中添加的邮箱地址。其它邮箱则视为无效。</td></tr><tr><td>outboxName</td><td>否</td><td>设置发件箱名称。若不指定，则使用发件箱管理中的名称。</td></tr><tr><td>cc</td><td>否</td><td>指定抄送人。多个抄送人使用逗号分隔。</td></tr><tr><td>templateId</td><td>否</td><td>指定邮件的模板 Id。该模板 Id 可在模板管理中查看，是一个数字。<br>若不指定，则从用户选择的模板中随机取一个使用。</td></tr><tr><td>templateName</td><td>否</td><td>指定邮件的模板名称。该名称可在模板管理中查看。templateName 的优先级低于 templateId。当两者同时指定时，以 templateId 为主。<br>若不指定，则从用户选择的模板中随机取一个使用。</td></tr><tr><td>body</td><td>否</td><td>指定邮件的正文内容。该优先级大于 templateId 和 templateName。</td></tr></tbody></table><p><strong>数据的优先级</strong>：</p><ul><li><p>主题</p><p>【Excel数据/subject】 &gt; 【界面/主题】</p></li><li><p>正文</p><p>【Excel数据/body】 &gt; 【Excel数据/templateId】 &gt; 【Excel数据/templateName】 &gt; 【界面/正文】 &gt;【 界面/模板 】</p></li><li><p>发件人</p><p>【Excel数据/outbox 】&gt;【 界面/发件人】</p></li><li><p>收件人</p><p>【Excel数据/inbox 】&gt; 【界面/收件人】</p></li><li><p>抄送人</p><p>【Excel数据/cc】 &gt; 【界面/抄送人】</p></li><li><p>附件</p><p>附件目前无法通过数据指定</p></li></ul><h3 id="发件历史" tabindex="-1"><a class="header-anchor" href="#发件历史"><span>发件历史</span></a></h3><p>发件历史显示历次所发的所有邮件记录，一次发送任务为一条历史。</p><p>单击 ID 或者右键【详细】可查看具体发件项。</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614225346191.png" alt="image-20240614225346191" tabindex="0" loading="lazy"><figcaption>image-20240614225346191</figcaption></figure><p>发件明细：</p><figure><img src="https://obs.uamazing.cn:52443/public/files/images/image-20240614231145776.png" alt="image-20240614231145776" tabindex="0" loading="lazy"><figcaption>image-20240614231145776</figcaption></figure>',48))])}const d=n(m,[["render",c],["__file","detail.html.vue"]]),u=JSON.parse('{"path":"/guide/functions/detail.html","title":"功能列表","lang":"zh-CN","frontmatter":{"title":"功能列表","icon":"list-check","order":2,"description":"登陆界面 uzon-mail-login-2uzon-mail-login-2 默认用户名和密码为：admin/admin1234 首页 image-20240614231957130image-20240614231957130 首页主要展示的内容有： 发件箱数量直方图 收件箱数量直方图 每月发件量折线图 系统设置 用户管理 image-202406...","head":[["meta",{"property":"og:url","content":"https://uzonmail.pages.dev/guide/functions/detail.html"}],["meta",{"property":"og:site_name","content":"宇正群邮"}],["meta",{"property":"og:title","content":"功能列表"}],["meta",{"property":"og:description","content":"登陆界面 uzon-mail-login-2uzon-mail-login-2 默认用户名和密码为：admin/admin1234 首页 image-20240614231957130image-20240614231957130 首页主要展示的内容有： 发件箱数量直方图 收件箱数量直方图 每月发件量折线图 系统设置 用户管理 image-202406..."}],["meta",{"property":"og:type","content":"article"}],["meta",{"property":"og:image","content":"https://obs.uamazing.cn:52443/public/files/images/uzon-mail-login-2.png"}],["meta",{"property":"og:locale","content":"zh-CN"}],["meta",{"property":"og:updated_time","content":"2025-03-15T14:43:09.000Z"}],["meta",{"property":"article:modified_time","content":"2025-03-15T14:43:09.000Z"}],["script",{"type":"application/ld+json"},"{\\"@context\\":\\"https://schema.org\\",\\"@type\\":\\"Article\\",\\"headline\\":\\"功能列表\\",\\"image\\":[\\"https://obs.uamazing.cn:52443/public/files/images/uzon-mail-login-2.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614231957130.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612122713293.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612123329057.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612123429178.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612125131168.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612125859579.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612131312091.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612130111084.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612131552355.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612131711338.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612131916319.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240612132056895.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614123302248.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614124814856.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614125056768.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614125417120.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614125609679.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614125845479.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614130827129.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614131034812.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614131326894.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614225842011.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614131606189.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614131851665.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614214517919.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614215520531.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614220243117.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614222544153.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614222617857.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614225346191.png\\",\\"https://obs.uamazing.cn:52443/public/files/images/image-20240614231145776.png\\"],\\"dateModified\\":\\"2025-03-15T14:43:09.000Z\\",\\"author\\":[{\\"@type\\":\\"Person\\",\\"name\\":\\"galens\\",\\"url\\":\\"galens.uamazing.cn\\"}]}"]]},"headers":[{"level":2,"title":"登陆界面","slug":"登陆界面","link":"#登陆界面","children":[]},{"level":2,"title":"首页","slug":"首页","link":"#首页","children":[]},{"level":2,"title":"系统设置","slug":"系统设置","link":"#系统设置","children":[{"level":3,"title":"用户管理","slug":"用户管理","link":"#用户管理","children":[]},{"level":3,"title":"基础设置","slug":"基础设置","link":"#基础设置","children":[]},{"level":3,"title":"代理管理","slug":"代理管理","link":"#代理管理","children":[]}]},{"level":2,"title":"邮箱管理","slug":"邮箱管理","link":"#邮箱管理","children":[{"level":3,"title":"发件箱","slug":"发件箱","link":"#发件箱","children":[]},{"level":3,"title":"收件箱","slug":"收件箱","link":"#收件箱","children":[]}]},{"level":2,"title":"模板管理","slug":"模板管理","link":"#模板管理","children":[{"level":3,"title":"新增模板","slug":"新增模板","link":"#新增模板","children":[]},{"level":3,"title":"模板编辑","slug":"模板编辑","link":"#模板编辑","children":[]},{"level":3,"title":"模板变量","slug":"模板变量","link":"#模板变量","children":[]}]},{"level":2,"title":"发件管理","slug":"发件管理","link":"#发件管理","children":[{"level":3,"title":"新建发件","slug":"新建发件","link":"#新建发件","children":[]},{"level":3,"title":"发件历史","slug":"发件历史","link":"#发件历史","children":[]}]}],"git":{"createdTime":1742049789000,"updatedTime":1742049789000,"contributors":[{"name":"galens","username":"galens","email":"gmx_galens@163.com","commits":1,"url":"https://github.com/galens"}]},"readingTime":{"minutes":10.54,"words":3162},"filePathRelative":"guide/functions/detail.md","localizedDate":"2025年3月15日","autoDesc":true}');export{d as comp,u as data};
