using Microsoft.EntityFrameworkCore;
using System.Reflection;
using UZonMail.DB.SQL.EntityConfigs;

namespace UZonMail.DB.SQL
{
    public class SqlContextBase : DbContext
    {
        #region 初始化
        public SqlContextBase() { }
        public SqlContextBase(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// 配置数据库
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
