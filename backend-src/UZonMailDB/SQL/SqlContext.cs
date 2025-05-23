﻿using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Core.Emails;
using UZonMail.DB.SQL.Core.EmailSending;
using UZonMail.DB.SQL.Core.Files;
using UZonMail.DB.SQL.Core.Organization;
using UZonMail.DB.SQL.Core.Permission;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.DB.SQL.Core.Templates;
using UZonMail.DB.SQL.EntityConfigs;
using UZonMail.Utils.Web.Service;

namespace UZonMail.DB.SQL
{
    /// <summary>
    /// Sql 上下文
    /// 参考：https://learn.microsoft.com/zh-cn/ef/core/modeling/relationships/conventions
    /// </summary>
    public class SqlContext : SqlContextBase, IScopedService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SqlContext));

        #region 初始化
        public SqlContext() { }
        public SqlContext(DbContextOptions<SqlContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 调用配置
            new EntityTypeConfig().Configure(modelBuilder);
        }
        #endregion

        #region 数据表定义
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<PermissionCode> PermissionCodes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRoles> UserRole { get; set; }

        public DbSet<FileBucket> FileBuckets { get; set; }
        public DbSet<FileObject> FileObjects { get; set; }
        public DbSet<FileUsage> FileUsages { get; set; }
        public DbSet<FileReader> FileReaders { get; set; }

        public DbSet<EmailGroup> EmailGroups { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Inbox> Inboxes { get; set; }
        public DbSet<Outbox> Outboxes { get; set; }

        public DbSet<SendingGroup> SendingGroups { get; set; }
        public DbSet<SendingItem> SendingItems { get; set; }
        public DbSet<SendingItemInbox> SendingItemInboxes { get; set; }

        public DbSet<AppSetting> AppSettings { get; set; }
        public DbSet<Proxy> Proxies { get; set; }

        // 设置相关
        public DbSet<SmtpInfo> SmtpInfos { get; set; }
        #endregion

        #region 测试
        //public DbSet<Post> Posts { get; set; }
        //public DbSet<Tag> Tags { get; set; }
        #endregion

        #region 通用方法
        /// <summary>
        /// 执行事务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<T> RunTransaction<T>(Func<SqlContext, Task<T>> func)
        {
            using var transaction = await Database.BeginTransactionAsync();
            try
            {
                // 执行一些数据库操作
                var result = await func(this);
                // 如果所有操作都成功，那么提交事务
                await transaction.CommitAsync();
                return result;
            }
            catch (Exception e)
            {
                // 如果有任何操作失败，那么回滚事务
                await transaction.RollbackAsync();

                // 向外抛出异常
                throw;
            }
        }
        #endregion
    }
}
