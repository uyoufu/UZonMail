# 优化我的邮件页面

按以下要求优化 ReceivingManager 页面

## 全局要求

移除所有的自定义 css，只通过 quasar 或自定义的 css 帮助类来实现样式；总体布局要紧凑

## 总体布局优化

1. 新增一个通用的 pageContainer 组件，统一页面外容器样式，四个角圆角; 然后将容器应用于 D:\Develop\Personal\UzonMail\src\web\src\pages\receivingManager\ReceivingManager.vue
2. 联系人与正文内容之间，使用 q-splitter 使得左右可拖动
3. 移除顶部的收件人 q-select 一行，将其改为一个小 Avatar 保存到搜索的左则，单击后，可以切换不同的联系人或者选择所有的联系人
4. src\web\src\pages\receivingManager\ 下所有自定义弹窗都复用 D:\Develop\Personal\UzonMail\src\web\src\components\windowLike\TitleBar.vue
5. 右侧聊天内容区，头部的发件人右侧显示 发件人邮箱--> 收件人邮箱 这样的内容，方便用户区分，同时减少顶部的高度
6. 内容区的布局使用如图片中的风格: C:\Users\galens\Desktop\chat_example.png, 每一条消息都显示头像、时间、内容，内容展开后，还可以折叠; 概览内容时，要显示更多的消息，方便用户回忆;对于大邮件，在加载的过程中，可使用 https://quasar.dev/vue-components/inner-loading 来显示内部加载状态
7. 发送框移到输入区右下角，减少占用
8. 引用方式不够高级且占用空间太多，按正常邮件逻辑，默认引用上一条消息，也可以手动选择部分消息引用

## 相关文件

1. src\web\src\pages\receivingManager\ReceivingManager.vue