# 查看发件内容时，显示完整内容

## src\web

优化 D:\Develop\Personal\UzonMail\src\web\src\pages\sendingManager\sendHistory\sendDetailContext.ts：

- 为 sendDetailContextItems 增加多语言
- 将 "查看正文" 必为查看 "邮件"
- 新增一个完整的邮件查看组件，包含邮件的标题、发送人、收件人、抄送人，密送人，邮件内容，邮件附件等，要求界面简洁美观

## src\api\Plugins\UzonMailCorePlugin

在 D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Controllers\Emails\SendingItemController.cs 新增路由来获取发送邮件的完整内容，该接口只能被自己调用