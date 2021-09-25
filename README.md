# README

## 🥝简介

![20210902000641.png](https://i.loli.net/2021/09/02/3FZ9QBYqcfrtuV1.png)

本程序名为“邮件群发助手”，旨在实现邮件发送自动化，批量化和内容可定制化。最简单的应用场景就是财务可以利用它向员工发送每个人对应的工资条。

<!--more-->

## 🍎特点

- 支持多个发件人
  
  可以添加多个**不同的**发件人，同时发件，提高发件效率。

- 支持多个收件人
  
  可以添加多个收件人，实现批量发送

- 多发件人多线程并发发送
  
  每个发件人采用单独的线程进行发送，所以一个线程挂掉之后并不会使发件进程停止，会由其它发件的所在线程继续发件。

- 发件中失败自动重发
  
  当有多个发件箱时，若 A 发件箱发件失败后，会转由 B 发件箱继续进行发送。
  
  如果仅有一个发件箱，当发件失败后，会在其它邮件发送完成后，再次发送，可重发次数最大为5次。

- 支持邮件内容模板自定义
  
  模板完全可自定义，可根据需要定义自己所需的模板，并保存到模板库，实现模板的复用。模板板采用 html 格式定义，程序也提供可视化界面进行编辑。
  
  对于新手和大神都很友好。

- 支持无限变量，使得模板内容根据数据自动变化
  
  可以在模板中引入变量，在发送的过程中，会自动将变量值替换成其真实的值进行发送，可以实现同一套模板，不同的收件人，接收的具体内容不同。

- 支持失败重发
  
  所有的发送过程都有记录，对于未发送成功的邮件，可以在发送任务完成后，手动进行重发。
  
  重发过程也支持多线程。

- 支持内容转为图片发送

## 🍇使用环境要求

1. windows 7 及以上

2. .NET Framework 4.6.2 及以上，下载地址：https://dotnet.microsoft.com/download/dotnet-framework

3. webview2环境，下载地址：https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/

## 😃如何使用

**编译版本：**

1. 下载程序，解压到自定义目录；
2. 单击 `邮件批量发送.exe` 运行。

**手动编译：**

1. 克隆仓库，切换到 `master` 分支；
2. 通过终端进入到 `UI` 目录，在此处运行 `npm install` 安装依赖包，然后执行 `npm run build:prod` 进行编译；
3. 打开 `Server`，用  `visual studio 2019` 或更高版本打开  `Server.sln`，打开后编为 `release；`
4. 将第 3 步编译的 `dist` 文件夹内所有文件夹及文件拷贝到 `release` 的 `public` 目录中（如果没有，请新建）；
5. 至此，编译完成。单击 release 中的 `邮件批量发送.exe` 即可运行。

> 注意，上述两种方法都需要 `webview2` 运行环境的支持，请提前自行安装。

## 📫发件步骤

1. 添加发件箱（已添加请忽略）
2. 添加收件箱（已添加请忽略）
3. 导入所需模板（已添加请忽略）
4. 打开【新建发件】，输入主题 --> 选择收件人 --> 选择模板 --> 选择数据 -->预览确认发件数量和模板正确性 --> 退出预览 -->点击【发送】
5. 如果提示发送失败，转到【发件历史】，进行重发

## 🥑功能介绍

### 登陆界面

![image-20210901235949132](https://i.loli.net/2021/09/01/aIc3Jsm41DK9Mdj.png)

如果系统中不存在账号名称，则系统会自动新建账号。

> 特别注意：
> 
> 注册的信息是保存在本地的数据库中。
> 
> 每个用户的数据是隔离的，不能相互访问

### 首页

![20210902000641.png](https://i.loli.net/2021/09/02/3FZ9QBYqcfrtuV1.png)

首页为欢迎页面。在首页中，会加载左下角的 Live2d 模块，如果不需要，可以手动关闭。

### 系统设置

![](https://i.loli.net/2021/09/02/mGNdJertsIbBFQc.png)

- 发件间隔范围
  
  为了避免因频繁发送邮件而导致被服务器认为是垃圾邮箱，所以，发送两封邮件之间需要有一定的时间间隔，为了使得发送时间间隔具有不规律性，用间隔范围来进行控制
  
  实际发件间隔值 = 最小值 + （0，1）之间的随机数*（最大值-最小值）

- 自动重发
  
  勾选后，在发送中，会对失败的邮件重新发送，当重发次数超过 5 次，则会退出重发。对于重发失败的，可以在【发件历史】中重新发送。

- 图文混发
  
  勾选后，在发件过程中，程序会将一半的 html 内容转成图片（dataUrl）来进行发送。
  
  > **注意**
  > 
  > 截止2021-09-15为止，转成图片功能不支持对内容的图片进行转换。如果内容中本身有图片，请不要勾选该设置。

### 邮箱管理

#### 发件人

![](https://i.loli.net/2021/09/02/UEqygPt4r7vHZfF.png)

此处用于管理发件人信息。下面列出在使用中需要注意的功能进行说明：

**组管理：**

![](https://i.loli.net/2021/09/02/yk4sDP3BATedCn5.png)

界面内左侧为分组区域，在此区域右键可以弹出上下文菜单，其功能分别为【添加组】、【添加子组】、【修改】和【删除】。其作用如其名，不多赘述。

**从EXCEL导入：**

批量从 Excel 中导入发件箱。在使用批量添加发件人功能时，Excel 表中第一行为表头，必须含有 `userName`、`email`、`smtp`、`password`  这些表头，可以参考 Excel 模板中的 “发件人” Sheet 来规范格式。

> 特别注意
> 
> 发件采用的 SMTP 服务器，所以，它的密码并不是邮箱的密码，而是登陆邮箱后，自己申请的 SMTP 密码。
> 
> 比如，163邮箱 SMTP 密码获取方式如下：https://www.yisu.com/zixun/97973.html

#### 收件人

![](https://i.loli.net/2021/09/02/PzjDUuBHa8Y1rnJ.png)

该模块主要用于对收件箱的分组和管理，使用方式、注意要点与发件人一致。

收件箱只需要姓名和邮箱。

#### 正文模板

![](https://i.loli.net/2021/09/02/k37Cm4cyNvxbPD8.png)

在【正文模板】用于管理用户下的所有模板，它是 html 格式，可以使用两种方式进行添加：

- 导入 Html
  
  先在外面用 html 定义好模板，然后通过上述中的【导入模板】功能将定义的模板导入到系统。对于自定义的 html 模板，要求其中的 css 必须为行内 css。可以通过 http://automattic.github.io/juice/ 自动将 css 合并到标签。

- 直接编辑
  
  可以选择【编辑模板】功能通过程序自带的编辑器添加模板。

##### 模板变量

在模板的编写过程中，可以使用 &#123;&#123;&#125;&#125; 双花括号来标记为变量，在发件的过程中，程序会在数据中查找该变量，如果查找到，就会使用实际的数据将变量替换掉。

变量定义的格式是为：&#123;&#123;变量名&#125;&#125;。

> 在发件中，模板也可以因发件人而异，需要在数据中增加 template 列来覆盖通用的模板。具体参考发件篇。

程序会为模板提供默认变量，在数据中可以覆盖这些变量：

| 序号  | 变量名      | 描述       |
| --- | -------- |:-------- |
| 1   | userName | 收件人的用户名  |
| 2   | inbox    | 收件人email |

### 发件管理

#### 新建发件

![](https://i.loli.net/2021/09/02/HcKWsiX4N9MeOnt.png)

此处进行邮件的发送。

当前录入了发件人、收件人和模板后，即可开始发件。

**主题：**

发件的主题是必须的，主有两个作用：一是为邮件的主题，二是同一次发件将会归到一个发件历史记录中，该主题为历史记录组的名称。

主题也支持变量声明，比如：`{{日期}}-工资明细`，`日期`即为定义的变量，在发送邮件时，它将被替换成 Excel 表中的实际数据。

**收件人：**

![20210902080111.png](https://i.loli.net/2021/09/02/zZRCqr459ylHt3g.png)

收件人选择框如上图，在选择收件人的时候，可以选择收件组，也可以选择组中的收件人。在选择的过程中，不需要担心收件人重复问题，系统会在后台自动去重。选择完成后，点击白色窗体空白部分即可确认和退出选择。

**模板:**

此处的模板即为邮件的正文内容，选择我们需要的模板即可。

在发件中，可以针对每个收件人设置单独的模板，需要在数据表中增加 `template` 列，并填入所需的模板名称。

除了自定义模板外，也可以直接针对每个收件人自定义内容，需要在数据表中增加 `body` 列，并填入发送的主体内容。

自定义内容的优先级高于自定义模板。

> 对单个收件人的自定义模板和自定义内容均支持变量声明。

**数据：**

选择模板后，还需要选择对应的数据。此处数据中的表头要与模板中定义的变量有对应关系，且数据中必须包含 `useName` 列，用于与发件箱匹配。

> **注意：**
> 
> 在发件之前，请点击预览，复核发件的数量是否正确，模板数据是否按预想修改。如果发件数量或者数据不正确，请复核 excel 数据是否正确。

**预览：**

![](https://i.loli.net/2021/09/02/nQGt3Deh2CrO7Xu.png)

#### 发件历史

![](https://i.loli.net/2021/09/02/EGNnkcAXY4PrKIQ.png)

发件历史显示历次所发的所有邮件记录，一次发送记录为一条历史。单击详细可以查看该次历史下所有的发件。如果有发件失败的，可以点【重发】进行发送，只会重发失败的邮件。

## ✔ToDo

- [ ] 解决图片内容转图片问题

- [ ] 修改首页，添加图表展示

- [ ] 本地图片上传支持
  
  支持模板中的本地图片转 dataUrl
  
  支持编辑器中插入本地图片

- [ ] 添加图床功能（远期）

## ⚒技术栈

本系统采用前后端分离的模式进行开发。

**前端：**

javascript + vue + element-admin + quasar

基于 element-admin 的框架，采用 quasar 组件进行界面开发。

**后端：**

C# + WPF + Stylet + EmbedIO + SuperSocket.Websocet

基于 Stylet 的轻量 MVVM 框架进行开发，使用 EmbedIO 为前端提供网页和 API 服务。利用 SuperSocket.Websocket 为前后端提供双工通信能力。 

## ❤反馈与建议

如果你在使用中发现了 bug, 或者对该软件有任何建议，都欢迎联系我，让我们将这款软件一起变得更优秀吧！

## 🤝联系方式

QQ群：877458612

邮箱：260827400@qq.com

GitHub：https://github.com/GalensGan/SendMultipleEmails

个人主页：https://galensgan.github.io
