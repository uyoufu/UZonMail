using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMail.Utils.Dictionary;

namespace UZonMail.Utils.Config
{
    /// <summary>
    /// 配置容器，可有多个配置
    /// </summary>
    public class ConfigContainer
    {
        private ConfigContainer() { }

        private DictionaryPlus<string, IConfig> _configs = new DictionaryPlus<string, IConfig>();

        public IConfig ActivatedConfig { get; private set; }

        #region 静态
        private static ConfigContainer _instance;
        public static ConfigContainer Instance
        {
            get
            {
                _instance ??= new ConfigContainer();
                return _instance;
            }
        }

        /// <summary>
        /// 添加文件配置
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="configPath"></param>
        public static void AddFileConfig(string configPath, ConfigType configType)
        {
            if (!File.Exists(configPath)) throw new ArgumentNullException($"文件{configPath}不存在");

            // 读取配置，然后激活
            // 通过 T 类型来生成 config

            switch (configType)
            {
                case ConfigType.Json:
                    ReadJsonConfig(configPath);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 激活配置文件
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns></returns>
        public static IConfig ActiveConfig(string configKey)
        {
            var config = Instance._configs[configKey];
            Instance.ActivatedConfig = config;
            return Instance.ActivatedConfig;
        }

        /// <summary>
        /// 获取激活的配置
        /// </summary>
        /// <returns></returns>
        public static IConfig GetActiveConfig() 
        {
            if (Instance.ActivatedConfig == null) throw new ArgumentNullException("没有激活的配置");
            return Instance.ActivatedConfig;
        }

        /// <summary>
        /// 读取json配置
        /// </summary>
        /// <param name="configPath"></param>
        private static void ReadJsonConfig(string configPath)
        {
            var streamReader = new StreamReader(configPath);
            JsonReader jsonReader = new JsonTextReader(streamReader);
            var configObj = JToken.ReadFrom(jsonReader) as JObject;
            if (configObj == null) throw new ArgumentNullException($"{configPath} 不是有效的 json 格式");

            Instance._configs.Add(configPath, new JsonConfig(data: configObj));
        }
        #endregion

    }
}
