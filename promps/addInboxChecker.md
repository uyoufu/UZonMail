# 添加收件箱清洗功能

为收件箱增加清洗功能，解决收件箱可能不存在影响发件箱的信誉

## web 前端

1. D:\Develop\Personal\UzonMail\src\web\src\pages\emailManager\inbox\inboxManager.vue 中的分类和数据行原有的右键菜单上分别 "验证" 功能，点击后对当前组、或者选中项、或者当前项进行发件箱验证
2. 若验证不通过，则将其移动到 "验证失败" 分类，若不存在该分类，则新建该分类

## api/CorePlugin

1. 当发件出现硬退信时，表示邮件不存在，对该邮件项进行标记，并不再重试。同时将收件箱从既有分类中移动到 "验证失败" 分类。
2. 抽象一个收件箱验证模块，调用时用于验证收件箱是否真实存在, 每个具体的验证并发进行调用，提升验证效率，同时支持批量验证，结果采用 且 进行合并

## api/ProPlugin

1. 基于 Core 中的收件箱验证接口，实现收件箱清洗模块，功能可以参考 https://pkg.go.dev/github.com/AfterShip/email-verifier?utm_source=godoc , 主要包含：

- 语法与格式校验
- DNS MX 记录校验, 需要将数据保存到数据库中，可以新建表，在使用时，缓存到内存中，且设置过期时间，过期后，自动从数据库中更新
- 其它常用验证方式

2. 这部分功能仅 pro 及以上版本提供

## 参考实现

1. https://github.com/truemail-rb/truemail
2. https://github.com/AfterShip/email-verifier
