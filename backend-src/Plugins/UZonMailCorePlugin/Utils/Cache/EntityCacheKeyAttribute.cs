namespace UZonMail.Core.Utils.Cache
{
    /// <summary>
    /// 设置的键名称特性
    /// </summary>
    /// <param name="key"></param>
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityCacheKeyAttribute(string key) : Attribute
    {
        public string Key => key;
    }
}
