using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.Utils.Config
{
    /// <summary>
    /// json 配置文件帮助类
    /// </summary>
    public class JsonConfig : IConfig
    {
        public JObject Data { get;private set; }
        public JsonConfig(JObject data)
        {
            Data = data;
        }
        
        public string GetStringValue(string path)
        {
          return GetValue<string>(path);
        }

        public T GetValue<T>(string path)
        {
            return Data.SelectToken(path).ToObject<T>();
        }
    }
}
