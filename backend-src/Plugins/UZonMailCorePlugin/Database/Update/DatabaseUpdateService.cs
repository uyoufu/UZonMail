using Microsoft.EntityFrameworkCore;
using UZonMail.Core.Config.SubConfigs;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;
using UZonMail.Utils.Web.Service;

namespace UZonMail.Core.Database.Updater
{
    /// <summary>
    /// 数据升级管理器
    /// </summary>
    /// <param name="db"></param>
    public class DatabaseUpdateService(IServiceProvider serviceProvider, SqlContext db, IConfiguration config) : IScopedService
    {
        private readonly static Version _minVersionSupport = new("0.10.0.0");
        private readonly static Version _requiredVersion = new("0.11.1.0");

        private readonly string _settingKey = "DataVersion";

        /// <summary>
        /// 获取所有的更新器
        /// 当新增更新器时，在此处添加
        /// </summary>
        /// <returns></returns>
        private List<IDatabaseUpdater> GetUpdaters()
        {
            return serviceProvider.GetServices<IDatabaseUpdater>().ToList();
        }

        /// <summary>
        /// 更新数据
        /// 内部会自动判断版本，执行对应的更新器
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task Update()
        {
            var cc = config;
            var userConfig = new UserConfig();
            config.GetSection("User")?.Bind(userConfig);

            // 获取数据版本
            var versionSetting = await db.SystemSettings.FirstOrDefaultAsync(x => x.Key == _settingKey);
            if (versionSetting == null)
            {
                // 初始化版本
                versionSetting = new SystemSetting
                {
                    Key = _settingKey,
                    StringValue = "0.0.0.0"
                };
                db.SystemSettings.Add(versionSetting);
            }
            else if (versionSetting.StringValue != "0.0.0.0")
            {
                // 判断是支持升级
                var dbVersion = new Version(versionSetting.StringValue);
                if (dbVersion < _minVersionSupport)
                    throw new ArgumentException("当前数据库版本太低，不支持直接升级。请删除数据库后，重启启动");
            }

            var originVersion = new Version(versionSetting.StringValue);
            if (originVersion > _requiredVersion)
                throw new ArgumentException("数据库版本高于当前所需版本，请更新程序后再使用");

            // 若版本一致时，说明已经更新过
            if (originVersion == _requiredVersion) return;

            // 执行数据库升级
            // 实例化所有的更新类
            var updaters = GetUpdaters().Where(x => x != null).Where(x => x.Version > originVersion && x.Version <= _requiredVersion)
                .OrderBy(x => x.Version); // 升序排列
            foreach (var updater in updaters)
            {
                await updater.Update();
            }

            // 更新版本号
            versionSetting.StringValue = _requiredVersion.ToString();
            await db.SaveChangesAsync();
        }
    }
}
