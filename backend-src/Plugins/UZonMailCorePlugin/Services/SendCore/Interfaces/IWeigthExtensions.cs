using log4net;

namespace UZonMail.CorePlugin.Services.SendCore.Interfaces
{
    public static class IWeigthExtensions
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(IWeigthExtensions));

        /// <summary>
        /// 根据权重随机获取一个对象
        /// 若为空，说明权重总和为 0
        /// </summary>
        /// <typeparam name="Tkey"></typeparam>
        /// <param name="datas"></param>
        /// <returns></returns>
        public static IWeight? GetDataByWeight<TKey, TValue>(
            this IDictionary<TKey, TValue> keyValuePairs
        )
            where TValue : IWeight
        {
            // 获取可用的项
            var useableValues = keyValuePairs.Values.Where(x => x.Enable).ToList();
            if (useableValues.Count == 0)
                return null;

            // 计算总的权重
            var totalWeight = useableValues.Sum(x => x.Weight);
            if (totalWeight == 0)
            {
                _logger.Error($"所有 {typeof(TValue).Name} 权重和为 0");
                return null;
            }
            var randomWeight = new Random().Next(totalWeight);
            int total = 0;
            int index = 0;
            while (total < randomWeight)
            {
                total += useableValues[total].Weight;
                index++;
            }

            return useableValues[index];
        }
    }
}
