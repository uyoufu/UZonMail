# 添加收件箱清洗功能

为收件箱增加清洗功能，解决收件箱可能不存在影响发件箱的信誉

## web 前端

1. D:\Develop\Personal\UzonMail\src\web\src\pages\emailManager\inbox\inboxManager.vue 中的分类和数据行原有的右键菜单上分别 "验证" 功能，点击后对当前组、或者选中项、或者当前项进行发件箱验证
2. 若验证不通过，则将其移动到 "验证失败" 分类，若不存在该分类，则新建该分类

## api/CorePlugin

1. 

## api/ProPlugin
