# 优化附件管理器

## 附件管理右键菜单优化

1. 在 D:\Develop\Personal\UzonMail\src\web\src\pages\sendingManager\fileManager\AttachmentManager.vue 中，将移动和批量删除功能合并到右键菜单中, 使用 `actionContext: IActionContext<IFileUsage>` 获取选中的附件，执行完成后，要清除选中
2. AttachmentManager.vue 右键菜单的逻辑重构为独立的 composable

## 折叠图标优化

1. D:\Develop\Personal\UzonMail\src\web\src\pages\sendingManager\fileManager\AttachmentManager.vue 中，当左侧的树宽度动态变化时，折叠图标没有同步变化位置，进行优化
2. D:\Develop\Personal\UzonMail\src\web\src\components\collapseIcon\CollapseRight.vue 可以能也在 1 中的问题，一并进行优化

## 上传文件进度条优化


1. 优化 D:\Develop\Personal\UzonMail\src\web\src\api\base\httpClient.ts 代码，修复可能存在的问题，对于错误返回值，表现应一致
1. D:\Develop\Personal\UzonMail\src\web\src\components\uploader\FilesUploaderPopup.vue 解决若上传文件出错，弹窗不会关闭的问题
