using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using UZonMail.Utils.Web.Service;
using UZonMail.Core.Utils.Database;
using UZonMail.DB.SQL.Base;
using UZonMail.DB.SQL;

namespace UZonMail.Core.Services.Common
{
    /// <summary>
    /// 通用的增删改查服务
    /// </summary>
    public abstract class CurdService<TEntity>(SqlContext db) : IScopedService where TEntity : SqlId
    {
        /// <summary>
        /// 执行事务
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<TEntity> Transaction(Func<SqlContext, Task<TEntity>> func)
        {
            return await db.RunTransaction(func);
        }

        /// <summary>
        /// 创建
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<TEntity> Create(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            db.Add(entity);
            await db.SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="modifiedPropertyNames"></param>
        /// <returns></returns>
        public virtual async Task<TEntity> Update(TEntity entity, List<string> modifiedPropertyNames)
        {
            ArgumentNullException.ThrowIfNull(entity);
            if(modifiedPropertyNames == null || modifiedPropertyNames.Count == 0)
            {
                throw new ArgumentNullException(nameof(modifiedPropertyNames));
            }

            return await db.UpdateById(entity, modifiedPropertyNames);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<bool> Delete(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            db.Remove(entity);
            await db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 通过 id 删除数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<bool> DeleteById(long id)
        {
            await db.Set<TEntity>().Where(x=>x.Id==id).ExecuteDeleteAsync();;
            return true;
        }

        /// <summary>
        /// 通过 id 获取值
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<TEntity?> FindOneById(long id)
        {
            return await db.FindAsync<TEntity>(id);
        }
    }
}
