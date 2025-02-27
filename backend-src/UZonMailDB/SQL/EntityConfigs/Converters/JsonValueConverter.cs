using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

namespace UZonMail.DB.SQL.EntityConfigs.Converters
{
    /// <summary>
    /// 将对象转换成 json 保存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonValueConverter : ValueConverter
    {
        /// <summary>
        /// Json 转换器
        /// </summary>
        /// <param name="modelType"></param>
        public JsonValueConverter(Type modelType)
            : base(() => "", (string v) => v)
        {
            ModelClrType = modelType;
            ProviderClrType = typeof(string);

            // 生成转换器
            ConvertToProvider = ConvertToJson;
            ConvertFromProvider = ConvertFromJson;
        }

        public override Func<object?, object?> ConvertToProvider { get; }

        public override Func<object?, object?> ConvertFromProvider { get; }

        public override LambdaExpression ConvertFromProviderExpression
        {
            get
            {
                ;
                return base.ConvertFromProviderExpression;
            }
        }
        public override LambdaExpression ConvertToProviderExpression
        {
            get
            {
                ;
                return base.ConvertToProviderExpression;
            }
        }
        public override Type ModelClrType { get; }

        public override Type ProviderClrType { get; }

        /// <summary>
        /// 转换成 Json
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string? ConvertToJson(object value)
        {
            if (value == null) return null;
            return JsonConvert.SerializeObject(value);
        }

        private object? ConvertFromJson(object? json)
        {
            if (json == null) return null;
            string jsonString = json.ToString();
            if (string.IsNullOrEmpty(jsonString)) return null;
            try
            {
                return JsonConvert.DeserializeObject(jsonString, ModelClrType);
            }
            catch
            {
                return null;
            }
        }
    }
}
