using log4net;
using Microsoft.EntityFrameworkCore;
using UZonMail.DB.SQL.Emails;
using UZonMail.DB.SQL.EmailSending;
using UZonMail.DB.SQL.EntityConfigs;
using UZonMail.DB.SQL.Settings;
using UZonMail.DB.SQL.Templates;

namespace UZonMail.DB.SQL
{
    /// <summary>
    /// Sql 上下文
    /// 参考：https://learn.microsoft.com/zh-cn/ef/core/modeling/relationships/conventions
    /// </summary>
    public class SqlContext : DbContext
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(SqlContext));

        #region 初始化
        public SqlContext() { }
        public SqlContext(DbContextOptions<SqlContext> options) : base(options)
        {
        }

        /// <summary>
        /// 配置数据库
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            new EntityTypeConfig().Configure(modelBuilder);
        }
        #endregion

        #region 数据表定义
        public DbSet<Organization.Department> Departments { get; set; }
        public DbSet<Organization.User> Users { get; set; }

        public DbSet<Permission.PermissionCode> PermissionCodes { get; set; }
        public DbSet<Permission.Role> Roles { get; set; }
        public DbSet<Permission.UserRoles> UserRole { get; set; }

        public DbSet<Files.FileBucket> FileBuckets { get; set; }
        public DbSet<Files.FileObject> FileObjects { get; set; }
        public DbSet<Files.FileUsage> FileUsages { get; set; }
        public DbSet<Files.FileReader> FileReaders { get; set; }

        public DbSet<EmailGroup> EmailGroups { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Inbox> Inboxes { get; set; }
        public DbSet<Outbox> Outboxes { get; set; }

        public DbSet<SendingGroup> SendingGroups { get; set; }
        public DbSet<SendingItem> SendingItems { get; set; }
        public DbSet<SendingItemInbox> SendingItemInboxes { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<OrganizationProxy> OrganizationProxies { get; set; }
        public DbSet<OrganizationSetting> OrganizationSettings { get; set; }

        public DbSet<ReadingTracker.EmailAnchor> EmailAnchors { get; set; }
        public DbSet<ReadingTracker.EmailVisitHistory> EmailVisitHistories { get; set; }
        public DbSet<ReadingTracker.IPInfo> IPInfos { get; set; }

        // 退定相关
        public DbSet<Unsubscribes.UnsubscribeSetting> UnsubscribeSettings { get; set; }
        public DbSet<Unsubscribes.UnsubscribePage> UnsubscribePages { get; set; }
        public DbSet<Unsubscribes.UnsubscribeEmail> UnsubscribeEmails { get; set; }
        public DbSet<Unsubscribes.UnsubscribeButton> UnsubscribeButtons { get; set; }
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
