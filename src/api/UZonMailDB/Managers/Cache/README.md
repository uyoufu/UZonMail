# 数据库依赖缓存

本目录实现进程内的数据库派生缓存。它将缓存分为“原始数据源”和“派生结果”两层：业务代码只需要在构建结果时读取原始数据源，缓存管理器会自动记录依赖版本；任一原始数据源更新或失效后，所有依赖它的不同派生结果都会在下次读取时自动重建。

该设计用于替代按 `TResult` 手工设置脏标记的方式，避免新增一种结果类型时遗漏更新逻辑。

## 核心模型

### 原始数据源

原始数据源由 `CacheSourceKey<TValue, TIdentity>` 唯一标识，其实际键由以下三部分组成：

- `TValue`：快照值类型。
- `TIdentity`：业务标识类型。
- `Identity`：具体业务标识值。

每次调用 `SetSourceAsync` 或 `InvalidateSourceAsync`，管理器都会为该数据源分配一个进程内单调递增的版本号。版本号用于判断派生结果是否仍然有效，可避免值相同或缓存过期后出现版本回退问题。

原始数据源有两种常见形式：

- **完整快照**：例如 `UserInfoSnapshot`、`AppSettingSnapshot`。更新数据库后直接发布新快照，后续读取不必再次查询数据库。
- **修订标记**：使用 `CacheRevision` 表示“某个数据集合发生了变化”。失效后，派生结果重建时重新查询数据库，适用于模板列表等集合数据。

### 派生结果

派生结果继承 `BaseDBCache<TSqlContext, TArg>`。结果缓存键由 `TResult`、`TArg` 类型和参数值共同组成，因此相同参数下的不同 `TResult` 会分别缓存，但可以依赖同一个原始数据源。

`UpdateCore` 通过 `CacheBuildContext.GetSourceAsync` 读取原始数据。构建上下文会自动保存读取到的数据源版本，业务代码不需要维护反向依赖列表。

每个派生结果必须至少读取一个原始数据源。没有登记依赖的结果无法判断新鲜度，管理器会抛出 `InvalidOperationException`。

## 读取与刷新流程

调用 `IDBCacheManager.GetCache` 时：

1. 按结果类型和业务参数查找已缓存结果。
2. 比较结果记录的所有依赖版本与当前原始数据源版本。
3. 版本全部一致时直接返回现有结果。
4. 任一依赖缺失、失效或版本变化时，创建新的 `TResult` 并执行 `UpdateCore`。
5. 构建完成后再次检查依赖版本。若构建期间有数据源变化，则丢弃本次结果并重新构建。
6. 依赖稳定后原子发布新结果。

刷新不会修改已经返回给调用方的旧对象。刷新失败时异常会传递给当前调用方，旧结果也不会被不完整的新结果覆盖；下一次读取仍会尝试刷新。

相同键的原始数据加载和派生结果构建使用异步键锁串行化，因此并发首次读取只会执行一次加载或构建。不同键之间可以并行执行。

## 创建派生缓存

下面的示例缓存用户展示信息，并依赖用户信息原始快照：

```csharp
public sealed class UserDisplayCache : BaseDBCache<SqlContext, long>
{
    public string DisplayName { get; private set; } = string.Empty;

    protected override async Task UpdateCore(
        CacheBuildContext buildContext,
        SqlContext db,
        CancellationToken cancellationToken
    )
    {
        var userInfo = await UserInfoCache.GetSnapshotAsync(
            buildContext,
            db,
            Args,
            cancellationToken
        );

        DisplayName = $"User-{userInfo.UserId}";
    }
}
```

通过依赖注入获取 `IDBCacheManager` 后读取：

```csharp
var userDisplay = await cacheManager.GetCache<UserDisplayCache>(
    db,
    userId,
    cancellationToken
);
```

若参数不是 `long`，使用完整泛型重载：

```csharp
var result = await cacheManager.GetCache<MyCache, SqlContext, MyCacheArgument>(
    db,
    cacheArgument,
    cancellationToken
);
```

`TArg` 会参与缓存键比较，应使用不可变且具有稳定值相等语义的类型，优先选择 `record`、`record struct` 或基础值类型。不要使用会在加入缓存后继续变化的对象作为参数。

## 定义和读取原始数据源

建议在拥有该原始数据业务语义的类型旁集中定义源键和加载方法，避免读写两端构造出不一致的键。

