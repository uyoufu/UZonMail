using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Utils.Database.LiteDB
{
    public static class LiteRepositoryExtension
    {
        /// <summary>
        /// 更新或者新建
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="repository"></param>
        /// <param name="filter"></param>
        /// <param name="data"></param>
        /// <param name="options">仅更新部分字段</param>
        /// <returns></returns>
        public static T UpsertOne<T>(this ILiteRepository liteRepository, Expression<Func<T, bool>> filter, T data, UpdateOptions options = null)
        {
            // 查找是否存在
            T exist = liteRepository.Query<T>().Where(filter).FirstOrDefault();
            // 如果不存在，新建
            if (exist == null)
            {
                liteRepository.Insert(data);
                return data;
            }

            // 更新数据
            Type dataType = data.GetType();
            var properties = dataType.GetProperties().Where(p => options == null || options.Validate(p.Name));
            foreach (var prop in properties)
            {
                object value = prop.GetValue(data);
                // 给exist赋值
                prop.SetValue(exist, value);
            }

            // 更新到数据库
            liteRepository.Upsert(exist);
            return exist;
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="liteRepository"></param>
        /// <param name="filter"></param>
        /// <param name="data"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static T UpdateOne<T>(this ILiteRepository liteRepository, Expression<Func<T, bool>> filter, T data, UpdateOptions options = null)
        {
            // 查找是否存在
            T exist = liteRepository.Query<T>().Where(filter).FirstOrDefault();
            // 如果不存在，返回空
            if (exist == null)
            {
                return default;
            }

            // 更新数据
            Type dataType = data.GetType();
            var properties = dataType.GetProperties().Where(p => options == null || options.Validate(p.Name));
            foreach (var prop in properties)
            {
                object value = prop.GetValue(data);
                // 如果为空，说明是默认值，不进行更新
                if (value == null) continue;

                // 给exist赋值
                prop.SetValue(exist, value);
            }

            // 更新到数据库
            liteRepository.Update(exist);
            return exist;
        }
    }
}
