using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UZonMail.Utils.Http
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// 将结果转换成 json
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<JObject> ToJson(this HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if(string.IsNullOrWhiteSpace(content))
            {
                return new JObject();
            }

            return JObject.Parse(content);
        }
    }
}
