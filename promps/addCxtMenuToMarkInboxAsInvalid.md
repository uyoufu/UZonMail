# 增加收件箱无效右键菜单

## src/web

1. 在收件箱页面中 D:\Develop\Personal\UzonMail\src\web\src\pages\emailManager\inbox\contextMenu.ts, 为右键菜单增加：
- 标记为无效: 仅当邮箱为非无效状态时，才显示该选项
- 标记为正常：仅当邮箱为非 Valid 状态时，才显示该选项

2. 上述两个功能都支持多选操作，若任一一个邮箱满足条件，则显示该菜单

3. 在顶总新增一个按钮，用于批量导入无效邮箱，邮箱之间使用常用分隔符分隔, 复用  splitString 函数解析

## CorePlugin

1. 新建发件组时，过滤掉掉 Invalid 状态的邮箱, 新建逻辑位于：D:\Develop\Personal\UzonMail\src\api\Plugins\UzonMailCorePlugin\Services\SendCore\SendingGroupCreationService.cs