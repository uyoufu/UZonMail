namespace UZonMail.DB.Extensions
{
    public static class ObjectCopyExtensions
    {
        /// <summary>
        /// 获取所有的属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <param name="source"></param>
        public static void CopyAllProperties<T>(this T target, T source)
        {
            var targetProperties = target.GetType().GetProperties();
            var sourceProperties = source.GetType().GetProperties();
            foreach (var targetProperty in targetProperties)
            {
                if (!targetProperty.CanWrite || !targetProperty.GetSetMethod(true).IsPublic) continue;

                var sourceProperty = sourceProperties.FirstOrDefault(x => x.Name == targetProperty.Name && x.CanRead);
                if (targetProperty == null) continue;

                // 开始赋值
                targetProperty.SetValue(target, sourceProperty.GetValue(source));
            }
        }
    }
}
