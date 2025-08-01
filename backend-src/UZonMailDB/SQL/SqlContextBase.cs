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

        #region 时间转换成 UTC 保存
        public override int SaveChanges()
        {
            ConvertDateTimesToUtc();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDateTimesToUtc()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    var properties = entry.Entity.GetType().GetProperties()
                        .Where(p => p.PropertyType == typeof(DateTime));
                    foreach (var prop in properties)
                    {
                        var value = (DateTime)prop.GetValue(entry.Entity)!;
                        if (value.Kind == DateTimeKind.Local)
                        {
                            prop.SetValue(entry.Entity, value.ToUniversalTime());
                        }
                        else if (value.Kind == DateTimeKind.Unspecified)
                        {
                            // 可选：假设未指定的时间为本地时间
                            prop.SetValue(entry.Entity, DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime());
                        }
                    }
                }
            }
        }
        #endregion
    }
}
