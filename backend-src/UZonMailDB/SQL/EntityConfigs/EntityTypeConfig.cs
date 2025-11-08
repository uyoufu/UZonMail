using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EntityConfigs
{
    /// <summary>
    /// 通用的实体配置
    /// </summary>
    public class EntityTypeConfig(Assembly? assembly = null)
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            // 取消级联删除
            foreach (
                var relationship in modelBuilder
                    .Model.GetEntityTypes()
                    .SelectMany(e => e.GetForeignKeys())
            )
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // 明确忽略 Newtonsoft.Json 的类型，避免 EF 将 JToken/JObject/JArray 误识别为实体
            // （作为保险措施，优先执行）
            //modelBuilder.Ignore(typeof(JToken));
            //modelBuilder.Ignore(typeof(JContainer));
            //modelBuilder.Ignore(typeof(JObject));
            //modelBuilder.Ignore(typeof(JArray));
            //modelBuilder.Ignore(typeof(JValue));

            // 应用配置，参考：https://learn.microsoft.com/zh-cn/ef/core/modeling/#applying-all-configurations-in-an-assembly
            assembly ??= Assembly.GetCallingAssembly();
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);

            // 对所有的实体配置 json 转换
            modelBuilder.AddJsonFields();

            // 为所有实现 ISoftDelete 接口的实体添加全局查询过滤器
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // Console.WriteLine($"检查实体类型: {entityType.ClrType.FullName}");
                // 添加全局删除过滤，一般不会影响索引，复杂情况需要使用 .ToQueryString() 查看语句
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var filter = Expression.Lambda(
                        Expression.Equal(
                            Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                            Expression.Constant(false)
                        ),
                        parameter
                    );
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }
        }
    }
}
