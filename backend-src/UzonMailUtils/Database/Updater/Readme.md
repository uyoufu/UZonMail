# 模块说明

用于数据自动升级

## 编写约定

1. 升级器应位于 `Updaters` 文件夹下，方便集中管理
2. 文件名和类名以 `Updater` 结尾
3. 继承 `IDataUpdater` 接口			
4. 每添加一个升级器，需要向 `DataUpdaterManager` 中添加一个实例化方法
5. 在 `DataUpdaterManager` 中设置当前程序需要的数据库版本号