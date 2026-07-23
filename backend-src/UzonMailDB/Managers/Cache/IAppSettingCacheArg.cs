using UzonMail.DB.SQL.Core.Settings;

namespace UzonMail.DB.Managers.Cache
{
    public interface IAppSettingCacheArg
    {
        AppSettingType SettingType { get; }

        long OwnerId { get; }

        public string Key { get; }
    }
}
