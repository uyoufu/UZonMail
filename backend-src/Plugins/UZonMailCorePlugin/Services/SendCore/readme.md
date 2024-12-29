# 开发说明

## 开发约定

1. GroupTasksList.cs、OutboxesPoolList 的结构只保存自己的数据，不与彼此交互
2. 上述对象的联动变化，在职责链中去实现
3. 所有资源清理都要在职责链过程中完成