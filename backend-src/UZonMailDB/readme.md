# 项目说明

本目录保存数据库的相关的定义 model,目前采用 sqlLite,服务器可以切换成 mysql,方便进行扩展

## 数据迁移说明

z uzonmaildb

1. Mysql

dotnet ef migrations add perfSendingItemInbox --context MysqlContext --output-dir Migrations/Mysql -v

2. SqLite

dotnet ef migrations add perfSendingItemInbox --context SqLiteContext --output-dir Migrations/SqLite -v

## 取消数据迁移

1. Mysql

dotnet ef migrations remove --context MysqlContext -v

2. SqLite

dotnet ef migrations remove --context SqLiteContext -v