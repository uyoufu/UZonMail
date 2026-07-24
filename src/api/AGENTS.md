# api 项目规范

## 交付目标

- 多组织、多用户
- 高并发发件、高吞吐量、高性能
- 后台可能同时存在上百万邮箱等待发送

## 服务注册

- 非泛型接口自动注册：所有实现 IScopedService、ISingletonService 或 ITransientService 的实现类型，都会自动注册为自身的服务类型
- 泛型接口自动注册：所有实现 IScopedService<T>、ISingletonService<T> 或 ITransientService<T> 的实现类型，都会自动注册为自身以及 T 对应的服务类型

## 配置调用

- 配置不要使用 `service.Configure<>` 注册，也不要使用 `IOptions<T>` 获取配置实例，使用 `IAppOptions<T> options` 来获取， 然后通过 `options.Value` 获取配置实例
