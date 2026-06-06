using UZonMail.DB.SQL.Core.Settings;

namespace UZonMail.DB.Managers.Cache
{
    public interface IAppSettingCacheArg
    {
        AppSettingType SettingType { get; }

        long OwnerId { get; }

        public string Key { get; }
    }
}
