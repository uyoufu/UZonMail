namespace UZonMail.Core.Services.Settings.Core
{
    /// <summary>
    /// 设置的键名称特性
    /// </summary>
    /// <param name="key"></param>
    [AttributeUsage(AttributeTargets.Class)]
    public class SettingEntityKeyAttribute(string key) : Attribute
    {
        public string Key => key;
    }
}
