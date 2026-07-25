# api 项目规范

## 交付目标

- 多组织、多用户
- 高并发发件、高吞吐量、高性能
- 后台可能同时存在上百万邮箱等待发送

## 服务注册

- 非泛型接口自动注册：所有实现 IScopedService、ISingletonService 或 ITransientService 的实现类型，都会自动注册为自身的服务类型
- 泛型接口自动注册：所有实现 IScopedService<T>、ISingletonService<T> 或 ITransientService<T> 的实现类型，都会自动注册为自身以及 T 对应的服务类型

## 配置调用

- 业务配置类型实现 `IAppOptions` 标记接口，由 `AddAllOptions` 自动扫描并注册，不要手动调用 `services.Configure<T>`
- 使用 `IOptions<T>` 获取固定配置，使用 `IOptionsSnapshot<T>` 或 `IOptionsMonitor<T>` 获取支持热更新的配置
