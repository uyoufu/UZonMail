# 开发文档

所有的设置都通过 `settingsService.GetSetting<SendingSetting>(sqlContext,outbox.UserId);` 的方式进行获取和保存。

AppSettingsManager 会自动处理缓存、部分设置继承关系等。