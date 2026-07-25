using Newtonsoft.Json;
using UzonMail.DB.Managers.Cache;
using UzonMail.DB.SQL;
using UzonMail.DB.SQL.Core.Settings;
using UzonMail.Utils.Extensions;
using UzonMail.Utils.Json;

namespace UzonMail.CorePlugin.Services.Settings.Model;

/// <summary>
/// 支持用户、组织、系统三级继承的设置模型基类。
/// </summary>
public abstract class BaseSettingModel : BaseDBCache<SqlContext, AppSettingCacheKey>
{
    [JsonIgnore]
    private IReadOnlyList<AppSettingSnapshot> _appSettings = [];

    /// <summary>
    /// 系统设置状态，默认为启用。
    /// </summary>
    public AppSettingStatus Status { get; set; } = AppSettingStatus.Enabled;

    /// <inheritdoc />
    protected override async Task UpdateCore(
        CacheBuildContext buildContext,
        SqlContext db,
        CancellationToken cancellationToken
    )
    {
        var settingKeys = new List<AppSettingCacheKey> { Args };
        if (Args.SettingType == AppSettingType.User)
        {
            var userInfo = await UserInfoCache.GetSnapshotAsync(
                buildContext,
                db,
                Args.OwnerId,
                cancellationToken
            );
            settingKeys.Add(
                new AppSettingCacheKey(
                    AppSettingType.Organization,
                    userInfo.OrganizationId,
                    Args.SettingKey
                )
            );
        }

        if (Args.SettingType != AppSettingType.System)
        {
            settingKeys.Add(new AppSettingCacheKey(AppSettingType.System, 0, Args.SettingKey));
        }

        var settings = new List<AppSettingSnapshot>(settingKeys.Count);
        foreach (var settingKey in settingKeys)
        {
            settings.Add(
                await AppSettingSnapshot.GetAsync(buildContext, db, settingKey, cancellationToken)
            );
        }

        _appSettings = settings;
        ReadValuesFromJson();
    }

    /// <summary>
    /// 从当前设置继承链初始化模型属性。
    /// </summary>
    protected abstract void ReadValuesFromJson();

    /// <summary>
    /// 按子级到父级顺序读取首个有效值。
    /// </summary>
    public (bool IsMatched, T Value) GetValue<T>(string key, Func<T, bool> match)
    {
        key = key.ToCamelCase();
        foreach (var jsonSetting in _appSettings)
        {
            var status = jsonSetting.JsonData.SelectTokenOrDefault(
                nameof(Status).ToCamelCase(),
                AppSettingStatus.Enabled
            );
            if (status == AppSettingStatus.Ignored)
                continue;

            if (status == AppSettingStatus.Enabled)
            {
                var value = jsonSetting.JsonData.SelectTokenOrDefault<T>(key, default);
                if (value is not null && match(value))
                    return (true, value);
            }

            if (status == AppSettingStatus.Disabled)
                break;
        }

        return (false, default!);
    }

    /// <summary>
    /// 获取首个有效字符串设置值。
    /// </summary>
    public string GetStringValue(string key, string defaultValue = "")
    {
        var (isMatched, value) = GetValue<string>(key, x => !string.IsNullOrEmpty(x));
        return isMatched ? value : defaultValue;
    }

    /// <summary>
    /// 获取首个非负 double 设置值。
    /// </summary>
    public double GetDoubleValue(string key, double defaultValue = -1)
    {
        var (isMatched, value) = GetValue<double>(key, x => x >= 0);
        return isMatched ? value : defaultValue;
    }

    /// <summary>
    /// 获取首个非负 int 设置值。
    /// </summary>
    public int GetIntValue(string key, int defaultValue = -1)
    {
        var (isMatched, value) = GetValue<int>(key, x => x >= 0);
        return isMatched ? value : defaultValue;
    }

    /// <summary>
    /// 获取首个非负 long 设置值。
    /// </summary>
    public long GetLongValue(string key, long defaultValue = -1)
    {
        var (isMatched, value) = GetValue<long>(key, x => x >= 0);
        return isMatched ? value : defaultValue;
    }

    /// <summary>
    /// 获取首个 bool 设置值。
    /// </summary>
    public bool GetBoolValue(string key, bool defaultValue = false)
    {
        var (isMatched, value) = GetValue<bool>(key, _ => true);
        return isMatched ? value : defaultValue;
    }
}