```csharp
public sealed record FeatureSnapshot(bool IsEnabled)
{
    public static CacheSourceKey<FeatureSnapshot, long> GetSourceKey(long userId) =>
        new(userId);

    public static Task<FeatureSnapshot> GetAsync(
        CacheBuildContext buildContext,
        SqlContext db,
        long userId,
        CancellationToken cancellationToken
    ) =>
        buildContext.GetSourceAsync(
            GetSourceKey(userId),
            async token =>
            {
                var isEnabled = await db.Users
                    .Where(user => user.Id == userId)
                    .Select(user => user.IsEnabled)
                    .SingleAsync(token);
                return new FeatureSnapshot(isEnabled);
            },
            cancellationToken
        );
}
```

加载委托仅在原始数据源不存在、已过期或已失效时执行。派生缓存必须通过 `CacheBuildContext` 读取数据源，直接调用数据库查询不会登记依赖。

## 数据库写入后的缓存通知

缓存通知必须在数据库写入成功后执行。若业务使用显式事务，应在事务提交后发布，避免缓存暴露尚未提交或最终回滚的数据。

### 发布完整快照

保存后已经持有完整且可信的新值时，使用 `SetSourceAsync`：

```csharp
await db.SaveChangesAsync(cancellationToken);

await cacheManager.SetSourceAsync(
    UserInfoCache.GetSourceKey(user.Id),
    UserInfoSnapshot.FromUser(user),
    cancellationToken
);
```

这会立即替换原始快照并提升版本。所有依赖该用户快照的结果，无论 `TResult` 是否相同，都会在下次读取时刷新。

### 标记数据源失效

写入后不方便构造完整快照，或原始数据表示一个集合时，使用 `InvalidateSourceAsync`：

```csharp
await db.SaveChangesAsync(cancellationToken);

await cacheManager.InvalidateSourceAsync(
    UserTemplatesCache.GetOwnedSourceKey(userId),
    cancellationToken
);
```

失效操作不会立即遍历或重建所有派生结果。下一次读取相关结果时，加载委托会重新获取原始数据，然后只重建实际被访问的结果。

对于一个变更影响多个范围的场景，应精确失效全部受影响的源键。例如模板共享关系变化时，应同时处理所有者、直接共享用户和共享组织范围，可复用 `UserTemplatesCache.InvalidateTemplateScopesAsync`。

## 仅表示集合变化的修订源

当派生结果需要自行查询复杂集合，但仍需感知集合变化时，可以依赖 `CacheRevision`：

```csharp
var revisionKey = new CacheSourceKey<CacheRevision, MyCollectionScope>(scope);

await buildContext.GetSourceAsync(
    revisionKey,
    static _ => Task.FromResult(CacheRevision.Current),
    cancellationToken
);

var rows = await db.MyEntities.AsNoTracking().ToListAsync(cancellationToken);
```

集合更新成功后调用：

```csharp
await cacheManager.InvalidateSourceAsync(revisionKey, cancellationToken);
```

修订源只负责传递变化信号，不保存集合内容。

## 配置

`DBCacheManager` 通过 `DBCacheOptions` 读取配置：

```json
{
  "Database": {
    "Cache": {
      "SlidingExpirationMinutes": 30,
      "ExpirationScanFrequencyMinutes": 5
    }
  }
}
```

- `SlidingExpirationMinutes`：原始数据源和派生结果的滑动过期时间，最小值为 1 分钟。
- `ExpirationScanFrequencyMinutes`：内存缓存扫描过期项的频率，最小值为 1 分钟。

服务实现了 `ISingletonService<IDBCacheManager>`，由项目的服务自动注册机制注册。缓存仅在当前服务进程内有效，不会在多实例之间同步；多实例部署若需要一致失效，应在业务层增加消息通知，并在每个实例调用对应的发布或失效方法。

## 使用约束

- 原始快照应与 EF 跟踪对象隔离，并尽量设计为不可变对象。
- 源键必须由业务含义稳定的强类型标识组成，不要使用字符串拼接生成键。
- 同一数据源的读取与写入通知必须使用完全一致的 `TValue`、`TIdentity` 和标识值。
- 所有数据库写入入口都必须负责发布快照或失效对应数据源；缓存管理器不会自动监听 EF 变更。
- `UpdateCore` 应构建完整的新结果，不要依赖或修改旧缓存实例。
- 不要在 `UpdateCore` 中调用同一结果键的 `GetCache`，否则会造成递归等待。
- 始终传递调用链上的 `CancellationToken`。
- 不要捕获并吞掉刷新异常，否则调用方无法识别当前结果未成功构建。

现有实现可参考 `UserInfoCache`、`UserTemplatesCache` 和 `AppSettingSnapshot`。
