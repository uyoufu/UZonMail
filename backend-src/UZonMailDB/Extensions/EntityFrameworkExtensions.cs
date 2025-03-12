using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Base;

namespace UZonMail.DB.Extensions
{
    public static class EntityFrameworkExtensions
    {
        /// <summary>
        /// 更新匹配到的实体
        /// 该方法只会请求数据库 1 次
        /// 不需要调用 SaveChangesAsync 方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="predicate"></param>
        /// <param name="update"></param>
        /// <returns></returns>
        public async static Task<int> UpdateAsync<T>(this IQueryable<T> entities, Expression<Func<T, bool>> predicate, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> update) where T : SqlId
        {
            return await entities.Where(predicate).ExecuteUpdateAsync(update);
        }

        /// <summary>
        /// 通过 Id 更新实体
        /// 里面会调用 SaveChangesAsync 方法
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="ctx"></param>
        /// <param name="entity"></param>
        /// <param name="modifiedPropertyNames"></param>
        /// <returns></returns>
        public async static Task<TEntity> UpdateById<TEntity>(this SqlContextBase ctx, TEntity entity, List<string> modifiedPropertyNames) where TEntity : SqlId
        {
            if (entity == null || entity.Id == 0)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var entry = ctx.ChangeTracker.Entries<TEntity>().FirstOrDefault(x => x.Entity.Id == entity.Id);
            if (entry == null)
            {
                entry = ctx.Attach(entity);
                foreach (var propertyName in modifiedPropertyNames)
                {
                    entry.Property(propertyName).IsModified = true;
                }
            }
            else
            {
                // 已经被跟踪
                // 通过反射赋值
                var properties = typeof(TEntity).GetProperties();
                foreach (var propertyName in modifiedPropertyNames)
                {
                    var property = properties.FirstOrDefault(x => x.Name == propertyName);
                    if (property != null)
                    {
                        entry.Property(propertyName).CurrentValue = property.GetValue(entity);
                    }
                }
            }

            await ctx.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// 将目标集合赋值给原集合
        /// 自动处理增加和减少
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="origin"></param>
        /// <param name="target"></param>
        public static void SetList<T>(this ICollection<T> origin, IEnumerable<T> target)
        {
            if (origin == null || target == null) return;

            // 计算新增
            var addList = target.Except(origin).ToList();
            // 计算删除
            var removeList = origin.Except(target).ToList();
            // 开始添加
            foreach (var item in addList)
            {
                origin.Add(item);
            }
            // 开始删除
            foreach (var item in removeList)
            {
                origin.Remove(item);
            }
        }
    }
}
