# api 项目规范

## 交付目标

- 多组织、多用户
- 单进程内高并发发件、高吞吐量、高性能
- 后台可能同时存在几千发件箱，上百万的邮箱待发送

## 服务注册

- 非泛型接口自动注册：所有实现 IScopedService、ISingletonService 或 ITransientService 的实现类型，都会自动注册为自身的服务类型
- 泛型接口自动注册：所有实现 IScopedService<T>、ISingletonService<T> 或 ITransientService<T> 的实现类型，都会自动注册为自身以及 T 对应的服务类型

## 配置调用

- 业务配置类型实现 `IAppOptions` 标记接口，由 `AddAllOptions` 自动扫描并注册，不要手动调用 `services.Configure<T>`
- 使用 `IOptions<T>` 获取固定配置，使用 `IOptionsSnapshot<T>` 或 `IOptionsMonitor<T>` 获取支持热更新的配置

## 控制器

- 所有的控制器继承 ControllerBaseV1 类
- 请求模型保存到控制器所在目录下的 ./DTOs/ 目录中，请示模型不要直接使用数据库实体类
- 简单验证使用 Data Annotations 标注，复杂验证使用 FluentValidation 验证

## 数据库

- 优先考虑逻辑删除
-删除数据时，禁止使用级联删除，所有删除操作在应用层手动分步删除。
