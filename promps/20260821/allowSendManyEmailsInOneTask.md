# 在一个任务组中，支持对相同用户发送多封邮件

## web 前端

1. D:\Develop\Personal\UzonMail\src\web\src\pages\systemSetting\basicSetting\expansionItems\SendSetting.vue 在中增加“允许重复发件”，表示在同一任务中，同一个收件人允许被多次发件，默认不允许 
2. D:\Develop\Personal\UzonMail\src\web\src\pages\sendingManager\sendingTask\components\SelectEmailData.vue 用户上传邮件数据后，进行收件人查重，若存在重复收件，显示在右侧功能区显示一个按钮, 用户可以单击，弹出一个信息弹窗，显示具体的重复数据，收件人名称、邮件人邮箱、发件数

## api 后端

1. D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Services\SendCore\SendingGroupCreationService.cs 中要验证 Excel 中的附件是否存在，若不存在，中断任务