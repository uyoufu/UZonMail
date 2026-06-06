using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UZonMail.Utils.Database.LiteDB
{
    public abstract class AutoObjectId
    {
        /// <summary>
        /// Id
        /// 采用字符串型 Id,如果是递增 Id,容易被其他用户知道规律从而删除
        /// </summary>
        [JsonPropertyName("_id"), Newtonsoft.Json.JsonProperty("_id")]
        [BsonId, BsonField("_id")]
        public string Id { get; set; } = ObjectId.NewObjectId().ToString();

        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate { get; set; }

        public AutoObjectId()
        {
            // 必须要用字符串当 id,方便格式化给前端使用
            // Id = ObjectId.NewObjectId().ToString();
        }

        /// <summary>
        /// 获取过滤字符串
        /// </summary>
        /// <returns></returns>
        public virtual string GetFilterString()
        {
            return Id.ToString();
        }

        /// <summary>
        /// 通过字段名称获取值
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object GetValue(string fieldName)
        {
            var propertyInfo = GetType().GetProperty(fieldName);
            if (propertyInfo == null) return string.Empty;

            return propertyInfo.GetValue(this, null);
        }
    }
}
