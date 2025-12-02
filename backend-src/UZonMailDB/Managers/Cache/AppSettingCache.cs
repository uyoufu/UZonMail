using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using UZonMail.DB.SQL;
using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.DB.Managers.Cache
{
    /// <summary>
    /// 系统设置缓存
    /// 一个 id 对应多个实例，需要通过 Key 或者其它语句进行区分
    /// </summary>
    public class AppSettingCache : BaseDBCache<SqlContext, IAppSettingCacheArg>
    {
        public long Id { get; private set; }

        public JToken JsonData { get; private set; }

        public override void Dispose() { }

        protected override async Task UpdateCore(SqlContext db)
        {
            var query = db
                .AppSettings.Where(x => x.Key == Args.Key)
                .Where(x => x.Type == Args.SettingType);
            switch (Args.SettingType)
            {
                case AppSettingType.User:
                    query = query.Where(x => x.UserId == Id);
                    break;
                default:
                    break;
            }

            var setting = await query.FirstOrDefaultAsync();

            if (setting == null)
            {
                JsonData = new JObject();
            }
            else
            {
                Id = setting.Id;
                JsonData = setting.Json ?? new JObject();
            }
        }
    }
}
