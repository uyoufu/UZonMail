using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static UZonMail.Utils.Extensions.NameStylesExtension;

namespace UZonMail.Utils.Json
{
    /// <summary>
    /// json 帮助类
    /// </summary>
    public class JsonHelper
    {
        /// <summary>
        /// 读取多个 json 文件然后合并它们
        /// </summary>
        /// <returns></returns>
        public static JObject ReadAndMergeJsonFiles(List<string> jsonFileNames)
        {
            JObject result = new JObject();
            foreach (string fileName in jsonFileNames)
            {
                StreamReader sr = File.OpenText(fileName);
                JsonTextReader reader = new JsonTextReader(sr);
                var obj = JToken.ReadFrom(reader);
                result.Merge(obj, new JsonMergeSettings() { MergeArrayHandling = MergeArrayHandling.Union });
            }
            return result;
        }

        /// <summary>
        /// 读取目录中的某类 json 文件
        /// </summary>
        /// <param name="dirName"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static JObject ReadAndMergeJsonFiles(string dirName, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories)
        {
            if (!searchPattern.ToLower().EndsWith(".json")) throw new ArgumentException($"only json files allowed to read");

            var files = Directory.GetFiles(dirName, searchPattern, searchOption);
            return ReadAndMergeJsonFiles(files.ToList());
        }

        /// <summary>
        /// 利用 JObject 来更新 model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="data"></param>
        /// <param name="nameStylesType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T UpdateModelByJObject<T>(T model,JObject data, NameStylesType nameStylesType = NameStylesType.PascalCase)
        {
            if (model == null || data == null) throw new ArgumentNullException("model or data is null");
            T updateData = data.ToObject<T>() ?? throw new ArgumentNullException($"could not convert JObject to {model.GetType()}");

            // 更新数据
            var propertyInfos = model.GetType().GetProperties();
            foreach(var item in data)
            {
                var fieldName = item.Key.ToNameStyle(nameStylesType);
                var propertyInfo = propertyInfos.FirstOrDefault(x=> x.Name == fieldName);
                if(propertyInfo == null)continue;

                propertyInfo.SetValue(model, propertyInfo.GetValue(updateData));
            }

            return model;
        }
    }
}
