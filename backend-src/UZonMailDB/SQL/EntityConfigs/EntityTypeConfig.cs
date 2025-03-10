using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.SQL.EntityConfigs
{
    /// <summary>
    /// 通用的实体配置
    /// </summary>
    internal class EntityTypeConfig : IEntityTypeConfig
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            // 取消级联删除
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            // 应用配置，参考：https://learn.microsoft.com/zh-cn/ef/core/modeling/#applying-all-configurations-in-an-assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // 对所有的实体配置 json 转换
            modelBuilder.AddJsonFields();

            // 为所有实现 ISoftDelete 接口的实体添加全局查询过滤器
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
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
